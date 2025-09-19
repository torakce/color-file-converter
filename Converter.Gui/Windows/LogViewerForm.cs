using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Converter.Core;

namespace Converter.Gui.Windows;

public partial class LogViewerForm : Form
{
    private readonly Logger _logger;
    private ListBox _logListBox;
    private ComboBox _levelFilterCombo;
    private Button _refreshButton;
    private Button _clearButton;
    private Button _exportButton;
    private CheckBox _autoScrollCheckBox;
    private System.Windows.Forms.Timer _refreshTimer;

    public LogViewerForm(Logger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeComponent();
        LoadLogs();
        
        // Rafraîchissement automatique toutes les 2 secondes
        _refreshTimer = new System.Windows.Forms.Timer();
        _refreshTimer.Interval = 2000;
        _refreshTimer.Tick += (s, e) => {
            if (_autoScrollCheckBox.Checked)
            {
                LoadLogs();
            }
        };
        _refreshTimer.Start();
    }

    private void InitializeComponent()
    {
        // Configuration de base du formulaire
        Text = "Visualiseur de logs - Color File Converter";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(600, 400);

        // Panel de contrôles
        var controlPanel = new Panel
        {
            Height = 40,
            Dock = DockStyle.Top,
            Padding = new Padding(10, 5, 10, 5)
        };

        // Filtre par niveau
        var levelLabel = new Label
        {
            Text = "Niveau:",
            Location = new Point(10, 12),
            Size = new Size(50, 20)
        };
        controlPanel.Controls.Add(levelLabel);

        _levelFilterCombo = new ComboBox
        {
            Location = new Point(70, 10),
            Size = new Size(100, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _levelFilterCombo.Items.AddRange(new[] { "Tous", "Debug", "Info", "Warning", "Error" });
        _levelFilterCombo.SelectedIndex = 0;
        _levelFilterCombo.SelectedIndexChanged += LevelFilterCombo_SelectedIndexChanged;
        controlPanel.Controls.Add(_levelFilterCombo);

        // Bouton rafraîchir
        _refreshButton = new Button
        {
            Text = "Rafraîchir",
            Location = new Point(180, 8),
            Size = new Size(80, 25)
        };
        _refreshButton.Click += (s, e) => LoadLogs();
        controlPanel.Controls.Add(_refreshButton);

        // Bouton vider
        _clearButton = new Button
        {
            Text = "Vider",
            Location = new Point(270, 8),
            Size = new Size(60, 25)
        };
        _clearButton.Click += ClearButton_Click;
        controlPanel.Controls.Add(_clearButton);

        // Bouton exporter
        _exportButton = new Button
        {
            Text = "Exporter",
            Location = new Point(340, 8),
            Size = new Size(70, 25)
        };
        _exportButton.Click += ExportButton_Click;
        controlPanel.Controls.Add(_exportButton);

        // Case à cocher scroll automatique
        _autoScrollCheckBox = new CheckBox
        {
            Text = "Défilement automatique",
            Location = new Point(420, 10),
            Size = new Size(150, 20),
            Checked = true
        };
        controlPanel.Controls.Add(_autoScrollCheckBox);

        Controls.Add(controlPanel);

        // ListBox pour les logs
        _logListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9),
            BackColor = Color.Black,
            ForeColor = Color.LightGray,
            IntegralHeight = false,
            HorizontalScrollbar = true,
            SelectionMode = SelectionMode.MultiExtended
        };
        
        // Menu contextuel pour la ListBox
        var contextMenu = new ContextMenuStrip();
        var copyItem = new ToolStripMenuItem("Copier la sélection");
        copyItem.Click += (s, e) => CopySelectedLogs();
        contextMenu.Items.Add(copyItem);
        
        var copyAllItem = new ToolStripMenuItem("Copier tout");
        copyAllItem.Click += (s, e) => CopyAllLogs();
        contextMenu.Items.Add(copyAllItem);
        
        _logListBox.ContextMenuStrip = contextMenu;
        Controls.Add(_logListBox);
    }

    private void LoadLogs()
    {
        try
        {
            var logs = _logger.GetRecentLogs(500);
            var selectedLevel = _levelFilterCombo.SelectedItem?.ToString();
            
            if (selectedLevel != null && selectedLevel != "Tous")
            {
                if (Enum.TryParse<LogLevel>(selectedLevel, out var filterLevel))
                {
                    logs = logs.Where(l => l.Level == filterLevel).ToList();
                }
            }

            var wasAtBottom = _logListBox.Items.Count == 0 || 
                             (_logListBox.TopIndex + _logListBox.ClientSize.Height / _logListBox.ItemHeight >= _logListBox.Items.Count - 1);

            _logListBox.BeginUpdate();
            _logListBox.Items.Clear();

            foreach (var log in logs)
            {
                var item = FormatLogEntry(log);
                _logListBox.Items.Add(item);
            }

            // Auto-scroll vers le bas si on était déjà en bas ou si l'option est activée
            if ((wasAtBottom || _autoScrollCheckBox.Checked) && _logListBox.Items.Count > 0)
            {
                _logListBox.TopIndex = Math.Max(0, _logListBox.Items.Count - 1);
            }

            _logListBox.EndUpdate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erreur lors du chargement des logs: {ex.Message}", 
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string FormatLogEntry(LogEntry log)
    {
        var levelPrefix = log.Level switch
        {
            LogLevel.Debug => "[DBG]",
            LogLevel.Info => "[INF]",
            LogLevel.Warning => "[WRN]",
            LogLevel.Error => "[ERR]",
            _ => "[---]"
        };

        var timestamp = log.Timestamp.ToString("HH:mm:ss.fff");
        var message = $"{timestamp} {levelPrefix} [{log.Category}] {log.Message}";
        
        if (!string.IsNullOrEmpty(log.Details))
        {
            message += $" - {log.Details}";
        }
        
        return message;
    }

    private void LevelFilterCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        LoadLogs();
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(this, 
            "Êtes-vous sûr de vouloir vider tous les logs?\n\nCette action est irréversible.", 
            "Confirmer la suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                _logger.ClearOldLogs(TimeSpan.Zero); // Supprimer tous les logs
                LoadLogs();
                MessageBox.Show(this, "Logs supprimés avec succès.", "Succès", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Erreur lors de la suppression: {ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ExportButton_Click(object? sender, EventArgs e)
    {
        using var saveDialog = new SaveFileDialog
        {
            Filter = "Fichiers texte (*.txt)|*.txt|Fichiers log (*.log)|*.log",
            DefaultExt = "txt",
            FileName = $"converter_logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };

        if (saveDialog.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                var logs = _logger.GetRecentLogs(1000);
                var content = string.Join(Environment.NewLine, logs.Select(l => l.ToString()));
                File.WriteAllText(saveDialog.FileName, content);
                
                MessageBox.Show(this, $"Logs exportés vers:\n{saveDialog.FileName}", 
                    "Export réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Erreur lors de l'export: {ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void CopySelectedLogs()
    {
        if (_logListBox.SelectedItems.Count > 0)
        {
            var selectedLogs = _logListBox.SelectedItems.Cast<string>().ToArray();
            var content = string.Join(Environment.NewLine, selectedLogs);
            Clipboard.SetText(content);
        }
    }

    private void CopyAllLogs()
    {
        if (_logListBox.Items.Count > 0)
        {
            var allLogs = _logListBox.Items.Cast<string>().ToArray();
            var content = string.Join(Environment.NewLine, allLogs);
            Clipboard.SetText(content);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        base.OnFormClosed(e);
    }
}