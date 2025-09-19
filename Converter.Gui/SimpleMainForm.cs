using System;
using System.Drawing;
using System.Windows.Forms;

namespace Converter.Gui;

public partial class SimpleMainForm : Form
{
    public SimpleMainForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        
        // Configure form
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(800, 600);
        this.Text = "Test Color File Converter";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        // Add a test label
        var testLabel = new Label();
        testLabel.Text = "INTERFACE TEST - Ceci devrait être visible!";
        testLabel.Font = new Font("Arial", 16, FontStyle.Bold);
        testLabel.ForeColor = Color.Red;
        testLabel.BackColor = Color.Yellow;
        testLabel.AutoSize = true;
        testLabel.Location = new Point(50, 50);
        this.Controls.Add(testLabel);

        // Add a test button
        var testButton = new Button();
        testButton.Text = "Bouton Test";
        testButton.Size = new Size(120, 40);
        testButton.Location = new Point(50, 100);
        testButton.BackColor = Color.LightBlue;
        testButton.Click += (s, e) => MessageBox.Show("Le bouton fonctionne!");
        this.Controls.Add(testButton);

        // Add scrollable content
        var scrollPanel = new Panel();
        scrollPanel.AutoScroll = true;
        scrollPanel.BackColor = Color.LightGreen;
        scrollPanel.BorderStyle = BorderStyle.FixedSingle;
        scrollPanel.Location = new Point(50, 160);
        scrollPanel.Size = new Size(300, 200);
        this.Controls.Add(scrollPanel);

        // Add content to scroll panel
        for (int i = 0; i < 20; i++)
        {
            var label = new Label();
            label.Text = $"Ligne scrollable {i + 1}";
            label.Location = new Point(10, i * 25);
            label.AutoSize = true;
            label.BackColor = Color.White;
            scrollPanel.Controls.Add(label);
        }

        this.ResumeLayout(false);
    }
}