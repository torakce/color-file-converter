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
using Converter.Core;
using Converter.Gui.Services;
using Converter.Gui.Profiles;

namespace Converter.Gui;

public partial class MainForm : Form
{
    private readonly ProfileRepository _profileRepository = new();
    private readonly BatchConversionService _conversionService = new();
    private readonly List<ConversionProfile> _profiles;
    private readonly string _settingsPath;
    private readonly string _logFilePath;
    private readonly Logger _logger;
    private UserSettings _settings;
    private readonly List<string> _files = new();
    private CancellationTokenSource? _conversionCts;
    private bool _isConverting;

    // Contrôles de l'interface
    private ListBox _filesList;
    private Label _dropInstructionLabel;
    private ComboBox _profileCombo;
    private TextBox _outputTextBox;
    private CheckBox _overwriteCheckBox;
    private CheckBox _openFolderCheckBox;
    private CheckBox _monoPagesCheckBox;
    private Button _convertButton;
    private Button _stopButton;
    private WatchOptionsButton _watchButton;
    private Label _watchStatusLabel;
    private ProgressBar _progressBar;
    private Label _statusLabel;
    
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
    


    public MainForm()
    {
        // Initialiser les services
        _settingsPath = Path.Combine(ProfileRepository.GetConfigurationDirectory(), "settings.json");
        _settings = UserSettings.Load(_settingsPath);
        _profiles = _profileRepository.Load().ToList();
        
        // Initialiser le logger
        _logFilePath = Path.Combine(ProfileRepository.GetConfigurationDirectory(), "converter.log");
        _logger = new Logger(_logFilePath, LogLevel.Info);
        
        _logger.LogInfo("Application", "Démarrage de Color File Converter", $"Version GUI, {_profiles.Count} profils chargés");

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
        _watchButton = new WatchOptionsButton(_settingsPath, _settings)
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
        CreateProfileSection(leftLayout, 1);
        CreateOutputSection(leftLayout, 2);
    }

    private void CreateFileSection(TableLayoutPanel parent, int row)
    {
        var filesGroup = new GroupBox
        {
            Text = "Fichiers PDF",
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            AllowDrop = true  // Permettre le drop sur toute la zone
        };
        parent.Controls.Add(filesGroup, 0, row);
        
        // Événements de drag & drop pour le GroupBox entier
        filesGroup.DragEnter += FilesList_DragEnter;
        filesGroup.DragDrop += FilesList_DragDrop;
        filesGroup.DragOver += FilesGroup_DragOver;
        filesGroup.DragLeave += FilesGroup_DragLeave;

        _filesList = new ListBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            AllowDrop = true
        };
        _filesList.DragEnter += FilesList_DragEnter;
        _filesList.DragDrop += FilesList_DragDrop;
        _filesList.SelectedIndexChanged += (s, e) =>
        {
            UpdateActions();
            // Déclencher l'aperçu automatiquement lors de la sélection
            RefreshPreview();
        };
        UpdateFilesList();

        // Utiliser un TableLayoutPanel pour organiser le contenu des fichiers
        var filesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(5)
        };
        filesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 75F)); // Liste des fichiers
        filesLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Label d'instruction
        filesLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Boutons
        filesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        
        filesGroup.Controls.Add(filesLayout);
        filesLayout.Controls.Add(_filesList, 0, 0);

        // Label d'instruction pour le drag & drop (visible quand la liste est vide)
        _dropInstructionLabel = new Label
        {
            Text = "📁 Glissez vos fichiers PDF ici\nou utilisez le bouton Charger",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = SystemColors.GrayText,
            Font = new Font(_filesList.Font.FontFamily, 10, FontStyle.Italic),
            Visible = _files.Count == 0,
            Margin = new Padding(5)
        };
        filesLayout.Controls.Add(_dropInstructionLabel, 0, 1);

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

    private void CreateProfileSection(TableLayoutPanel parent, int row)
    {
        var profileGroup = new GroupBox
        {
            Text = "Profil de conversion",
            Dock = DockStyle.Fill,
            Margin = new Padding(5)
        };
        parent.Controls.Add(profileGroup, 0, row);

        // Utiliser un TableLayoutPanel pour organiser les contrôles horizontalement
        var profileLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(5)
        };
        profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // Label
        profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F)); // ComboBox
        profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Button (taille fixe)
        profileLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        
        profileGroup.Controls.Add(profileLayout);

        var profileLabel = new Label
        {
            Text = "Profil:",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(3),
            MinimumSize = new Size(60, 23),
            MaximumSize = new Size(60, 23)
        };
        profileLayout.Controls.Add(profileLabel, 0, 0);

        _profileCombo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(5, 3, 5, 3),
            Height = 23
        };
        _profileCombo.SelectedIndexChanged += ProfileCombo_SelectedIndexChanged;
        profileLayout.Controls.Add(_profileCombo, 1, 0);

        var manageProfilesButton = new Button
        {
            Text = "Gérer",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(3, 3, 3, 3),
            Height = 23,
            FlatStyle = FlatStyle.Standard
        };
        manageProfilesButton.Click += ManageProfilesButton_Click;
        profileLayout.Controls.Add(manageProfilesButton, 2, 0);

        // Modifier le layout principal pour inclure une deuxième ligne pour les paramètres (collée)
        profileLayout.RowCount = 2;
        profileLayout.RowStyles.Clear();
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Ligne du profil
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Ligne des paramètres
        
        // Ajouter les contrôles de paramètres de conversion (une seule ligne, collée au profil)
        var paramsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Margin = new Padding(3, 0, 3, 3),
            Padding = new Padding(0)
        };
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Résolution
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F)); // Résolution
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Compression
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F)); // Compression
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Label Profondeur
        paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F)); // Profondeur
        paramsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Résolution
        var resLabel = new Label { Text = "Résolution:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
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
        
        // Tooltip pour la résolution
        var toolTip1 = new ToolTip();
        toolTip1.SetToolTip(_resolutionCombo, 
            "Résolution en DPI (points par pouce)\n" +
            "• 72-96: Web/écran\n" +
            "• 150-200: Usage général\n" +
            "• 300+: Impression/archivage\n" +
            "Plus élevé = meilleure qualité mais fichiers plus gros");

        // Compression TIFF
        var compressionLabel = new Label { Text = "Compression:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
        paramsLayout.Controls.Add(compressionLabel, 2, 0);
        
        _compressionCombo = new ComboBox
        {
            Name = "compressionCombo",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        
        // Compressions TIFF disponibles avec leurs descriptions
        _compressionCombo.Items.AddRange(new[] { 
            "Aucune (non compressé)",
            "LZW (sans perte)",
            "ZIP/Deflate (sans perte)", 
            "PackBits (sans perte)",
            "G3 (Fax, N&B uniquement)",
            "G4 (Fax, N&B uniquement)",
            "JPEG (avec perte, couleur/gris)"
        });
        _compressionCombo.SelectedIndex = 1; // LZW par défaut
        _compressionCombo.SelectedIndexChanged += ParameterChanged;
        paramsLayout.Controls.Add(_compressionCombo, 3, 0);
        
        // Tooltip pour la compression
        var toolTip2 = new ToolTip();
        toolTip2.SetToolTip(_compressionCombo,
            "Type de compression TIFF\n" +
            "• LZW: Équilibre qualité/taille (recommandé)\n" +
            "• G3/G4: Fax, nécessite N&B, très compact\n" +
            "• ZIP: Excellente compression sans perte\n" +
            "• JPEG: Petits fichiers mais perte de qualité\n" +
            "• Aucune: Qualité max, fichiers très gros");

        // Bit depth / Profondeur
        var bitDepthLabel = new Label { Text = "Profondeur:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
        paramsLayout.Controls.Add(bitDepthLabel, 4, 0);
        
        _bitDepthCombo = new ComboBox
        {
            Name = "bitDepthCombo",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        _bitDepthCombo.Items.AddRange(new[] { "1 bit (N&B)", "8 bits (256 niveaux)", "24 bits (16M couleurs)" });
        _bitDepthCombo.SelectedIndex = 2; // 24 bits par défaut
        _bitDepthCombo.SelectedIndexChanged += ParameterChanged;
        paramsLayout.Controls.Add(_bitDepthCombo, 5, 0);
        
        // Tooltip pour la profondeur
        var toolTip3 = new ToolTip();
        toolTip3.SetToolTip(_bitDepthCombo,
            "Profondeur de couleur\n" +
            "• 1 bit: Noir & Blanc seulement (G3/G4 obligatoire)\n" +
            "• 8 bits: Niveaux de gris (256 nuances)\n" +
            "• 24 bits: Couleurs complètes (16M couleurs)\n" +
            "Plus élevé = plus de détails mais fichiers plus gros");

        // Lissage supprimé - utilisation d'une valeur par défaut dans la logique

        // Ajouter les paramètres à la deuxième ligne du layout principal des profils
        profileLayout.Controls.Add(paramsLayout, 0, 1);
        profileLayout.SetColumnSpan(paramsLayout, 3); // Étendre sur les 3 colonnes

        // Ajouter une ligne pour les avertissements de validation
        profileLayout.RowCount = 3;
        profileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Ligne d'avertissement
        
        _validationWarningLabel = new Label
        {
            Text = "",
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = Color.Orange,
            Font = new Font(this.Font.FontFamily, this.Font.Size - 1, FontStyle.Italic),
            Margin = new Padding(6, 0, 3, 3),
            Visible = false
        };
        profileLayout.Controls.Add(_validationWarningLabel, 0, 2);
        profileLayout.SetColumnSpan(_validationWarningLabel, 3); // Étendre sur les 3 colonnes

        // Ajouter les validations pour les compressions incompatibles
        _compressionCombo.SelectedIndexChanged += ValidateCompressionCompatibility;
        _bitDepthCombo.SelectedIndexChanged += ValidateCompressionCompatibility;
    }

    // Structure pour les paramètres de conversion
    public class ConversionParameters
    {
        public int Resolution { get; set; } = 300;
        public string Compression { get; set; } = "LZW (sans perte)";
        public string BitDepth { get; set; } = "24 bits (16M couleurs)";
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
    
    private void ValidateCompressionCompatibility(object? sender, EventArgs e)
    {
        var parameters = GetCurrentConversionParameters();
        
        // Réinitialiser l'avertissement
        _validationWarningLabel.Visible = false;
        _validationWarningLabel.Text = "";
        
        // Vérifier les incompatibilités et afficher des avertissements discrets
        if ((parameters.Compression.Contains("G3") || parameters.Compression.Contains("G4")) 
            && !parameters.BitDepth.Contains("1 bit"))
        {
            // Afficher un avertissement visuel sans bloquer
            _validationWarningLabel.Text = "⚠️ G3/G4 nécessite 1 bit (N&B). Correction automatique appliquée.";
            _validationWarningLabel.Visible = true;
            
            // Changer automatiquement vers 1 bit (sans MessageBox intrusif)
            SetBitDepth("1 bit (N&B)");
        }
        else if (parameters.Compression.Contains("JPEG") && parameters.BitDepth.Contains("1 bit"))
        {
            // Nouveau : Avertissement pour JPEG avec monochrome
            _validationWarningLabel.Text = "💡 Conseil: JPEG fonctionne mieux avec 8 ou 24 bits pour de meilleurs résultats.";
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
                    var compression = GetCompressionValue(parameters.Compression);

                    // Créer le profil personnalisé
                    var customProfile = new ConversionProfile(
                        "Personnalisé",
                        device,
                        compression,
                        dpi,
                        Array.Empty<string>()
                    );

                    // Remplacer le profil "Personnalisé" s'il existe déjà
                    var existingCustomIndex = -1;
                    for (int i = 0; i < _profileCombo.Items.Count; i++)
                    {
                        if (_profileCombo.Items[i] is ConversionProfile p && p.Name == "Personnalisé")
                        {
                            existingCustomIndex = i;
                            break;
                        }
                    }

                    if (existingCustomIndex >= 0)
                    {
                        _profileCombo.Items[existingCustomIndex] = customProfile;
                    }
                    else
                    {
                        _profileCombo.Items.Add(customProfile);
                    }

                    // Sélectionner le profil personnalisé (temporairement désactiver l'événement pour éviter la boucle)
                    _profileCombo.SelectedIndexChanged -= ProfileCombo_SelectedIndexChanged;
                    _profileCombo.SelectedItem = customProfile;
                    _profileCombo.SelectedIndexChanged += ProfileCombo_SelectedIndexChanged;
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
            parameters.BitDepth = _bitDepthCombo.SelectedItem.ToString() ?? "24 bits (16M couleurs)";
        
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
            "1 bit (N&B)" => "tiffg4", // Toujours G4 pour le monochrome
            "8 bits (256 niveaux)" => "tiffgray",
            "24 bits (16M couleurs)" => "tiff24nc",
            _ => "tiff24nc"
        };
    }

    private string? GetCompressionValue(string compressionDisplay)
    {
        return compressionDisplay switch
        {
            "Aucune (non compressé)" => null,
            "LZW (sans perte)" => "lzw",
            "ZIP/Deflate (sans perte)" => "zip",
            "PackBits (sans perte)" => "packbits",
            "G3 (Fax, N&B uniquement)" => "g3",
            "G4 (Fax, N&B uniquement)" => "g4",
            "JPEG (avec perte, couleur/gris)" => "jpeg",
            _ => "lzw"
        };
    }

    // Méthode publique pour obtenir les paramètres de conversion actuels (utilisée par l'aperçu et la conversion)
    public ConversionProfile GetActiveConversionProfile()
    {
        var parameters = GetCurrentConversionParameters();
        var device = CreateDeviceFromParameters(parameters);
        var compression = GetCompressionValue(parameters.Compression);
        
        // Ajouter les paramètres de lissage comme paramètres extra
        var extraParams = new List<string>();
        var smoothingLevel = parameters.Smoothing switch
        {
            "Aucun" => 1,
            "Léger (2x)" => 2,
            "Normal (4x)" => 4,
            "Fort (8x)" => 8,
            _ => 4
        };
        
        if (smoothingLevel > 1)
        {
            extraParams.Add($"-dTextAlphaBits={smoothingLevel}");
            extraParams.Add($"-dGraphicsAlphaBits={smoothingLevel}");
        }
        
        return new ConversionProfile(
            _profileCombo.SelectedItem?.ToString() ?? "Actuel",
            device,
            compression,
            parameters.Resolution,
            extraParams
        );
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
            RowCount = 3,
            Margin = new Padding(5)
        };
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F)); // TextBox/CheckBox
        outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // Button
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // Label
        outputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // TextBox + Button
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
            Text = _settings.LastOutputFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
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

        // Panel pour les checkboxes
        var checkBoxPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };
        
        _overwriteCheckBox = new CheckBox
        {
            Text = "Écraser fichiers existants",
            AutoSize = true,
            Location = new Point(0, 0)
        };
        checkBoxPanel.Controls.Add(_overwriteCheckBox);

        _openFolderCheckBox = new CheckBox
        {
            Text = "Ouvrir dossier après conversion",
            AutoSize = true,
            Location = new Point(0, 25),
            Checked = _settings.OpenExplorerAfterConversion
        };
        checkBoxPanel.Controls.Add(_openFolderCheckBox);

        _monoPagesCheckBox = new CheckBox
        {
            Text = "Créer des fichiers séparés par page (mono-pages)",
            AutoSize = true,
            Location = new Point(0, 50),
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
        
        outputLayout.Controls.Add(checkBoxPanel, 0, 2);
        outputLayout.SetColumnSpan(checkBoxPanel, 2);

        // Performance supprimée - le RowCount reste à 3
    }



    private void InitializeData()
    {
        RefreshProfiles();
        UpdateFilesList();
        UpdateActions();
    }

    private void RefreshProfiles()
    {
        _profileCombo.BeginUpdate();
        _profileCombo.Items.Clear();
        foreach (var profile in _profiles)
        {
            _profileCombo.Items.Add(profile);
        }
        _profileCombo.EndUpdate();

        if (_profiles.Count > 0)
        {
            var selected = _profiles.FirstOrDefault(p => string.Equals(p.Name, _settings.LastProfileName, StringComparison.OrdinalIgnoreCase))
                           ?? _profiles[0];
            _profileCombo.SelectedItem = selected;
        }
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
        bool hasProfile = _profileCombo.SelectedItem != null;

        _convertButton.Enabled = hasFiles && hasOutput && hasProfile && !_isConverting;
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

    private void ProfileCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_profileCombo.SelectedItem is ConversionProfile profile)
        {
            _settings.LastProfileName = profile.Name;
            UpdateActions();
            
            // Mettre à jour les paramètres affichés selon le profil sélectionné
            UpdateParametersFromProfile(profile);
            
            // Regénérer l'aperçu avec le nouveau profil
            RefreshPreview();
        }
    }

    private void UpdateParametersFromProfile(ConversionProfile profile)
    {
        // Utiliser les références directes aux contrôles
        
        // Désactiver temporairement les événements pour éviter les boucles
        _resolutionCombo.TextChanged -= ParameterChanged;
        _resolutionCombo.SelectedIndexChanged -= ParameterChanged;
        _compressionCombo.SelectedIndexChanged -= ParameterChanged;
        _bitDepthCombo.SelectedIndexChanged -= ParameterChanged;
        
        try
        {
            // Résolution (DPI)
            _resolutionCombo.Text = profile.Dpi.ToString();
            
            // Compression basée sur le profil
            var compressionDisplay = GetCompressionDisplay(profile.Compression);
            if (_compressionCombo.Items.Contains(compressionDisplay))
                _compressionCombo.SelectedItem = compressionDisplay;
            
            // Profondeur de bits basée sur le device
            var bitDepth = GetBitDepthFromDevice(profile.Device);
            if (_bitDepthCombo.Items.Contains(bitDepth))
                _bitDepthCombo.SelectedItem = bitDepth;
            
            // Lissage supprimé - utilisation d'une valeur par défaut
        }
        finally
        {
            // Réactiver les événements
            _resolutionCombo.TextChanged += ParameterChanged;
            _resolutionCombo.SelectedIndexChanged += ParameterChanged;
            _compressionCombo.SelectedIndexChanged += ParameterChanged;
            _bitDepthCombo.SelectedIndexChanged += ParameterChanged;
        }
    }

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
            var d when d.Contains("g4") || d.Contains("g3") || d.Contains("mono") => "1 bit (N&B)",
            var d when d.Contains("gray") => "8 bits (256 niveaux)",
            var d when d.Contains("24") || d.Contains("color") => "24 bits (16M couleurs)",
            _ => "24 bits (16M couleurs)"
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



    private void ManageProfilesButton_Click(object? sender, EventArgs e)
    {
        var manager = new ProfileManagerForm(_profiles);
        if (manager.ShowDialog(this) == DialogResult.OK)
        {
            _profiles.Clear();
            _profiles.AddRange(manager.Profiles);
            _profileRepository.Save(_profiles);
            RefreshProfiles();
        }
    }

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
            _settings.LastOutputFolder = folderDialog.SelectedPath;
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
                    _bitDepthCombo.SelectedItem = "8 bits (256 niveaux)";
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
            if (_openFolderCheckBox.Checked)
            {
                try
                {
                    _logger.LogInfo("UI", "Ouverture du dossier de sortie", outputFolder);
                    System.Diagnostics.Process.Start("explorer.exe", outputFolder);
                }
                catch (Exception ex)
                {
                    _logger.LogError("UI", "Erreur lors de l'ouverture de l'explorateur", ex, outputFolder);
                }
            }
        }

        // Sauvegarder les paramètres
        _settings.OpenExplorerAfterConversion = _openFolderCheckBox.Checked;
        _settings.Save(_settingsPath);
        
        // Mettre à jour le bouton de surveillance
        _watchButton.UpdateSettings(_settingsPath, _settings);

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
        
        // Sauvegarder les paramètres avant fermeture
        _settings.Save(_settingsPath);
        
        // Mettre à jour le bouton de surveillance
        _watchButton.UpdateSettings(_settingsPath, _settings);
        
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

        // Zoom avec molette (sans Ctrl nécessaire)
        // Position du curseur dans la PictureBox pour centrer le zoom
        var mousePos = _previewPictureBox!.PointToClient(Cursor.Position);
        var oldZoomFactor = _zoomFactor;
        
        int currentZoom = _zoomTrackBar.Value;
        int newZoom;

        if (e.Delta > 0)
        {
            // Molette vers le haut : zoom in (plus doux: 10%)
            newZoom = Math.Min(_zoomTrackBar.Maximum, currentZoom + 10);
        }
        else
        {
            // Molette vers le bas : zoom out (plus doux: 10%)
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

        // Nettoyer l'image précédente
        _currentPreviewImage?.Dispose();
        
        // Créer une copie de la page courante
        var currentPage = _allPreviewPages[_currentPageIndex];
        _currentPreviewImage = new Bitmap(currentPage);
        
        // Réinitialiser pan et zoom
        _panOffset = PointF.Empty;
        _zoomFactor = _zoomTrackBar!.Value / 100.0f;
        
        // S'assurer que la PictureBox n'a pas d'image (on dessine manuellement)
        _previewPictureBox!.Image?.Dispose();
        _previewPictureBox.Image = null;
        
        // Redessiner avec le nouveau système
        _previewPictureBox.Invalidate();
        
        // Force le redraw
        _previewPictureBox.Refresh();
        _previewPanel.Refresh();
        
        // Appliquer le zoom fit-to-width après que les contrôles soient dimensionnés
        this.BeginInvoke(new Action(ApplyFitToWidthZoom));
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
            DisplayCurrentPage();
            UpdatePageNavigation();
        }
    }

    private void NextPage_Click(object? sender, EventArgs e)
    {
        if (_currentPageIndex < _totalPages - 1)
        {
            _currentPageIndex++;
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
            
            // Créer une signature unique des paramètres pour le cache (incluant l'option mono-pages)
            var conversionParams = GetCurrentConversionParameters();
            var currentParameters = $"{activeProfile.Device}_{activeProfile.Dpi}_{activeProfile.Compression}_{conversionParams.MonoPages}_{string.Join(",", activeProfile.ExtraParameters)}";
            
            // Vérifier si on peut réutiliser l'aperçu existant (cache)
            if (_lastPreviewFilePath == pdfPath && 
                _lastPreviewParameters == currentParameters && 
                _currentPreviewImage != null)
            {
                UpdatePreviewStatus("Aperçu (paramètres inchangés)");
                return; // Pas besoin de régénérer
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
                    
                    // Initialiser la navigation
                    _currentPageIndex = 0;
                    _totalPages = _allPreviewPages?.Count ?? 0;
                    UpdatePageNavigation();
                    
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
            UpdatePreviewStatus("Aperçu annulé");
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
            
        _currentPreviewImage?.Dispose();
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
            _currentPreviewImage?.Dispose();
            _previewPictureBox?.Image?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void UpdateWatchStatus()
    {
        try
        {
            // Recharger les settings pour voir si la surveillance est active
            var currentSettings = UserSettings.Load(_settingsPath);
            
            if (currentSettings.WatchFolderEnabled && 
                !string.IsNullOrEmpty(currentSettings.WatchFolderPath) && 
                Directory.Exists(currentSettings.WatchFolderPath))
            {
                _watchStatusLabel.Text = $"Surveillant: {Path.GetFileName(currentSettings.WatchFolderPath)}";
                _watchStatusLabel.ForeColor = Color.Green;
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