using System;
using System.Drawing;
using System.Windows.Forms;

namespace Converter.Gui;

public partial class TestForm : Form
{
    public TestForm()
    {
        InitializeComponent();
        CreateControls();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 600);
        this.Name = "TestForm";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Test Interface - Diagnostic";
        this.BackColor = Color.LightBlue;
        
        this.ResumeLayout(false);
    }

    private void CreateControls()
    {
        // Ajouter un gros label visible
        var bigLabel = new Label
        {
            Text = "INTERFACE DE TEST - VISIBLE ?",
            Font = new Font("Arial", 24, FontStyle.Bold),
            ForeColor = Color.Red,
            BackColor = Color.Yellow,
            Location = new Point(50, 50),
            Size = new Size(600, 100),
            TextAlign = ContentAlignment.MiddleCenter
        };
        this.Controls.Add(bigLabel);

        // Ajouter un bouton de test
        var testButton = new Button
        {
            Text = "CLIQUEZ-MOI POUR TEST",
            Font = new Font("Arial", 16, FontStyle.Bold),
            Size = new Size(300, 80),
            Location = new Point(200, 200),
            BackColor = Color.Green,
            ForeColor = Color.White
        };
        testButton.Click += (s, e) => MessageBox.Show("Interface fonctionne !", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Controls.Add(testButton);

        // Ajouter un panel avec scrolling
        var scrollPanel = new Panel
        {
            AutoScroll = true,
            BackColor = Color.LightGray,
            BorderStyle = BorderStyle.FixedSingle,
            Location = new Point(50, 320),
            Size = new Size(400, 200)
        };
        
        // Ajouter du contenu au panel
        for (int i = 0; i < 30; i++)
        {
            var itemLabel = new Label
            {
                Text = $"Element scrollable {i + 1}",
                Location = new Point(10, i * 30),
                Size = new Size(200, 25),
                BackColor = i % 2 == 0 ? Color.White : Color.LightYellow
            };
            scrollPanel.Controls.Add(itemLabel);
        }
        
        this.Controls.Add(scrollPanel);
    }
}