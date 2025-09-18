using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Converter.Core;
using Converter.Gui.Profiles;
using Converter.Gui.Services;

namespace Converter.Gui;

public partial class MainForm : Form
{
    private readonly ProfileRepository _profileRepository = new();
    private readonly BatchConversionService _conversionService = new();
    private readonly List<ConversionProfile> _profiles;
    private readonly string _settingsPath;
    private UserSettings _settings;
    private readonly List<PdfFileItem> _files = new();
    private readonly Dictionary<string, ListViewItem> _fileItems = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _conversionCts;
    private CancellationTokenSource? _previewCts;
    private readonly SemaphoreSlim _conversionSemaphore = new(1, 1);
    private FileSystemWatcher? _watcher;
    private CancellationTokenSource? _watcherCts;
    private readonly HashSet<string> _watchQueue = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _watchSemaphore = new(1, 1);
    private bool _isConverting;
    private Image? _afterPreview;
    private readonly List<WatchLogEntry> _watchLogs = new();
    private const int MaxWatchLogEntries = 200;
    private string? _currentWatchFolder;
    private string _currentSuggestedSuffix = string.Empty;
    private bool _suffixUserEdited;
    private bool _isUpdatingSuffixText;
    private bool _shouldAutoFitPreview = true;

    public MainForm()
    {
        InitializeComponent();

        _settingsPath = Path.Combine(ProfileRepository.GetConfigurationDirectory(), "settings.json");
        _settings = UserSettings.Load(_settingsPath);
        _profiles = _profileRepository.Load().ToList();

        InitializeData();
    }

    private void InitializeData()
    {
        openPdfDialog.Multiselect = true;
        openPdfDialog.Filter = "PDF (*.pdf)|*.pdf|Tous les fichiers (*.*)|*.*";

        _suffixUserEdited = !string.IsNullOrWhiteSpace(_settings.FileNameSuffix);

        RefreshProfiles();

        if (!string.IsNullOrWhiteSpace(_settings.FileNameSuffix))
        {
            SetSuffixTextSafely(_settings.FileNameSuffix!);
            _suffixUserEdited = true;
        }

        separateTiffCheckBox.Checked = _settings.SeparateTiffPages;

        if (!string.IsNullOrWhiteSpace(_settings.LastOutputFolder))
        {
            outputFolderTextBox.Text = _settings.LastOutputFolder;
        }

        openExplorerCheckBox.Checked = _settings.OpenExplorerAfterConversion;
        openLogCheckBox.Checked = _settings.OpenLogAfterConversion;

        var zoomValue = (int)Math.Clamp(_settings.PreviewZoom * 100, previewZoomTrackBar.Minimum, previewZoomTrackBar.Maximum);
        previewZoomTrackBar.Value = zoomValue;
        UpdatePreviewZoomLabel();

        watchFolderTextBox.Text = _settings.WatchFolderPath ?? string.Empty;
        watchLogTextBox.Clear();
        _watchLogs.Clear();

        if (_settings.WatchFolderEnabled && Directory.Exists(_settings.WatchFolderPath ?? string.Empty))
        {
            StartWatchFolder();
        }
        else
        {
            watchFolderCheckBox.Checked = false;
            UpdateWatchToggleAppearance();
            AppendWatchLog("Surveillance inactive");
        }

        UpdateOutputFolderControls();
        UpdateOutputDetails();
        UpdateSelectedFileDetails();
        UpdateActions();
        statusStripLabel.Text = "Pret";
    }

    private void RefreshProfiles()
    {
        profileComboBox.BeginUpdate();
        profileComboBox.Items.Clear();
        foreach (var profile in _profiles)
        {
            profileComboBox.Items.Add(profile);
        }
        profileComboBox.EndUpdate();

        if (_profiles.Count > 0)
        {
            var selected = _profiles.FirstOrDefault(p => string.Equals(p.Name, _settings.LastProfileName, StringComparison.OrdinalIgnoreCase))
                           ?? _profiles[0];
            profileComboBox.SelectedItem = selected;
        }

        UpdateProfileDetails();
    }

    private void AddFiles(IEnumerable<string> filePaths)
    {
        bool added = false;
        foreach (var path in filePaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (!File.Exists(path) || !string.Equals(Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (_fileItems.ContainsKey(path))
            {
                continue;
            }

            var item = new PdfFileItem(path);
            _files.Add(item);

            var listViewItem = new ListViewItem(item.FileName)
            {
                Tag = item
            };
            listViewItem.SubItems.Add(item.Directory);
            listViewItem.SubItems.Add(string.Empty);
            _fileItems[path] = listViewItem;
            filesListView.Items.Add(listViewItem);
            added = true;
        }

        if (added)
        {
            filesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            if (string.IsNullOrWhiteSpace(outputFolderTextBox.Text))
            {
                var firstDir = _files.FirstOrDefault()?.Directory;
                if (!string.IsNullOrWhiteSpace(firstDir))
                {
                    outputFolderTextBox.Text = firstDir;
                    UpdateOutputFolderControls();
                }
            }
        }

        UpdateActions();
        UpdateOutputFolderControls();
        UpdateSelectedFileDetails();
    }

    private void RemoveSelectedFiles()
    {
        foreach (ListViewItem selected in filesListView.SelectedItems)
        {
            if (selected.Tag is PdfFileItem file)
            {
                _files.Remove(file);
                _fileItems.Remove(file.Path);
            }

            filesListView.Items.Remove(selected);
        }

        UpdateActions();
        UpdateSelectedFileDetails();
    }

    private void ClearFiles()
    {
        _files.Clear();
        _fileItems.Clear();
        filesListView.Items.Clear();
        UpdateActions();
        UpdateSelectedFileDetails();
    }

    private void UpdateActions()
    {
        bool hasFiles = _files.Count > 0;
        bool ready = hasFiles && !_isConverting;

        convertButton.Enabled = ready;
        removeFilesButton.Enabled = filesListView.SelectedItems.Count > 0 && !_isConverting;
        clearFilesButton.Enabled = hasFiles && !_isConverting;
        addFilesButton.Enabled = !_isConverting;
        stopButton.Enabled = _isConverting;
    }

    private void UpdateSelectedFileDetails()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(UpdateSelectedFileDetails));
            return;
        }

        if (filesListView.SelectedItems.Count == 0 || filesListView.SelectedItems[0].Tag is not PdfFileItem item)
        {
            selectedFileDetailsLabel.Text = "Selectionnez un PDF pour afficher ses details.";
            return;
        }

        try
        {
            var info = new FileInfo(item.Path);
            if (!info.Exists)
            {
                selectedFileDetailsLabel.Text = $"{item.FileName} - introuvable";
                return;
            }

            var size = FormatBytes(info.Length);
            var modified = info.LastWriteTime.ToString("f", CultureInfo.CurrentUICulture);
            selectedFileDetailsLabel.Text = $"{item.FileName} - {size} - Modifie le {modified}";
        }
        catch (Exception ex)
        {
            selectedFileDetailsLabel.Text = $"{item.FileName} - informations indisponibles ({ex.Message})";
        }
    }

    private void UpdateOutputFolderControls()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(UpdateOutputFolderControls));
            return;
        }

        var folder = outputFolderTextBox.Text.Trim();
        openOutputFolderButton.Enabled = Directory.Exists(folder);
        UpdateOutputDetails();
    }

    private void UpdateOutputDetails()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(UpdateOutputDetails));
            return;
        }

        var folder = outputFolderTextBox.Text.Trim();
        var suffix = suffixTextBox.Text.Trim();
        var separate = separateTiffCheckBox.Checked;

        var builder = new StringBuilder();

        if (string.IsNullOrWhiteSpace(folder))
        {
            builder.AppendLine("Dossier : (aucun)");
        }
        else
        {
            builder.AppendLine($"Dossier : {folder}");
        }

        builder.AppendLine(string.IsNullOrWhiteSpace(suffix)
            ? "Suffixe : (aucun)"
            : $"Suffixe : {suffix}");

        builder.Append(separate
            ? "TIFF multipage : un fichier par page"
            : "TIFF multipage : fichier unique");

        outputDetailsLabel.Text = builder.ToString();
    }

    private void UpdateWatchToggleAppearance()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(UpdateWatchToggleAppearance));
            return;
        }

        bool active = _watcher is not null;
        watchFolderCheckBox.Checked = active;
        watchFolderCheckBox.Text = active ? "Surveillance en cours" : "Activer la surveillance";
        watchFolderCheckBox.BackColor = active ? Color.FromArgb(0, 120, 215) : SystemColors.Control;
        watchFolderCheckBox.ForeColor = active ? Color.White : SystemColors.ControlText;

        if (active && !string.IsNullOrEmpty(_currentWatchFolder))
        {
            watchFolderStatusLabel.Text = $"Surveillance en cours : {_currentWatchFolder}";
            watchFolderStatusLabel.ForeColor = Color.FromArgb(0, 120, 215);
        }
        else
        {
            watchFolderStatusLabel.Text = "Surveillance inactive";
            watchFolderStatusLabel.ForeColor = SystemColors.GrayText;
        }
    }

    private void AppendWatchLog(string message, Color? color = null)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => AppendWatchLog(message, color)));
            return;
        }

        var entry = new WatchLogEntry($"[{DateTime.Now:HH:mm:ss}] {message}", color ?? SystemColors.ControlText);
        _watchLogs.Add(entry);
        if (_watchLogs.Count > MaxWatchLogEntries)
        {
            _watchLogs.RemoveRange(0, _watchLogs.Count - MaxWatchLogEntries);
        }

        watchLogTextBox.SuspendLayout();
        watchLogTextBox.Clear();
        foreach (var log in _watchLogs)
        {
            watchLogTextBox.SelectionStart = watchLogTextBox.TextLength;
            watchLogTextBox.SelectionColor = log.Color;
            watchLogTextBox.AppendText(log.Text + Environment.NewLine);
        }

        watchLogTextBox.SelectionColor = watchLogTextBox.ForeColor;
        watchLogTextBox.SelectionStart = watchLogTextBox.TextLength;
        watchLogTextBox.ScrollToCaret();
        watchLogTextBox.ResumeLayout();
    }

    private ConversionProfile? GetSelectedProfile() => profileComboBox.SelectedItem as ConversionProfile;

    private async void ConvertButton_Click(object? sender, EventArgs e)
    {
        if (_isConverting)
        {
            return;
        }

        if (_files.Count == 0)
        {
            MessageBox.Show(this, "Ajoutez au moins un fichier PDF.", "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var outputFolder = outputFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            MessageBox.Show(this, "Choisissez un dossier de sortie.", "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var profile = GetSelectedProfile();
        if (profile is null)
        {
            MessageBox.Show(this, "Selectionnez un profil de conversion.", "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Directory.CreateDirectory(outputFolder);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Impossible de creer le dossier de sortie : {ex.Message}", "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();

        conversionProgressBar.Value = 0;
        conversionProgressBar.Maximum = _files.Count;
        conversionProgressBar.Style = ProgressBarStyle.Continuous;

        foreach (var item in _fileItems.Values)
        {
            item.SubItems[2].Text = string.Empty;
            item.BackColor = SystemColors.Window;
            item.ForeColor = SystemColors.WindowText;
        }

        _isConverting = true;
        UpdateActions();
        statusStripLabel.Text = "Conversion en cours...";

        _conversionCts = new CancellationTokenSource();
        var token = _conversionCts.Token;

        try
        {
            await _conversionSemaphore.WaitAsync(token).ConfigureAwait(true);
            var progress = new Progress<BatchConversionProgress>(HandleProgressReport);
            var filePaths = _files.Select(f => f.Path).ToArray();

            BatchConversionResult? result = null;
            Exception? capturedException = null;

            try
            {
                result = await _conversionService.ConvertAsync(
                    filePaths,
                    outputFolder,
                    profile,
                    progress,
                    token,
                    GetOutputSuffix(),
                    separateTiffCheckBox.Checked).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                statusStripLabel.Text = "Conversion annulee.";
                MessageBox.Show(this, "La conversion a ete annulee.", "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            catch (Exception ex)
            {
                capturedException = ex;
            }
            finally
            {
                _conversionSemaphore.Release();
            }

            if (capturedException is not null)
            {
                statusStripLabel.Text = "Erreur lors de la conversion.";
                MessageBox.Show(this, capturedException.Message, "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (result is null)
            {
                return;
            }

            HandleConversionResult(result, profile, outputFolder);
        }
        finally
        {
            _conversionCts?.Dispose();
            _conversionCts = null;
            _isConverting = false;
            UpdateActions();
        }
    }

    private void HandleProgressReport(BatchConversionProgress progress)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => HandleProgressReport(progress)));
            return;
        }

        if (_fileItems.TryGetValue(progress.InputPath, out var item))
        {
            if (progress.Stage == BatchConversionStage.Starting)
            {
                item.SubItems[2].Text = "Conversion...";
                item.BackColor = Color.FromArgb(255, 249, 219);
                item.ForeColor = Color.FromArgb(102, 60, 0);
            }
            else if (progress.Stage == BatchConversionStage.Completed && progress.Result is not null)
            {
                if (progress.Result.Success)
                {
                    var count = progress.Result.OutputPaths.Count;
                    item.SubItems[2].Text = count > 1 ? $"Succes ({count} fichiers)" : "Succes";
                    item.BackColor = Color.FromArgb(209, 231, 221);
                    item.ForeColor = Color.FromArgb(21, 87, 36);
                }
                else
                {
                    item.SubItems[2].Text = "Erreur";
                    item.BackColor = Color.FromArgb(248, 215, 218);
                    item.ForeColor = Color.FromArgb(114, 28, 36);
                }
            }
        }

        if (progress.Total > 0)
        {
            conversionProgressBar.Maximum = progress.Total;
            conversionProgressBar.Value = Math.Clamp(progress.Completed, 0, progress.Total);
        }

        statusStripLabel.Text = progress.Stage == BatchConversionStage.Starting
            ? $"Conversion de {Path.GetFileName(progress.InputPath)}..."
            : $"Progression : {progress.Completed}/{progress.Total}";
    }

    private void HandleConversionResult(BatchConversionResult result, ConversionProfile profile, string outputFolder)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => HandleConversionResult(result, profile, outputFolder)));
            return;
        }

        conversionProgressBar.Value = conversionProgressBar.Maximum;

        var successes = result.Files.Where(f => f.Success).ToList();
        var failures = result.Files.Where(f => !f.Success).ToList();

        statusStripLabel.Text = failures.Count == 0
            ? "Conversion terminee."
            : failures.Count == result.Files.Count ? "Conversion echouee." : "Conversion terminee avec des erreurs.";

        var message = successes.Count > 0
            ? $"{successes.Count} fichier(s) converti(s)."
            : "Aucun fichier converti.";

        if (successes.Count > 0)
        {
            var generated = successes.Sum(f => f.OutputPaths.Count);
            if (generated > successes.Count)
            {
                message += Environment.NewLine + $"{generated} fichier(s) TIFF généré(s).";
            }
        }

        if (failures.Count > 0)
        {
            message += Environment.NewLine + string.Join(Environment.NewLine, failures.Select(f => $"- {Path.GetFileName(f.InputPath)} : {f.ErrorMessage}"));
            MessageBox.Show(this, message, "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else
        {
            MessageBox.Show(this, message, "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        if (successes.Count > 0 && openExplorerCheckBox.Checked && OperatingSystem.IsWindows())
        {
            try
            {
                var firstOutput = successes.SelectMany(f => f.OutputPaths).FirstOrDefault();
                ProcessStartInfo psi;
                if (!string.IsNullOrEmpty(firstOutput))
                {
                    psi = new ProcessStartInfo("explorer.exe", $"/select,\"{firstOutput}\"")
                    {
                        UseShellExecute = true
                    };
                }
                else
                {
                    psi = new ProcessStartInfo("explorer.exe", $"\"{outputFolder}\"")
                    {
                        UseShellExecute = true
                    };
                }

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Impossible d'ouvrir l'explorateur : {ex.Message}", "Explorateur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        try
        {
            var logPath = CreateLogFile(result, profile, outputFolder);
            if (openLogCheckBox.Checked && File.Exists(logPath))
            {
                if (OperatingSystem.IsWindows())
                {
                    Process.Start(new ProcessStartInfo("notepad.exe", logPath) { UseShellExecute = true });
                }
                else
                {
                    Process.Start(new ProcessStartInfo("open", logPath) { UseShellExecute = true });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Impossible d'ecrire le journal : {ex.Message}", "Journal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private string CreateLogFile(BatchConversionResult result, ConversionProfile profile, string outputFolder)
    {
        var logsDirectory = Path.Combine(ProfileRepository.GetConfigurationDirectory(), "logs");
        Directory.CreateDirectory(logsDirectory);
        var fileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        var logPath = Path.Combine(logsDirectory, fileName);

        using var writer = new StreamWriter(logPath, false, System.Text.Encoding.UTF8);
        writer.WriteLine($"Date : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine($"Profil : {profile.Name} ({profile.Describe()})");
        writer.WriteLine($"Dossier de sortie : {outputFolder}");
        writer.WriteLine();
        writer.WriteLine("Fichier;Duree;Entree;Sortie;Fichiers generes;Statut;Message");

        foreach (var file in result.Files)
        {
            var status = file.Success ? "Succes" : "Erreur";
            var message = file.Success ? string.Empty : file.ErrorMessage ?? string.Empty;
            var generated = string.Join('|', file.OutputPaths.Select(p => Path.GetFileName(p)?.Replace(';', ',') ?? string.Empty));
            writer.WriteLine($"{Path.GetFileName(file.InputPath)};{file.Duration.TotalSeconds:F1}s;{FormatBytes(file.InputSize)};{FormatBytes(file.OutputSize)};{generated};{status};{message.Replace(';', ',')}");
        }

        return logPath;
    }

    private static string FormatBytes(long? size)
    {
        if (size is null)
        {
            return "-";
        }

        double value = size.Value;
        string[] units = { "o", "Ko", "Mo", "Go" };
        int unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }

    private void StopButton_Click(object? sender, EventArgs e)
    {
        if (!_isConverting)
        {
            return;
        }

        stopButton.Enabled = false;
        _conversionCts?.Cancel();
        statusStripLabel.Text = "Annulation en cours...";
    }

    private async void FilesListView_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateActions();
        UpdateSelectedFileDetails();
        _shouldAutoFitPreview = true;
        await LoadPreviewAsync();
    }

    private async void RefreshPreviewButton_Click(object? sender, EventArgs e)
    {
        _shouldAutoFitPreview = true;
        await LoadPreviewAsync(force: true);
    }

    private async Task LoadPreviewAsync(bool force = false)
    {
        if (_isConverting && !force)
        {
            return;
        }

        var selected = filesListView.SelectedItems.Count > 0 ? filesListView.SelectedItems[0].Tag as PdfFileItem : null;
        if (selected is null)
        {
            ClearPreview();
            return;
        }

        _previewCts?.Cancel();
        _previewCts?.Dispose();
        _previewCts = new CancellationTokenSource();
        var token = _previewCts.Token;

        var profile = GetSelectedProfile();
        if (profile is null)
        {
            SetPreviewStatus("Selectionnez un profil de conversion.");
            return;
        }

        string? afterPath = null;

        try
        {
            SetPreviewStatus("Previsualisation en cours...");

            afterPath = CreatePreviewTempFile(".tiff");

            await GhostscriptRunner.ConvertPdfToTiffAsync(selected.Path, afterPath, profile.Device, profile.Dpi, profile.Compression, profile.ExtraParameters, 1, 1, token).ConfigureAwait(true);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var afterImage = LoadImageSafely(afterPath);

            Invoke(new Action(() =>
            {
                _afterPreview?.Dispose();
                _afterPreview = afterImage;
                afterPictureBox.Image = afterImage;
                ApplyPreviewZoom(previewZoomTrackBar.Value / 100.0);
                SetPreviewStatus($"Previsualisation : {Path.GetFileName(selected.Path)}");
                AutoFitPreviewToWidthIfNeeded();
            }));
        }
        catch (OperationCanceledException)
        {
            SetPreviewStatus("Previsualisation annulee.");
        }
        catch (Exception ex)
        {
            SetPreviewStatus($"Previsualisation impossible : {ex.Message}");
        }
        finally
        {
            if (afterPath is not null && File.Exists(afterPath))
            {
                try { File.Delete(afterPath); } catch { }
            }
        }
    }
    private static Image LoadImageSafely(string path)
    {
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var ms = new MemoryStream();
        file.CopyTo(ms);
        ms.Position = 0;
        return Image.FromStream(ms);
    }

    private static string CreatePreviewTempFile(string extension)
    {
        var name = $"color-converter-preview_{Guid.NewGuid():N}{extension}";
        return Path.Combine(Path.GetTempPath(), name);
    }

    private void ClearPreview()
    {
        _afterPreview?.Dispose();
        _afterPreview = null;
        afterPictureBox.Image = null;
        SetPreviewStatus("Selectionnez un PDF pour afficher l'apercu.");
    }

    private void SetPreviewStatus(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => SetPreviewStatus(message)));
            return;
        }

        previewStatusLabel.Text = message;
    }

    private void PreviewZoomTrackBar_ValueChanged(object? sender, EventArgs e)
    {
        ApplyPreviewZoom(previewZoomTrackBar.Value / 100.0);
        UpdatePreviewZoomLabel();
        SaveSettings();
    }

    private void UpdatePreviewZoomLabel()
    {
        previewZoomValueLabel.Text = $"Zoom : {previewZoomTrackBar.Value}%";
    }

    private void AutoFitPreviewToWidthIfNeeded()
    {
        if (!_shouldAutoFitPreview)
        {
            return;
        }

        if (_afterPreview is null)
        {
            _shouldAutoFitPreview = false;
            return;
        }

        var availableWidth = afterPreviewPanel.ClientSize.Width;
        if (availableWidth <= 0)
        {
            return;
        }

        var zoom = availableWidth / (double)_afterPreview.Width;
        zoom = Math.Clamp(zoom, previewZoomTrackBar.Minimum / 100.0, previewZoomTrackBar.Maximum / 100.0);
        var zoomPercent = (int)Math.Round(zoom * 100);
        zoomPercent = Math.Max(previewZoomTrackBar.Minimum, Math.Min(previewZoomTrackBar.Maximum, zoomPercent));

        _shouldAutoFitPreview = false;

        if (previewZoomTrackBar.Value != zoomPercent)
        {
            previewZoomTrackBar.Value = zoomPercent;
        }
        else
        {
            ApplyPreviewZoom(zoomPercent / 100.0);
            UpdatePreviewZoomLabel();
        }
    }

    private void ApplyPreviewZoom(double zoom)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ApplyPreviewZoom(zoom)));
            return;
        }

        if (_afterPreview is not null)
        {
            afterPictureBox.Width = (int)(_afterPreview.Width * zoom);
            afterPictureBox.Height = (int)(_afterPreview.Height * zoom);
        }
    }

    private void AfterPreviewPanel_SizeChanged(object? sender, EventArgs e)
    {
        AutoFitPreviewToWidthIfNeeded();
    }

    private void AddFilesButton_Click(object? sender, EventArgs e)
    {
        if (openPdfDialog.ShowDialog(this) == DialogResult.OK)
        {
            AddFiles(openPdfDialog.FileNames);
        }
    }

    private void RemoveFilesButton_Click(object? sender, EventArgs e) => RemoveSelectedFiles();

    private void ClearFilesButton_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show(this, "Effacer la liste des fichiers ?", "Fichiers", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            ClearFiles();
        }
    }

    private void FilesListView_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data is not null && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var paths = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (paths?.Any(p => string.Equals(Path.GetExtension(p), ".pdf", StringComparison.OrdinalIgnoreCase)) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
    }

    private void FilesListView_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        var paths = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (paths is not null)
        {
            AddFiles(paths);
        }
    }

    private void ProfileComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateProfileDetails();
        SaveSettings();
        _shouldAutoFitPreview = true;
        _ = LoadPreviewAsync(force: true);
    }

    private void UpdateProfileDetails()
    {
        if (profileComboBox.SelectedItem is ConversionProfile profile)
        {
            var compression = string.IsNullOrWhiteSpace(profile.Compression) ? "Auto" : profile.Compression;
            var colorLabel = GetProfileColorLabel(profile.Device);
            var extras = profile.ExtraParameters is { Count: > 0 }
                ? string.Join(", ", profile.ExtraParameters)
                : "Aucun";

            profileDetailsLabel.Text =
                $"Appareil : {profile.Device}{Environment.NewLine}" +
                $"Compression : {compression}{Environment.NewLine}" +
                $"Résolution : {profile.Dpi} dpi{Environment.NewLine}" +
                $"Couleur : {colorLabel}{Environment.NewLine}" +
                $"Paramètres avancés : {extras}";

            UpdateSuggestedSuffix(profile, compression, colorLabel);
        }
        else
        {
            profileDetailsLabel.Text = "Aucun profil sélectionné.";
            _currentSuggestedSuffix = string.Empty;
        }
    }

    private static string GetProfileColorLabel(string device)
    {
        if (string.IsNullOrWhiteSpace(device))
        {
            return "Auto";
        }

        var normalized = device.ToLowerInvariant();
        if (normalized.Contains("gray") || normalized.Contains("grey"))
        {
            return "Niveaux de gris";
        }

        if (normalized.Contains("g4") || normalized.Contains("mono") || normalized.Contains("bw"))
        {
            return "Noir et blanc";
        }

        return "Couleur";
    }

    private void UpdateSuggestedSuffix(ConversionProfile profile, string compression, string colorLabel)
    {
        var compressionPart = NormalizeSuffixPart(compression);
        var colorPart = NormalizeSuffixPart(colorLabel);
        var suffixBuilder = new StringBuilder();
        suffixBuilder.Append('_');
        suffixBuilder.Append(profile.Dpi);
        suffixBuilder.Append("dpi");

        if (!string.IsNullOrEmpty(compressionPart))
        {
            suffixBuilder.Append('_');
            suffixBuilder.Append(compressionPart);
        }

        if (!string.IsNullOrEmpty(colorPart))
        {
            suffixBuilder.Append('_');
            suffixBuilder.Append(colorPart);
        }

        _currentSuggestedSuffix = suffixBuilder.ToString();

        if (!_suffixUserEdited || string.Equals(suffixTextBox.Text, _currentSuggestedSuffix, StringComparison.Ordinal))
        {
            SetSuffixTextSafely(_currentSuggestedSuffix);
            _suffixUserEdited = false;
        }
    }

    private static string NormalizeSuffixPart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var raw in value.Trim().Normalize(NormalizationForm.FormD).ToLowerInvariant())
        {
            if (CharUnicodeInfo.GetUnicodeCategory(raw) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var c = raw;
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else if (c is ' ' or '-' or '_' or '.')
            {
                if (builder.Length > 0 && builder[^1] != '_')
                {
                    builder.Append('_');
                }
            }
        }

        return builder.ToString().Trim('_');
    }

    private void SetSuffixTextSafely(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => SetSuffixTextSafely(text)));
            return;
        }

        _isUpdatingSuffixText = true;
        suffixTextBox.Text = text;
        _isUpdatingSuffixText = false;
        UpdateOutputDetails();
    }

    private string? GetOutputSuffix()
    {
        var suffix = suffixTextBox.Text.Trim();
        return string.IsNullOrEmpty(suffix) ? null : suffix;
    }

    private void ManageProfilesButton_Click(object? sender, EventArgs e)
    {
        using var manager = new ProfileManagerForm(_profiles);
        if (manager.ShowDialog(this) == DialogResult.OK)
        {
            _profiles.Clear();
            _profiles.AddRange(manager.Profiles);
            _profileRepository.Save(_profiles);
            RefreshProfiles();
            SaveSettings();
        }
    }

    private void OutputFolderTextBox_TextChanged(object? sender, EventArgs e) => UpdateOutputFolderControls();

    private void SuffixTextBox_TextChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingSuffixText)
        {
            return;
        }

        _suffixUserEdited = !string.Equals(suffixTextBox.Text, _currentSuggestedSuffix, StringComparison.Ordinal);
        UpdateOutputDetails();
        SaveSettings();
    }

    private void SeparateTiffCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateOutputDetails();
        SaveSettings();
    }

    private void BrowseOutputFolderButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(outputFolderTextBox.Text) && Directory.Exists(outputFolderTextBox.Text))
        {
            outputFolderDialog.SelectedPath = outputFolderTextBox.Text;
        }

        if (outputFolderDialog.ShowDialog(this) == DialogResult.OK)
        {
            outputFolderTextBox.Text = outputFolderDialog.SelectedPath;
            UpdateOutputFolderControls();
            SaveSettings();
        }
    }

    private void OpenOutputFolderButton_Click(object? sender, EventArgs e)
    {
        var folder = outputFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            MessageBox.Show(this, "Selectionnez un dossier de sortie valide.", "Dossier de sortie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            ProcessStartInfo psi;
            if (OperatingSystem.IsWindows())
            {
                psi = new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true };
            }
            else if (OperatingSystem.IsMacOS())
            {
                psi = new ProcessStartInfo("open", folder) { UseShellExecute = true };
            }
            else
            {
                psi = new ProcessStartInfo("xdg-open", folder) { UseShellExecute = true };
            }

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Impossible d'ouvrir le dossier : {ex.Message}", "Dossier de sortie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void OpenExplorerCheckBox_CheckedChanged(object? sender, EventArgs e) => SaveSettings();
    private void OpenLogCheckBox_CheckedChanged(object? sender, EventArgs e) => SaveSettings();

    private void WatchFolderCheckBox_Click(object? sender, EventArgs e)
    {
        if (_watcher is not null)
        {
            StopWatchFolder();
        }
        else
        {
            StartWatchFolder();
        }

        SaveSettings();
    }

    private void BrowseWatchFolderButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(watchFolderTextBox.Text) && Directory.Exists(watchFolderTextBox.Text))
        {
            outputFolderDialog.SelectedPath = watchFolderTextBox.Text;
        }

        if (outputFolderDialog.ShowDialog(this) == DialogResult.OK)
        {
            watchFolderTextBox.Text = outputFolderDialog.SelectedPath;
            SaveSettings();

            if (watchFolderCheckBox.Checked)
            {
                StopWatchFolder(silent: true);
                StartWatchFolder();
            }
            else
            {
                UpdateWatchToggleAppearance();
            }
        }
    }

    private void StartWatchFolder()
    {
        var folder = watchFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(folder))
        {
            UpdateWatchToggleAppearance();
            AppendWatchLog("Activation impossible : dossier non renseigne", Color.FromArgb(220, 53, 69));
            watchFolderCheckBox.Checked = false;
            return;
        }

        if (!Directory.Exists(folder))
        {
            UpdateWatchToggleAppearance();
            AppendWatchLog("Activation impossible : dossier introuvable", Color.FromArgb(220, 53, 69));
            MessageBox.Show(this, "Selectionnez un dossier a surveiller valide.", "Automatisation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            watchFolderCheckBox.Checked = false;
            return;
        }

        try
        {
            StopWatchFolder(silent: true);

            _watcherCts = new CancellationTokenSource();
            _watcher = new FileSystemWatcher(folder)
            {
                EnableRaisingEvents = true,
                Filter = "*.pdf",
                IncludeSubdirectories = false
            };

            _watcher.Created += WatcherOnChanged;
            _watcher.Changed += WatcherOnChanged;
            _watcher.Renamed += WatcherOnRenamed;

            _currentWatchFolder = folder;
            UpdateWatchToggleAppearance();
            watchFolderStatusLabel.Text = $"Surveillance active : {folder}";
            AppendWatchLog($"Surveillance active sur {folder}", Color.FromArgb(0, 120, 215));
        }
        catch (Exception ex)
        {
            StopWatchFolder(silent: true);
            AppendWatchLog($"Activation impossible : {ex.Message}", Color.FromArgb(220, 53, 69));
            MessageBox.Show(this, $"Impossible d'acceder au dossier a surveiller : {ex.Message}", "Automatisation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            watchFolderCheckBox.Checked = false;
        }
    }

    private void StopWatchFolder(bool silent = false)
    {
        _watcherCts?.Cancel();
        _watcherCts?.Dispose();
        _watcherCts = null;

        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= WatcherOnChanged;
            _watcher.Changed -= WatcherOnChanged;
            _watcher.Renamed -= WatcherOnRenamed;
            _watcher.Dispose();
            _watcher = null;
        }

        lock (_watchQueue)
        {
            _watchQueue.Clear();
        }

        _currentWatchFolder = null;
        UpdateWatchToggleAppearance();

        if (!silent)
        {
            AppendWatchLog("Surveillance arretee", SystemColors.ControlText);
        }

        watchFolderStatusLabel.Text = "Surveillance inactive";
    }

    private void StopWatchFolder()
    {
        StopWatchFolder(silent: false);
    }

    private void WatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        if (!string.Equals(Path.GetExtension(e.FullPath), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        QueueWatchFile(e.FullPath);
    }

    private void WatcherOnRenamed(object sender, RenamedEventArgs e) => WatcherOnChanged(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(e.FullPath) ?? string.Empty, e.Name ?? string.Empty));

    private void QueueWatchFile(string path)
    {
        bool added;
        lock (_watchQueue)
        {
            added = _watchQueue.Add(path);
        }

        if (!added)
        {
            return;
        }

        AppendWatchLog($"PDF détecté : {Path.GetFileName(path)}");

        _ = ProcessWatchFileAsync(path);
    }

    private async Task ProcessWatchFileAsync(string path)
    {
        if (_watcherCts is null)
        {
            return;
        }

        var token = _watcherCts.Token;

        bool watchLockTaken = false;

        try
        {
            await _watchSemaphore.WaitAsync(token).ConfigureAwait(false);
            watchLockTaken = true;
            await WaitForFileAvailableAsync(path, token).ConfigureAwait(false);

            var profile = GetSelectedProfile();
            var outputFolder = outputFolderTextBox.Text.Trim();
            if (profile is null || string.IsNullOrWhiteSpace(outputFolder))
            {
                return;
            }

            Directory.CreateDirectory(outputFolder);

            bool conversionLockTaken = false;
            try
            {
                await _conversionSemaphore.WaitAsync(token).ConfigureAwait(false);
                conversionLockTaken = true;
                var watchResult = await _conversionService.ConvertAsync(
                    new[] { path },
                    outputFolder,
                    profile,
                    null,
                    token,
                    GetOutputSuffix(),
                    separateTiffCheckBox.Checked).ConfigureAwait(false);
                var failure = watchResult.Files.FirstOrDefault(f => !f.Success);
                if (failure is not null)
                {
                    var error = string.IsNullOrWhiteSpace(failure.ErrorMessage) ? "erreur inconnue" : failure.ErrorMessage;
                    AppendWatchLog($"Conversion échouée : {Path.GetFileName(path)} ({error})", Color.FromArgb(220, 53, 69));
                    BeginInvoke(new Action(() => statusStripLabel.Text = $"Erreur conversion auto : {error}"));
                }
                else
                {
                    var outputNames = watchResult.Files
                        .SelectMany(f => f.OutputPaths)
                        .Select(Path.GetFileName)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList();
                    var outputsText = outputNames.Count > 0
                        ? string.Join(", ", outputNames)
                        : "aucun fichier généré";
                    AppendWatchLog(
                        $"Conversion terminée : {Path.GetFileName(path)} → {outputsText}",
                        Color.FromArgb(25, 135, 84));
                    BeginInvoke(new Action(() => statusStripLabel.Text = $"Conversion auto : {Path.GetFileName(path)}"));
                }
            }
            finally
            {
                if (conversionLockTaken)
                {
                    _conversionSemaphore.Release();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            AppendWatchLog($"Erreur conversion : {Path.GetFileName(path)} ({ex.Message})", Color.FromArgb(220, 53, 69));
            BeginInvoke(new Action(() => statusStripLabel.Text = $"Erreur conversion auto : {ex.Message}"));
        }
        finally
        {
            if (watchLockTaken)
            {
                _watchSemaphore.Release();
            }

            lock (_watchQueue)
            {
                _watchQueue.Remove(path);
            }
        }
    }


    private async Task WaitForFileAvailableAsync(string path, CancellationToken token)
    {
        for (int i = 0; i < 10; i++)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return;
            }
            catch (FileNotFoundException)
            {
                await Task.Delay(500, token).ConfigureAwait(false);
            }
            catch (IOException)
            {
                await Task.Delay(500, token).ConfigureAwait(false);
            }
        }

        StopWatchFolder(silent: true);
    }


    private sealed record WatchLogEntry(string Text, Color Color);
    private void SaveSettings()
    {
        _settings.LastProfileName = (profileComboBox.SelectedItem as ConversionProfile)?.Name;
        _settings.LastOutputFolder = outputFolderTextBox.Text.Trim();
        _settings.OpenExplorerAfterConversion = openExplorerCheckBox.Checked;
        _settings.OpenLogAfterConversion = openLogCheckBox.Checked;
        _settings.FileNameSuffix = GetOutputSuffix();
        _settings.SeparateTiffPages = separateTiffCheckBox.Checked;
        _settings.WatchFolderEnabled = watchFolderCheckBox.Checked;
        _settings.WatchFolderPath = watchFolderTextBox.Text.Trim();
        _settings.PreviewZoom = previewZoomTrackBar.Value / 100.0;
        _settings.Save(_settingsPath);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        _previewCts?.Cancel();
        _previewCts?.Dispose();

        _conversionCts?.Cancel();
        _conversionCts?.Dispose();

        StopWatchFolder();
        SaveSettings();
    }

    private sealed record PdfFileItem(string Path)
    {
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Directory => System.IO.Path.GetDirectoryName(Path) ?? string.Empty;
    }
}


