#nullable disable
namespace Converter.Gui;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        openPdfDialog = new System.Windows.Forms.OpenFileDialog();
        outputFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
        mainLayout = new System.Windows.Forms.TableLayoutPanel();
        leftLayout = new System.Windows.Forms.TableLayoutPanel();
        filesGroup = new System.Windows.Forms.GroupBox();
        filesGroupLayout = new System.Windows.Forms.TableLayoutPanel();
        filesButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
        addFilesButton = new System.Windows.Forms.Button();
        removeFilesButton = new System.Windows.Forms.Button();
        clearFilesButton = new System.Windows.Forms.Button();
        dropFilesLabel = new System.Windows.Forms.Label();
        selectedFileDetailsLabel = new System.Windows.Forms.Label();
        filesListView = new System.Windows.Forms.ListView();
        fileColumnHeader = new System.Windows.Forms.ColumnHeader();
        directoryColumnHeader = new System.Windows.Forms.ColumnHeader();
        statusColumnHeader = new System.Windows.Forms.ColumnHeader();
        profileGroup = new System.Windows.Forms.GroupBox();
        profileLayout = new System.Windows.Forms.TableLayoutPanel();
        profileHeaderPanel = new System.Windows.Forms.FlowLayoutPanel();
        profileComboBox = new System.Windows.Forms.ComboBox();
        manageProfilesButton = new System.Windows.Forms.Button();
        refreshPreviewButton = new System.Windows.Forms.Button();
        profileDetailsLabel = new System.Windows.Forms.Label();
        outputGroup = new System.Windows.Forms.GroupBox();
        outputLayout = new System.Windows.Forms.TableLayoutPanel();
        outputFolderTextBox = new System.Windows.Forms.TextBox();
        browseOutputFolderButton = new System.Windows.Forms.Button();
        openOutputFolderButton = new System.Windows.Forms.Button();
        openExplorerCheckBox = new System.Windows.Forms.CheckBox();
        openLogCheckBox = new System.Windows.Forms.CheckBox();
        suffixLabel = new System.Windows.Forms.Label();
        suffixTextBox = new System.Windows.Forms.TextBox();
        separateTiffCheckBox = new System.Windows.Forms.CheckBox();
        outputDetailsLabel = new System.Windows.Forms.Label();
        automationGroup = new System.Windows.Forms.GroupBox();
        automationLayout = new System.Windows.Forms.TableLayoutPanel();
        watchFolderCheckBox = new System.Windows.Forms.CheckBox();
        watchFolderTextBox = new System.Windows.Forms.TextBox();
        browseWatchFolderButton = new System.Windows.Forms.Button();
        watchFolderStatusLabel = new System.Windows.Forms.Label();
        watchLogTextBox = new System.Windows.Forms.RichTextBox();
        previewGroup = new System.Windows.Forms.GroupBox();
        previewLayout = new System.Windows.Forms.TableLayoutPanel();
        previewStatusLabel = new System.Windows.Forms.Label();
        previewImagesLayout = new System.Windows.Forms.TableLayoutPanel();
        afterPreviewContainer = new System.Windows.Forms.Panel();
        afterPreviewPanel = new System.Windows.Forms.Panel();
        afterPictureBox = new System.Windows.Forms.PictureBox();
        afterPreviewLabel = new System.Windows.Forms.Label();
        previewControlsPanel = new System.Windows.Forms.FlowLayoutPanel();
        previewZoomTrackBar = new System.Windows.Forms.TrackBar();
        previewZoomValueLabel = new System.Windows.Forms.Label();
        conversionProgressBar = new System.Windows.Forms.ProgressBar();
        actionsPanel = new System.Windows.Forms.FlowLayoutPanel();
        convertButton = new System.Windows.Forms.Button();
        stopButton = new System.Windows.Forms.Button();
        statusStrip = new System.Windows.Forms.StatusStrip();
        statusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
        mainLayout.SuspendLayout();
        leftLayout.SuspendLayout();
        filesGroup.SuspendLayout();
        filesGroupLayout.SuspendLayout();
        filesButtonsPanel.SuspendLayout();
        profileGroup.SuspendLayout();
        profileLayout.SuspendLayout();
        profileHeaderPanel.SuspendLayout();
        outputGroup.SuspendLayout();
        outputLayout.SuspendLayout();
        automationGroup.SuspendLayout();
        automationLayout.SuspendLayout();
        previewGroup.SuspendLayout();
        previewLayout.SuspendLayout();
        previewImagesLayout.SuspendLayout();
        afterPreviewContainer.SuspendLayout();
        afterPreviewPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)afterPictureBox).BeginInit();
        previewControlsPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)previewZoomTrackBar).BeginInit();
        actionsPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // mainLayout
        // 
        mainLayout.ColumnCount = 2;
        mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
        mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
        mainLayout.Controls.Add(leftLayout, 0, 0);
        mainLayout.Controls.Add(previewGroup, 1, 0);
        mainLayout.Controls.Add(conversionProgressBar, 0, 1);
        mainLayout.Controls.Add(actionsPanel, 1, 1);
        mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        mainLayout.Location = new System.Drawing.Point(9, 9);
        mainLayout.Name = "mainLayout";
        mainLayout.RowCount = 2;
        mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        mainLayout.Size = new System.Drawing.Size(1266, 742);
        mainLayout.TabIndex = 0;
        // 
        // leftLayout
        // 
        leftLayout.ColumnCount = 1;
        leftLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        leftLayout.Controls.Add(filesGroup, 0, 0);
        leftLayout.Controls.Add(profileGroup, 0, 1);
        leftLayout.Controls.Add(outputGroup, 0, 2);
        leftLayout.Controls.Add(automationGroup, 0, 3);
        leftLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        leftLayout.Location = new System.Drawing.Point(3, 3);
        leftLayout.Name = "leftLayout";
        leftLayout.RowCount = 4;
        leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
        leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
        leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
        leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
        leftLayout.Size = new System.Drawing.Size(753, 685);
        leftLayout.TabIndex = 0;
        // 
        // filesGroup
        // 
        filesGroup.Controls.Add(filesGroupLayout);
        filesGroup.Dock = System.Windows.Forms.DockStyle.Fill;
        filesGroup.Location = new System.Drawing.Point(3, 3);
        filesGroup.Name = "filesGroup";
        filesGroup.Padding = new System.Windows.Forms.Padding(12);
        filesGroup.Size = new System.Drawing.Size(747, 336);
        filesGroup.TabIndex = 0;
        filesGroup.TabStop = false;
        filesGroup.Text = "Fichiers PDF";
        // 
        // filesGroupLayout
        // 
        filesGroupLayout.ColumnCount = 1;
        filesGroupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        filesGroupLayout.Controls.Add(filesButtonsPanel, 0, 0);
        filesGroupLayout.Controls.Add(dropFilesLabel, 0, 1);
        filesGroupLayout.Controls.Add(selectedFileDetailsLabel, 0, 2);
        filesGroupLayout.Controls.Add(filesListView, 0, 3);
        filesGroupLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        filesGroupLayout.Location = new System.Drawing.Point(12, 28);
        filesGroupLayout.Name = "filesGroupLayout";
        filesGroupLayout.RowCount = 4;
        filesGroupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        filesGroupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        filesGroupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        filesGroupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        filesGroupLayout.Size = new System.Drawing.Size(723, 296);
        filesGroupLayout.TabIndex = 0;
        // 
        // filesButtonsPanel
        // 
        filesButtonsPanel.AutoSize = true;
        filesButtonsPanel.Controls.Add(addFilesButton);
        filesButtonsPanel.Controls.Add(removeFilesButton);
        filesButtonsPanel.Controls.Add(clearFilesButton);
        filesButtonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        filesButtonsPanel.Location = new System.Drawing.Point(3, 3);
        filesButtonsPanel.Name = "filesButtonsPanel";
        filesButtonsPanel.Size = new System.Drawing.Size(717, 35);
        filesButtonsPanel.TabIndex = 0;
        // 
        // addFilesButton
        // 
        addFilesButton.AutoSize = true;
        addFilesButton.Location = new System.Drawing.Point(3, 3);
        addFilesButton.Name = "addFilesButton";
        addFilesButton.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        addFilesButton.Size = new System.Drawing.Size(101, 33);
        addFilesButton.TabIndex = 0;
        addFilesButton.Text = "Ajouter des PDF";
        addFilesButton.UseVisualStyleBackColor = true;
        addFilesButton.Click += AddFilesButton_Click;
        // 
        // removeFilesButton
        // 
        removeFilesButton.AutoSize = true;
        removeFilesButton.Location = new System.Drawing.Point(110, 3);
        removeFilesButton.Name = "removeFilesButton";
        removeFilesButton.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        removeFilesButton.Size = new System.Drawing.Size(118, 33);
        removeFilesButton.TabIndex = 1;
        removeFilesButton.Text = "Retirer selection";
        removeFilesButton.UseVisualStyleBackColor = true;
        removeFilesButton.Click += RemoveFilesButton_Click;
        // 
        // clearFilesButton
        // 
        clearFilesButton.AutoSize = true;
        clearFilesButton.Location = new System.Drawing.Point(234, 3);
        clearFilesButton.Name = "clearFilesButton";
        clearFilesButton.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        clearFilesButton.Size = new System.Drawing.Size(132, 33);
        clearFilesButton.TabIndex = 2;
        clearFilesButton.Text = "Tout supprimer";
        clearFilesButton.UseVisualStyleBackColor = true;
        clearFilesButton.Click += ClearFilesButton_Click;
        // 
        // dropFilesLabel
        //
        dropFilesLabel.AutoSize = true;
        dropFilesLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        dropFilesLabel.ForeColor = System.Drawing.SystemColors.GrayText;
        dropFilesLabel.Location = new System.Drawing.Point(3, 41);
        dropFilesLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
        dropFilesLabel.Name = "dropFilesLabel";
        dropFilesLabel.Size = new System.Drawing.Size(717, 30);
        dropFilesLabel.TabIndex = 1;
        dropFilesLabel.Text = "Glissez-deposez vos PDF ici ou utilisez le bouton Ajouter.";
        //
        // selectedFileDetailsLabel
        //
        selectedFileDetailsLabel.AutoSize = true;
        selectedFileDetailsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        selectedFileDetailsLabel.ForeColor = System.Drawing.SystemColors.GrayText;
        selectedFileDetailsLabel.Location = new System.Drawing.Point(3, 79);
        selectedFileDetailsLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
        selectedFileDetailsLabel.Name = "selectedFileDetailsLabel";
        selectedFileDetailsLabel.Size = new System.Drawing.Size(717, 30);
        selectedFileDetailsLabel.TabIndex = 3;
        selectedFileDetailsLabel.Text = "Selectionnez un PDF pour afficher ses details.";
        // 
        // filesListView
        // 
        filesListView.AllowDrop = true;
        filesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { fileColumnHeader, directoryColumnHeader, statusColumnHeader });
        filesListView.Dock = System.Windows.Forms.DockStyle.Fill;
        filesListView.FullRowSelect = true;
        filesListView.GridLines = true;
        filesListView.HideSelection = false;
        filesListView.Location = new System.Drawing.Point(3, 112);
        filesListView.MultiSelect = true;
        filesListView.Name = "filesListView";
        filesListView.Size = new System.Drawing.Size(717, 181);
        filesListView.TabIndex = 2;
        filesListView.UseCompatibleStateImageBehavior = false;
        filesListView.View = System.Windows.Forms.View.Details;
        filesListView.SelectedIndexChanged += FilesListView_SelectedIndexChanged;
        filesListView.DragDrop += FilesListView_DragDrop;
        filesListView.DragEnter += FilesListView_DragEnter;
        // 
        // fileColumnHeader
        // 
        fileColumnHeader.Text = "Fichier";
        fileColumnHeader.Width = 220;
        // 
        // directoryColumnHeader
        // 
        directoryColumnHeader.Text = "Dossier";
        directoryColumnHeader.Width = 320;
        // 
        // statusColumnHeader
        // 
        statusColumnHeader.Text = "Statut";
        statusColumnHeader.Width = 120;
        // 
        // profileGroup
        // 
        profileGroup.AutoSize = false;
        profileGroup.Controls.Add(profileLayout);
        profileGroup.Dock = System.Windows.Forms.DockStyle.Fill;
        profileGroup.Location = new System.Drawing.Point(3, 345);
        profileGroup.Name = "profileGroup";
        profileGroup.Padding = new System.Windows.Forms.Padding(12);
        profileGroup.Size = new System.Drawing.Size(747, 132);
        profileGroup.TabIndex = 1;
        profileGroup.TabStop = false;
        profileGroup.Text = "Profil et reglages";
        // 
        // profileLayout
        // 
        profileLayout.ColumnCount = 1;
        profileLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        profileLayout.Controls.Add(profileHeaderPanel, 0, 0);
        profileLayout.Controls.Add(profileDetailsLabel, 0, 1);
        profileLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        profileLayout.Location = new System.Drawing.Point(12, 28);
        profileLayout.Name = "profileLayout";
        profileLayout.RowCount = 2;
        profileLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        profileLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        profileLayout.Size = new System.Drawing.Size(723, 92);
        profileLayout.TabIndex = 0;
        // 
        // profileHeaderPanel
        // 
        profileHeaderPanel.AutoSize = true;
        profileHeaderPanel.Controls.Add(profileComboBox);
        profileHeaderPanel.Controls.Add(manageProfilesButton);
        profileHeaderPanel.Controls.Add(refreshPreviewButton);
        profileHeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        profileHeaderPanel.Location = new System.Drawing.Point(3, 3);
        profileHeaderPanel.Name = "profileHeaderPanel";
        profileHeaderPanel.Size = new System.Drawing.Size(717, 38);
        profileHeaderPanel.TabIndex = 0;
        // 
        // profileComboBox
        // 
        profileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        profileComboBox.FormattingEnabled = true;
        profileComboBox.Location = new System.Drawing.Point(3, 3);
        profileComboBox.Name = "profileComboBox";
        profileComboBox.Size = new System.Drawing.Size(280, 23);
        profileComboBox.TabIndex = 0;
        profileComboBox.SelectedIndexChanged += ProfileComboBox_SelectedIndexChanged;
        // 
        // manageProfilesButton
        // 
        manageProfilesButton.AutoSize = true;
        manageProfilesButton.Location = new System.Drawing.Point(289, 3);
        manageProfilesButton.Name = "manageProfilesButton";
        manageProfilesButton.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
        manageProfilesButton.Size = new System.Drawing.Size(110, 33);
        manageProfilesButton.TabIndex = 1;
        manageProfilesButton.Text = "Gerer les profils";
        manageProfilesButton.UseVisualStyleBackColor = true;
        manageProfilesButton.Click += ManageProfilesButton_Click;
        // 
        // refreshPreviewButton
        // 
        refreshPreviewButton.AutoSize = true;
        refreshPreviewButton.Location = new System.Drawing.Point(405, 3);
        refreshPreviewButton.Name = "refreshPreviewButton";
        refreshPreviewButton.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
        refreshPreviewButton.Size = new System.Drawing.Size(145, 33);
        refreshPreviewButton.TabIndex = 2;
        refreshPreviewButton.Text = "Actualiser la previsualisation";
        refreshPreviewButton.UseVisualStyleBackColor = true;
        refreshPreviewButton.Click += RefreshPreviewButton_Click;
        // 
        // profileDetailsLabel
        // 
        profileDetailsLabel.AutoSize = true;
        profileDetailsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        profileDetailsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
        profileDetailsLabel.Location = new System.Drawing.Point(3, 44);
        profileDetailsLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
        profileDetailsLabel.Name = "profileDetailsLabel";
        profileDetailsLabel.Padding = new System.Windows.Forms.Padding(3);
        profileDetailsLabel.TabIndex = 1;
        profileDetailsLabel.Text = "Aucun profil sélectionné.";
        // 
        // outputGroup
        // 
        outputGroup.AutoSize = false;
        outputGroup.Controls.Add(outputLayout);
        outputGroup.Dock = System.Windows.Forms.DockStyle.Fill;
        outputGroup.Location = new System.Drawing.Point(3, 483);
        outputGroup.Name = "outputGroup";
        outputGroup.Padding = new System.Windows.Forms.Padding(12);
        outputGroup.Size = new System.Drawing.Size(747, 230);
        outputGroup.TabIndex = 2;
        outputGroup.TabStop = false;
        outputGroup.Text = "Dossier de sortie";
        // 
        // outputLayout
        // 
        outputLayout.ColumnCount = 3;
        outputLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        outputLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.Controls.Add(outputFolderTextBox, 0, 0);
        outputLayout.Controls.Add(browseOutputFolderButton, 1, 0);
        outputLayout.Controls.Add(openOutputFolderButton, 2, 0);
        outputLayout.Controls.Add(suffixLabel, 0, 1);
        outputLayout.Controls.Add(suffixTextBox, 0, 2);
        outputLayout.Controls.Add(separateTiffCheckBox, 0, 3);
        outputLayout.Controls.Add(openExplorerCheckBox, 0, 4);
        outputLayout.Controls.Add(openLogCheckBox, 0, 5);
        outputLayout.Controls.Add(outputDetailsLabel, 0, 6);
        outputLayout.SetColumnSpan(suffixLabel, 3);
        outputLayout.SetColumnSpan(suffixTextBox, 3);
        outputLayout.SetColumnSpan(separateTiffCheckBox, 3);
        outputLayout.SetColumnSpan(openExplorerCheckBox, 3);
        outputLayout.SetColumnSpan(openLogCheckBox, 3);
        outputLayout.SetColumnSpan(outputDetailsLabel, 3);
        outputLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        outputLayout.Location = new System.Drawing.Point(12, 28);
        outputLayout.Name = "outputLayout";
        outputLayout.RowCount = 7;
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        outputLayout.Size = new System.Drawing.Size(723, 190);
        outputLayout.TabIndex = 0;
        // 
        // outputFolderTextBox
        // 
        outputFolderTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        outputFolderTextBox.Location = new System.Drawing.Point(3, 3);
        outputFolderTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        outputFolderTextBox.Name = "outputFolderTextBox";
        outputFolderTextBox.PlaceholderText = "Choisissez un dossier...";
        outputFolderTextBox.Size = new System.Drawing.Size(620, 23);
        outputFolderTextBox.TabIndex = 0;
        outputFolderTextBox.TextChanged += OutputFolderTextBox_TextChanged;
        // 
        // browseOutputFolderButton
        // 
        browseOutputFolderButton.AutoSize = true;
        browseOutputFolderButton.Location = new System.Drawing.Point(632, 3);
        browseOutputFolderButton.Name = "browseOutputFolderButton";
        browseOutputFolderButton.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        browseOutputFolderButton.Size = new System.Drawing.Size(88, 33);
        browseOutputFolderButton.TabIndex = 1;
        browseOutputFolderButton.Text = "Parcourir";
        browseOutputFolderButton.UseVisualStyleBackColor = true;
        browseOutputFolderButton.Click += BrowseOutputFolderButton_Click;
        //
        // openOutputFolderButton
        //
        openOutputFolderButton.AutoSize = true;
        openOutputFolderButton.Enabled = false;
        openOutputFolderButton.Location = new System.Drawing.Point(726, 3);
        openOutputFolderButton.Name = "openOutputFolderButton";
        openOutputFolderButton.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        openOutputFolderButton.Size = new System.Drawing.Size(69, 33);
        openOutputFolderButton.TabIndex = 2;
        openOutputFolderButton.Text = "Ouvrir";
        openOutputFolderButton.UseVisualStyleBackColor = true;
        openOutputFolderButton.Click += OpenOutputFolderButton_Click;
        //
        // suffixLabel
        //
        suffixLabel.AutoSize = true;
        suffixLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        suffixLabel.Location = new System.Drawing.Point(3, 39);
        suffixLabel.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
        suffixLabel.Name = "suffixLabel";
        suffixLabel.Size = new System.Drawing.Size(717, 15);
        suffixLabel.TabIndex = 3;
        suffixLabel.Text = "Suffixe ajouté au nom du fichier";
        //
        // suffixTextBox
        //
        suffixTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        suffixTextBox.Location = new System.Drawing.Point(3, 57);
        suffixTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
        suffixTextBox.Name = "suffixTextBox";
        suffixTextBox.PlaceholderText = "ex : _300dpi_lzw_couleur";
        suffixTextBox.Size = new System.Drawing.Size(717, 23);
        suffixTextBox.TabIndex = 4;
        suffixTextBox.TextChanged += SuffixTextBox_TextChanged;
        //
        // separateTiffCheckBox
        //
        separateTiffCheckBox.AutoSize = true;
        separateTiffCheckBox.Location = new System.Drawing.Point(3, 89);
        separateTiffCheckBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
        separateTiffCheckBox.Name = "separateTiffCheckBox";
        separateTiffCheckBox.Size = new System.Drawing.Size(259, 19);
        separateTiffCheckBox.TabIndex = 5;
        separateTiffCheckBox.Text = "Créer un fichier TIFF par page (PDF multipages)";
        separateTiffCheckBox.UseVisualStyleBackColor = true;
        separateTiffCheckBox.CheckedChanged += SeparateTiffCheckBox_CheckedChanged;
        //
        // openExplorerCheckBox
        //
        openExplorerCheckBox.AutoSize = true;
        openExplorerCheckBox.Checked = true;
        openExplorerCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        openExplorerCheckBox.Location = new System.Drawing.Point(3, 114);
        openExplorerCheckBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
        openExplorerCheckBox.Name = "openExplorerCheckBox";
        openExplorerCheckBox.Size = new System.Drawing.Size(248, 19);
        openExplorerCheckBox.TabIndex = 6;
        openExplorerCheckBox.Text = "Ouvrir l'explorateur après la conversion";
        openExplorerCheckBox.UseVisualStyleBackColor = true;
        openExplorerCheckBox.CheckedChanged += OpenExplorerCheckBox_CheckedChanged;
        //
        // openLogCheckBox
        //
        openLogCheckBox.AutoSize = true;
        openLogCheckBox.Location = new System.Drawing.Point(3, 140);
        openLogCheckBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
        openLogCheckBox.Name = "openLogCheckBox";
        openLogCheckBox.Size = new System.Drawing.Size(213, 19);
        openLogCheckBox.TabIndex = 7;
        openLogCheckBox.Text = "Ouvrir le journal après la conversion";
        openLogCheckBox.UseVisualStyleBackColor = true;
        openLogCheckBox.CheckedChanged += OpenLogCheckBox_CheckedChanged;
        //
        // outputDetailsLabel
        //
        outputDetailsLabel.AutoSize = true;
        outputDetailsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        outputDetailsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
        outputDetailsLabel.Location = new System.Drawing.Point(3, 165);
        outputDetailsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
        outputDetailsLabel.Name = "outputDetailsLabel";
        outputDetailsLabel.TabIndex = 8;
        outputDetailsLabel.Text = "Dossier : (aucun)\nSuffixe : (aucun)\nTIFF multipage : fichier unique";
        // 
        // automationGroup
        // 
        automationGroup.AutoSize = true;
        automationGroup.Controls.Add(automationLayout);
        automationGroup.Dock = System.Windows.Forms.DockStyle.Fill;
        automationGroup.Location = new System.Drawing.Point(3, 609);
        automationGroup.Name = "automationGroup";
        automationGroup.Padding = new System.Windows.Forms.Padding(12);
        automationGroup.Size = new System.Drawing.Size(747, 179);
        automationGroup.TabIndex = 3;
        automationGroup.TabStop = false;
        automationGroup.Text = "Automatisation";
        // 
        // automationLayout
        // 
        automationLayout.AutoSize = true;
        automationLayout.ColumnCount = 3;
        automationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        automationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        automationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        automationLayout.Controls.Add(watchFolderCheckBox, 0, 0);
        automationLayout.Controls.Add(watchFolderTextBox, 1, 0);
        automationLayout.Controls.Add(browseWatchFolderButton, 2, 0);
        automationLayout.Controls.Add(watchFolderStatusLabel, 0, 1);
        automationLayout.Controls.Add(watchLogTextBox, 0, 2);
        automationLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        automationLayout.Location = new System.Drawing.Point(12, 28);
        automationLayout.Name = "automationLayout";
        automationLayout.RowCount = 3;
        automationLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        automationLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        automationLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        automationLayout.Size = new System.Drawing.Size(723, 123);
        automationLayout.TabIndex = 0;
        // 
        // watchFolderCheckBox
        // 
        watchFolderCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
        watchFolderCheckBox.AutoCheck = false;
        watchFolderCheckBox.AutoSize = false;
        watchFolderCheckBox.Location = new System.Drawing.Point(3, 3);
        watchFolderCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        watchFolderCheckBox.MinimumSize = new System.Drawing.Size(220, 0);
        watchFolderCheckBox.Name = "watchFolderCheckBox";
        watchFolderCheckBox.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        watchFolderCheckBox.Size = new System.Drawing.Size(220, 33);
        watchFolderCheckBox.TabIndex = 0;
        watchFolderCheckBox.Text = "Activer la surveillance";
        watchFolderCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        watchFolderCheckBox.UseVisualStyleBackColor = false;
        watchFolderCheckBox.Click += WatchFolderCheckBox_Click;
        // 
        // watchFolderTextBox
        // 
        watchFolderTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        watchFolderTextBox.Location = new System.Drawing.Point(153, 3);
        watchFolderTextBox.Margin = new System.Windows.Forms.Padding(0, 3, 6, 3);
        watchFolderTextBox.Name = "watchFolderTextBox";
        watchFolderTextBox.PlaceholderText = "Dossier a surveiller";
        watchFolderTextBox.Size = new System.Drawing.Size(510, 23);
        watchFolderTextBox.TabIndex = 1;
        // 
        // browseWatchFolderButton
        // 
        browseWatchFolderButton.AutoSize = true;
        browseWatchFolderButton.Location = new System.Drawing.Point(672, 3);
        browseWatchFolderButton.Name = "browseWatchFolderButton";
        browseWatchFolderButton.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        browseWatchFolderButton.Size = new System.Drawing.Size(48, 33);
        browseWatchFolderButton.TabIndex = 2;
        browseWatchFolderButton.Text = "...";
        browseWatchFolderButton.UseVisualStyleBackColor = true;
        browseWatchFolderButton.Click += BrowseWatchFolderButton_Click;
        // 
        // watchFolderStatusLabel
        //
        automationLayout.SetColumnSpan(watchFolderStatusLabel, 3);
        watchFolderStatusLabel.AutoSize = true;
        watchFolderStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        watchFolderStatusLabel.ForeColor = System.Drawing.SystemColors.GrayText;
        watchFolderStatusLabel.Location = new System.Drawing.Point(3, 39);
        watchFolderStatusLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
        watchFolderStatusLabel.Name = "watchFolderStatusLabel";
        watchFolderStatusLabel.Size = new System.Drawing.Size(717, 15);
        watchFolderStatusLabel.TabIndex = 3;
        watchFolderStatusLabel.Text = "Surveillance inactive";
        //
        // watchLogTextBox
        //
        automationLayout.SetColumnSpan(watchLogTextBox, 3);
        watchLogTextBox.BackColor = System.Drawing.SystemColors.Window;
        watchLogTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        watchLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        watchLogTextBox.Location = new System.Drawing.Point(3, 63);
        watchLogTextBox.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        watchLogTextBox.MinimumSize = new System.Drawing.Size(0, 48);
        watchLogTextBox.Name = "watchLogTextBox";
        watchLogTextBox.ReadOnly = true;
        watchLogTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
        watchLogTextBox.Size = new System.Drawing.Size(717, 57);
        watchLogTextBox.TabIndex = 4;
        watchLogTextBox.Text = "";
        // 
        // previewGroup
        // 
        previewGroup.Controls.Add(previewLayout);
        previewGroup.Dock = System.Windows.Forms.DockStyle.Fill;
        previewGroup.Location = new System.Drawing.Point(762, 3);
        previewGroup.Name = "previewGroup";
        previewGroup.Padding = new System.Windows.Forms.Padding(12);
        previewGroup.Size = new System.Drawing.Size(501, 685);
        previewGroup.TabIndex = 1;
        previewGroup.TabStop = false;
        previewGroup.Text = "Previsualisation TIFF";
        // 
        // previewLayout
        // 
        previewLayout.ColumnCount = 1;
        previewLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        previewLayout.Controls.Add(previewStatusLabel, 0, 0);
        previewLayout.Controls.Add(previewImagesLayout, 0, 1);
        previewLayout.Controls.Add(previewControlsPanel, 0, 2);
        previewLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        previewLayout.Location = new System.Drawing.Point(12, 28);
        previewLayout.Name = "previewLayout";
        previewLayout.RowCount = 3;
        previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        previewLayout.Size = new System.Drawing.Size(477, 645);
        previewLayout.TabIndex = 0;
        // 
        // previewStatusLabel
        // 
        previewStatusLabel.AutoSize = true;
        previewStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        previewStatusLabel.ForeColor = System.Drawing.SystemColors.GrayText;
        previewStatusLabel.Location = new System.Drawing.Point(3, 0);
        previewStatusLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
        previewStatusLabel.Name = "previewStatusLabel";
        previewStatusLabel.Size = new System.Drawing.Size(471, 15);
        previewStatusLabel.TabIndex = 0;
        previewStatusLabel.Text = "Selectionnez un PDF pour afficher l'apercu.";
        // 
        // previewImagesLayout
        // 
        previewImagesLayout.ColumnCount = 1;
        previewImagesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        previewImagesLayout.Controls.Add(afterPreviewContainer, 0, 0);
        previewImagesLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        previewImagesLayout.Location = new System.Drawing.Point(3, 26);
        previewImagesLayout.Name = "previewImagesLayout";
        previewImagesLayout.RowCount = 1;
        previewImagesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        previewImagesLayout.Size = new System.Drawing.Size(471, 557);
        previewImagesLayout.TabIndex = 1;
        //
        // afterPreviewContainer
        //
        afterPreviewContainer.Controls.Add(afterPreviewPanel);
        afterPreviewContainer.Controls.Add(afterPreviewLabel);
        afterPreviewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        afterPreviewContainer.Location = new System.Drawing.Point(3, 3);
        afterPreviewContainer.Name = "afterPreviewContainer";
        afterPreviewContainer.Size = new System.Drawing.Size(465, 551);
        afterPreviewContainer.TabIndex = 1;
        // 
        // afterPreviewPanel
        // 
        afterPreviewPanel.AutoScroll = true;
        afterPreviewPanel.Controls.Add(afterPictureBox);
        afterPreviewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        afterPreviewPanel.Location = new System.Drawing.Point(0, 23);
        afterPreviewPanel.Name = "afterPreviewPanel";
        afterPreviewPanel.Size = new System.Drawing.Size(230, 528);
        afterPreviewPanel.TabIndex = 1;
        afterPreviewPanel.SizeChanged += AfterPreviewPanel_SizeChanged;
        // 
        // afterPictureBox
        // 
        afterPictureBox.Location = new System.Drawing.Point(0, 0);
        afterPictureBox.Name = "afterPictureBox";
        afterPictureBox.Size = new System.Drawing.Size(100, 50);
        afterPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        afterPictureBox.TabIndex = 0;
        afterPictureBox.TabStop = false;
        // 
        // afterPreviewLabel
        // 
        afterPreviewLabel.AutoSize = true;
        afterPreviewLabel.Dock = System.Windows.Forms.DockStyle.Top;
        afterPreviewLabel.Location = new System.Drawing.Point(0, 0);
        afterPreviewLabel.Name = "afterPreviewLabel";
        afterPreviewLabel.Size = new System.Drawing.Size(131, 15);
        afterPreviewLabel.TabIndex = 0;
        afterPreviewLabel.Text = "Aperçu après conversion";
        // 
        // previewControlsPanel
        // 
        previewControlsPanel.AutoSize = true;
        previewControlsPanel.Controls.Add(previewZoomTrackBar);
        previewControlsPanel.Controls.Add(previewZoomValueLabel);
        previewControlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        previewControlsPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        previewControlsPanel.Location = new System.Drawing.Point(3, 589);
        previewControlsPanel.Name = "previewControlsPanel";
        previewControlsPanel.Size = new System.Drawing.Size(471, 53);
        previewControlsPanel.TabIndex = 2;
        // 
        // previewZoomTrackBar
        // 
        previewZoomTrackBar.LargeChange = 25;
        previewZoomTrackBar.Location = new System.Drawing.Point(3, 3);
        previewZoomTrackBar.Maximum = 400;
        previewZoomTrackBar.Minimum = 10;
        previewZoomTrackBar.Name = "previewZoomTrackBar";
        previewZoomTrackBar.Size = new System.Drawing.Size(280, 45);
        previewZoomTrackBar.TabIndex = 0;
        previewZoomTrackBar.TickFrequency = 10;
        previewZoomTrackBar.Value = 100;
        previewZoomTrackBar.ValueChanged += PreviewZoomTrackBar_ValueChanged;
        // 
        // previewZoomValueLabel
        // 
        previewZoomValueLabel.AutoSize = true;
        previewZoomValueLabel.Location = new System.Drawing.Point(289, 12);
        previewZoomValueLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
        previewZoomValueLabel.Name = "previewZoomValueLabel";
        previewZoomValueLabel.Size = new System.Drawing.Size(66, 15);
        previewZoomValueLabel.TabIndex = 1;
        previewZoomValueLabel.Text = "Zoom : 100%";
        // 
        // conversionProgressBar
        // 
        conversionProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
        conversionProgressBar.Location = new System.Drawing.Point(3, 694);
        conversionProgressBar.Margin = new System.Windows.Forms.Padding(3, 6, 12, 0);
        conversionProgressBar.Name = "conversionProgressBar";
        conversionProgressBar.Size = new System.Drawing.Size(741, 45);
        conversionProgressBar.TabIndex = 2;
        // 
        // actionsPanel
        // 
        actionsPanel.AutoSize = true;
        actionsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        actionsPanel.Controls.Add(convertButton);
        actionsPanel.Controls.Add(stopButton);
        actionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        actionsPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        actionsPanel.Location = new System.Drawing.Point(762, 694);
        actionsPanel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
        actionsPanel.Name = "actionsPanel";
        actionsPanel.Size = new System.Drawing.Size(501, 45);
        actionsPanel.TabIndex = 3;
        // 
        // convertButton
        // 
        convertButton.AutoSize = true;
        convertButton.Name = "convertButton";
        convertButton.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
        convertButton.Size = new System.Drawing.Size(136, 39);
        convertButton.TabIndex = 0;
        convertButton.Text = "Lancer la conversion";
        convertButton.UseVisualStyleBackColor = true;
        convertButton.Click += ConvertButton_Click;
        // 
        // stopButton
        // 
        stopButton.AutoSize = true;
        stopButton.Enabled = false;
        stopButton.Name = "stopButton";
        stopButton.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
        stopButton.Size = new System.Drawing.Size(136, 39);
        stopButton.TabIndex = 1;
        stopButton.Text = "Arreter";
        stopButton.UseVisualStyleBackColor = true;
        stopButton.Click += StopButton_Click;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusStripLabel });
        statusStrip.Location = new System.Drawing.Point(0, 751);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new System.Drawing.Size(1284, 22);
        statusStrip.TabIndex = 1;
        statusStrip.Text = "statusStrip";
        // 
        // statusStripLabel
        // 
        statusStripLabel.Name = "statusStripLabel";
        statusStripLabel.Size = new System.Drawing.Size(30, 17);
        statusStripLabel.Text = "Pret";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1284, 782);
        Controls.Add(mainLayout);
        Controls.Add(statusStrip);
        MinimumSize = new System.Drawing.Size(1100, 720);
        Name = "MainForm";
        Padding = new System.Windows.Forms.Padding(9);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Convertisseur PDF en TIFF";
        mainLayout.ResumeLayout(false);
        mainLayout.PerformLayout();
        leftLayout.ResumeLayout(false);
        leftLayout.PerformLayout();
        filesGroup.ResumeLayout(false);
        filesGroupLayout.ResumeLayout(false);
        filesGroupLayout.PerformLayout();
        filesButtonsPanel.ResumeLayout(false);
        filesButtonsPanel.PerformLayout();
        profileGroup.ResumeLayout(false);
        profileLayout.ResumeLayout(false);
        profileLayout.PerformLayout();
        profileHeaderPanel.ResumeLayout(false);
        profileHeaderPanel.PerformLayout();
        outputGroup.ResumeLayout(false);
        outputLayout.ResumeLayout(false);
        outputLayout.PerformLayout();
        automationGroup.ResumeLayout(false);
        automationLayout.ResumeLayout(false);
        automationLayout.PerformLayout();
        previewGroup.ResumeLayout(false);
        previewLayout.ResumeLayout(false);
        previewLayout.PerformLayout();
        previewImagesLayout.ResumeLayout(false);
        afterPreviewContainer.ResumeLayout(false);
        afterPreviewContainer.PerformLayout();
        afterPreviewPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)afterPictureBox).EndInit();
        previewControlsPanel.ResumeLayout(false);
        previewControlsPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)previewZoomTrackBar).EndInit();
        actionsPanel.ResumeLayout(false);
        actionsPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openPdfDialog;
    private System.Windows.Forms.FolderBrowserDialog outputFolderDialog;
    private System.Windows.Forms.TableLayoutPanel mainLayout;
    private System.Windows.Forms.TableLayoutPanel leftLayout;
    private System.Windows.Forms.GroupBox filesGroup;
    private System.Windows.Forms.TableLayoutPanel filesGroupLayout;
    private System.Windows.Forms.FlowLayoutPanel filesButtonsPanel;
    private System.Windows.Forms.Button addFilesButton;
    private System.Windows.Forms.Button removeFilesButton;
    private System.Windows.Forms.Button clearFilesButton;
    private System.Windows.Forms.Label dropFilesLabel;
    private System.Windows.Forms.Label selectedFileDetailsLabel;
    private System.Windows.Forms.ListView filesListView;
    private System.Windows.Forms.ColumnHeader fileColumnHeader;
    private System.Windows.Forms.ColumnHeader directoryColumnHeader;
    private System.Windows.Forms.ColumnHeader statusColumnHeader;
    private System.Windows.Forms.GroupBox profileGroup;
    private System.Windows.Forms.TableLayoutPanel profileLayout;
    private System.Windows.Forms.FlowLayoutPanel profileHeaderPanel;
    private System.Windows.Forms.ComboBox profileComboBox;
    private System.Windows.Forms.Button manageProfilesButton;
    private System.Windows.Forms.Button refreshPreviewButton;
    private System.Windows.Forms.Label profileDetailsLabel;
    private System.Windows.Forms.GroupBox outputGroup;
    private System.Windows.Forms.TableLayoutPanel outputLayout;
    private System.Windows.Forms.TextBox outputFolderTextBox;
    private System.Windows.Forms.Button browseOutputFolderButton;
    private System.Windows.Forms.Button openOutputFolderButton;
    private System.Windows.Forms.CheckBox openExplorerCheckBox;
    private System.Windows.Forms.CheckBox openLogCheckBox;
    private System.Windows.Forms.Label suffixLabel;
    private System.Windows.Forms.TextBox suffixTextBox;
    private System.Windows.Forms.CheckBox separateTiffCheckBox;
    private System.Windows.Forms.Label outputDetailsLabel;
    private System.Windows.Forms.GroupBox automationGroup;
    private System.Windows.Forms.TableLayoutPanel automationLayout;
    private System.Windows.Forms.CheckBox watchFolderCheckBox;
    private System.Windows.Forms.TextBox watchFolderTextBox;
    private System.Windows.Forms.Button browseWatchFolderButton;
    private System.Windows.Forms.Label watchFolderStatusLabel;
    private System.Windows.Forms.GroupBox previewGroup;
    private System.Windows.Forms.TableLayoutPanel previewLayout;
    private System.Windows.Forms.Label previewStatusLabel;
    private System.Windows.Forms.TableLayoutPanel previewImagesLayout;
    private System.Windows.Forms.Panel afterPreviewContainer;
    private System.Windows.Forms.Panel afterPreviewPanel;
    private System.Windows.Forms.PictureBox afterPictureBox;
    private System.Windows.Forms.Label afterPreviewLabel;
    private System.Windows.Forms.FlowLayoutPanel previewControlsPanel;
    private System.Windows.Forms.TrackBar previewZoomTrackBar;
    private System.Windows.Forms.Label previewZoomValueLabel;
    private System.Windows.Forms.ProgressBar conversionProgressBar;
    private System.Windows.Forms.FlowLayoutPanel actionsPanel;
    private System.Windows.Forms.Button convertButton;
    private System.Windows.Forms.Button stopButton;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel statusStripLabel;
    private System.Windows.Forms.RichTextBox watchLogTextBox;
}
