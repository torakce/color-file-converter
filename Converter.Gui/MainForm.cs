using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Converter.Core;

namespace Converter.Gui;

public partial class MainForm : Form
{
    private readonly IReadOnlyList<ColorModeChoice> _colorModes = new[]
    {
        new ColorModeChoice(
            "Noir et blanc (Fax CCITT G4)",
            "tiffg4",
            new[]
            {
                new CompressionOption("CCITT Groupe 4 (par défaut)", null, true)
            }),
        new ColorModeChoice(
            "Niveaux de gris (8 bits)",
            "tiffgray",
            new[]
            {
                new CompressionOption("LZW (sans perte)", "lzw", true),
                new CompressionOption("PackBits", "packbits"),
                new CompressionOption("JPEG", "jpeg"),
                new CompressionOption("Aucune", "none")
            }),
        new ColorModeChoice(
            "Couleur (24 bits)",
            "tiff24nc",
            new[]
            {
                new CompressionOption("LZW (sans perte)", "lzw", true),
                new CompressionOption("PackBits", "packbits"),
                new CompressionOption("JPEG", "jpeg"),
                new CompressionOption("Aucune", "none")
            })
    };

    public MainForm()
    {
        InitializeComponent();
        InitializeData();
    }

    private void InitializeData()
    {
        AcceptButton = convertButton;
        colorCombo.Items.AddRange(_colorModes.Cast<object>().ToArray());
        if (_colorModes.Count > 0)
        {
            colorCombo.SelectedIndex = 0;
        }
        statusLabel.Text = "Prêt";
    }

    private void BrowseInputButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(inputTextBox.Text))
        {
            var dir = Path.GetDirectoryName(inputTextBox.Text);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                openPdfDialog.InitialDirectory = dir;
            }
        }

        if (openPdfDialog.ShowDialog(this) == DialogResult.OK)
        {
            inputTextBox.Text = openPdfDialog.FileName;
            SuggestOutputPath();
        }
    }

    private void BrowseOutputButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(outputTextBox.Text))
        {
            saveTiffDialog.FileName = Path.GetFileName(outputTextBox.Text);
            var dir = Path.GetDirectoryName(outputTextBox.Text);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                saveTiffDialog.InitialDirectory = dir;
            }
        }
        else if (!string.IsNullOrWhiteSpace(inputTextBox.Text))
        {
            saveTiffDialog.FileName = Path.ChangeExtension(Path.GetFileName(inputTextBox.Text), "tif");
            var dir = Path.GetDirectoryName(inputTextBox.Text);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                saveTiffDialog.InitialDirectory = dir;
            }
        }

        if (saveTiffDialog.ShowDialog(this) == DialogResult.OK)
        {
            outputTextBox.Text = saveTiffDialog.FileName;
        }
    }

    private void ColorCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (colorCombo.SelectedItem is not ColorModeChoice mode)
        {
            return;
        }

        compressionCombo.BeginUpdate();
        compressionCombo.Items.Clear();
        compressionCombo.Items.AddRange(mode.Compressions.Cast<object>().ToArray());
        compressionCombo.EndUpdate();

        var defaultCompression = mode.Compressions.FirstOrDefault(c => c.IsDefault) ?? mode.Compressions.FirstOrDefault();
        if (defaultCompression is not null)
        {
            compressionCombo.SelectedItem = defaultCompression;
        }

        compressionCombo.Enabled = mode.Compressions.Length > 1 || defaultCompression?.GhostscriptValue is not null;
    }

    private async void ConvertButton_Click(object? sender, EventArgs e)
    {
        await ConvertAsync();
    }

    private async Task ConvertAsync()
    {
        var inputPath = inputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
        {
            MessageBox.Show(this, "Veuillez sélectionner un fichier PDF valide.", "Fichier manquant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var outputPath = outputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            outputPath = Path.ChangeExtension(inputPath, ".tif") ?? inputPath + ".tif";
            outputTextBox.Text = outputPath;
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Impossible de créer le dossier de sortie : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        if (colorCombo.SelectedItem is not ColorModeChoice selectedMode)
        {
            MessageBox.Show(this, "Veuillez sélectionner un mode couleur.", "Option manquante", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedCompression = compressionCombo.SelectedItem as CompressionOption;
        var compressionValue = selectedCompression?.GhostscriptValue;
        var dpi = (int)Math.Round(dpiUpDown.Value, MidpointRounding.AwayFromZero);

        try
        {
            SetUiEnabled(false);
            statusLabel.Text = "Conversion en cours...";

            await GhostscriptRunner.ConvertPdfToTiffAsync(
                inputPath,
                outputPath,
                selectedMode.Device,
                dpi,
                compressionValue,
                Array.Empty<string>());

            statusLabel.Text = "Conversion terminée.";
            MessageBox.Show(this, "La conversion est terminée.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Erreur lors de la conversion.";
            MessageBox.Show(this, ex.Message, "Erreur Ghostscript", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetUiEnabled(true);
        }
    }

    private void SetUiEnabled(bool enabled)
    {
        inputTextBox.Enabled = enabled;
        outputTextBox.Enabled = enabled;
        browseInputButton.Enabled = enabled;
        browseOutputButton.Enabled = enabled;
        colorCombo.Enabled = enabled;
        compressionCombo.Enabled = enabled && (compressionCombo.Items.Count > 1 || (compressionCombo.SelectedItem as CompressionOption)?.GhostscriptValue is not null);
        dpiUpDown.Enabled = enabled;
        convertButton.Enabled = enabled;
        UseWaitCursor = !enabled;
        Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
    }

    private void SuggestOutputPath()
    {
        if (string.IsNullOrWhiteSpace(inputTextBox.Text))
        {
            return;
        }

        var suggestion = Path.ChangeExtension(inputTextBox.Text, ".tif");
        if (!string.IsNullOrEmpty(suggestion))
        {
            outputTextBox.Text = suggestion;
        }
    }

    private sealed record ColorModeChoice(string Display, string Device, CompressionOption[] Compressions)
    {
        public override string ToString() => Display;
    }

    private sealed record CompressionOption(string Display, string? GhostscriptValue, bool IsDefault = false)
    {
        public override string ToString() => Display;
    }
}
