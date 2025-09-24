using System;
using System.Windows.Forms;
using Converter.Core;
using Converter.Gui.Windows;

namespace Converter.Gui;

internal sealed class WatchOptionsButton : Button
{
    private WatchFolderForm? _watchFolderForm;
    private readonly BatchConversionService _conversionService = new();

    public WatchOptionsButton()
    {
        Text = "(Avancé) Surveillance de dossier";
        AutoSize = true;
        Padding = new Padding(10, 4, 10, 4);
        Click += WatchOptionsButton_Click;
    }

    private void WatchOptionsButton_Click(object? sender, EventArgs e)
    {
        if (_watchFolderForm == null || _watchFolderForm.IsDisposed)
        {
            _watchFolderForm = new WatchFolderForm(_conversionService);
            _watchFolderForm.FormClosed += (s, e) => _watchFolderForm = null;
        }

        _watchFolderForm.Show();
        if (_watchFolderForm.WindowState == FormWindowState.Minimized)
        {
            _watchFolderForm.WindowState = FormWindowState.Normal;
        }
        _watchFolderForm.BringToFront();
    }
}