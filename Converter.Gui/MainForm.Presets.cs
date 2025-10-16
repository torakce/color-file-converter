using System;
using System.Drawing;
using System.Windows.Forms;
using Converter.Core;
using Converter.Gui.Controls;

namespace Converter.Gui;

/// <summary>
/// Partie de MainForm dédiée aux presets visuels
/// </summary>
public partial class MainForm
{
    private PresetCard? _selectedPresetCard;
    private Panel? _customParametersPanel;
    
    /// <summary>
    /// Crée la nouvelle section de presets visuels avec cartes cliquables
    /// </summary>
    private void CreatePresetsSection(TableLayoutPanel parent, int row)
    {
        var presetsGroup = new GroupBox
        {
            Text = "🎯 Presets et paramètres de conversion",
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        parent.Controls.Add(presetsGroup, 0, row);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(10),
            Padding = new Padding(5),
            AutoScroll = true  // Activer le scroll si nécessaire
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F)); // Cartes de presets (hauteur fixe)
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Paramètres (toujours visibles)
        presetsGroup.Controls.Add(mainLayout);

        // FlowLayoutPanel pour les cartes de presets
        var presetsFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(5)
        };
        mainLayout.Controls.Add(presetsFlow, 0, 0);

        // Créer les 3 cartes de presets (sans "Personnalisé")
        var bwCard = PresetCard.CreateBlackAndWhitePreset();
        var grayCard = PresetCard.CreateGrayscalePreset();
        var colorCard = PresetCard.CreateColorPreset();

        // Événements de sélection
        bwCard.CardClicked += (s, e) => SelectPresetCard(bwCard);
        grayCard.CardClicked += (s, e) => SelectPresetCard(grayCard);
        colorCard.CardClicked += (s, e) => SelectPresetCard(colorCard);

        presetsFlow.Controls.AddRange(new Control[] { bwCard, grayCard, colorCard });

        // Panel pour les paramètres (TOUJOURS VISIBLE maintenant)
        _customParametersPanel = CreateCustomParametersPanel();
        _customParametersPanel.Visible = true; // Toujours visible
        mainLayout.Controls.Add(_customParametersPanel, 0, 1);

        // Sélectionner "Couleurs" par défaut
        SelectPresetCard(colorCard);
    }

    /// <summary>
    /// Crée le panel des paramètres personnalisés
    /// </summary>
    private Panel CreateCustomParametersPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Visible = false,
            Padding = new Padding(10, 5, 10, 5),
            MinimumSize = new Size(0, 80)  // Hauteur minimale pour garantir la visibilité
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 2,
            AutoSize = true,
            Padding = new Padding(5)
        };
        
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Résolution
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F)); // Résolution
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Compression
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F)); // Compression
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Couleurs
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F)); // Couleurs
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        panel.Controls.Add(layout);

        // Ligne d'information
        var infoLabel = new Label
        {
            Text = "⚙️ Ajuster les paramètres",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.FromArgb(70, 70, 70),
            Margin = new Padding(3, 5, 3, 10)
        };
        layout.Controls.Add(infoLabel, 0, 0);
        layout.SetColumnSpan(infoLabel, 6);

        // Résolution (DPI)
        var resLabel = new Label { Text = "DPI:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(3) };
        layout.Controls.Add(resLabel, 0, 1);
        
        _resolutionCombo = new ComboBox
        {
            Name = "resolutionCombo",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDown,
            Margin = new Padding(3)
        };
        _resolutionCombo.Items.AddRange(new[] { "72", "96", "150", "200", "300", "400", "600" });
        _resolutionCombo.Text = "300";
        _resolutionCombo.TextChanged += ParameterChanged;
        _resolutionCombo.SelectedIndexChanged += ParameterChanged;
        layout.Controls.Add(_resolutionCombo, 1, 1);
        
        var toolTip1 = new ToolTip();
        toolTip1.SetToolTip(_resolutionCombo, "Résolution en DPI (points par pouce). Plus élevé = meilleure qualité mais fichiers plus gros.");

        // Compression TIFF
        var compressionLabel = new Label { Text = "Compression:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(3) };
        layout.Controls.Add(compressionLabel, 2, 1);
        
        _compressionCombo = new ComboBox
        {
            Name = "compressionCombo",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        
        _compressionCombo.Items.AddRange(new[] { 
            "Aucune",
            "LZW",
            "ZIP", 
            "PackBits",
            "G3 (N&B)",
            "G4 (N&B)",
            "JPEG"
        });
        _compressionCombo.SelectedIndex = 1; // LZW par défaut
        _compressionCombo.SelectedIndexChanged += ParameterChanged;
        layout.Controls.Add(_compressionCombo, 3, 1);
        
        var toolTip2 = new ToolTip();
        toolTip2.SetToolTip(_compressionCombo, "Type de compression TIFF. LZW recommandé pour l'équilibre qualité/taille.");

        // Type de couleurs
        var bitDepthLabel = new Label { Text = "Couleurs:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(3) };
        layout.Controls.Add(bitDepthLabel, 4, 1);
        
        _bitDepthCombo = new ComboBox
        {
            Name = "bitDepthCombo",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        _bitDepthCombo.Items.AddRange(new[] { "N&B", "Gris", "Couleurs" });
        _bitDepthCombo.SelectedIndex = 2; // Couleurs par défaut
        _bitDepthCombo.SelectedIndexChanged += ParameterChanged;
        layout.Controls.Add(_bitDepthCombo, 5, 1);
        
        var toolTip3 = new ToolTip();
        toolTip3.SetToolTip(_bitDepthCombo, "Type de couleurs. N&B permet G3/G4, Couleurs pour documents complexes.");

        // Validation des paramètres
        _validationWarningLabel = new Label
        {
            Text = "",
            ForeColor = Color.Orange,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(5, 5, 5, 0),
            Visible = false
        };
        
        return panel;
    }

    /// <summary>
    /// Sélectionne une carte de preset
    /// </summary>
    private void SelectPresetCard(PresetCard card)
    {
        // Désélectionner la carte précédente
        if (_selectedPresetCard != null)
        {
            _selectedPresetCard.IsSelected = false;
        }

        // Sélectionner la nouvelle carte
        _selectedPresetCard = card;
        card.IsSelected = true;

        // Les paramètres sont toujours visibles maintenant,
        // on applique simplement le profil s'il existe
        if (card.Profile != null)
        {
            ApplyPresetProfile(card.Profile);
        }
    }

    /// <summary>
    /// Applique un profil de preset aux contrôles
    /// </summary>
    private void ApplyPresetProfile(ConversionProfile profile)
    {
        // Mettre à jour la résolution
        if (_resolutionCombo != null)
        {
            _resolutionCombo.Text = profile.Dpi.ToString();
        }

        // Mettre à jour la compression
        if (_compressionCombo != null)
        {
            string compressionDisplay = (profile.Compression ?? "LZW").ToUpperInvariant() switch
            {
                "NONE" => "Aucune",
                "LZW" => "LZW",
                "ZIP" => "ZIP",
                "PACKBITS" => "PackBits",
                "G3" => "G3 (N&B)",
                "G4" => "G4 (N&B)",
                "JPEG" => "JPEG",
                _ => "LZW"
            };
            
            int index = _compressionCombo.Items.IndexOf(compressionDisplay);
            if (index >= 0)
            {
                _compressionCombo.SelectedIndex = index;
            }
        }

        // Mettre à jour le type de couleurs basé sur le device
        if (_bitDepthCombo != null)
        {
            string bitDepthDisplay = profile.Device.ToLowerInvariant() switch
            {
                "tiffg4" => "N&B",
                "tiffgray" => "Gris",
                "tiff24nc" => "Couleurs",
                _ => "Couleurs"
            };
            
            int index = _bitDepthCombo.Items.IndexOf(bitDepthDisplay);
            if (index >= 0)
            {
                _bitDepthCombo.SelectedIndex = index;
            }
        }

        // Régénérer l'aperçu avec le nouveau profil
        CreateCustomProfile();
    }
}
