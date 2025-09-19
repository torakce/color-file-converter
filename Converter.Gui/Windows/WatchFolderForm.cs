using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Converter.Core;
using Converter.Gui.Services;

namespace Converter.Gui.Windows;

internal sealed class WatchFolderForm : Form
{
    private readonly CheckBox _watchFolderCheckBox;
    private readonly TextBox _watchFolderTextBox;
    private readonly Button _browseWatchFolderButton;
    private readonly Label _watchFolderStatusLabel;
    private readonly RichTextBox _watchLogTextBox;
    private readonly FolderBrowserDialog _folderBrowserDialog;
    private readonly TableLayoutPanel _mainLayout;
    private readonly UserSettings _settings;
    private readonly string _settingsPath;
    private readonly BatchConversionService _conversionService;

    private FileSystemWatcher? _watcher;
    private CancellationTokenSource? _watcherCts;
    private readonly HashSet<string> _watchQueue = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _watchSemaphore = new(1, 1);
    private readonly List<WatchLogEntry> _watchLogs = new();
    private const int MaxWatchLogEntries = 200;
    private string? _currentWatchFolder;

    public WatchFolderForm(string settingsPath, UserSettings settings, BatchConversionService conversionService)
    {
        _settingsPath = settingsPath;
        _settings = settings;
        _conversionService = conversionService;

        Text = "Surveillance de dossier";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        AutoScaleMode = AutoScaleMode.Font;
        MinimizeBox = true;
        MaximizeBox = false;
        ShowInTaskbar = false;
        Padding = new Padding(12);
        MinimumSize = new Size(600, 400);
        Size = new Size(800, 500);

        _folderBrowserDialog = new FolderBrowserDialog();

        _mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 3,
            Margin = new Padding(0)
        };

        // First column (button): 30% of space
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        // Second column (textbox): 50% of space
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        // Third column (browse button): 20% of space
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

        _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _watchFolderCheckBox = new CheckBox
        {
            Appearance = Appearance.Button,
            AutoCheck = false,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0),
            MinimumSize = new Size(180, 0),
            Padding = new Padding(10, 4, 10, 4),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Activer la surveillance",
            UseVisualStyleBackColor = false
        };
        _watchFolderCheckBox.Click += WatchFolderCheckBox_Click;

        _watchFolderTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Dossier à surveiller",
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(0, 0, 10, 0)
        };
        _watchFolderTextBox.Text = _settings.WatchFolderPath ?? string.Empty;

        _browseWatchFolderButton = new Button
        {
            AutoSize = true,
            Text = "Parcourir...",
            MinimumSize = new Size(80, 0),
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 4, 10, 4)
        };
        _browseWatchFolderButton.Click += BrowseWatchFolderButton_Click;

        _watchFolderStatusLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = SystemColors.GrayText,
            Text = "Surveillance inactive"
        };

        _watchLogTextBox = new RichTextBox
        {
            BackColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            MinimumSize = new Size(0, 48),
            ReadOnly = true,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };

        // First row: controls with margins
        var firstRow = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Margin = new Padding(0, 0, 0, 10)
        };
        firstRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        firstRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        firstRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        firstRow.Controls.Add(_watchFolderCheckBox, 0, 0);
        firstRow.Controls.Add(_watchFolderTextBox, 1, 0);
        firstRow.Controls.Add(_browseWatchFolderButton, 2, 0);

        _mainLayout.Controls.Add(firstRow, 0, 0);
        _mainLayout.SetColumnSpan(firstRow, 3);

        // Status label with margin
        _watchFolderStatusLabel.Margin = new Padding(0, 0, 0, 10);
        _mainLayout.Controls.Add(_watchFolderStatusLabel, 0, 1);
        _mainLayout.SetColumnSpan(_watchFolderStatusLabel, 3);

        // Log textbox
        _mainLayout.Controls.Add(_watchLogTextBox, 0, 2);
        _mainLayout.SetColumnSpan(_watchLogTextBox, 3);

        Controls.Add(_mainLayout);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_settings.WatchFolderEnabled)
        {
            StartWatchFolder();
        }
        else
        {
            _watchFolderCheckBox.Checked = false;
            UpdateWatchToggleAppearance();
            AppendWatchLog("Surveillance inactive");
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
    }

    private void UpdateWatchToggleAppearance()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(UpdateWatchToggleAppearance));
            return;
        }

        bool active = _watcher is not null;
        _watchFolderCheckBox.Checked = active;
        _watchFolderCheckBox.Text = active ? "Surveillance en cours" : "Activer la surveillance";
        _watchFolderCheckBox.BackColor = active ? Color.FromArgb(0, 120, 215) : SystemColors.Control;
        _watchFolderCheckBox.ForeColor = active ? Color.White : SystemColors.ControlText;

        if (active && !string.IsNullOrEmpty(_currentWatchFolder))
        {
            _watchFolderStatusLabel.Text = $"Surveillance en cours : {_currentWatchFolder}";
            _watchFolderStatusLabel.ForeColor = Color.FromArgb(0, 120, 215);
        }
        else
        {
            _watchFolderStatusLabel.Text = "Surveillance inactive";
            _watchFolderStatusLabel.ForeColor = SystemColors.GrayText;
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

        _watchLogTextBox.SuspendLayout();
        _watchLogTextBox.Clear();
        foreach (var log in _watchLogs)
        {
            _watchLogTextBox.SelectionStart = _watchLogTextBox.TextLength;
            _watchLogTextBox.SelectionColor = log.Color;
            _watchLogTextBox.AppendText(log.Text + Environment.NewLine);
        }

        _watchLogTextBox.SelectionColor = _watchLogTextBox.ForeColor;
        _watchLogTextBox.SelectionStart = _watchLogTextBox.TextLength;
        _watchLogTextBox.ScrollToCaret();
        _watchLogTextBox.ResumeLayout();
    }

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
        if (!string.IsNullOrWhiteSpace(_watchFolderTextBox.Text) && Directory.Exists(_watchFolderTextBox.Text))
        {
            _folderBrowserDialog.SelectedPath = _watchFolderTextBox.Text;
        }

        if (_folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
        {
            _watchFolderTextBox.Text = _folderBrowserDialog.SelectedPath;
            SaveSettings();

            if (_watchFolderCheckBox.Checked)
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
        var folder = _watchFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(folder))
        {
            UpdateWatchToggleAppearance();
            AppendWatchLog("Activation impossible : dossier non renseigne", Color.FromArgb(220, 53, 69));
            _watchFolderCheckBox.Checked = false;
            return;
        }

        if (!Directory.Exists(folder))
        {
            UpdateWatchToggleAppearance();
            AppendWatchLog("Activation impossible : dossier introuvable", Color.FromArgb(220, 53, 69));
            MessageBox.Show(this, "Selectionnez un dossier a surveiller valide.", "Automatisation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _watchFolderCheckBox.Checked = false;
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
            _watchFolderStatusLabel.Text = $"Surveillance active : {folder}";
            AppendWatchLog($"Surveillance active sur {folder}", Color.FromArgb(0, 120, 215));
        }
        catch (Exception ex)
        {
            StopWatchFolder(silent: true);
            AppendWatchLog($"Activation impossible : {ex.Message}", Color.FromArgb(220, 53, 69));
            MessageBox.Show(this, $"Impossible d'acceder au dossier a surveiller : {ex.Message}", "Automatisation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _watchFolderCheckBox.Checked = false;
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

        _watchFolderStatusLabel.Text = "Surveillance inactive";
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

            bool conversionLockTaken = false;
            try
            {
                await ConvertFileAsync(path, token);
                AppendWatchLog($"Conversion réussie : {Path.GetFileName(path)}", Color.FromArgb(40, 167, 69));
            }
            finally
            {
                if (conversionLockTaken)
                {
                    // Nothing to release as we're handling conversion internally
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

    private void SaveSettings()
    {
        _settings.WatchFolderEnabled = _watchFolderCheckBox.Checked;
        _settings.WatchFolderPath = _watchFolderTextBox.Text.Trim();
        _settings.Save(_settingsPath);
    }

    private sealed record WatchLogEntry(string Text, Color Color);

    private async Task ConvertFileAsync(string path, CancellationToken token)
    {
        // On utilise le dossier du fichier source comme dossier de sortie
        var outputFolder = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(outputFolder))
        {
            throw new InvalidOperationException("Le dossier du fichier source est invalide.");
        }

        // On récupère les paramètres depuis les settings
        var profile = new ConversionProfile(
            "Conversion Automatique",
            _settings.LastDevice ?? "tiff24nc",
            _settings.LastCompression ?? "lzw",
            _settings.LastDpi ?? 300,
            _settings.LastExtraParameters?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()
        );

        var progress = new Progress<BatchConversionProgress>(p =>
        {
            var status = p.Stage switch
            {
                BatchConversionStage.Starting => "Démarrage...",
                BatchConversionStage.Completed when p.ErrorMessage == null => "Terminé",
                BatchConversionStage.Completed => $"Erreur : {p.ErrorMessage}",
                BatchConversionStage.Failed => $"Échec : {p.ErrorMessage}",
                _ => "En cours..."
            };

            BeginInvoke(() => AppendWatchLog($"{Path.GetFileName(p.InputPath)}: {status}"));
        });

        await _conversionService.ConvertAsync(
            new[] { path },
            outputFolder,
            profile,
            progress,
            token,
            _settings.FileNameSuffix,
            _settings.SeparateTiffPages,
            useInputFolder: true
        );
    }
}
