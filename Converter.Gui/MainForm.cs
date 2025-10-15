using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Converter.Core;
using Converter.Gui.Controls;

namespace Converter.Gui;

public partial class MainForm : Form
{
    // Windows API imports pour gestion intelligente des fenêtres
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
    
    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsDelegate enumFunc, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    
    private delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lParam);
    private const int SW_RESTORE = 9;

    private readonly BatchConversionService _conversionService = new();
    private readonly string _logFilePath;
    private readonly Logger _logger;
    private readonly List<string> _files = new();
    private CancellationTokenSource? _conversionCts;
    private bool _isConverting;

    // Contrôles de l'interface
    private ListBox _filesList;
    private Label _dropInstructionLabel;
    // _colorModeCombo supprimé - interface simplifiée sans profils
    private TextBox _outputTextBox;
    private CheckBox _monoPagesCheckBox;
    private Button _convertButton;
    private Button _stopButton;
    private WatchOptionsButton _watchButton;
    private Label _watchStatusLabel;
    private ProgressBar _progressBar;
    private Label _statusLabel;
    private ConversionProgressPanel? _conversionProgressPanel;
    
    // Contrôles d'aperçu
    private Panel _previewPanel;
    private PictureBox _previewPictureBox;
    private Label _previewStatusLabel;
    private TrackBar _zoomTrackBar;
    private Label _zoomLabel;
    private Button _refreshPreviewButton;
    private Label _fileInfoLabel;
    
    // Contrôles de paramètres
    private ComboBox _resolutionCombo;
    private ComboBox _compressionCombo;
    private ComboBox _bitDepthCombo;
    private Label _validationWarningLabel;
    
    // Aperçu optimisé avec cache
    private CancellationTokenSource? _previewCts;
    private Image? _currentPreviewImage;
    private string? _lastPreviewTempFile;
    private string? _lastPreviewParameters; // Cache des derniers paramètres
    private string? _lastPreviewFilePath;   // Cache du dernier fichier
    
    // Cache des pages converties pour éviter les re-conversions
    private Dictionary<string, List<Image>>? _previewPagesCache; // Key: "filepath_params"
    private const int MaxCacheEntries = 3; // Limiter la taille du cache
    
    // Zoom et pan avancés
    private float _zoomFactor = 1.0f;
    private PointF _panOffset = PointF.Empty;
    private bool _isPanning = false;
    private Point _lastPanPoint = Point.Empty;
    
    // Navigation par pages
    private List<Image>? _allPreviewPages;
    private int _currentPageIndex = 0;
    private int _totalPages = 0;
    private Button _previousPageButton;
    private Button _nextPageButton;
    private Label _pageInfoLabel;
    private ThumbnailStripPanel? _thumbnailStrip;
    


    public MainForm()
    {
        // Initialiser le logger
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ColorFileConverter");
        Directory.CreateDirectory(appDataPath);
        _logFilePath = Path.Combine(appDataPath, "converter.log");
        _logger = new Logger(_logFilePath, LogLevel.Info);
        
        _logger.LogInfo("Application", "Démarrage de Color File Converter", "Version GUI v2.1 - Ghostscript intégré");

        // Initialiser le cache de prévisualisation
        _previewPagesCache = new Dictionary<string, List<Image>>();

        InitializeComponent();
        CreateInterface();
        InitializeData();
        
        // Timer pour mettre à jour le statut de surveillance
        var watchTimer = new System.Windows.Forms.Timer();
        watchTimer.Interval = 2000; // Vérifier toutes les 2 secondes
        watchTimer.Tick += (s, e) => UpdateWatchStatus();
        watchTimer.Start();
        
        _logger.LogInfo("Interface", "Interface utilisateur initialisée avec succès");
    }



    private void CreateInterface()
    {
        // Panel principal avec scrolling
        var mainScrollPanel = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            BackColor = SystemColors.Control
        };
        this.Controls.Add(mainScrollPanel);

        // Layout principal
        var mainLayout = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Dock = DockStyle.Fill,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };
        
        // Configuration des colonnes : 60% pour les contrôles, 40% pour l'aperçu
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        
        // Configuration des lignes : 90% pour le contenu principal, 10% pour les actions
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));

        mainScrollPanel.Controls.Add(mainLayout);

        // Panel gauche avec les contrôles
        var leftPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(10)
        };
        mainLayout.Controls.Add(leftPanel, 0, 0);

        // Panel droit pour l'aperçu
        var previewPanel = CreatePreviewSection(mainLayout);
        mainLayout.Controls.Add(previewPanel, 1, 0);

        // Panel pour les boutons d'actions
        var actionsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        
        // Layout principal pour les actions
        var actionsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 2
        };
        actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));   // Convertir
        actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));   // Arrêter
        actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));   // Surveillance
        actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));   // Logs
        actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Reste
        actionsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // Boutons
        actionsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // Statut et progress
        
        actionsPanel.Controls.Add(actionsLayout);
        
        _convertButton = new Button
        {
            Text = "Convertir",
            Size = new Size(100, 35),
            BackColor = Color.LightGreen,
            Font = new Font("Arial", 10, FontStyle.Bold),
            UseVisualStyleBackColor = false,
            Margin = new Padding(3)
        };
        _convertButton.Click += ConvertButton_Click;
        actionsLayout.Controls.Add(_convertButton, 0, 0);

        _stopButton = new Button
        {
            Text = "Arrêter",
            Size = new Size(80, 35),
            BackColor = Color.LightCoral,
            Font = new Font("Arial", 10, FontStyle.Bold),
            UseVisualStyleBackColor = false,
            Enabled = false,
            Margin = new Padding(3)
        };
        _stopButton.Click += StopButton_Click;
        actionsLayout.Controls.Add(_stopButton, 1, 0);

        // Bouton de surveillance de dossiers
        _watchButton = new WatchOptionsButton()
        {
            Size = new Size(120, 35),
            BackColor = Color.LightBlue,
            Font = new Font("Arial", 9, FontStyle.Regular),
            UseVisualStyleBackColor = false,
            Margin = new Padding(3)
        };
        actionsLayout.Controls.Add(_watchButton, 2, 0);

        // Bouton pour voir les logs
        var logsButton = new Button
        {
            Text = "📋 Logs",
            Size = new Size(70, 35),
            BackColor = Color.LightGray,
            Font = new Font("Arial", 8, FontStyle.Regular),
            UseVisualStyleBackColor = false,
            Margin = new Padding(3)
        };
        logsButton.Click += LogsButton_Click;
        actionsLayout.Controls.Add(logsButton, 3, 0);

        // Panel pour statut et informations (deuxième ligne)
        var statusPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 25,
            Margin = new Padding(3)
        };
        
        // Label de statut de surveillance
        _watchStatusLabel = new Label
        {
            Text = "Surveillance inactive",
            Location = new Point(0, 0),
            Size = new Size(200, 20),
            ForeColor = Color.Gray,
            Font = new Font("Arial", 8, FontStyle.Italic),
            TextAlign = ContentAlignment.MiddleLeft
        };
        statusPanel.Controls.Add(_watchStatusLabel);

        // Barre de progression
        _progressBar = new ProgressBar
        {
            Location = new Point(210, 0),
            Size = new Size(300, 20),
            Visible = false
        };
        statusPanel.Controls.Add(_progressBar);

        // Label de statut
        _statusLabel = new Label
        {
            Text = "Prêt",
            Location = new Point(520, 0),
            AutoSize = true,
            Font = new Font("Arial", 8, FontStyle.Regular)
        };
        statusPanel.Controls.Add(_statusLabel);
        
        actionsLayout.Controls.Add(statusPanel, 0, 1);
        actionsLayout.SetColumnSpan(statusPanel, 5);

        mainLayout.Controls.Add(actionsPanel, 0, 1);
        mainLayout.SetColumnSpan(actionsPanel, 2); // Étendre sur les 2 colonnes

        // Utiliser un TableLayoutPanel pour organiser les sections dans le panel gauche
        var leftLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(5)
        };
        
        // Configuration des lignes : 35% fichiers, 40% profils, 25% sortie
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        
        leftPanel.Controls.Add(leftLayout);

        // Créer les sections dans le layout gauche
        CreateFileSection(leftLayout, 0);
        CreatePresetsSection(leftLayout, 1);
        CreateOutputSection(leftLayout, 2);
    }

    private void CreateFileSection(TableLayoutPanel parent, int row)
    {
        var filesGroup = new GroupBox
        {
            Text = "📂 Fichiers PDF",
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        parent.Controls.Add(filesGroup, 0, row);

        // Utiliser un TableLayoutPanel pour organiser le contenu des fichiers
        var filesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(5)
        };
        filesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 75F)); // Zone de drop
        filesLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Label d'instruction
        filesLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Boutons
        filesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        
        filesGroup.Controls.Add(filesLayout);

        // Créer le DropZonePanel pour un meilleur feedback visuel
        var dropZone = new DropZonePanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(5)
        };
        
        // ListBox à l'intérieur du DropZonePanel
        _filesList = new ListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9)
        };
        _filesList.SelectedIndexChanged += (s, e) =>
        {
            UpdateActions();
            // Déclencher l'aperçu automatiquement lors de la sélection
            RefreshPreview();
        };
        UpdateFilesList();

        dropZone.Controls.Add(_filesList);
        
        // Événements de drag & drop sur le DropZonePanel
        dropZone.DragEnter += FilesList_DragEnter;
        dropZone.DragDrop += FilesList_DragDrop;
        dropZone.DragOver += FilesGroup_DragOver;
        dropZone.DragLeave += FilesGroup_DragLeave;
        
        // Également sur la ListBox pour compatibilité
        _filesList.DragEnter += FilesList_DragEnter;
        _filesList.DragDrop += FilesList_DragDrop;

        filesLayout.Controls.Add(dropZone, 0, 0);

        // Label d'instruction pour le drag & drop (visible quand la liste est vide)
        _dropInstructionLabel = new Label
        {
            Text = "📁 Glissez vos fichiers PDF ici\nou utilisez le bouton Charger",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(100, 100, 100),
            Font = new Font("Segoe UI", 10, FontStyle.Italic),
            Visible = _files.Count == 0,
            Margin = new Padding(5)
        };
        
        // Ajouter le label au DropZonePanel pour qu'il soit visible
        dropZone.Controls.Add(_dropInstructionLabel);
        _dropInstructionLabel.BringToFront();

        // Panel pour les boutons d'action des fichiers
        var buttonsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 35,
            Margin = new Padding(5)
        };
        
        var loadButton = new Button
        {
            Text = "Charger",
            Size = new Size(80, 30),
            Location = new Point(0, 0),
            BackColor = SystemColors.ButtonFace
        };
        loadButton.Click += LoadButton_Click;
        buttonsPanel.Controls.Add(loadButton);

        var removeButton = new Button
        {
            Text = "Retirer",
            Size = new Size(80, 30),
            Location = new Point(90, 0),
            BackColor = SystemColors.ButtonFace
        };
        removeButton.Click += RemoveButton_Click;
        buttonsPanel.Controls.Add(removeButton);

        var clearButton = new Button
        {
            Text = "Vider tout",
            Size = new Size(80, 30),
            Location = new Point(180, 0),
            BackColor = SystemColors.ButtonFace
        };
        clearButton.Click += ClearButton_Click;
        buttonsPanel.Controls.Add(clearButton);
        
        filesLayout.Controls.Add(buttonsPanel, 0, 2);

        parent.Controls.Add(filesGroup);
    }

    private void CreateParametersSection(TableLayoutPanel parent, int row)
    {
        var paramsGroup = new GroupBox
        {
            Text = "Paramètres de conversion",
            Dock = DockStyle.Fill,
            Margin = new Padding(5)
        };
        parent.Controls.Add(paramsGroup, 0, row);

        // Layout pour les 3 paramètres essentiels
        var paramsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Margin = new Padding(5),
            Padding = new Padding(0)
        };
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Résolution
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F)); // Résolution
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Compression
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F)); // Compression
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Couleurs
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F)); // Couleurs
        paramsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        
        paramsGroup.Controls.Add(paramsLayout);

        // Résolution (DPI)
        var resLabel = new Label { Text = "DPI:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(3) };
        paramsLayout.Controls.Add(resLabel, 0, 0);
        
        _resolutionCombo = new ComboBox
        {
            Name = "resolutionCombo",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDown,
            Margin = new Padding(3)
        };
        _resolutionCombo.Items.AddRange(new[] { "72", "96", "150", "200", "300", "400", "600" });
        _resolutionCombo.Text = "300";
        _resolutionCombo.TextChanged += ParameterChanged;
        _resolutionCombo.SelectedIndexChanged += ParameterChanged;
        paramsLayout.Controls.Add(_resolutionCombo, 1, 0);
        
        var toolTip1 = new ToolTip();
        toolTip1.SetToolTip(_resolutionCombo, "Résolution en DPI (points par pouce). Plus élevé = meilleure qualité mais fichiers plus gros.");

        // Compression TIFF
        var compressionLabel = new Label { Text = "Compression:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(3) };
        paramsLayout.Controls.Add(compressionLabel, 2, 0);
        
        _compressionCombo = new ComboBox
        {
            Name = "compressionCombo",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        
        _compressionCombo.Items.AddRange(new[] { 
            "Aucune",
            "LZW",
            "ZIP", 
            "PackBits",
            "G3 (N&B)",
            "G4 (N&B)",
            "JPEG"
        });
        _compressionCombo.SelectedIndex = 1; // LZW par défaut
        _compressionCombo.SelectedIndexChanged += ParameterChanged;
        paramsLayout.Controls.Add(_compressionCombo, 3, 0);
        
        var toolTip2 = new ToolTip();
        toolTip2.SetToolTip(_compressionCombo, "Type de compression TIFF. LZW recommandé pour l'équilibre qualité/taille.");

        // Type de couleurs
        var bitDepthLabel = new Label { Text = "Couleurs:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(3) };
        paramsLayout.Controls.Add(bitDepthLabel, 4, 0);
        
        _bitDepthCombo = new ComboBox
        {
            Name = "bitDepthCombo",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        _bitDepthCombo.Items.AddRange(new[] { "N&B", "Gris", "Couleurs" });
        _bitDepthCombo.SelectedIndex = 2; // Couleurs par défaut
        _bitDepthCombo.SelectedIndexChanged += ParameterChanged;
        paramsLayout.Controls.Add(_bitDepthCombo, 5, 0);
        
        var toolTip3 = new ToolTip();
        toolTip3.SetToolTip(_bitDepthCombo, "Type de couleurs. N&B permet G3/G4, Couleurs pour documents complexes.");

        // Validation des paramètres (pour afficher les avertissements)
        _validationWarningLabel = new Label
        {
            Text = "",
            ForeColor = Color.Orange,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(5, 0, 5, 0),
            Visible = false
        };
        // Ajouter une deuxième ligne au layout pour les avertissements
        paramsLayout.RowCount = 2;
        paramsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        
        paramsLayout.Controls.Add(_validationWarningLabel, 0, 1);
        paramsLayout.SetColumnSpan(_validationWarningLabel, 6);

        // Ajouter les validations pour les compressions incompatibles
        _compressionCombo.SelectedIndexChanged += ValidateCompressionCompatibility;
        _bitDepthCombo.SelectedIndexChanged += OnBitDepthChanged;
        UpdateCompressionOptions();
    }

    // Structure pour les paramètres de conversion
    public class ConversionParameters
    {
        public int Resolution { get; set; } = 300;
        public string Compression { get; set; } = "LZW (sans perte)";
        public string BitDepth { get; set; } = "Couleurs (24 bits)";
        public string Smoothing { get; set; } = "Normal (4x)";
        public bool MonoPages { get; set; } = false;
    }

    private void ParameterChanged(object? sender, EventArgs e)
    {
        // Quand l'utilisateur modifie manuellement un paramètre,
        // créer un profil personnalisé temporaire et régénérer l'aperçu
        CreateCustomProfile();
        
        // Régénérer l'aperçu avec les nouveaux paramètres
        RefreshPreview();
    }
    
    private void OnBitDepthChanged(object? sender, EventArgs e)
    {
        UpdateCompressionOptions();
        ValidateCompressionCompatibility(sender, e);
    }

    private void UpdateCompressionOptions()
    {
        if (_bitDepthCombo == null || _compressionCombo == null) return;

        var selectedBitDepth = _bitDepthCombo.SelectedItem?.ToString();
        var currentCompression = _compressionCombo.SelectedItem?.ToString();

        // Sauvegarder la sélection actuelle si possible
        _compressionCombo.SelectedIndexChanged -= ValidateCompressionCompatibility;

        try
        {
            _compressionCombo.Items.Clear();

            if (selectedBitDepth == "N&B")
            {
                // Pour le noir et blanc, seules certaines compressions sont appropriées
                _compressionCombo.Items.AddRange(new[] { 
                    "Aucune",
                    "G3 (N&B)",
                    "G4 (N&B)",
                    "LZW",
                    "ZIP", 
                    "PackBits"
                });
                
                // Sélectionner G4 par défaut pour le noir et blanc
                if (_compressionCombo.Items.Contains("G4 (N&B)"))
                    _compressionCombo.SelectedItem = "G4 (N&B)";
                else
                    _compressionCombo.SelectedIndex = 0;
            }
            else if (selectedBitDepth == "Gris")
            {
                // Pour les niveaux de gris
                _compressionCombo.Items.AddRange(new[] { 
                    "Aucune",
                    "LZW",
                    "ZIP", 
                    "PackBits",
                    "JPEG"
                });
                
                // Essayer de conserver la sélection précédente si compatible
                if (currentCompression != null && _compressionCombo.Items.Contains(currentCompression))
                    _compressionCombo.SelectedItem = currentCompression;
                else if (_compressionCombo.Items.Contains("LZW"))
                    _compressionCombo.SelectedItem = "LZW";
                else
                    _compressionCombo.SelectedIndex = 0;
            }
            else if (selectedBitDepth == "Couleurs")
            {
                // Pour les couleurs
                _compressionCombo.Items.AddRange(new[] { 
                    "Aucune",
                    "LZW",
                    "ZIP", 
                    "PackBits",
                    "JPEG"
                });
                
                // Essayer de conserver la sélection précédente si compatible
                if (currentCompression != null && _compressionCombo.Items.Contains(currentCompression))
                    _compressionCombo.SelectedItem = currentCompression;
                else if (_compressionCombo.Items.Contains("LZW"))
                    _compressionCombo.SelectedItem = "LZW";
                else
                    _compressionCombo.SelectedIndex = 0;
            }
        }
        finally
        {
            _compressionCombo.SelectedIndexChanged += ValidateCompressionCompatibility;
        }
    }

    private void ValidateCompressionCompatibility(object? sender, EventArgs e)
    {
        var parameters = GetCurrentConversionParameters();
        
        // Réinitialiser l'avertissement
        _validationWarningLabel.Visible = false;
        _validationWarningLabel.Text = "";
        
        // Vérifier les incompatibilités et afficher des avertissements discrets
        if ((parameters.Compression.Contains("G3") || parameters.Compression.Contains("G4")) 
            && !parameters.BitDepth.Contains("N&B"))
        {
            // Afficher un avertissement visuel sans bloquer
            _validationWarningLabel.Text = "⚠️ G3/G4 nécessite N&B. Correction automatique appliquée.";
            _validationWarningLabel.Visible = true;
            
            // Changer automatiquement vers N&B (sans MessageBox intrusif)
            SetBitDepth("N&B");
        }
        else if (parameters.Compression.Contains("JPEG") && parameters.BitDepth.Contains("N&B"))
        {
            // Nouveau : Avertissement pour JPEG avec monochrome
            _validationWarningLabel.Text = "💡 Conseil: JPEG fonctionne mieux avec Gris ou Couleurs.";
            _validationWarningLabel.ForeColor = Color.DarkBlue;
            _validationWarningLabel.Visible = true;
        }
        else if (parameters.Resolution < 150 && parameters.Compression.Contains("G4"))
        {
            // Nouveau : Avertissement pour résolution faible avec G4
            _validationWarningLabel.Text = "💡 G4 + faible résolution peut créer des fichiers plus volumineux que LZW.";
            _validationWarningLabel.ForeColor = Color.DarkBlue;
            _validationWarningLabel.Visible = true;
        }
        else
        {
            // Réinitialiser la couleur par défaut
            _validationWarningLabel.ForeColor = Color.Orange;
        }
        
        // Appeler le gestionnaire de changement de paramètre
        ParameterChanged(sender, e);
    }

    private void CreateCustomProfile()
    {
        // Récupérer les valeurs des contrôles de paramètres
        var profileGroup = this.Controls.OfType<Panel>().FirstOrDefault()?.Controls.OfType<TableLayoutPanel>().FirstOrDefault()?.Controls[1];
        if (profileGroup is GroupBox group)
        {
            var paramsLayout = group.Controls.OfType<TableLayoutPanel>().LastOrDefault();
            if (paramsLayout != null)
            {
                // Utilisation des références directes aux contrôles
                if (_resolutionCombo != null && _compressionCombo != null && _bitDepthCombo != null)
                {
                    // Obtenir les paramètres de conversion actuels
                    var parameters = GetCurrentConversionParameters();
                    
                    // Créer un device basé sur les paramètres
                    var device = CreateDeviceFromParameters(parameters);
                    var dpi = parameters.Resolution;
                    var compression = GetCompressionFromDisplay(parameters.Compression);

                    // Créer le profil personnalisé
                    var customProfile = new ConversionProfile(
                        "Personnalisé",
                        device,
                        compression,
                        dpi,
                        Array.Empty<string>()
                    );

                    // La gestion personnalisée des profils a été supprimée 
                    // Les paramètres sont maintenant automatiques selon le mode de couleur
                }
            }
        }
    }

    private string CreateDeviceFromParameters(string format, string colorMode)
    {
        return (format.ToLower(), colorMode.ToLower()) switch
        {
            ("tiff", "mono") => "tiffg4",
            ("tiff", "gray") => "tiffgray",
            ("tiff", "rgb") => "tiff24nc",
            ("png", _) => "png16m",
            ("jpeg", _) => "jpeg",
            ("bmp", _) => "bmp16m",
            _ => "tiff24nc"
        };
    }

    private string? GetCompressionFromQuality(string quality, string format)
    {
        if (format.ToLower() == "tiff")
        {
            return int.TryParse(quality, out var q) && q >= 95 ? "lzw" : null;
        }
        return null;
    }

    private ConversionParameters GetCurrentConversionParameters()
    {
        var parameters = new ConversionParameters();
        
        // Utiliser les références directes aux contrôles
        if (_resolutionCombo != null && int.TryParse(_resolutionCombo.Text, out var resolution))
            parameters.Resolution = resolution;
        
        if (_compressionCombo?.SelectedItem != null)
            parameters.Compression = _compressionCombo.SelectedItem.ToString() ?? "LZW (sans perte)";
        
        if (_bitDepthCombo?.SelectedItem != null)
            parameters.BitDepth = _bitDepthCombo.SelectedItem.ToString() ?? "Couleurs (24 bits)";
        
        // Lissage fixe à valeur par défaut optimale
        parameters.Smoothing = "Normal (4x)";
        
        // Option mono-pages
        if (_monoPagesCheckBox != null)
            parameters.MonoPages = _monoPagesCheckBox.Checked;
        
        return parameters;
    }

    private void SetBitDepth(string bitDepth)
    {
        if (_bitDepthCombo != null)
        {
            _bitDepthCombo.SelectedItem = bitDepth;
        }
    }

    private string CreateDeviceFromParameters(ConversionParameters parameters)
    {
        // Convertir les paramètres en device GhostScript approprié
        return parameters.BitDepth switch
        {
            "N&B" => "tiffg4", // Toujours G4 pour le monochrome
            "Gris" => "tiffgray",
            "Couleurs" => "tiff24nc",
            _ => "tiff24nc"
        };
    }

    private string GetCompressionFromDisplay(string compressionDisplay)
    {
        return compressionDisplay switch
        {
            "Aucune" => "none",
            "LZW" => "lzw",
            "ZIP" => "lzw", // ZIP n'est pas supporté par TIFF, utiliser LZW à la place
            "PackBits" => "pack", // Ghostscript utilise "pack" et non "packbits"
            "G3 (N&B)" => "g3",
            "G4 (N&B)" => "g4",
            "JPEG" => "none", // JPEG compression n'est pas supportée pour TIFF, utiliser none
            _ => "lzw"
        };
    }

    // Méthode publique pour obtenir les paramètres de conversion actuels (basée sur les contrôles)
    public ConversionProfile GetActiveConversionProfile()
    {
        // Lire directement les valeurs des contrôles
        var dpi = int.TryParse(_resolutionCombo.Text, out var parsedDpi) ? parsedDpi : 300;
        var compression = GetCompressionFromDisplay(_compressionCombo.SelectedItem?.ToString() ?? "LZW");
        
        // Déterminer le device selon le type de couleur sélectionné
        var device = _bitDepthCombo.SelectedIndex switch
        {
            0 => "tiffg4",      // N&B
            1 => "tiffgray",    // Gris 
            2 => "tiff24nc",    // Couleurs
            _ => "tiff24nc"
        };
        
        var profileName = _bitDepthCombo.SelectedItem?.ToString() ?? "Couleurs";
        
        return new ConversionProfile(profileName, device, compression, dpi, Array.Empty<string>());
    }

    private void OpenExplorerIntelligently(string folderPath)
    {
        try
        {
            _logger.LogInfo("UI", "Recherche d'une fenêtre explorateur existante", folderPath);
            
            // D'abord, vérifier si le dossier est déjà ouvert dans une fenêtre existante
            var existingWindow = FindExplorerWindowForFolder(folderPath);
            if (existingWindow != IntPtr.Zero)
            {
                _logger.LogInfo("UI", "Fenêtre existante trouvée, mise au premier plan");
                ShowWindow(existingWindow, SW_RESTORE);
                SetForegroundWindow(existingWindow);
                return;
            }
            
            _logger.LogInfo("UI", "Aucune fenêtre existante, ouverture d'une nouvelle instance", folderPath);
            
            // Aucune fenêtre existante trouvée, ouvrir une nouvelle instance
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true
            };
            
            var process = Process.Start(startInfo);
            
            if (process != null)
            {
                _logger.LogInfo("UI", "Nouvelle instance d'explorateur lancée", $"PID: {process.Id}");
                
                // Attendre un peu que l'explorateur se lance
                Thread.Sleep(300);
                
                // Tenter de mettre la nouvelle fenêtre au premier plan
                try
                {
                    if (!process.HasExited)
                    {
                        // Chercher la nouvelle fenêtre d'explorateur
                        var newWindow = FindExplorerWindowForFolder(folderPath);
                        if (newWindow != IntPtr.Zero)
                        {
                            ShowWindow(newWindow, SW_RESTORE);
                            SetForegroundWindow(newWindow);
                            _logger.LogInfo("UI", "Nouvelle fenêtre explorateur mise au premier plan");
                        }
                        else
                        {
                            _logger.LogWarning("UI", "Impossible de trouver la nouvelle fenêtre d'explorateur");
                        }
                    }
                }
                catch (Exception focusEx)
                {
                    _logger.LogWarning("UI", "Impossible de mettre la nouvelle fenêtre au premier plan", focusEx.Message);
                }
            }
            else
            {
                _logger.LogError("UI", "Échec du lancement du processus explorateur");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("UI", "Erreur lors de l'ouverture intelligente de l'explorateur", ex, folderPath);
        }
    }

    private IntPtr FindExplorerWindowForFolder(string targetPath)
    {
        var foundWindow = IntPtr.Zero;
        var normalizedTargetPath = Path.GetFullPath(targetPath).TrimEnd('\\').ToLowerInvariant();
        
        _logger.LogInfo("UI", "Recherche de fenêtre pour le chemin", normalizedTargetPath);

        try
        {
            EnumWindows((hWnd, lParam) =>
            {
                try
                {
                    // Vérifier si c'est une fenêtre d'explorateur
                    GetWindowThreadProcessId(hWnd, out uint processId);
                    var process = Process.GetProcessById((int)processId);
                    
                    if (process.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                    {
                        // Obtenir le titre de la fenêtre
                        var length = GetWindowTextLength(hWnd);
                        if (length > 0)
                        {
                            var title = new System.Text.StringBuilder(length + 1);
                            GetWindowText(hWnd, title, title.Capacity);
                            var windowTitle = title.ToString();
                            
                            _logger.LogInfo("UI", "Fenêtre explorateur trouvée", $"Titre: {windowTitle}");
                            
                            // Vérifier si le titre contient le nom du dossier ou le chemin
                            var folderName = Path.GetFileName(normalizedTargetPath);
                            if (!string.IsNullOrEmpty(folderName))
                            {
                                if (windowTitle.ToLowerInvariant().Contains(folderName.ToLowerInvariant()) ||
                                    windowTitle.ToLowerInvariant().Contains(normalizedTargetPath))
                                {
                                    _logger.LogInfo("UI", "Correspondance trouvée", $"Fenêtre: {windowTitle} pour {normalizedTargetPath}");
                                    foundWindow = hWnd;
                                    return false; // Arrêter l'énumération
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("UI", "Erreur lors de l'analyse d'une fenêtre", ex.Message);
                }
                
                return true; // Continuer l'énumération
            }, IntPtr.Zero);
        }
        catch (Exception ex)
        {
            _logger.LogError("UI", "Erreur lors de l'énumération des fenêtres", ex);
        }

        return foundWindow;
    }

    private void CreateOutputSection(TableLayoutPanel parent, int row)
    {
        var outputGroup = new GroupBox
        {
            Text = "Dossier de sortie",
            Dock = DockStyle.Fill,
            Margin = new Padding(5)
        };
        parent.Controls.Add(outputGroup, 0, row);

        // Utiliser un TableLayoutPanel pour organiser les contrôles
        var outputLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Margin = new Padding(5)
        };
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F)); // TextBox/CheckBox
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // Button
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // Label
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // TextBox + Button
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // Progress Panel
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // CheckBoxes
        
        outputGroup.Controls.Add(outputLayout);

        var outputLabel = new Label
        {
            Text = "Dossier de sortie:",
            AutoSize = true,
            Margin = new Padding(3)
        };
        outputLayout.Controls.Add(outputLabel, 0, 0);
        outputLayout.SetColumnSpan(outputLabel, 2);

        _outputTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Margin = new Padding(3)
        };
        outputLayout.Controls.Add(_outputTextBox, 0, 1);

        var browseButton = new Button
        {
            Text = "Parcourir...",
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };
        browseButton.Click += BrowseButton_Click;
        outputLayout.Controls.Add(browseButton, 1, 1);

        // Panel de progression détaillée
        _conversionProgressPanel = new ConversionProgressPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            Visible = false // Masqué par défaut, visible pendant conversion
        };
        outputLayout.Controls.Add(_conversionProgressPanel, 0, 2);
        outputLayout.SetColumnSpan(_conversionProgressPanel, 2);

        // Panel pour les checkboxes
        var checkBoxPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };

        _monoPagesCheckBox = new CheckBox
        {
            Text = "Créer des fichiers séparés par page (mono-pages)",
            AutoSize = true,
            Location = new Point(0, 0),
            Checked = false
        };
        _monoPagesCheckBox.CheckedChanged += ParameterChanged;
        checkBoxPanel.Controls.Add(_monoPagesCheckBox);

        // Tooltip pour l'option mono-pages
        var toolTip4 = new ToolTip();
        toolTip4.SetToolTip(_monoPagesCheckBox,
            "Fichiers mono-pages\n" +
            "• Coché: Crée un fichier TIFF séparé pour chaque page\n" +
            "  Exemple: document.pdf → document_page1.tiff, document_page2.tiff...\n" +
            "• Décoché: Crée un seul fichier TIFF multi-pages\n" +
            "  Exemple: document.pdf → document.tiff (toutes les pages)\n" +
            "Utile pour traiter les pages individuellement");
        
        outputLayout.Controls.Add(checkBoxPanel, 0, 3);
        outputLayout.SetColumnSpan(checkBoxPanel, 2);

        // Performance supprimée - le RowCount reste à 3
    }



    private void InitializeData()
    {
        InitializeParameters();
        UpdateFilesList();
        UpdateActions();
    }

    private void InitializeParameters()
    {
        // Les paramètres sont déjà initialisés dans CreateParametersSection
        // avec des valeurs par défaut appropriées
    }

    private void UpdateFilesList(bool selectLast = false)
    {
        _filesList.Items.Clear();
        
        // Gérer la visibilité du label d'instruction
        if (_dropInstructionLabel != null)
        {
            _dropInstructionLabel.Visible = _files.Count == 0;
        }
        
        if (_files.Count > 0)
        {
            foreach (var file in _files)
            {
                _filesList.Items.Add(Path.GetFileName(file));
            }
            
            // Sélectionner le dernier fichier ajouté si demandé
            if (selectLast)
            {
                _filesList.SelectedIndex = _files.Count - 1;
            }
        }
    }

    private void UpdateActions()
    {
        bool hasFiles = _files.Count > 0;
        bool hasOutput = !string.IsNullOrWhiteSpace(_outputTextBox.Text);
        bool hasValidParameters = _bitDepthCombo.SelectedIndex >= 0 && _compressionCombo.SelectedIndex >= 0;

        _convertButton.Enabled = hasFiles && hasOutput && hasValidParameters && !_isConverting;
        _stopButton.Enabled = _isConverting;
    }

    // Event handlers pour drag & drop amélioré
    private void FilesList_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            // Vérifier si au moins un fichier PDF est présent
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                var pdfCount = files.Count(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
                if (pdfCount > 0)
                {
                    e.Effect = DragDropEffects.Copy;
                    
                    // Changer l'apparence du contrôle pour indiquer la zone de drop
                    if (sender is Control control)
                    {
                        control.BackColor = Color.LightGreen;
                    }
                    
                    // Mettre à jour le statut avec le nombre de fichiers
                    _statusLabel.Text = $"Prêt à ajouter {pdfCount} fichier(s) PDF...";
                    _statusLabel.ForeColor = Color.Green;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    _statusLabel.Text = "Seuls les fichiers PDF sont acceptés";
                    _statusLabel.ForeColor = Color.Orange;
                }
            }
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void FilesGroup_DragOver(object? sender, DragEventArgs e)
    {
        // Maintenir l'effet visuel pendant le survol
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void FilesGroup_DragLeave(object? sender, EventArgs e)
    {
        // Restaurer l'apparence normale quand on quitte la zone
        if (sender is Control control)
        {
            control.BackColor = SystemColors.Control;
        }
        _statusLabel.Text = "Glissez des fichiers PDF ici ou utilisez le bouton Charger";
        _statusLabel.ForeColor = Color.Black;
    }

    private void FilesList_DragDrop(object? sender, DragEventArgs e)
    {
        // Restaurer l'apparence normale
        if (sender is Control control)
        {
            control.BackColor = SystemColors.Window;
        }

        if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
        {
            var validFiles = new List<string>();
            var duplicateCount = 0;
            var errors = new List<string>();
            var invalidExtensions = new List<string>();

            foreach (var file in files)
            {
                // Vérifier l'extension d'abord
                if (!Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    invalidExtensions.Add(Path.GetFileName(file));
                    continue;
                }

                // Éviter les doublons
                if (_files.Contains(file))
                {
                    duplicateCount++;
                    continue;
                }

                // Valider le fichier
                var (isValid, errorMessage) = ValidateFile(file);
                if (isValid)
                {
                    validFiles.Add(file);
                }
                else
                {
                    errors.Add(errorMessage);
                }
            }

            // Ajouter les fichiers valides
            _files.AddRange(validFiles);
            UpdateFilesList(selectLast: validFiles.Count > 0); // Sélectionner le dernier si des fichiers ont été ajoutés
            UpdateActions();

            // Générer le message de feedback
            var messageParts = new List<string>();
            if (validFiles.Count > 0)
            {
                messageParts.Add($"{validFiles.Count} fichier(s) ajouté(s)");
            }
            if (duplicateCount > 0)
            {
                messageParts.Add($"{duplicateCount} doublon(s) ignoré(s)");
            }
            if (invalidExtensions.Count > 0)
            {
                messageParts.Add($"{invalidExtensions.Count} fichier(s) non-PDF ignoré(s)");
            }
            if (errors.Count > 0)
            {
                messageParts.Add($"{errors.Count} erreur(s) de validation");
            }

            // Afficher le feedback
            if (validFiles.Count > 0)
            {
                _statusLabel.Text = string.Join(", ", messageParts);
                _statusLabel.ForeColor = errors.Count > 0 ? Color.Orange : Color.Green;
                
                // Auto-sélectionner le dernier fichier ajouté pour l'aperçu
                if (_filesList.Items.Count > 0)
                {
                    _filesList.SelectedIndex = _filesList.Items.Count - 1;
                }
            }
            else
            {
                _statusLabel.Text = messageParts.Count > 0 ? string.Join(", ", messageParts) : "Aucun fichier valide";
                _statusLabel.ForeColor = Color.Orange;
            }

            // Afficher les erreurs détaillées si nécessaire
            if (errors.Count > 0)
            {
                var errorDetails = string.Join("\n\n", errors);
                if (invalidExtensions.Count > 0)
                {
                    errorDetails += $"\n\nFichiers non-PDF ignorés :\n• {string.Join("\n• ", invalidExtensions)}";
                }
                
                MessageBox.Show(this, $"Problèmes détectés :\n\n{errorDetails}", 
                    "Validation des fichiers", MessageBoxButtons.OK, 
                    validFiles.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
            }
        }
    }

    private (bool IsValid, string ErrorMessage) ValidateFile(string filePath)
    {
        _logger.LogDebug("Validation", $"Validation du fichier: {Path.GetFileName(filePath)}");
        
        try
        {
            // Vérifier si le fichier existe
            if (!File.Exists(filePath))
            {
                var error = $"Le fichier '{Path.GetFileName(filePath)}' n'existe pas ou n'est plus accessible.";
                _logger.LogWarning("Validation", "Fichier introuvable", filePath);
                return (false, error);
            }

            // Vérifier l'extension
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension != ".pdf")
            {
                var error = $"Le fichier '{Path.GetFileName(filePath)}' n'est pas un fichier PDF valide (extension: {extension}).";
                _logger.LogWarning("Validation", "Extension invalide", $"{Path.GetFileName(filePath)}: {extension}");
                return (false, error);
            }

            // Vérifier la taille du fichier
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                var error = $"Le fichier '{Path.GetFileName(filePath)}' est vide.";
                _logger.LogWarning("Validation", "Fichier vide", filePath);
                return (false, error);
            }

            // Vérifier que le fichier n'est pas trop volumineux (limite à 500 MB pour l'interface)
            const long maxSize = 500 * 1024 * 1024; // 500 MB
            if (fileInfo.Length > maxSize)
            {
                var error = $"Le fichier '{Path.GetFileName(filePath)}' est trop volumineux ({fileInfo.Length / (1024 * 1024)} MB > {maxSize / (1024 * 1024)} MB).";
                _logger.LogWarning("Validation", "Fichier trop volumineux", $"{Path.GetFileName(filePath)}: {fileInfo.Length / (1024 * 1024)} MB");
                return (false, error);
            }

            // Vérifier les permissions de lecture
            try
            {
                using var stream = File.OpenRead(filePath);
                // Lire les premiers octets pour vérifier qu'il s'agit bien d'un PDF
                var buffer = new byte[4];
                if (stream.Read(buffer, 0, 4) >= 4)
                {
                    var signature = System.Text.Encoding.ASCII.GetString(buffer);
                    if (!signature.StartsWith("%PDF"))
                    {
                        var error = $"Le fichier '{Path.GetFileName(filePath)}' ne semble pas être un PDF valide (signature manquante).";
                        _logger.LogWarning("Validation", "Signature PDF invalide", $"{Path.GetFileName(filePath)}: {signature}");
                        return (false, error);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                var error = $"Accès refusé au fichier '{Path.GetFileName(filePath)}'. Vérifiez les permissions.";
                _logger.LogError("Validation", "Accès refusé au fichier", ex, filePath);
                return (false, error);
            }
            catch (IOException ex)
            {
                var error = $"Impossible d'accéder au fichier '{Path.GetFileName(filePath)}': {ex.Message}";
                _logger.LogError("Validation", "Erreur d'accès au fichier", ex, filePath);
                return (false, error);
            }

            _logger.LogDebug("Validation", $"Fichier valide: {Path.GetFileName(filePath)}", $"Taille: {fileInfo.Length / 1024} KB");
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            var error = $"Erreur lors de la validation du fichier '{Path.GetFileName(filePath)}': {ex.Message}";
            _logger.LogError("Validation", "Erreur de validation générique", ex, filePath);
            return (false, error);
        }
    }

    private void LoadButton_Click(object? sender, EventArgs e)
    {
        _logger.LogInfo("UI", "Ouverture de la boîte de dialogue de sélection de fichiers");
        
        using var openDialog = new OpenFileDialog
        {
            Filter = "Fichiers PDF|*.pdf|Tous les fichiers|*.*",
            Multiselect = true,
            Title = "Sélectionner les fichiers PDF"
        };

        if (openDialog.ShowDialog(this) == DialogResult.OK)
        {
            _logger.LogInfo("UI", $"Sélection de {openDialog.FileNames.Length} fichier(s) par l'utilisateur");
            
            var validFiles = new List<string>();
            var errors = new List<string>();

            foreach (var file in openDialog.FileNames)
            {
                // Éviter les doublons
                if (_files.Contains(file))
                {
                    _logger.LogDebug("UI", $"Fichier déjà présent ignoré: {Path.GetFileName(file)}");
                    continue;
                }

                // Valider le fichier
                var (isValid, errorMessage) = ValidateFile(file);
                if (isValid)
                {
                    validFiles.Add(file);
                }
                else
                {
                    errors.Add(errorMessage);
                }
            }

            // Ajouter les fichiers valides
            _files.AddRange(validFiles);
            UpdateFilesList(selectLast: validFiles.Count > 0); // Sélectionner le dernier si des fichiers ont été ajoutés
            UpdateActions();

            // Log du résultat
            _logger.LogInfo("UI", $"Chargement terminé: {validFiles.Count} fichiers ajoutés, {errors.Count} erreurs");
            if (validFiles.Count > 0)
            {
                _logger.LogInfo("Files", $"Fichiers ajoutés: {string.Join(", ", validFiles.Select(Path.GetFileName))}");
            }

            // Afficher les résultats
            if (validFiles.Count > 0 && errors.Count == 0)
            {
                _statusLabel.Text = $"{validFiles.Count} fichier(s) ajouté(s) avec succès";
                _statusLabel.ForeColor = Color.Green;
            }
            else if (validFiles.Count > 0 && errors.Count > 0)
            {
                _statusLabel.Text = $"{validFiles.Count} fichier(s) ajouté(s), {errors.Count} erreur(s)";
                _statusLabel.ForeColor = Color.Orange;
                
                // Afficher les erreurs dans une boîte de dialogue
                var errorDetails = string.Join("\n\n", errors);
                MessageBox.Show(this, $"Certains fichiers n'ont pas pu être ajoutés :\n\n{errorDetails}", 
                    "Erreurs de validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (errors.Count > 0)
            {
                _statusLabel.Text = "Aucun fichier valide ajouté";
                _statusLabel.ForeColor = Color.Red;
                
                var errorDetails = string.Join("\n\n", errors);
                MessageBox.Show(this, $"Aucun fichier n'a pu être ajouté :\n\n{errorDetails}", 
                    "Erreurs de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            _logger.LogInfo("UI", "Sélection de fichiers annulée par l'utilisateur");
        }
    }

    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        if (_filesList.SelectedIndex >= 0 && _filesList.SelectedIndex < _files.Count)
        {
            _files.RemoveAt(_filesList.SelectedIndex);
            UpdateFilesList();
            UpdateActions();
        }
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        _files.Clear();
        UpdateFilesList();
        UpdateActions();
    }

    private void LogsButton_Click(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInfo("UI", "Ouverture du visualiseur de logs");
            var logViewerForm = new Converter.Gui.Windows.LogViewerForm(_logger);
            logViewerForm.Show();
        }
        catch (Exception ex)
        {
            _logger.LogError("UI", "Erreur lors de l'ouverture du visualiseur de logs", ex);
            MessageBox.Show(this, $"Impossible d'ouvrir le visualiseur de logs: {ex.Message}", 
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Méthodes de profils automatiques supprimées - interface simplifiée

    private string GetCompressionDisplay(string? compression)
    {
        return compression switch
        {
            null => "Aucune (non compressé)",
            "lzw" => "LZW (sans perte)",
            "zip" => "ZIP/Deflate (sans perte)",
            "packbits" => "PackBits (sans perte)",
            "g3" => "G3 (Fax, N&B uniquement)",
            "g4" => "G4 (Fax, N&B uniquement)",
            "jpeg" => "JPEG (avec perte, couleur/gris)",
            _ => "LZW (sans perte)"
        };
    }

    private string GetBitDepthFromDevice(string device)
    {
        return device.ToLower() switch
        {
            var d when d.Contains("g4") || d.Contains("g3") || d.Contains("mono") => "Noir & Blanc (1bit)",
            var d when d.Contains("gray") => "Niveaux de gris (8 bits)",
            var d when d.Contains("24") || d.Contains("color") => "Couleurs (24 bits)",
            _ => "Couleurs (24 bits)"
        };
    }

    private string GetSmoothingFromProfile(ConversionProfile profile)
    {
        // Chercher les paramètres de lissage dans ExtraParameters
        var alphaParam = profile.ExtraParameters.FirstOrDefault(p => p.Contains("TextAlphaBits"));
        if (alphaParam != null)
        {
            var match = System.Text.RegularExpressions.Regex.Match(alphaParam, @"=(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var level))
            {
                return level switch
                {
                    1 => "Aucun",
                    2 => "Léger (2x)",
                    4 => "Normal (4x)",
                    8 => "Fort (8x)",
                    _ => "Normal (4x)"
                };
            }
        }
        return "Normal (4x)"; // Par défaut
    }



    // La gestion de profils a été supprimée - paramètres automatiques selon le mode de couleur

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Sélectionner le dossier de sortie",
            SelectedPath = _outputTextBox.Text
        };

        if (folderDialog.ShowDialog(this) == DialogResult.OK)
        {
            _outputTextBox.Text = folderDialog.SelectedPath;
            // Sauvegarde des préférences supprimée - interface simplifiée
            UpdateActions();
        }
    }

    private (bool IsValid, string ErrorMessage) ValidateOutputFolder(string folderPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return (false, "Le dossier de sortie n'est pas spécifié.");
            }

            // Vérifier que le chemin est valide
            if (folderPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return (false, "Le chemin du dossier de sortie contient des caractères invalides.");
            }

            // Essayer de créer le dossier s'il n'existe pas
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return (false, $"Accès refusé au dossier '{folderPath}'. Vérifiez les permissions.");
            }
            catch (DirectoryNotFoundException)
            {
                return (false, $"Le chemin '{folderPath}' est invalide ou inaccessible.");
            }
            catch (IOException ex)
            {
                return (false, $"Impossible d'accéder au dossier '{folderPath}': {ex.Message}");
            }

            // Vérifier les permissions d'écriture
            try
            {
                var testFile = Path.Combine(folderPath, $"test_write_{Guid.NewGuid()}.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch (UnauthorizedAccessException)
            {
                return (false, $"Pas de permission d'écriture dans le dossier '{folderPath}'.");
            }
            catch (IOException ex)
            {
                return (false, $"Impossible d'écrire dans le dossier '{folderPath}': {ex.Message}");
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"Erreur lors de la validation du dossier de sortie: {ex.Message}");
        }
    }



    private async void ConvertButton_Click(object? sender, EventArgs e)
    {
        if (_isConverting) return;

        // Récupérer les paramètres de conversion actuels (source de vérité)
        var activeProfile = GetActiveConversionProfile();

        if (_files.Count == 0)
        {
            MessageBox.Show(this, "Aucun fichier à convertir.", "Erreur", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var outputFolder = _outputTextBox.Text;
        var (isValidFolder, folderError) = ValidateOutputFolder(outputFolder);
        if (!isValidFolder)
        {
            MessageBox.Show(this, folderError, "Erreur de dossier de sortie", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Re-valider tous les fichiers d'entrée avant conversion
        var invalidFiles = new List<string>();
        foreach (var file in _files.ToArray()) // ToArray pour éviter les modifications pendant l'itération
        {
            var (isValid, error) = ValidateFile(file);
            if (!isValid)
            {
                invalidFiles.Add($"• {Path.GetFileName(file)}: {error}");
                _files.Remove(file);
            }
        }

        if (invalidFiles.Count > 0)
        {
            UpdateFilesList();
            UpdateActions();
            
            var message = $"Des fichiers ne sont plus valides et ont été retirés de la liste :\n\n{string.Join("\n", invalidFiles)}";
            if (_files.Count == 0)
            {
                message += "\n\nAucun fichier valide restant pour la conversion.";
                MessageBox.Show(this, message, "Fichiers invalides", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                message += $"\n\nConversion des {_files.Count} fichiers restants ?";
                var result = MessageBox.Show(this, message, "Fichiers invalides", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }
        }

        // Log du démarrage de conversion
        var parameters = GetCurrentConversionParameters();
        _logger.LogInfo("Conversion", $"Démarrage de la conversion de {_files.Count} fichiers", 
            $"Paramètres: {parameters.Resolution} DPI, {parameters.Compression}, Sortie: {outputFolder}");

        _isConverting = true;
        _conversionCts = new CancellationTokenSource();
        
        _progressBar.Value = 0;
        _progressBar.Maximum = _files.Count;
        _progressBar.Visible = true;
        _statusLabel.Text = "Conversion en cours...";
        
        // Afficher et démarrer le panel de progression détaillée
        if (_conversionProgressPanel != null)
        {
            _conversionProgressPanel.Visible = true;
            _conversionProgressPanel.Reset();
            if (_files.Count > 0)
            {
                _conversionProgressPanel.StartConversion(Path.GetFileName(_files[0]), 0);
            }
        }
        
        UpdateActions();

        try
        {
            var progress = new Progress<BatchConversionProgress>(HandleProgress);
            
            // Utiliser une performance optimale fixe (2 threads)
            int maxConcurrency = 2;

            _logger.LogInfo("Conversion", $"Utilisation de {maxConcurrency} thread(s) de conversion simultané(s)");
            
            // Obtenir les paramètres de conversion actuels pour l'option mono-pages
            var conversionParams = GetCurrentConversionParameters();
            
            var result = await _conversionService.ConvertAsync(
                _files,
                outputFolder,
                activeProfile,
                progress,
                _conversionCts.Token,
                splitMultipage: conversionParams.MonoPages,
                maxConcurrency: maxConcurrency);

            HandleConversionComplete(result, activeProfile, outputFolder);
        }
        catch (OperationCanceledException)
        {
            _statusLabel.Text = "Conversion annulée";
        }
        catch (Exception ex)
        {
            ShowImprovedErrorDialog("Erreur de conversion", ex, 
                new[] {
                    "• Vérifiez que le fichier PDF n'est pas corrompu",
                    "• Essayez avec une résolution plus faible (150 DPI)",
                    "• Changez le type de compression (essayez LZW)",
                    "• Assurez-vous d'avoir assez d'espace disque",
                    "• Redémarrez l'application si le problème persiste"
                });
            _statusLabel.Text = "❌ Erreur de conversion";
        }
        finally
        {
            _isConverting = false;
            _progressBar.Visible = false;
            _conversionCts?.Dispose();
            _conversionCts = null;
            UpdateActions();
        }
    }

    private void ShowImprovedErrorDialog(string title, Exception ex, string[] solutions)
    {
        var message = $"❌ {ex.Message}\n\n💡 Solutions possibles:\n";
        message += string.Join("\n", solutions);
        
        var result = MessageBox.Show(this, message, title, 
            MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button2);
            
        switch (result)
        {
            case DialogResult.Retry:
                // Proposer de réessayer avec des paramètres plus sûrs
                if (title.Contains("conversion"))
                {
                    _resolutionCombo.Text = "150";
                    _compressionCombo.SelectedItem = "LZW (sans perte)";
                    _bitDepthCombo.SelectedItem = "Niveaux de gris (8 bits)";
                }
                break;
            case DialogResult.Ignore:
                // Continuer malgré l'erreur
                break;
            case DialogResult.Abort:
            default:
                // Arrêter l'opération
                break;
        }
    }

    private void StopButton_Click(object? sender, EventArgs e)
    {
        _conversionCts?.Cancel();
        _statusLabel.Text = "Annulation en cours...";
    }

    private void HandleProgress(BatchConversionProgress progress)
    {
        if (InvokeRequired)
        {
            Invoke(() => HandleProgress(progress));
            return;
        }

        _progressBar.Value = progress.Completed;
        _statusLabel.Text = $"Conversion: {progress.Completed}/{progress.Total} - {Path.GetFileName(progress.InputPath)}";
        
        // Mettre à jour le panel de progression détaillée
        if (_conversionProgressPanel != null && progress.Total > 0)
        {
            var percentComplete = (progress.Completed * 100) / progress.Total;
            _conversionProgressPanel.UpdateProgress(progress.Completed, progress.Total, percentComplete);
            
            // Mettre à jour le fichier en cours si changé
            if (progress.Completed < progress.Total)
            {
                var nextFileIndex = Math.Min(progress.Completed, _files.Count - 1);
                if (nextFileIndex >= 0 && nextFileIndex < _files.Count)
                {
                    _conversionProgressPanel.StartConversion(Path.GetFileName(_files[nextFileIndex]), 0);
                }
            }
        }
        
        // Log du progrès (seulement tous les 10% pour éviter le spam)
        if (progress.Total > 0 && (progress.Completed % Math.Max(1, progress.Total / 10) == 0 || progress.Completed == progress.Total))
        {
            var percentage = (progress.Completed * 100) / progress.Total;
            _logger.LogInfo("Conversion", $"Progrès: {percentage}% ({progress.Completed}/{progress.Total})", 
                $"Fichier actuel: {Path.GetFileName(progress.InputPath)}");
        }
        else if (progress.Total == 0)
        {
            _logger.LogInfo("Conversion", "Progrès: Démarrage de la conversion", 
                $"Fichier actuel: {Path.GetFileName(progress.InputPath)}");
        }
    }

    private void HandleConversionComplete(BatchConversionResult result, ConversionProfile profile, string outputFolder)
    {
        var successCount = result.SuccessCount;
        var failureCount = result.FailureCount;
        var totalTime = result.TotalDuration;
        
        // Mettre à jour le panel de progression
        if (_conversionProgressPanel != null)
        {
            if (failureCount == 0)
            {
                _conversionProgressPanel.Complete();
            }
            else
            {
                _conversionProgressPanel.Fail($"{failureCount} fichier(s) en échec");
            }
            
            // Masquer le panel après 2 secondes pour laisser voir le résultat
            var timer = new System.Windows.Forms.Timer { Interval = 2000 };
            timer.Tick += (s, ev) =>
            {
                if (_conversionProgressPanel != null)
                {
                    _conversionProgressPanel.Visible = false;
                }
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }
        
        // Log détaillé du résultat
        _logger.LogInfo("Conversion", $"Conversion terminée: {successCount} succès, {failureCount} échecs", 
            $"Temps total: {totalTime.TotalSeconds:F2}s, Profil: {profile.Name}");

        if (successCount > 0)
        {
            var successFiles = result.Files.Where(f => f.Success).Select(f => Path.GetFileName(f.InputPath));
            _logger.LogInfo("Conversion", $"Fichiers convertis avec succès: {string.Join(", ", successFiles)}");
        }

        if (failureCount > 0)
        {
            var failedFiles = result.Files.Where(f => !f.Success);
            foreach (var file in failedFiles)
            {
                _logger.LogError("Conversion", $"Échec de conversion: {Path.GetFileName(file.InputPath)}", 
                    details: file.ErrorMessage);
            }
        }
        
        _statusLabel.Text = $"Terminé: {successCount} succès, {failureCount} échecs";

        if (successCount > 0)
        {
            // Ouverture automatique du dossier après conversion réussie
            try
            {
                _logger.LogInfo("UI", "Ouverture du dossier de sortie", outputFolder);
                OpenExplorerIntelligently(outputFolder);
            }
            catch (Exception ex)
            {
                _logger.LogError("UI", "Erreur lors de l'ouverture de l'explorateur", ex, outputFolder);
            }
        }

        // Sauvegarde des préférences supprimée - interface simplifiée

        if (failureCount > 0)
        {
            var message = $"Conversion terminée avec {failureCount} erreur(s).\n\nVoulez-vous voir les détails ?";
            if (MessageBox.Show(this, message, "Conversion terminée", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ShowConversionDetails(result);
            }
        }
    }

    private void ShowConversionDetails(BatchConversionResult result)
    {
        var details = string.Join("\n", result.Files
            .Where(f => !f.Success)
            .Select(f => $"{Path.GetFileName(f.InputPath)}: {f.ErrorMessage}"));
        
        MessageBox.Show(this, $"Détails des erreurs:\n\n{details}", "Erreurs de conversion", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _logger.LogInfo("Application", "Fermeture de l'application");
        
        // Sauvegarde des préférences supprimée - interface simplifiée
        
        // Annuler toute conversion en cours
        _conversionCts?.Cancel();
        
        // Fermer proprement le logger
        _logger.Dispose();
        
        base.OnFormClosing(e);
    }

    private Panel CreatePreviewSection(TableLayoutPanel mainLayout)
    {
        // Panel principal d'aperçu
        _previewPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.LightGray
        };

        // PictureBox pour l'aperçu PDF avec dessin personnalisé
        _previewPictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Normal,  // Mode normal pour contrôle total
            BackColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D
        };
        
        // Activer le double buffering pour éviter le scintillement
        typeof(PictureBox).InvokeMember("DoubleBuffered",
            BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
            null, _previewPictureBox, new object[] { true });
        
        // Événements pour le zoom avec Ctrl+molette et pan
        _previewPictureBox.MouseWheel += PreviewPictureBox_MouseWheel;
        _previewPictureBox.MouseDown += PreviewPictureBox_MouseDown;
        _previewPictureBox.MouseMove += PreviewPictureBox_MouseMove;
        _previewPictureBox.MouseUp += PreviewPictureBox_MouseUp;
        _previewPictureBox.Paint += PreviewPictureBox_Paint;

        // Panel pour les contrôles de zoom
        var zoomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = Color.FromArgb(240, 240, 240)
        };

        // TrackBar pour le zoom
        _zoomTrackBar = new TrackBar
        {
            Minimum = 10,
            Maximum = 300,
            Value = 100,
            TickFrequency = 50,
            SmallChange = 10,
            LargeChange = 25,
            Location = new Point(10, 10),
            Size = new Size(200, 45)
        };
        _zoomTrackBar.ValueChanged += ZoomTrackBar_ValueChanged;

        // Label pour afficher le niveau de zoom
        _zoomLabel = new Label
        {
            Text = "100%",
            Location = new Point(220, 20),
            Size = new Size(50, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Boutons de navigation par page
        _previousPageButton = new Button
        {
            Text = "◀",
            Location = new Point(280, 15),
            Size = new Size(35, 30),
            UseVisualStyleBackColor = true,
            Enabled = false,
            Font = new Font(Font.FontFamily, 12f, FontStyle.Bold)
        };
        _previousPageButton.Click += PreviousPage_Click;

        _pageInfoLabel = new Label
        {
            Text = "0/0",
            Location = new Point(320, 20),
            Size = new Size(60, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 8.25f, FontStyle.Regular)
        };

        _nextPageButton = new Button
        {
            Text = "▶",
            Location = new Point(385, 15),
            Size = new Size(35, 30),
            UseVisualStyleBackColor = true,
            Enabled = false,
            Font = new Font(Font.FontFamily, 12f, FontStyle.Bold)
        };
        _nextPageButton.Click += NextPage_Click;

        // Bouton pour actualiser l'aperçu
        _refreshPreviewButton = new Button
        {
            Text = "Actualiser",
            Location = new Point(430, 15),
            Size = new Size(80, 30),
            UseVisualStyleBackColor = true
        };
        _refreshPreviewButton.Click += RefreshPreview_Click;

        // Label de statut de l'aperçu
        _previewStatusLabel = new Label
        {
            Text = "Sélectionnez un fichier PDF pour l'aperçu post-conversion",
            Dock = DockStyle.Top,
            Height = 25,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(250, 250, 250),
            ForeColor = Color.Gray,
            Font = new Font(Font.FontFamily, 8.25f, FontStyle.Italic)
        };

        // Label d'informations du fichier de sortie
        _fileInfoLabel = new Label
        {
            Text = "",
            Dock = DockStyle.Top,
            Height = 90, // Augmenté pour plus de confort visuel
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(245, 245, 245),
            ForeColor = Color.Black,
            Font = new Font(Font.FontFamily, 8f, FontStyle.Regular),
            Padding = new Padding(5),
            BorderStyle = BorderStyle.FixedSingle
        };

        // Strip de vignettes pour navigation pages
        _thumbnailStrip = new ThumbnailStripPanel
        {
            Dock = DockStyle.Bottom,
            Height = 140, // Augmenté pour meilleures miniatures
            Visible = false // Masqué par défaut, visible quand multi-pages
        };
        _thumbnailStrip.ThumbnailClicked += (s, pageIndex) =>
        {
            _currentPageIndex = pageIndex;
            DisplayCurrentPage();
            UpdatePageNavigation();
        };

        // Assemblage des contrôles
        zoomPanel.Controls.Add(_zoomTrackBar);
        zoomPanel.Controls.Add(_zoomLabel);
        zoomPanel.Controls.Add(_previousPageButton);
        zoomPanel.Controls.Add(_pageInfoLabel);
        zoomPanel.Controls.Add(_nextPageButton);
        zoomPanel.Controls.Add(_refreshPreviewButton);

        // IMPORTANT: L'ordre d'ajout avec Dock est crucial
        // D'abord le status label (Dock.Top)
        _previewPanel.Controls.Add(_previewStatusLabel);
        // Puis le label d'infos fichier (Dock.Top)
        _previewPanel.Controls.Add(_fileInfoLabel);
        // Puis le strip de vignettes (Dock.Bottom)
        _previewPanel.Controls.Add(_thumbnailStrip);
        // Puis le zoom panel (Dock.Bottom)
        _previewPanel.Controls.Add(zoomPanel);
        // Enfin le PictureBox (Dock.Fill) qui prendra l'espace restant
        _previewPanel.Controls.Add(_previewPictureBox);

        return _previewPanel;
    }

    private void ZoomTrackBar_ValueChanged(object? sender, EventArgs e)
    {
        if (_zoomTrackBar != null && _zoomLabel != null)
        {
            _zoomLabel.Text = $"{_zoomTrackBar.Value}%";
            UpdatePreviewZoom();
        }
    }

    private void RefreshPreview_Click(object? sender, EventArgs e)
    {
        RefreshPreview();
    }

    private void PreviewPictureBox_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (_currentPreviewImage == null || _zoomTrackBar == null)
            return;

        // Zoom avec molette (pas besoin de Ctrl)
        // Position du curseur dans la PictureBox pour centrer le zoom
        var mousePos = _previewPictureBox!.PointToClient(Cursor.Position);
        var oldZoomFactor = _zoomFactor;
        
        int currentZoom = _zoomTrackBar.Value;
        int newZoom;

        if (e.Delta > 0)
        {
            // Molette vers le haut : zoom in (incrément 10%)
            newZoom = Math.Min(_zoomTrackBar.Maximum, currentZoom + 10);
        }
        else
        {
            // Molette vers le bas : zoom out (décrément 10%)
            newZoom = Math.Max(_zoomTrackBar.Minimum, currentZoom - 10);
        }

        if (newZoom != currentZoom)
        {
            _zoomTrackBar.Value = newZoom;
            _zoomLabel!.Text = $"{newZoom}%";
            _zoomFactor = newZoom / 100.0f;
            
            // Ajuster le pan pour centrer le zoom sur le curseur
            if (oldZoomFactor > 0)
            {
                var zoomRatio = _zoomFactor / oldZoomFactor;
                _panOffset.X = mousePos.X - (mousePos.X - _panOffset.X) * zoomRatio;
                _panOffset.Y = mousePos.Y - (mousePos.Y - _panOffset.Y) * zoomRatio;
            }
            
            _previewPictureBox.Invalidate(); // Redessiner
        }
    }

    private void PreviewPictureBox_Paint(object? sender, PaintEventArgs e)
    {
        if (_currentPreviewImage == null)
            return;

        var g = e.Graphics;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        // Calculer les dimensions avec zoom
        var scaledWidth = (int)(_currentPreviewImage.Width * _zoomFactor);
        var scaledHeight = (int)(_currentPreviewImage.Height * _zoomFactor);

        // Position centrée par défaut si pas de pan
        var x = _panOffset.X;
        var y = _panOffset.Y;
        
        if (_panOffset == PointF.Empty)
        {
            // Centrer l'image dans le contrôle
            x = (_previewPictureBox!.Width - scaledWidth) / 2.0f;
            y = (_previewPictureBox.Height - scaledHeight) / 2.0f;
        }

        // Dessiner l'image avec transformation
        var destRect = new RectangleF(x, y, scaledWidth, scaledHeight);
        var srcRect = new Rectangle(0, 0, _currentPreviewImage.Width, _currentPreviewImage.Height);
        
        g.DrawImage(_currentPreviewImage, destRect, srcRect, GraphicsUnit.Pixel);
    }

    private void PreviewPictureBox_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _currentPreviewImage != null)
        {
            // Si c'est le premier pan et que _panOffset est vide, l'initialiser avec la position centrée actuelle
            if (_panOffset == PointF.Empty)
            {
                var scaledWidth = (int)(_currentPreviewImage.Width * _zoomFactor);
                var scaledHeight = (int)(_currentPreviewImage.Height * _zoomFactor);
                _panOffset.X = (_previewPictureBox!.Width - scaledWidth) / 2.0f;
                _panOffset.Y = (_previewPictureBox.Height - scaledHeight) / 2.0f;
            }
            
            _isPanning = true;
            _lastPanPoint = e.Location;
            _previewPictureBox!.Cursor = Cursors.Hand;
        }
    }

    private void PreviewPictureBox_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isPanning && _currentPreviewImage != null)
        {
            var deltaX = e.X - _lastPanPoint.X;
            var deltaY = e.Y - _lastPanPoint.Y;
            
            _panOffset.X += deltaX;
            _panOffset.Y += deltaY;
            _lastPanPoint = e.Location;
            
            _previewPictureBox!.Invalidate(); // Redessiner
        }
    }

    private void PreviewPictureBox_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isPanning = false;
            _previewPictureBox!.Cursor = Cursors.Default;
        }
    }



    private void UpdatePreviewZoom()
    {
        if (_currentPreviewImage == null || _zoomTrackBar == null)
            return;

        // Mettre à jour le facteur de zoom
        _zoomFactor = _zoomTrackBar.Value / 100.0f;
        
        // Redessiner avec le nouveau zoom
        _previewPictureBox?.Invalidate();
    }

    private void ApplyFitToWidthZoom()
    {
        if (_currentPreviewImage == null || _zoomTrackBar == null || _previewPictureBox == null)
            return;

        // Attendre que la PictureBox soit correctement dimensionnée
        if (_previewPictureBox.Width <= 0 || _currentPreviewImage.Width <= 0)
            return;

        // Calculer le zoom pour ajuster la largeur de l'image à la largeur de la PictureBox (moins une petite marge)
        var availableWidth = _previewPictureBox.Width - 20; // Marge de 10px de chaque côté
        var fitZoomFactor = (float)availableWidth / _currentPreviewImage.Width;
        var fitZoomPercent = (int)(fitZoomFactor * 100);
        
        // Limiter le zoom entre les bornes du TrackBar
        fitZoomPercent = Math.Max(_zoomTrackBar.Minimum, Math.Min(_zoomTrackBar.Maximum, fitZoomPercent));
        
        // Appliquer le zoom fit-to-width seulement si c'est différent du zoom actuel
        if (fitZoomPercent != _zoomTrackBar.Value)
        {
            _zoomTrackBar.Value = fitZoomPercent;
            _zoomFactor = fitZoomPercent / 100.0f;
            _zoomLabel!.Text = $"{fitZoomPercent}%";
            
            // Réinitialiser le pan pour centrer l'image
            _panOffset = PointF.Empty;
            
            // Redessiner avec le nouveau zoom
            _previewPictureBox.Invalidate();
        }
    }

    private void LoadAllPreviewPages(string tiffPath)
    {
        // Nettoyer les pages précédentes
        if (_allPreviewPages != null)
        {
            foreach (var page in _allPreviewPages)
            {
                page?.Dispose();
            }
        }
        _allPreviewPages = new List<Image>();

        try
        {
            using var originalImage = new Bitmap(tiffPath);
            
            // Compter le nombre de pages/frames dans le TIFF
            var frameCount = originalImage.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page);
            
            for (int i = 0; i < frameCount; i++)
            {
                // Sélectionner la frame
                originalImage.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, i);
                
                // Créer une copie optimisée de la page
                const int maxPreviewSize = 2048; // Limite pour optimiser la mémoire
                
                if (originalImage.Width > maxPreviewSize || originalImage.Height > maxPreviewSize)
                {
                    var scale = Math.Min((float)maxPreviewSize / originalImage.Width, 
                                       (float)maxPreviewSize / originalImage.Height);
                    var newWidth = (int)(originalImage.Width * scale);
                    var newHeight = (int)(originalImage.Height * scale);
                    
                    var resizedPage = new Bitmap(newWidth, newHeight);
                    using var graphics = Graphics.FromImage(resizedPage);
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                    
                    _allPreviewPages.Add(resizedPage);
                }
                else
                {
                    _allPreviewPages.Add(new Bitmap(originalImage));
                }
            }
            
            _logger.LogInfo("Preview", $"Chargé {frameCount} page(s) pour navigation", tiffPath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Preview", "Erreur lors du chargement des pages", ex, tiffPath);
            _allPreviewPages = new List<Image>();
        }
    }

    private void DisplayCurrentPage()
    {
        if (_allPreviewPages == null || _currentPageIndex < 0 || _currentPageIndex >= _allPreviewPages.Count)
            return;

        // Référencer directement la page courante (pas de copie pour performance)
        _currentPreviewImage = _allPreviewPages[_currentPageIndex];
        
        // S'assurer que la PictureBox n'a pas d'image (on dessine manuellement)
        _previewPictureBox!.Image = null;
        
        // Redessiner avec le nouveau système (invalidate suffit, c'est rapide)
        _previewPictureBox.Invalidate();
    }

    private void UpdatePageNavigation()
    {
        // Mettre à jour le label d'information
        _pageInfoLabel!.Text = _totalPages > 0 ? $"{_currentPageIndex + 1}/{_totalPages}" : "0/0";
        
        // Activer/désactiver les boutons selon la position
        _previousPageButton!.Enabled = _currentPageIndex > 0;
        _nextPageButton!.Enabled = _currentPageIndex < _totalPages - 1;
    }

    private void PreviousPage_Click(object? sender, EventArgs e)
    {
        if (_currentPageIndex > 0)
        {
            _currentPageIndex--;
            _thumbnailStrip?.SelectThumbnail(_currentPageIndex);
            DisplayCurrentPage();
            UpdatePageNavigation();
        }
    }

    private void NextPage_Click(object? sender, EventArgs e)
    {
        if (_currentPageIndex < _totalPages - 1)
        {
            _currentPageIndex++;
            _thumbnailStrip?.SelectThumbnail(_currentPageIndex);
            DisplayCurrentPage();
            UpdatePageNavigation();
        }
    }

    private async void RefreshPreview()
    {
        if (_filesList == null || _files == null || _filesList.SelectedIndex < 0 || _filesList.SelectedIndex >= _files.Count)
        {
            UpdatePreviewStatus("Aucun fichier sélectionné");
            ClearPreview();
            return;
        }

        var selectedFile = _files[_filesList.SelectedIndex];

        // Validation complète du fichier sélectionné
        var (isValid, errorMessage) = ValidateFile(selectedFile);
        if (!isValid)
        {
            UpdatePreviewStatus($"Fichier invalide : {errorMessage}");
            ClearPreview();
            
            // Proposer de retirer le fichier invalide de la liste
            var result = MessageBox.Show(this, 
                $"Le fichier sélectionné n'est plus valide :\n\n{errorMessage}\n\nVoulez-vous le retirer de la liste ?",
                "Fichier invalide", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                _files.RemoveAt(_filesList.SelectedIndex);
                UpdateFilesList();
                UpdateActions();
                
                // Essayer de sélectionner un autre fichier
                if (_files.Count > 0)
                {
                    _filesList.SelectedIndex = Math.Min(_filesList.SelectedIndex, _files.Count - 1);
                    RefreshPreview(); // Récursion contrôlée pour le fichier suivant
                }
            }
            return;
        }

        await GeneratePreviewAsync(selectedFile);
    }

    private async Task GeneratePreviewAsync(string pdfPath)
    {
        try
        {
            // Récupérer les paramètres de conversion actuels (source de vérité)
            var activeProfile = GetActiveConversionProfile();
            
            // Créer une signature unique des paramètres directement des contrôles (plus de profils)
            var conversionParams = GetCurrentConversionParameters();
            var currentParameters = $"{_resolutionCombo.Text}_{_compressionCombo.SelectedItem}_{_bitDepthCombo.SelectedItem}_{conversionParams.MonoPages}";
            
            // Créer une clé de cache unique pour ce fichier et ces paramètres
            var cacheKey = $"{pdfPath}_{currentParameters}";
            
            // Vérifier si les pages sont déjà en cache
            if (_previewPagesCache != null && _previewPagesCache.ContainsKey(cacheKey))
            {
                // Réutiliser les pages en cache
                _allPreviewPages = _previewPagesCache[cacheKey];
                _lastPreviewFilePath = pdfPath;
                _lastPreviewParameters = currentParameters;
                
                // Afficher les pages en cache
                _currentPageIndex = 0;
                _totalPages = _allPreviewPages?.Count ?? 0;
                UpdatePageNavigation();
                
                if (_thumbnailStrip != null && _allPreviewPages != null)
                {
                    _thumbnailStrip.LoadThumbnails(_allPreviewPages);
                    _thumbnailStrip.Visible = _allPreviewPages.Count > 1;
                    if (_allPreviewPages.Count > 1)
                    {
                        _thumbnailStrip.SelectThumbnail(0);
                    }
                }
                
                if (_allPreviewPages != null && _allPreviewPages.Count > 0)
                {
                    DisplayCurrentPage();
                }
                
                UpdatePreviewStatus($"Aperçu (depuis cache): {Path.GetFileName(pdfPath)} → {_totalPages} page(s)");
                return; // Aperçu depuis le cache
            }
            
            _previewCts?.Cancel();
            _previewCts = new CancellationTokenSource();

            UpdatePreviewStatus("Génération de l'aperçu optimisé...");
            UpdateFileInfo("");

            // Générer un nom de fichier temporaire pour l'aperçu converti
            var tempTiffPath = Path.Combine(Path.GetTempPath(), $"preview_converted_{Guid.NewGuid()}.tiff");
            _lastPreviewTempFile = tempTiffPath;

            try
            {
                // Conversion avec les paramètres utilisateur actuels pour aperçu
                // Extraire toutes les pages pour permettre la navigation
                await GhostscriptRunner.ConvertPdfToTiffAsync(
                    pdfPath, 
                    tempTiffPath,
                    device: activeProfile.Device,           // Device basé sur les paramètres utilisateur
                    dpi: activeProfile.Dpi,                // DPI des paramètres utilisateur
                    compression: activeProfile.Compression, // Compression des paramètres utilisateur
                    extraParameters: activeProfile.ExtraParameters, // Paramètres extra (lissage, etc.)
                    firstPage: null,                         // Toutes les pages pour navigation
                    lastPage: null,                          // Toutes les pages pour navigation
                    cancellationToken: _previewCts.Token);

                if (_previewCts.Token.IsCancellationRequested)
                    return;

                // Chargement des pages multiples pour navigation
                if (File.Exists(tempTiffPath))
                {
                    var fileInfo = new FileInfo(tempTiffPath);
                    
                    // Charger toutes les pages du TIFF multi-page
                    LoadAllPreviewPages(tempTiffPath);
                    
                    // Ajouter au cache (avec limite de taille)
                    if (_previewPagesCache != null && _allPreviewPages != null)
                    {
                        // Si le cache est plein, supprimer les anciennes entrées
                        while (_previewPagesCache.Count >= MaxCacheEntries)
                        {
                            var oldestKey = _previewPagesCache.Keys.First();
                            var oldPages = _previewPagesCache[oldestKey];
                            // Libérer les images de la mémoire
                            foreach (var page in oldPages)
                            {
                                page?.Dispose();
                            }
                            _previewPagesCache.Remove(oldestKey);
                        }
                        
                        // Créer des copies des images pour le cache (pour éviter les problèmes de disposal)
                        var cachedPages = new List<Image>();
                        foreach (var page in _allPreviewPages)
                        {
                            if (page != null)
                            {
                                cachedPages.Add((Image)page.Clone());
                            }
                        }
                        _previewPagesCache[cacheKey] = cachedPages;
                    }
                    
                    // Initialiser la navigation
                    _currentPageIndex = 0;
                    _totalPages = _allPreviewPages?.Count ?? 0;
                    UpdatePageNavigation();
                    
                    // Charger les miniatures dans le bandeau (si multi-pages)
                    if (_thumbnailStrip != null && _allPreviewPages != null)
                    {
                        _thumbnailStrip.LoadThumbnails(_allPreviewPages);
                        _thumbnailStrip.Visible = _allPreviewPages.Count > 1; // Visible seulement si multi-pages
                        if (_allPreviewPages.Count > 1)
                        {
                            _thumbnailStrip.SelectThumbnail(0); // Sélectionner la première miniature
                        }
                    }
                    
                    // Afficher la première page
                    if (_allPreviewPages != null && _allPreviewPages.Count > 0)
                    {
                        DisplayCurrentPage();
                    }
                    
                    // Sauvegarder le cache pour éviter les régénérations inutiles
                    _lastPreviewFilePath = pdfPath;
                    _lastPreviewParameters = currentParameters;
                    
                    // Mise à jour du statut et des informations fichier
                    UpdatePreviewStatus($"Aperçu optimisé: {Path.GetFileName(pdfPath)} → {_totalPages} page(s)");
                    
                    var fileSizeKB = fileInfo.Length / 1024.0;
                    var fileSizeMB = fileSizeKB / 1024.0;
                    var sizeText = fileSizeMB >= 1 ? $"{fileSizeMB:F2} MB" : $"{fileSizeKB:F1} KB";
                    
                    var parameters = GetCurrentConversionParameters();
                    var modeText = parameters.MonoPages ? "📄 Mode: Fichiers séparés par page" : "📑 Mode: Fichier multi-pages";
                    var dimensions = _currentPreviewImage != null ? $"{_currentPreviewImage.Width}×{_currentPreviewImage.Height}px" : "N/A";
                    UpdateFileInfo($"📊 {parameters.Resolution} DPI • {parameters.Compression} • {parameters.BitDepth}\n" +
                                  $"💾 {sizeText} • {activeProfile.Device.ToUpper()} • {dimensions}\n" +
                                  $"{modeText} • ⚡ Aperçu mis en cache pour performances");
                    
                    // Reset du zoom
                    if (_zoomTrackBar != null)
                    {
                        _zoomTrackBar.Value = 100;
                        _zoomLabel!.Text = "100%";
                    }
                }
                else
                {
                    UpdatePreviewStatus("Erreur lors de la génération de l'aperçu");
                    ClearPreview();
                }
            }
            finally
            {
                // Nettoyage du fichier temporaire
                if (File.Exists(tempTiffPath))
                {
                    try { File.Delete(tempTiffPath); } catch { }
                }
            }
        }
        catch (OperationCanceledException)
        {
            UpdatePreviewStatus("Génération de l'aperçu ...");
        }
        catch (Exception ex)
        {
            UpdatePreviewStatus($"Erreur: {ex.Message}");
            ClearPreview();
        }
    }

    private void UpdatePreviewStatus(string status)
    {
        if (_previewStatusLabel != null)
        {
            _previewStatusLabel.Text = status;
        }
    }

    private void UpdateFileInfo(string info)
    {
        if (_fileInfoLabel != null)
        {
            _fileInfoLabel.Text = info;
            _fileInfoLabel.Visible = !string.IsNullOrEmpty(info);
        }
    }

    private void ClearPreview()
    {
        _previewPictureBox?.Image?.Dispose();
        if (_previewPictureBox != null)
            _previewPictureBox.Image = null;
            
        // Ne pas disposer _currentPreviewImage car elle référence une image de _allPreviewPages
        // _currentPreviewImage?.Dispose();
        _currentPreviewImage = null;
        
        // Nettoyer toutes les pages de navigation
        if (_allPreviewPages != null)
        {
            foreach (var page in _allPreviewPages)
            {
                page?.Dispose();
            }
            _allPreviewPages = null;
        }
        
        // Réinitialiser la navigation
        _currentPageIndex = 0;
        _totalPages = 0;
        UpdatePageNavigation();
        
        // Nettoyer et masquer le bandeau de miniatures
        if (_thumbnailStrip != null)
        {
            _thumbnailStrip.Clear();
            _thumbnailStrip.Visible = false;
        }
        
        UpdateFileInfo("");
        
        // Nettoyer le fichier temporaire précédent
        if (!string.IsNullOrEmpty(_lastPreviewTempFile) && File.Exists(_lastPreviewTempFile))
        {
            try { File.Delete(_lastPreviewTempFile); } catch { }
        }
        _lastPreviewTempFile = null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _previewCts?.Cancel();
            _previewCts?.Dispose();
            
            // Nettoyer le cache de prévisualisation
            if (_previewPagesCache != null)
            {
                foreach (var cachedPages in _previewPagesCache.Values)
                {
                    foreach (var page in cachedPages)
                    {
                        page?.Dispose();
                    }
                }
                _previewPagesCache.Clear();
            }
            
            // Ne pas disposer _currentPreviewImage car elle référence une image de _allPreviewPages
            // _currentPreviewImage?.Dispose();
            _previewPictureBox?.Image?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void UpdateWatchStatus()
    {
        try
        {
            // Statut de surveillance simplifié - pas de persistance de settings
            if (_watchButton != null)
            {
                _watchStatusLabel.Text = "Surveillance disponible";
                _watchStatusLabel.ForeColor = Color.Gray;
            }
            else
            {
                _watchStatusLabel.Text = "Surveillance inactive";
                _watchStatusLabel.ForeColor = Color.Gray;
            }
        }
        catch
        {
            _watchStatusLabel.Text = "Surveillance inactive";
            _watchStatusLabel.ForeColor = Color.Gray;
        }
    }
}