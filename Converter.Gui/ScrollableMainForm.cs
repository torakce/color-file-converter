using System.Windows.Forms;

namespace Converter.Gui
{
    public class ScrollableMainForm : Form
    {
        private TableLayoutPanel mainLayout;
        private Panel leftScrollPanel;
        private TableLayoutPanel leftLayout;

        public ScrollableMainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Configuration de la fenêtre principale
            this.Size = new System.Drawing.Size(1284, 782);
            this.MinimumSize = new System.Drawing.Size(1100, 720);
            this.Text = "Convertisseur PDF en TIFF";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(9);

            // Panel principal avec scroll
            var mainScrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(9)
            };

            // Layout principal
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Panel de gauche avec scroll
            leftScrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(12)
            };

            // Layout gauche
            leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 4
            };
            leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Construction de la hiérarchie
            mainScrollPanel.Controls.Add(mainLayout);
            leftScrollPanel.Controls.Add(leftLayout);
            mainLayout.Controls.Add(leftScrollPanel, 0, 0);

            // Ajout à la fenêtre
            this.Controls.Add(mainScrollPanel);
        }
    }
}