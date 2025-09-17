using System;
using System.Linq;
using System.Windows.Forms;
using Converter.Core;

namespace Converter.Gui.Profiles;

internal sealed class ProfileEditorForm : Form
{
    private readonly TextBox _nameBox = new();
    private readonly TextBox _deviceBox = new();
    private readonly TextBox _compressionBox = new();
    private readonly NumericUpDown _dpiUpDown = new();
    private readonly TextBox _extraParametersBox = new();
    private readonly Button _okButton = new();
    private readonly Button _cancelButton = new();

    public ConversionProfile Result { get; private set; }

    public ProfileEditorForm(ConversionProfile profile)
    {
        Result = profile;

        Text = "Profil de conversion";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _dpiUpDown.Minimum = 72;
        _dpiUpDown.Maximum = 2400;
        _dpiUpDown.Increment = 25;

        _extraParametersBox.Multiline = true;
        _extraParametersBox.ScrollBars = ScrollBars.Vertical;
        _extraParametersBox.Height = 80;

        _okButton.Text = "Enregistrer";
        _okButton.DialogResult = DialogResult.OK;
        _okButton.Click += OkButtonOnClick;

        _cancelButton.Text = "Annuler";
        _cancelButton.DialogResult = DialogResult.Cancel;

        var table = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        table.Controls.Add(new Label { Text = "Nom", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _nameBox.Width = 240;
        table.Controls.Add(_nameBox, 1, 0);

        table.Controls.Add(new Label { Text = "Device Ghostscript", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        table.Controls.Add(_deviceBox, 1, 1);

        table.Controls.Add(new Label { Text = "Compression", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
        table.Controls.Add(_compressionBox, 1, 2);

        table.Controls.Add(new Label { Text = "Résolution (dpi)", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
        table.Controls.Add(_dpiUpDown, 1, 3);

        table.Controls.Add(new Label { Text = "Paramètres supplémentaires\n(un par ligne)", AutoSize = true }, 0, 4);
        table.Controls.Add(_extraParametersBox, 1, 4);

        var buttonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
        };
        buttonsPanel.Controls.AddRange(new Control[] { _okButton, _cancelButton });
        table.Controls.Add(buttonsPanel, 0, 5);
        table.SetColumnSpan(buttonsPanel, 2);

        Controls.Add(table);

        LoadProfile(profile);
    }

    private void LoadProfile(ConversionProfile profile)
    {
        _nameBox.Text = profile.Name;
        _deviceBox.Text = profile.Device;
        _compressionBox.Text = profile.Compression ?? string.Empty;
        _dpiUpDown.Value = Math.Clamp(profile.Dpi, (int)_dpiUpDown.Minimum, (int)_dpiUpDown.Maximum);
        _extraParametersBox.Text = string.Join(Environment.NewLine, profile.ExtraParameters ?? Array.Empty<string>());
    }

    private void OkButtonOnClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            MessageBox.Show(this, "Veuillez saisir un nom.", "Profil", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        if (string.IsNullOrWhiteSpace(_deviceBox.Text))
        {
            MessageBox.Show(this, "Veuillez saisir un device Ghostscript.", "Profil", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        var extras = _extraParametersBox.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToArray();

        Result = new ConversionProfile(
            _nameBox.Text.Trim(),
            _deviceBox.Text.Trim(),
            string.IsNullOrWhiteSpace(_compressionBox.Text) ? null : _compressionBox.Text.Trim(),
            (int)_dpiUpDown.Value,
            extras);
    }
}
