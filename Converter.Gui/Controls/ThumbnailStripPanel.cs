using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Converter.Gui.Controls;

/// <summary>
/// Panel affichant un strip horizontal de vignettes de pages
/// </summary>
public class ThumbnailStripPanel : Panel
{
    private readonly Panel _containerPanel;
    private readonly FlowLayoutPanel _flowPanel;
    private readonly HScrollBar _scrollBar;
    private readonly List<ThumbnailItem> _thumbnails = new();
    private int _selectedIndex = -1;

    public event EventHandler<int>? ThumbnailClicked;

    public ThumbnailStripPanel()
    {
        Height = 170;
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(240, 240, 240);
        Padding = new Padding(5);
        
        // Pas d'AutoScroll - on gère manuellement
        AutoScroll = false;

        // Scrollbar horizontale manuelle
        _scrollBar = new HScrollBar
        {
            Dock = DockStyle.Bottom,
            Height = 20,
            Visible = true,
            Enabled = true
        };
        _scrollBar.Scroll += ScrollBar_Scroll;
        
        // Panel conteneur pour clipper le FlowPanel
        _containerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = false,
            Padding = new Padding(0),
            BackColor = Color.FromArgb(240, 240, 240)
        };
        
        // FlowPanel à l'intérieur du conteneur
        _flowPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0),
            Location = new Point(0, 0),
            BackColor = Color.FromArgb(240, 240, 240)
        };
        
        _containerPanel.Controls.Add(_flowPanel);
        
        // Ajouter la scrollbar puis le conteneur
        Controls.Add(_scrollBar);
        Controls.Add(_containerPanel);
        
        _flowPanel.SizeChanged += (s, e) => UpdateScrollBar();
        SizeChanged += (s, e) => UpdateScrollBar();
        
        // Gestion de la molette de souris pour le scroll horizontal
        MouseWheel += ThumbnailStripPanel_MouseWheel;
    }

    private void ThumbnailStripPanel_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (_scrollBar.Visible)
        {
            int newValue = _scrollBar.Value - (e.Delta / 3);
            newValue = Math.Max(_scrollBar.Minimum, Math.Min(_scrollBar.Maximum - _scrollBar.LargeChange + 1, newValue));
            _scrollBar.Value = newValue;
            _flowPanel.Left = -newValue;
        }
    }

    /// <summary>
    /// Charge les vignettes à partir d'une liste d'images
    /// </summary>
    public void LoadThumbnails(List<Image> pages)
    {
        Clear();

        for (int i = 0; i < pages.Count; i++)
        {
            var pageIndex = i; // Capture locale pour éviter problème de closure
            var thumbnail = new ThumbnailItem(pages[i], pageIndex);
            thumbnail.Click += (s, e) => OnThumbnailClick(pageIndex);
            
            _thumbnails.Add(thumbnail);
            _flowPanel.Controls.Add(thumbnail);
        }

        if (_thumbnails.Count > 0)
        {
            SelectThumbnail(0);
        }
        
        // Forcer la mise à jour de la scrollbar après ajout des miniatures
        // Utiliser un timer pour s'assurer que le layout est complètement terminé
        var timer = new System.Windows.Forms.Timer { Interval = 100 };
        timer.Tick += (s, e) =>
        {
            _flowPanel.PerformLayout();
            UpdateScrollBar();
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }

    /// <summary>
    /// Sélectionne une vignette (sans déclencher l'événement)
    /// </summary>
    public void SelectThumbnail(int index)
    {
        if (index < 0 || index >= _thumbnails.Count)
            return;

        // Ne rien faire si déjà sélectionné (évite les boucles)
        if (_selectedIndex == index)
            return;

        // Désélectionner l'ancienne
        if (_selectedIndex >= 0 && _selectedIndex < _thumbnails.Count)
        {
            _thumbnails[_selectedIndex].IsSelected = false;
        }

        // Sélectionner la nouvelle
        _selectedIndex = index;
        _thumbnails[index].IsSelected = true;

        // Scroll vers la vignette
        ScrollToThumbnail(index);
    }

    /// <summary>
    /// Scroll vers une vignette spécifique
    /// </summary>
    private void ScrollToThumbnail(int index)
    {
        if (index < 0 || index >= _thumbnails.Count)
            return;

        var thumbnail = _thumbnails[index];
        var x = thumbnail.Left - (Width / 2) + (thumbnail.Width / 2);
        
        if (x < 0) x = 0;
        
        var maxScroll = Math.Max(0, _flowPanel.Width - Width);
        if (x > maxScroll) x = maxScroll;
        
        _flowPanel.Left = -x;
        if (_scrollBar.Visible)
        {
            _scrollBar.Value = Math.Min(x, _scrollBar.Maximum - _scrollBar.LargeChange + 1);
        }
    }

    /// <summary>
    /// Met à jour la scrollbar en fonction de la taille du contenu
    /// </summary>
    private void UpdateScrollBar()
    {
        // Calculer la largeur réelle du contenu
        int contentWidth = 0;
        if (_flowPanel.Controls.Count > 0)
        {
            var lastControl = _flowPanel.Controls[_flowPanel.Controls.Count - 1];
            contentWidth = lastControl.Right + _flowPanel.Padding.Right;
        }
        
        var panelWidth = Width - Padding.Left - Padding.Right;
        
        if (contentWidth > panelWidth)
        {
            _scrollBar.Visible = true;
            _scrollBar.Minimum = 0;
            _scrollBar.Maximum = contentWidth;
            _scrollBar.LargeChange = panelWidth;
            _scrollBar.SmallChange = 50;
        }
        else
        {
            _scrollBar.Visible = false;
            _flowPanel.Left = 0;
        }
    }

    /// <summary>
    /// Gère le défilement de la scrollbar
    /// </summary>
    private void ScrollBar_Scroll(object? sender, ScrollEventArgs e)
    {
        _flowPanel.Left = -e.NewValue;
    }

    /// <summary>
    /// Vide toutes les vignettes
    /// </summary>
    public void Clear()
    {
        foreach (var thumbnail in _thumbnails)
        {
            thumbnail.Dispose();
        }
        
        _thumbnails.Clear();
        _flowPanel.Controls.Clear();
        _selectedIndex = -1;
    }

    private void OnThumbnailClick(int index)
    {
        SelectThumbnail(index);
        ThumbnailClicked?.Invoke(this, index);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Clear();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Item de vignette individuelle
/// </summary>
internal class ThumbnailItem : Panel
{
    private readonly PictureBox _pictureBox;
    private readonly Label _label;
    private bool _isSelected;

    public int PageIndex { get; }
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            UpdateVisualState();
        }
    }

    public ThumbnailItem(Image image, int pageIndex)
    {
        PageIndex = pageIndex;
        Width = 100;  // Réduit de 110 à 100
        Height = 135; // Réduit de 150 à 135
        Margin = new Padding(3);
        Cursor = Cursors.Hand;
        BackColor = Color.White;

        // PictureBox pour l'image
        _pictureBox = new PictureBox
        {
            Image = CreateThumbnail(image),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(5, 5),
            Size = new Size(90, 105), // Réduit de 100×120 à 90×105
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        // Propager les clics du PictureBox vers le Panel parent
        _pictureBox.Click += (s, e) => this.OnClick(e);
        Controls.Add(_pictureBox);

        // Label pour le numéro de page
        _label = new Label
        {
            Text = $"Page {pageIndex + 1}",
            Font = new Font("Segoe UI", 7.5f), // Réduit de 8 à 7.5
            Location = new Point(5, 112), // Ajusté de 127 à 112
            Size = new Size(90, 18), // Réduit de 100×18 à 90×18
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        // Propager les clics du Label vers le Panel parent
        _label.Click += (s, e) => this.OnClick(e);
        Controls.Add(_label);

        // L'événement Click du Panel sera automatiquement levé
        // Pas besoin de gestionnaires redondants qui pourraient causer des problèmes
        
        Paint += OnPaintBorder;
        UpdateVisualState();
    }

    private Image CreateThumbnail(Image original)
    {
        var thumbnail = new Bitmap(90, 100); // Augmenté de 80×80 à 90×100
        using (var g = Graphics.FromImage(thumbnail))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Calculer les dimensions proportionnelles
            float ratio = Math.Min(90f / original.Width, 100f / original.Height);
            int newWidth = (int)(original.Width * ratio);
            int newHeight = (int)(original.Height * ratio);
            int x = (90 - newWidth) / 2;
            int y = (100 - newHeight) / 2;
            
            g.Clear(Color.White);
            g.DrawImage(original, x, y, newWidth, newHeight);
        }
        return thumbnail;
    }

    private void UpdateVisualState()
    {
        if (_isSelected)
        {
            BackColor = Color.FromArgb(220, 240, 255);
            _label.Font = new Font("Segoe UI", 7, FontStyle.Bold);
        }
        else
        {
            BackColor = Color.White;
            _label.Font = new Font("Segoe UI", 7);
        }
        Invalidate();
    }

    private void OnPaintBorder(object? sender, PaintEventArgs e)
    {
        var borderColor = _isSelected ? Color.FromArgb(33, 150, 243) : Color.FromArgb(200, 200, 200);
        var borderWidth = _isSelected ? 3 : 1;

        using var pen = new Pen(borderColor, borderWidth);
        var rect = new Rectangle(
            borderWidth / 2,
            borderWidth / 2,
            Width - borderWidth - 1,
            Height - borderWidth - 1
        );
        e.Graphics.DrawRectangle(pen, rect);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pictureBox.Image?.Dispose();
            _pictureBox.Dispose();
            _label.Dispose();
        }
        base.Dispose(disposing);
    }
}
