using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Converter.Core;

namespace Converter.Gui.Profiles;

internal sealed class ProfileManagerForm : Form
{
    private readonly BindingList<ConversionProfile> _profiles;
    private readonly ListBox _profilesList = new();
    private readonly Button _addButton = new();
    private readonly Button _editButton = new();
    private readonly Button _duplicateButton = new();
    private readonly Button _deleteButton = new();
    private readonly Button _resetButton = new();
    private readonly Button _closeButton = new();

    public IReadOnlyList<ConversionProfile> Profiles => _profiles.ToList();

    public ProfileManagerForm(IEnumerable<ConversionProfile> profiles)
    {
        _profiles = new BindingList<ConversionProfile>(profiles.Select(p => p).ToList());

        Text = "Profils de conversion";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _profilesList.DisplayMember = nameof(ConversionProfile.Name);
        _profilesList.DataSource = _profiles;
        _profilesList.Width = 320;
        _profilesList.Height = 220;
        _profilesList.SelectedIndexChanged += (_, _) => UpdateButtonsState();

        _addButton.Text = "Nouveau";
        _addButton.Click += (_, _) => AddProfile();

        _editButton.Text = "Modifier";
        _editButton.Click += (_, _) => EditSelected();

        _duplicateButton.Text = "Dupliquer";
        _duplicateButton.Click += (_, _) => DuplicateSelected();

        _deleteButton.Text = "Supprimer";
        _deleteButton.Click += (_, _) => DeleteSelected();

        _resetButton.Text = "Restaurer défauts";
        _resetButton.Click += (_, _) => ResetDefaults();

        _closeButton.Text = "Fermer";
        _closeButton.DialogResult = DialogResult.OK;

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(12, 0, 0, 0)
        };

        buttons.Controls.AddRange(new Control[]
        {
            _addButton,
            _editButton,
            _duplicateButton,
            _deleteButton,
            _resetButton,
            _closeButton
        });

        var layout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill
        };
        layout.Controls.AddRange(new Control[] { _profilesList, buttons });

        Controls.Add(layout);
        UpdateButtonsState();
    }

    private ConversionProfile? SelectedProfile => _profilesList.SelectedItem as ConversionProfile;

    private void UpdateButtonsState()
    {
        bool hasSelection = SelectedProfile is not null;
        _editButton.Enabled = hasSelection;
        _duplicateButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    private void AddProfile()
    {
        var editor = new ProfileEditorForm(new ConversionProfile(
            "Nouveau profil",
            "tiffg4",
            null,
            300,
            Array.Empty<string>()));
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            _profiles.Add(editor.Result);
            _profilesList.SelectedItem = editor.Result;
        }
    }

    private void EditSelected()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var editor = new ProfileEditorForm(SelectedProfile);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            var index = _profiles.IndexOf(SelectedProfile);
            _profiles[index] = editor.Result;
            _profiles.ResetBindings();
            _profilesList.SelectedIndex = index;
        }
    }

    private void DuplicateSelected()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var duplicated = SelectedProfile with { Name = SelectedProfile.Name + " (copie)" };
        _profiles.Add(duplicated);
        _profilesList.SelectedItem = duplicated;
    }

    private void DeleteSelected()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        if (MessageBox.Show(this, "Supprimer ce profil ?", "Profils", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        _profiles.Remove(SelectedProfile);
    }

    private void ResetDefaults()
    {
        if (MessageBox.Show(this, "Restaurer les profils par défaut ?", "Profils", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        _profiles.Clear();
        foreach (var profile in ConversionProfile.GetDefaultProfiles())
        {
            _profiles.Add(profile);
        }
    }
}
