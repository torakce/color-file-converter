using System;
using System.Drawing;
using System.Windows.Forms;

namespace Converter.Gui.Controls;

/// <summary>
/// Panel affichant la progression détaillée d'une conversion
/// </summary>
public class ConversionProgressPanel : Panel
{
    private readonly Label _fileLabel;
    private readonly Label _statusLabel;
    private readonly ProgressBar _progressBar;
    private readonly Label _speedLabel;
    private readonly Label _timeLabel;
    private readonly Label _percentLabel;
    
    private DateTime _startTime;
    private long _totalBytes;
    private long _processedBytes;

    public ConversionProgressPanel()
    {
        Height = 120;
        Dock = DockStyle.Fill;
        Padding = new Padding(10);
        BackColor = Color.FromArgb(245, 248, 250);
        
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(0)
        };
        
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Fichier
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10)); // Espacement
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Barre
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 5)); // Espacement
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Infos
        
        Controls.Add(layout);

        // Nom du fichier en cours
        _fileLabel = new Label
        {
            Text = "En attente...",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50),
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(_fileLabel, 0, 0);

        // Barre de progression
        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Style = ProgressBarStyle.Continuous,
            Minimum = 0,
            Maximum = 100,
            Value = 0
        };
        layout.Controls.Add(_progressBar, 0, 2);

        // Panel des infos détaillées
        var infoPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0)
        };
        layout.Controls.Add(infoPanel, 0, 4);

        // Pourcentage
        _percentLabel = new Label
        {
            Text = "0%",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 150, 243),
            AutoSize = true,
            Margin = new Padding(0, 0, 15, 0)
        };
        infoPanel.Controls.Add(_percentLabel);

        // Statut
        _statusLabel = new Label
        {
            Text = "En attente",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(100, 100, 100),
            AutoSize = true,
            Margin = new Padding(0, 0, 15, 0)
        };
        infoPanel.Controls.Add(_statusLabel);

        // Vitesse
        _speedLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(100, 100, 100),
            AutoSize = true,
            Margin = new Padding(0, 0, 15, 0)
        };
        infoPanel.Controls.Add(_speedLabel);

        // Temps restant
        _timeLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(100, 100, 100),
            AutoSize = true
        };
        infoPanel.Controls.Add(_timeLabel);
    }

    /// <summary>
    /// Démarre une nouvelle conversion
    /// </summary>
    public void StartConversion(string fileName, long totalBytes)
    {
        _fileLabel.Text = $"📄 {fileName}";
        _statusLabel.Text = "Conversion en cours...";
        _progressBar.Value = 0;
        _percentLabel.Text = "0%";
        _speedLabel.Text = "";
        _timeLabel.Text = "";
        
        _startTime = DateTime.Now;
        _totalBytes = totalBytes;
        _processedBytes = 0;
        
        UpdateProgressBarColor(0);
    }

    /// <summary>
    /// Met à jour la progression
    /// </summary>
    public void UpdateProgress(int fileIndex, int totalFiles, int percentComplete)
    {
        if (percentComplete < 0) percentComplete = 0;
        if (percentComplete > 100) percentComplete = 100;
        
        _progressBar.Value = percentComplete;
        _percentLabel.Text = $"{percentComplete}%";
        _statusLabel.Text = $"Fichier {fileIndex}/{totalFiles}";
        
        // Calculer la vitesse et le temps restant
        var elapsed = DateTime.Now - _startTime;
        if (elapsed.TotalSeconds > 0 && percentComplete > 0)
        {
            var bytesPerSecond = (_processedBytes / elapsed.TotalSeconds);
            _speedLabel.Text = $"⚡ {FormatSpeed(bytesPerSecond)}";
            
            var remainingPercent = 100 - percentComplete;
            var estimatedTotalSeconds = (elapsed.TotalSeconds / percentComplete) * 100;
            var remainingSeconds = estimatedTotalSeconds - elapsed.TotalSeconds;
            
            if (remainingSeconds > 0)
            {
                _timeLabel.Text = $"⏱️ {FormatTime(TimeSpan.FromSeconds(remainingSeconds))} restant";
            }
        }
        
        UpdateProgressBarColor(percentComplete);
    }

    /// <summary>
    /// Met à jour avec des infos sur les octets traités
    /// </summary>
    public void UpdateProgressWithBytes(long processedBytes, int percentComplete)
    {
        _processedBytes = processedBytes;
        UpdateProgress(1, 1, percentComplete);
    }

    /// <summary>
    /// Marque la conversion comme terminée avec succès
    /// </summary>
    public void Complete()
    {
        _progressBar.Value = 100;
        _percentLabel.Text = "100%";
        _percentLabel.ForeColor = Color.FromArgb(76, 175, 80); // Vert
        _statusLabel.Text = "✅ Terminé !";
        _statusLabel.ForeColor = Color.FromArgb(76, 175, 80);
        _speedLabel.Text = "";
        _timeLabel.Text = "";
        
        UpdateProgressBarColor(100);
    }

    /// <summary>
    /// Marque la conversion comme échouée
    /// </summary>
    public void Fail(string errorMessage)
    {
        _percentLabel.Text = "❌";
        _percentLabel.ForeColor = Color.FromArgb(244, 67, 54); // Rouge
        _statusLabel.Text = $"Erreur : {errorMessage}";
        _statusLabel.ForeColor = Color.FromArgb(244, 67, 54);
        _speedLabel.Text = "";
        _timeLabel.Text = "";
        
        BackColor = Color.FromArgb(255, 235, 238); // Rouge clair
    }

    /// <summary>
    /// Réinitialise le panel
    /// </summary>
    public void Reset()
    {
        _fileLabel.Text = "En attente...";
        _statusLabel.Text = "En attente";
        _statusLabel.ForeColor = Color.FromArgb(100, 100, 100);
        _percentLabel.Text = "0%";
        _percentLabel.ForeColor = Color.FromArgb(33, 150, 243);
        _speedLabel.Text = "";
        _timeLabel.Text = "";
        _progressBar.Value = 0;
        BackColor = Color.FromArgb(245, 248, 250);
    }

    /// <summary>
    /// Change la couleur de la barre selon le pourcentage
    /// </summary>
    private void UpdateProgressBarColor(int percent)
    {
        // Vert : 0-70%, Orange : 71-90%, Rouge : 91-100%
        // Note: En WinForms, la couleur de la ProgressBar est difficile à changer
        // On pourrait utiliser un custom paint, mais pour simplifier on garde le style par défaut
        
        if (percent >= 90)
        {
            _percentLabel.ForeColor = Color.FromArgb(76, 175, 80); // Vert foncé
        }
        else if (percent >= 70)
        {
            _percentLabel.ForeColor = Color.FromArgb(255, 152, 0); // Orange
        }
        else
        {
            _percentLabel.ForeColor = Color.FromArgb(33, 150, 243); // Bleu
        }
    }

    /// <summary>
    /// Formate une vitesse en octets/seconde
    /// </summary>
    private static string FormatSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024)
            return $"{bytesPerSecond:F0} B/s";
        if (bytesPerSecond < 1024 * 1024)
            return $"{bytesPerSecond / 1024:F1} KB/s";
        return $"{bytesPerSecond / (1024 * 1024):F1} MB/s";
    }

    /// <summary>
    /// Formate un temps en format lisible
    /// </summary>
    private static string FormatTime(TimeSpan time)
    {
        if (time.TotalSeconds < 60)
            return $"{time.TotalSeconds:F0}s";
        if (time.TotalMinutes < 60)
            return $"{time.TotalMinutes:F0}m {time.Seconds}s";
        return $"{time.TotalHours:F0}h {time.Minutes}m";
    }
}
