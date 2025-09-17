using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
    private Image? _beforePreview;
    private Image? _afterPreview;

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

        RefreshProfiles();

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
        watchFolderCheckBox.CheckedChanged -= WatchFolderCheckBox_CheckedChanged;
        watchFolderCheckBox.Checked = _settings.WatchFolderEnabled && Directory.Exists(_settings.WatchFolderPath ?? string.Empty);
        watchFolderCheckBox.CheckedChanged += WatchFolderCheckBox_CheckedChanged;

        if (watchFolderCheckBox.Checked)
        {
            StartWatchFolder();
        }

        UpdateActions();
        statusStripLabel.Text = "Prêt";
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
                }
            }
        }

        UpdateActions();
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
    }

    private void ClearFiles()
    {
        _files.Clear();
        _fileItems.Clear();
        filesListView.Items.Clear();
        UpdateActions();
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
            MessageBox.Show(this, "Sélectionnez un profil de conversion.", "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Directory.CreateDirectory(outputFolder);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Impossible de créer le dossier de sortie : {ex.Message}", "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                result = await _conversionService.ConvertAsync(filePaths, outputFolder, profile, progress, token).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                statusStripLabel.Text = "Conversion annulée.";
                MessageBox.Show(this, "La conversion a été annulée.", "Conversion", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    item.SubItems[2].Text = "Succès";
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
            ? "Conversion terminée."
            : failures.Count == result.Files.Count ? "Conversion échouée." : "Conversion terminée avec des erreurs.";

        var message = successes.Count > 0
            ? $"{successes.Count} fichier(s) converti(s)."
            : "Aucun fichier converti.";

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
                var first = successes.First();
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{first.OutputPath}\"")
                {
                    UseShellExecute = true
                });
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
            MessageBox.Show(this, $"Impossible d'écrire le journal : {ex.Message}", "Journal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        writer.WriteLine("Fichier;Durée;Entrée;Sortie;Statut;Message");

        foreach (var file in result.Files)
        {
            var status = file.Success ? "Succès" : "Erreur";
            var message = file.Success ? string.Empty : file.ErrorMessage ?? string.Empty;
            writer.WriteLine($"{Path.GetFileName(file.InputPath)};{file.Duration.TotalSeconds:F1}s;{FormatBytes(file.InputSize)};{FormatBytes(file.OutputSize)};{status};{message.Replace(';', ',')}");
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
        await LoadPreviewAsync();
    }

    private async void RefreshPreviewButton_Click(object? sender, EventArgs e)
    {
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

        SetPreviewStatus("Préparation de la prévisualisation...");

        string? beforePath = null;
        string? afterPath = null;

        try
        {


            await GhostscriptRunner.RenderPdfAsync(selected.Path, beforePath, "png16m", 150, firstPage: 1, lastPage: 1, cancellationToken: token).ConfigureAwait(true);
            await GhostscriptRunner.ConvertPdfToTiffAsync(selected.Path, afterPath, profile.Device, profile.Dpi, profile.Compression, profile.ExtraParameters, 1, 1, token).ConfigureAwait(true);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var beforeImage = LoadImageSafely(beforePath);
            var afterImage = LoadImageSafely(afterPath);

            Invoke(new Action(() =>
            {
                _beforePreview?.Dispose();
                _afterPreview?.Dispose();
                _beforePreview = beforeImage;
                _afterPreview = afterImage;
                beforePictureBox.Image = beforeImage;
                afterPictureBox.Image = afterImage;
                ApplyPreviewZoom(previewZoomTrackBar.Value / 100.0);
                SetPreviewStatus($"Prévisualisation : {Path.GetFileName(selected.Path)}");
            }));

        }
        catch (OperationCanceledException)
        {
            SetPreviewStatus("Prévisualisation annulée.");
        }
        catch (Exception ex)
        {
            SetPreviewStatus($"Prévisualisation impossible : {ex.Message}");
        }
        finally
        {
            try
            {
                if (beforePath is not null && File.Exists(beforePath)) File.Delete(beforePath);
            }
            catch
            {
            }

            try
            {
                if (afterPath is not null && File.Exists(afterPath)) File.Delete(afterPath);
            }
            catch
            {
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

    private void ClearPreview()
    {
        _beforePreview?.Dispose();
        _afterPreview?.Dispose();
        _beforePreview = null;
        _afterPreview = null;
        beforePictureBox.Image = null;
        afterPictureBox.Image = null;
        SetPreviewStatus("Sélectionnez un PDF pour afficher l'aperçu.");
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

    private void ApplyPreviewZoom(double zoom)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ApplyPreviewZoom(zoom)));
            return;
        }

        if (_beforePreview is not null)
        {
            beforePictureBox.Width = (int)(_beforePreview.Width * zoom);
            beforePictureBox.Height = (int)(_beforePreview.Height * zoom);
        }

        if (_afterPreview is not null)
        {
            afterPictureBox.Width = (int)(_afterPreview.Width * zoom);
            afterPictureBox.Height = (int)(_afterPreview.Height * zoom);
        }
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
        _ = LoadPreviewAsync(force: true);
    }

    private void UpdateProfileDetails()
    {
        if (profileComboBox.SelectedItem is ConversionProfile profile)
        {
            profileDetailsLabel.Text = profile.Describe();
        }
        else
        {
            profileDetailsLabel.Text = string.Empty;
        }
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

    private void BrowseOutputFolderButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(outputFolderTextBox.Text) && Directory.Exists(outputFolderTextBox.Text))
        {
            outputFolderDialog.SelectedPath = outputFolderTextBox.Text;
        }

        if (outputFolderDialog.ShowDialog(this) == DialogResult.OK)
        {
            outputFolderTextBox.Text = outputFolderDialog.SelectedPath;
            SaveSettings();
        }
    }

    private void OpenExplorerCheckBox_CheckedChanged(object? sender, EventArgs e) => SaveSettings();
    private void OpenLogCheckBox_CheckedChanged(object? sender, EventArgs e) => SaveSettings();

    private void WatchFolderCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (watchFolderCheckBox.Checked)
        {
            StartWatchFolder();
        }
        else
        {
            StopWatchFolder();
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
                StopWatchFolder();
                StartWatchFolder();
            }
        }
    }

    private void StartWatchFolder()
    {
        var folder = watchFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(folder))
        {
            MessageBox.Show(this, "Sélectionnez un dossier à surveiller.", "Automatisation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            watchFolderCheckBox.Checked = false;
            return;
        }

        try
        {
            Directory.CreateDirectory(folder);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Impossible d'accéder au dossier à surveiller : {ex.Message}", "Automatisation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            watchFolderCheckBox.Checked = false;
            return;
        }

        StopWatchFolder();
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

        watchFolderStatusLabel.Text = $"Surveillance active : {folder}";
    }

    private void StopWatchFolder()
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

        watchFolderStatusLabel.Text = "Surveillance inactive";
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
        lock (_watchQueue)
        {
            if (!_watchQueue.Add(path))
            {
                return;
            }
        }

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
                await _conversionService.ConvertAsync(new[] { path }, outputFolder, profile, null, token).ConfigureAwait(false);
                BeginInvoke(new Action(() => statusStripLabel.Text = $"Conversion auto : {Path.GetFileName(path)}"));
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


    {
        for (int i = 0; i < 10; i++)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return;
            }
            catch (IOException)
            {
                await Task.Delay(500, token).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                await Task.Delay(500, token).ConfigureAwait(false);
            }
        }

        throw new IOException($"Impossible d'accéder au fichier {path}.");
    }

    private void SaveSettings()
    {
        _settings.LastProfileName = (profileComboBox.SelectedItem as ConversionProfile)?.Name;
        _settings.LastOutputFolder = outputFolderTextBox.Text.Trim();
        _settings.OpenExplorerAfterConversion = openExplorerCheckBox.Checked;
        _settings.OpenLogAfterConversion = openLogCheckBox.Checked;
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
