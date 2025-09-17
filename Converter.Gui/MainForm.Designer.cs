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
        openExplorerCheckBox = new System.Windows.Forms.CheckBox();
        openLogCheckBox = new System.Windows.Forms.CheckBox();
        automationGroup = new System.Windows.Forms.GroupBox();
        automationLayout = new System.Windows.Forms.TableLayoutPanel();
        watchFolderCheckBox = new System.Windows.Forms.CheckBox();
        watchFolderTextBox = new System.Windows.Forms.TextBox();
        browseWatchFolderButton = new System.Windows.Forms.Button();
        watchFolderStatusLabel = new System.Windows.Forms.Label();
        previewGroup = new System.Windows.Forms.GroupBox();
        previewLayout = new System.Windows.Forms.TableLayoutPanel();
        previewStatusLabel = new System.Windows.Forms.Label();
        previewImagesLayout = new System.Windows.Forms.TableLayoutPanel();
        beforePreviewContainer = new System.Windows.Forms.Panel();
        beforePreviewPanel = new System.Windows.Forms.Panel();
        beforePictureBox = new System.Windows.Forms.PictureBox();
        beforePreviewLabel = new System.Windows.Forms.Label();
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
        beforePreviewContainer.SuspendLayout();
        beforePreviewPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)beforePictureBox).BeginInit();
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
        mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
        mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
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
        leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
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
        filesGroupLayout.Controls.Add(filesListView, 0, 2);
        filesGroupLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        filesGroupLayout.Location = new System.Drawing.Point(12, 28);
        filesGroupLayout.Name = "filesGroupLayout";
        filesGroupLayout.RowCount = 3;
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
        removeFilesButton.Text = "Retirer sélection";
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
        dropFilesLabel.Text = "Glissez-déposez vos PDF ici ou utilisez le bouton Ajouter.";
        // 
        // filesListView
        // 
        filesListView.AllowDrop = true;
        filesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { fileColumnHeader, directoryColumnHeader, statusColumnHeader });
        filesListView.Dock = System.Windows.Forms.DockStyle.Fill;
        filesListView.FullRowSelect = true;
        filesListView.GridLines = true;
        filesListView.HideSelection = false;
        filesListView.Location = new System.Drawing.Point(3, 82);
        filesListView.MultiSelect = true;
        filesListView.Name = "filesListView";
        filesListView.Size = new System.Drawing.Size(717, 211);
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
        profileGroup.AutoSize = true;
        profileGroup.Controls.Add(profileLayout);
        profileGroup.Dock = System.Windows.Forms.DockStyle.Fill;
        profileGroup.Location = new System.Drawing.Point(3, 345);
        profileGroup.Name = "profileGroup";
        profileGroup.Padding = new System.Windows.Forms.Padding(12);
        profileGroup.Size = new System.Drawing.Size(747, 132);
        profileGroup.TabIndex = 1;
        profileGroup.TabStop = false;
        profileGroup.Text = "Profil et réglages";
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
        profileLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
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
        manageProfilesButton.Text = "Gérer les profils";
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
        refreshPreviewButton.Text = "Actualiser la prévisualisation";
        refreshPreviewButton.UseVisualStyleBackColor = true;
        refreshPreviewButton.Click += RefreshPreviewButton_Click;
        // 
        // profileDetailsLabel
        // 
        profileDetailsLabel.AutoSize = true;
        profileDetailsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        profileDetailsLabel.ForeColor = System.Drawing.SystemColors.GrayText;
        profileDetailsLabel.Location = new System.Drawing.Point(3, 44);
        profileDetailsLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
        profileDetailsLabel.Name = "profileDetailsLabel";
        profileDetailsLabel.Size = new System.Drawing.Size(717, 45);
        profileDetailsLabel.TabIndex = 1;
        profileDetailsLabel.Text = "";
        // 
        // outputGroup
        // 
        outputGroup.AutoSize = true;
        outputGroup.Controls.Add(outputLayout);
        outputGroup.Dock = System.Windows.Forms.DockStyle.Fill;
        outputGroup.Location = new System.Drawing.Point(3, 483);
        outputGroup.Name = "outputGroup";
        outputGroup.Padding = new System.Windows.Forms.Padding(12);
        outputGroup.Size = new System.Drawing.Size(747, 120);
        outputGroup.TabIndex = 2;
        outputGroup.TabStop = false;
        outputGroup.Text = "Dossier de sortie";
        // 
        // outputLayout
        // 
        outputLayout.ColumnCount = 2;
        outputLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        outputLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.Controls.Add(outputFolderTextBox, 0, 0);
        outputLayout.Controls.Add(browseOutputFolderButton, 1, 0);
        outputLayout.Controls.Add(openExplorerCheckBox, 0, 1);
        outputLayout.Controls.Add(openLogCheckBox, 0, 2);
        outputLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        outputLayout.Location = new System.Drawing.Point(12, 28);
        outputLayout.Name = "outputLayout";
        outputLayout.RowCount = 3;
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        outputLayout.Size = new System.Drawing.Size(723, 80);
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
        // openExplorerCheckBox
        // 
        openExplorerCheckBox.AutoSize = true;
        openExplorerCheckBox.Location = new System.Drawing.Point(3, 42);
        openExplorerCheckBox.Name = "openExplorerCheckBox";
        openExplorerCheckBox.Size = new System.Drawing.Size(252, 19);
        openExplorerCheckBox.TabIndex = 2;
        openExplorerCheckBox.Text = "Ouvrir l'explorateur à la fin de la conversion";
        openExplorerCheckBox.UseVisualStyleBackColor = true;
        openExplorerCheckBox.CheckedChanged += OpenExplorerCheckBox_CheckedChanged;
        // 
        // openLogCheckBox
        // 
        openLogCheckBox.AutoSize = true;
        openLogCheckBox.Location = new System.Drawing.Point(3, 67);
        openLogCheckBox.Name = "openLogCheckBox";
        openLogCheckBox.Size = new System.Drawing.Size(267, 19);
        openLogCheckBox.TabIndex = 3;
        openLogCheckBox.Text = "Afficher le journal détaillé après conversion";
        openLogCheckBox.UseVisualStyleBackColor = true;
        openLogCheckBox.CheckedChanged += OpenLogCheckBox_CheckedChanged;
        // 
        // automationGroup
        // 
        automationGroup.AutoSize = true;
        automationGroup.Controls.Add(automationLayout);
        automationGroup.Dock = System.Windows.Forms.DockStyle.Fill;
        automationGroup.Location = new System.Drawing.Point(3, 609);
        automationGroup.Name = "automationGroup";
        automationGroup.Padding = new System.Windows.Forms.Padding(12);
        automationGroup.Size = new System.Drawing.Size(747, 73);
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
        automationLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        automationLayout.Location = new System.Drawing.Point(12, 28);
        automationLayout.Name = "automationLayout";
        automationLayout.RowCount = 2;
        automationLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        automationLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        automationLayout.Size = new System.Drawing.Size(723, 33);
        automationLayout.TabIndex = 0;
        // 
        // watchFolderCheckBox
        // 
        watchFolderCheckBox.AutoSize = true;
        watchFolderCheckBox.Location = new System.Drawing.Point(3, 3);
        watchFolderCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        watchFolderCheckBox.Name = "watchFolderCheckBox";
        watchFolderCheckBox.Size = new System.Drawing.Size(144, 19);
        watchFolderCheckBox.TabIndex = 0;
        watchFolderCheckBox.Text = "Activer le dossier surveillé";
        watchFolderCheckBox.UseVisualStyleBackColor = true;
        watchFolderCheckBox.CheckedChanged += WatchFolderCheckBox_CheckedChanged;
        // 
        // watchFolderTextBox
        // 
        watchFolderTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        watchFolderTextBox.Location = new System.Drawing.Point(153, 3);
        watchFolderTextBox.Margin = new System.Windows.Forms.Padding(0, 3, 6, 3);
        watchFolderTextBox.Name = "watchFolderTextBox";
        watchFolderTextBox.PlaceholderText = "Dossier à surveiller";
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
        previewGroup.Text = "Prévisualisation TIFF";
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
        previewStatusLabel.Text = "Sélectionnez un PDF pour afficher l'aperçu.";
        // 
        // previewImagesLayout
        // 
        previewImagesLayout.ColumnCount = 2;
        previewImagesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        previewImagesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        previewImagesLayout.Controls.Add(beforePreviewContainer, 0, 0);
        previewImagesLayout.Controls.Add(afterPreviewContainer, 1, 0);
        previewImagesLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        previewImagesLayout.Location = new System.Drawing.Point(3, 26);
        previewImagesLayout.Name = "previewImagesLayout";
        previewImagesLayout.RowCount = 1;
        previewImagesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        previewImagesLayout.Size = new System.Drawing.Size(471, 557);
        previewImagesLayout.TabIndex = 1;
        // 
        // beforePreviewContainer
        // 
        beforePreviewContainer.Controls.Add(beforePreviewPanel);
        beforePreviewContainer.Controls.Add(beforePreviewLabel);
        beforePreviewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        beforePreviewContainer.Location = new System.Drawing.Point(3, 3);
        beforePreviewContainer.Name = "beforePreviewContainer";
        beforePreviewContainer.Size = new System.Drawing.Size(229, 551);
        beforePreviewContainer.TabIndex = 0;
        // 
        // beforePreviewPanel
        // 
        beforePreviewPanel.AutoScroll = true;
        beforePreviewPanel.Controls.Add(beforePictureBox);
        beforePreviewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        beforePreviewPanel.Location = new System.Drawing.Point(0, 23);
        beforePreviewPanel.Name = "beforePreviewPanel";
        beforePreviewPanel.Size = new System.Drawing.Size(229, 528);
        beforePreviewPanel.TabIndex = 1;
        // 
        // beforePictureBox
        // 
        beforePictureBox.Location = new System.Drawing.Point(0, 0);
        beforePictureBox.Name = "beforePictureBox";
        beforePictureBox.Size = new System.Drawing.Size(100, 50);
        beforePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        beforePictureBox.TabIndex = 0;
        beforePictureBox.TabStop = false;
        // 
        // beforePreviewLabel
        // 
        beforePreviewLabel.AutoSize = true;
        beforePreviewLabel.Dock = System.Windows.Forms.DockStyle.Top;
        beforePreviewLabel.Location = new System.Drawing.Point(0, 0);
        beforePreviewLabel.Name = "beforePreviewLabel";
        beforePreviewLabel.Size = new System.Drawing.Size(101, 15);
        beforePreviewLabel.TabIndex = 0;
        beforePreviewLabel.Text = "Avant conversion";
        // 
        // afterPreviewContainer
        // 
        afterPreviewContainer.Controls.Add(afterPreviewPanel);
        afterPreviewContainer.Controls.Add(afterPreviewLabel);
        afterPreviewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        afterPreviewContainer.Location = new System.Drawing.Point(238, 3);
        afterPreviewContainer.Name = "afterPreviewContainer";
        afterPreviewContainer.Size = new System.Drawing.Size(230, 551);
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
        afterPreviewLabel.Size = new System.Drawing.Size(115, 15);
        afterPreviewLabel.TabIndex = 0;
        afterPreviewLabel.Text = "Après conversion";
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
        stopButton.Text = "Arrêter";
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
        statusStripLabel.Text = "Prêt";
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
        Text = "Convertisseur PDF → TIFF";
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
        beforePreviewContainer.ResumeLayout(false);
        beforePreviewContainer.PerformLayout();
        beforePreviewPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)beforePictureBox).EndInit();
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
    private System.Windows.Forms.CheckBox openExplorerCheckBox;
    private System.Windows.Forms.CheckBox openLogCheckBox;
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
    private System.Windows.Forms.Panel beforePreviewContainer;
    private System.Windows.Forms.Panel beforePreviewPanel;
    private System.Windows.Forms.PictureBox beforePictureBox;
    private System.Windows.Forms.Label beforePreviewLabel;
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
}
