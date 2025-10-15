using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Converter.Core;

namespace Converter.Gui.Controls;

/// <summary>
/// Carte visuelle pour un preset de conversion (Fax, Archive, Photo, etc.)
/// </summary>
public class PresetCard : Panel
{
    private readonly Panel _iconPanel;
    private readonly Label _titleLabel;
    private readonly Label _descriptionLabel;
    private readonly IconType _iconType;
    private bool _isSelected;
    private bool _isHovered;

    private static readonly Color NormalColor = Color.White;
    private static readonly Color HoverColor = Color.FromArgb(245, 250, 255);
    private static readonly Color SelectedColor = Color.FromArgb(230, 245, 255);
    private static readonly Color BorderNormal = Color.FromArgb(200, 200, 200);
    private static readonly Color BorderHover = Color.FromArgb(100, 150, 200);
    private static readonly Color BorderSelected = Color.FromArgb(33, 150, 243);

    public enum IconType
    {
        BlackAndWhite,
        Grayscale,
        Color
    }

    public ConversionProfile? Profile { get; }
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            UpdateVisualState();
        }
    }

    public event EventHandler? CardClicked;

    public PresetCard(IconType iconType, string title, string description, ConversionProfile? profile)
    {
        Profile = profile;
        _iconType = iconType;
        
        Width = 180;
        Height = 190;
        Margin = new Padding(8);
        Cursor = Cursors.Hand;
        BackColor = NormalColor;
        DoubleBuffered = true;

        // Panel pour l'icône dessinée
        _iconPanel = new Panel
        {
            Location = new Point(55, 10), // Centré
            Width = 70,
            Height = 70,
            BackColor = Color.Transparent
        };
        _iconPanel.Paint += IconPanel_Paint;

        // Titre (zone milieu)
        _titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Location = new Point(5, 98),
            Width = 170,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
            AutoSize = false,
            UseMnemonic = false
        };

        // Description (zone basse)
        _descriptionLabel = new Label
        {
            Text = description,
            Font = new Font("Segoe UI", 7.5f),
            Location = new Point(5, 125),
            Width = 170,
            Height = 60,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = Color.Gray,
            BackColor = Color.Transparent,
            AutoSize = false,
            UseMnemonic = false
        };

        // Ajouter tous les contrôles
        Controls.AddRange(new Control[] { _iconPanel, _titleLabel, _descriptionLabel });

        // Événements
        MouseEnter += (s, e) => { _isHovered = true; UpdateVisualState(); };
        MouseLeave += (s, e) => { _isHovered = false; UpdateVisualState(); };
        Click += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);
        
        // Propager les événements des enfants au parent
        foreach (Control child in Controls)
        {
            child.MouseEnter += (s, e) => { _isHovered = true; UpdateVisualState(); };
            child.MouseLeave += (s, e) => { _isHovered = false; UpdateVisualState(); };
            child.Click += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);
        }

        Paint += OnPaintBorder;
    }

    private void UpdateVisualState()
    {
        if (_isSelected)
        {
            BackColor = SelectedColor;
        }
        else if (_isHovered)
        {
            BackColor = HoverColor;
        }
        else
        {
            BackColor = NormalColor;
        }

        // Mettre à jour la police du titre
        _titleLabel.Font = new Font("Segoe UI", 10, _isSelected ? FontStyle.Bold : FontStyle.Bold);
        
        Invalidate();
    }

    private void IconPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        
        switch (_iconType)
        {
            case IconType.BlackAndWhite:
                // Carré noir pur
                using (var brush = new SolidBrush(Color.Black))
                {
                    g.FillRectangle(brush, 10, 10, 50, 50);
                }
                break;

            case IconType.Grayscale:
                // Bandes de gris (noir → blanc)
                for (int i = 0; i < 5; i++)
                {
                    int grayValue = 30 + i * 50; // 30, 80, 130, 180, 230
                    using (var brush = new SolidBrush(Color.FromArgb(grayValue, grayValue, grayValue)))
                    {
                        g.FillRectangle(brush, 10 + i * 10, 10, 10, 50);
                    }
                }
                break;

            case IconType.Color:
                // 3 carrés colorés côte à côte (Rouge, Vert, Bleu)
                using (var redBrush = new SolidBrush(Color.FromArgb(220, 50, 50)))
                using (var greenBrush = new SolidBrush(Color.FromArgb(50, 200, 50)))
                using (var blueBrush = new SolidBrush(Color.FromArgb(50, 120, 220)))
                {
                    g.FillRectangle(redBrush, 10, 15, 15, 40);
                    g.FillRectangle(greenBrush, 27, 15, 15, 40);
                    g.FillRectangle(blueBrush, 44, 15, 15, 40);
                }
                break;
        }
    }

    private void OnPaintBorder(object? sender, PaintEventArgs e)
    {
        Color borderColor;
        int borderWidth;

        if (_isSelected)
        {
            borderColor = BorderSelected;
            borderWidth = 3;
        }
        else if (_isHovered)
        {
            borderColor = BorderHover;
            borderWidth = 2;
        }
        else
        {
            borderColor = BorderNormal;
            borderWidth = 1;
        }

        using var pen = new Pen(borderColor, borderWidth);
        var rect = new Rectangle(borderWidth / 2, borderWidth / 2, 
            Width - borderWidth, Height - borderWidth);
        e.Graphics.DrawRectangle(pen, rect);
    }

    /// <summary>
    /// Crée un preset N&B 150 DPI G4
    /// </summary>
    public static PresetCard CreateBlackAndWhitePreset()
    {
        var profile = new ConversionProfile(
            "N&B 150 DPI G4",
            "tiffg4",
            "g4",
            150,
            Array.Empty<string>()
        );

        return new PresetCard(
            IconType.BlackAndWhite,
            "Noir & Blanc",
            "Compression G4 • 150 DPI",
            profile
        );
    }

    /// <summary>
    /// Crée un preset Niveaux de gris LZW 150 DPI
    /// </summary>
    public static PresetCard CreateGrayscalePreset()
    {
        var profile = new ConversionProfile(
            "Niveaux de gris LZW 150 DPI",
            "tiffgray",
            "lzw",
            150,
            Array.Empty<string>()
        );

        return new PresetCard(
            IconType.Grayscale,
            "Nuances de gris",
            "Compression LZW • 150 DPI",
            profile
        );
    }

    /// <summary>
    /// Crée un preset Couleurs LZW 150 DPI
    /// </summary>
    public static PresetCard CreateColorPreset()
    {
        var profile = new ConversionProfile(
            "Couleurs LZW 150 DPI",
            "tiff24nc",
            "lzw",
            150,
            Array.Empty<string>()
        );

        return new PresetCard(
            IconType.Color,
            "Couleurs",
            "Compression LZW • 150 DPI",
            profile
        );
    }

}
