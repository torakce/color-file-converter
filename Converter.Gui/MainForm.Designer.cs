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
        saveTiffDialog = new System.Windows.Forms.SaveFileDialog();
        tableLayout = new System.Windows.Forms.TableLayoutPanel();
        inputLabel = new System.Windows.Forms.Label();
        inputTextBox = new System.Windows.Forms.TextBox();
        browseInputButton = new System.Windows.Forms.Button();
        outputLabel = new System.Windows.Forms.Label();
        outputTextBox = new System.Windows.Forms.TextBox();
        browseOutputButton = new System.Windows.Forms.Button();
        colorLabel = new System.Windows.Forms.Label();
        colorCombo = new System.Windows.Forms.ComboBox();
        compressionLabel = new System.Windows.Forms.Label();
        compressionCombo = new System.Windows.Forms.ComboBox();
        dpiLabel = new System.Windows.Forms.Label();
        dpiUpDown = new System.Windows.Forms.NumericUpDown();
        statusLabel = new System.Windows.Forms.Label();
        buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
        convertButton = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)dpiUpDown).BeginInit();
        buttonsPanel.SuspendLayout();
        SuspendLayout();
        // 
        // openPdfDialog
        // 
        openPdfDialog.DefaultExt = "pdf";
        openPdfDialog.Filter = "PDF (*.pdf)|*.pdf|Tous les fichiers (*.*)|*.*";
        openPdfDialog.Title = "Sélectionner un fichier PDF";
        // 
        // saveTiffDialog
        // 
        saveTiffDialog.AddExtension = true;
        saveTiffDialog.DefaultExt = "tif";
        saveTiffDialog.Filter = "TIFF (*.tif)|*.tif|TIFF (*.tiff)|*.tiff|Tous les fichiers (*.*)|*.*";
        saveTiffDialog.Title = "Enregistrer le fichier TIFF";
        // 
        // tableLayout
        // 
        tableLayout.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        tableLayout.AutoSize = true;
        tableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tableLayout.ColumnCount = 3;
        tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        tableLayout.Controls.Add(inputLabel, 0, 0);
        tableLayout.Controls.Add(inputTextBox, 1, 0);
        tableLayout.Controls.Add(browseInputButton, 2, 0);
        tableLayout.Controls.Add(outputLabel, 0, 1);
        tableLayout.Controls.Add(outputTextBox, 1, 1);
        tableLayout.Controls.Add(browseOutputButton, 2, 1);
        tableLayout.Controls.Add(colorLabel, 0, 2);
        tableLayout.Controls.Add(colorCombo, 1, 2);
        tableLayout.Controls.Add(compressionLabel, 0, 3);
        tableLayout.Controls.Add(compressionCombo, 1, 3);
        tableLayout.Controls.Add(dpiLabel, 0, 4);
        tableLayout.Controls.Add(dpiUpDown, 1, 4);
        tableLayout.Controls.Add(statusLabel, 0, 5);
        tableLayout.Controls.Add(buttonsPanel, 0, 6);
        tableLayout.Location = new System.Drawing.Point(12, 12);
        tableLayout.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
        tableLayout.Name = "tableLayout";
        tableLayout.RowCount = 7;
        tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayout.Size = new System.Drawing.Size(660, 213);
        tableLayout.TabIndex = 0;
        // 
        // inputLabel
        // 
        inputLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        inputLabel.AutoSize = true;
        inputLabel.Location = new System.Drawing.Point(3, 6);
        inputLabel.Name = "inputLabel";
        inputLabel.Size = new System.Drawing.Size(88, 15);
        inputLabel.TabIndex = 0;
        inputLabel.Text = "PDF à convertir";
        // 
        // inputTextBox
        // 
        inputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        inputTextBox.Location = new System.Drawing.Point(114, 3);
        inputTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        inputTextBox.Name = "inputTextBox";
        inputTextBox.PlaceholderText = "Choisissez un fichier PDF...";
        inputTextBox.Size = new System.Drawing.Size(451, 23);
        inputTextBox.TabIndex = 1;
        // 
        // browseInputButton
        // 
        browseInputButton.AutoSize = true;
        browseInputButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        browseInputButton.Location = new System.Drawing.Point(574, 3);
        browseInputButton.Name = "browseInputButton";
        browseInputButton.Size = new System.Drawing.Size(83, 25);
        browseInputButton.TabIndex = 2;
        browseInputButton.Text = "Parcourir...";
        browseInputButton.UseVisualStyleBackColor = true;
        browseInputButton.Click += BrowseInputButton_Click;
        // 
        // outputLabel
        // 
        outputLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        outputLabel.AutoSize = true;
        outputLabel.Location = new System.Drawing.Point(3, 40);
        outputLabel.Name = "outputLabel";
        outputLabel.Size = new System.Drawing.Size(80, 15);
        outputLabel.TabIndex = 3;
        outputLabel.Text = "TIFF de sortie";
        // 
        // outputTextBox
        // 
        outputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        outputTextBox.Location = new System.Drawing.Point(114, 37);
        outputTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        outputTextBox.Name = "outputTextBox";
        outputTextBox.PlaceholderText = "Nom du fichier TIFF";
        outputTextBox.Size = new System.Drawing.Size(451, 23);
        outputTextBox.TabIndex = 4;
        // 
        // browseOutputButton
        // 
        browseOutputButton.AutoSize = true;
        browseOutputButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        browseOutputButton.Location = new System.Drawing.Point(574, 37);
        browseOutputButton.Name = "browseOutputButton";
        browseOutputButton.Size = new System.Drawing.Size(83, 25);
        browseOutputButton.TabIndex = 5;
        browseOutputButton.Text = "Parcourir...";
        browseOutputButton.UseVisualStyleBackColor = true;
        browseOutputButton.Click += BrowseOutputButton_Click;
        // 
        // colorLabel
        // 
        colorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        colorLabel.AutoSize = true;
        colorLabel.Location = new System.Drawing.Point(3, 74);
        colorLabel.Name = "colorLabel";
        colorLabel.Size = new System.Drawing.Size(81, 15);
        colorLabel.TabIndex = 6;
        colorLabel.Text = "Mode couleur";
        // 
        // colorCombo
        // 
        colorCombo.Dock = System.Windows.Forms.DockStyle.Fill;
        colorCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        colorCombo.FormattingEnabled = true;
        colorCombo.Location = new System.Drawing.Point(114, 71);
        colorCombo.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        colorCombo.Name = "colorCombo";
        colorCombo.Size = new System.Drawing.Size(451, 23);
        colorCombo.TabIndex = 7;
        colorCombo.SelectedIndexChanged += ColorCombo_SelectedIndexChanged;
        // 
        // compressionLabel
        // 
        compressionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        compressionLabel.AutoSize = true;
        compressionLabel.Location = new System.Drawing.Point(3, 108);
        compressionLabel.Name = "compressionLabel";
        compressionLabel.Size = new System.Drawing.Size(77, 15);
        compressionLabel.TabIndex = 8;
        compressionLabel.Text = "Compression";
        // 
        // compressionCombo
        // 
        compressionCombo.Dock = System.Windows.Forms.DockStyle.Fill;
        compressionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        compressionCombo.FormattingEnabled = true;
        compressionCombo.Location = new System.Drawing.Point(114, 105);
        compressionCombo.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        compressionCombo.Name = "compressionCombo";
        compressionCombo.Size = new System.Drawing.Size(451, 23);
        compressionCombo.TabIndex = 9;
        // 
        // dpiLabel
        // 
        dpiLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        dpiLabel.AutoSize = true;
        dpiLabel.Location = new System.Drawing.Point(3, 142);
        dpiLabel.Name = "dpiLabel";
        dpiLabel.Size = new System.Drawing.Size(64, 15);
        dpiLabel.TabIndex = 10;
        dpiLabel.Text = "Résolution";
        // 
        // dpiUpDown
        // 
        dpiUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
        dpiUpDown.Increment = new decimal(new int[] { 50, 0, 0, 0 });
        dpiUpDown.Location = new System.Drawing.Point(114, 139);
        dpiUpDown.Maximum = new decimal(new int[] { 2400, 0, 0, 0 });
        dpiUpDown.Minimum = new decimal(new int[] { 72, 0, 0, 0 });
        dpiUpDown.Name = "dpiUpDown";
        dpiUpDown.Size = new System.Drawing.Size(120, 23);
        dpiUpDown.TabIndex = 11;
        dpiUpDown.Value = new decimal(new int[] { 300, 0, 0, 0 });
        // 
        // statusLabel
        // 
        statusLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        statusLabel.AutoSize = true;
        tableLayout.SetColumnSpan(statusLabel, 3);
        statusLabel.ForeColor = System.Drawing.SystemColors.GrayText;
        statusLabel.Location = new System.Drawing.Point(3, 174);
        statusLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new System.Drawing.Size(30, 15);
        statusLabel.TabIndex = 12;
        statusLabel.Text = "Prêt";
        // 
        // buttonsPanel
        // 
        buttonsPanel.AutoSize = true;
        buttonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tableLayout.SetColumnSpan(buttonsPanel, 3);
        buttonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        buttonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        buttonsPanel.Location = new System.Drawing.Point(3, 195);
        buttonsPanel.Name = "buttonsPanel";
        buttonsPanel.Size = new System.Drawing.Size(654, 15);
        buttonsPanel.TabIndex = 13;
        // 
        // convertButton
        // 
        convertButton.AutoSize = true;
        convertButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        convertButton.Location = new System.Drawing.Point(555, 3);
        convertButton.Name = "convertButton";
        convertButton.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
        convertButton.Size = new System.Drawing.Size(96, 31);
        convertButton.TabIndex = 0;
        convertButton.Text = "Convertir";
        convertButton.UseVisualStyleBackColor = true;
        convertButton.Click += ConvertButton_Click;
        buttonsPanel.Controls.Add(convertButton);
        // 
        // MainForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(684, 241);
        Controls.Add(tableLayout);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MainForm";
        Padding = new System.Windows.Forms.Padding(9);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Convertisseur PDF → TIFF";
        buttonsPanel.ResumeLayout(false);
        buttonsPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dpiUpDown).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openPdfDialog;
    private System.Windows.Forms.SaveFileDialog saveTiffDialog;
    private System.Windows.Forms.TableLayoutPanel tableLayout;
    private System.Windows.Forms.Label inputLabel;
    private System.Windows.Forms.TextBox inputTextBox;
    private System.Windows.Forms.Button browseInputButton;
    private System.Windows.Forms.Label outputLabel;
    private System.Windows.Forms.TextBox outputTextBox;
    private System.Windows.Forms.Button browseOutputButton;
    private System.Windows.Forms.Label colorLabel;
    private System.Windows.Forms.ComboBox colorCombo;
    private System.Windows.Forms.Label compressionLabel;
    private System.Windows.Forms.ComboBox compressionCombo;
    private System.Windows.Forms.Label dpiLabel;
    private System.Windows.Forms.NumericUpDown dpiUpDown;
    private System.Windows.Forms.Label statusLabel;
    private System.Windows.Forms.FlowLayoutPanel buttonsPanel;
    private System.Windows.Forms.Button convertButton;
}
