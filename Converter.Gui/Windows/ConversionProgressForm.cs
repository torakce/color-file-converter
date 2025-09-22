using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using Converter.Core;

namespace Converter.Gui.Windows;

internal sealed class ConversionProgressForm : Form
{
    private ProgressBar _progressBar = new();
    private Label _statusLabel = new();
    private Label _fileLabel = new();
    private RichTextBox _logTextBox = new();
    private Button _closeButton = new();

    public ConversionProgressForm(CancellationTokenSource cancellationTokenSource)
    {
        Text = "Conversion en cours";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        AutoScaleMode = AutoScaleMode.Font;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ControlBox = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 5,
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _statusLabel.AutoSize = true;
        _statusLabel.Text = "Initialisation...";
        layout.Controls.Add(_statusLabel, 0, 0);

        _fileLabel.AutoSize = true;
        _fileLabel.Margin = new Padding(0, 4, 0, 8);
        layout.Controls.Add(_fileLabel, 0, 1);

        _progressBar.Dock = DockStyle.Fill;
        _progressBar.Minimum = 0;
        _progressBar.Maximum = 1;
        _progressBar.Value = 0;
        layout.Controls.Add(_progressBar, 0, 2);

        _logTextBox.Dock = DockStyle.Fill;
        _logTextBox.ReadOnly = true;
        _logTextBox.BorderStyle = BorderStyle.FixedSingle;
        _logTextBox.BackColor = SystemColors.Window;
        _logTextBox.Height = 140;
        _logTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        layout.Controls.Add(_logTextBox, 0, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        _closeButton.AutoSize = true;
        _closeButton.Padding = new Padding(8, 4, 8, 4);
        _closeButton.Text = "Fermer";
        _closeButton.Enabled = false;
        _closeButton.Click += (_, _) => Close();
        
        var cancelButton = new Button
        {
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Text = "Annuler"
        };
        cancelButton.Click += (_, _) => cancellationTokenSource.Cancel();
        
        buttons.Controls.Add(_closeButton);
        buttons.Controls.Add(cancelButton);
        layout.Controls.Add(buttons, 0, 4);

        Controls.Add(layout);
    }

    public void Initialize(int totalFiles)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => Initialize(totalFiles)));
            return;
        }

        var maximum = Math.Max(1, totalFiles);
        _progressBar.Maximum = maximum;
        _progressBar.Value = 0;
        _statusLabel.Text = totalFiles == 0 ? "Aucun fichier" : $"Préparation ({totalFiles})";
        _fileLabel.Text = string.Empty;
        _logTextBox.Clear();
        _closeButton.Enabled = false;
        ControlBox = false;
    }

    public void UpdateProgressInternal(BatchConversionProgress progress)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateProgressInternal(progress)));
            return;
        }

        _progressBar.Maximum = Math.Max(1, progress.Total);
        _progressBar.Value = Math.Min(progress.Completed, progress.Total);

        _statusLabel.Text = $"Conversion en cours ({progress.Completed}/{progress.Total})";
        
        if (progress.Stage == BatchConversionStage.Starting)
        {
            _fileLabel.Text = Path.GetFileName(progress.InputPath);
            _logTextBox.AppendText($"Conversion de {progress.InputPath}\n");
        }
        else if (progress.Stage == BatchConversionStage.Failed)
        {
            _logTextBox.AppendText($"Erreur: {progress.ErrorMessage}\n");
        }
    }

    public void ShowResult(string title, string message, bool success)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowResult(title, message, success)));
            return;
        }

        Text = title;
        _statusLabel.Text = message;
        _fileLabel.Text = string.Empty;
        _closeButton.Enabled = true;
        ControlBox = true;
        
        _logTextBox.AppendText($"\n{message}\n");
        if (success)
        {
            _progressBar.Value = _progressBar.Maximum;
        }
    }

    public void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => AppendLog(message)));
            return;
        }

        if (_logTextBox.TextLength > 0)
        {
            _logTextBox.AppendText(Environment.NewLine);
        }

        _logTextBox.AppendText(message);
        _logTextBox.SelectionStart = _logTextBox.TextLength;
        _logTextBox.ScrollToCaret();
    }
}
