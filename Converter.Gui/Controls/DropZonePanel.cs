using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Converter.Gui;

/// <summary>
/// Panel stylisé pour le drag & drop de fichiers avec feedback visuel
/// </summary>
public class DropZonePanel : Panel
{
    private bool _isDraggingOver;
    private readonly Color _normalColor = Color.FromArgb(250, 250, 255);
    private readonly Color _hoverColor = Color.FromArgb(230, 245, 255);
    private readonly Color _borderColor = Color.FromArgb(100, 150, 200);
    private readonly Color _borderHoverColor = Color.FromArgb(33, 150, 243);

    public DropZonePanel()
    {
        AllowDrop = true;
        BackColor = _normalColor;
        DoubleBuffered = true;
        
        // Événements drag & drop
        DragEnter += OnDragEnterInternal;
        DragLeave += OnDragLeaveInternal;
        DragOver += OnDragOverInternal;
        
        // Repaint sur changement d'état
        Paint += OnPaintBorder;
    }

    private void OnDragEnterInternal(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            _isDraggingOver = true;
            BackColor = _hoverColor;
            Invalidate();
        }
    }

    private void OnDragLeaveInternal(object? sender, EventArgs e)
    {
        _isDraggingOver = false;
        BackColor = _normalColor;
        Invalidate();
    }

    private void OnDragOverInternal(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void OnPaintBorder(object? sender, PaintEventArgs e)
    {
        // Dessiner une bordure en pointillés
        int borderWidth = 2;
        using var pen = new Pen(_isDraggingOver ? _borderHoverColor : _borderColor, borderWidth)
        {
            DashStyle = DashStyle.Dash,
            DashPattern = new float[] { 8, 4 }
        };

        // Ajuster le rectangle pour tenir compte de la largeur du trait
        var rect = new Rectangle(
            borderWidth / 2, 
            borderWidth / 2, 
            Width - borderWidth - 1, 
            Height - borderWidth - 1
        );
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawRectangle(pen, rect);

        // Dessiner un rectangle interne si survol
        if (_isDraggingOver)
        {
            using var innerPen = new Pen(_borderHoverColor, 1)
            {
                DashStyle = DashStyle.Solid
            };
            var innerRect = new Rectangle(6, 6, Width - 13, Height - 13);
            e.Graphics.DrawRectangle(innerPen, innerRect);
        }
    }

    /// <summary>
    /// Réinitialise l'état visuel
    /// </summary>
    public void ResetVisualState()
    {
        _isDraggingOver = false;
        BackColor = _normalColor;
        Invalidate();
    }
}
