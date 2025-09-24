# Color File Converter v2.1# Color File Conv- **⚡ Ghostscri## 🚀 Utilisatio### 📁 Mode surveillance automatique

- **Journal automatique** : cha2. Dossier `Ghostscript` **Prérequis :**

**Convertisseur PDF vers TIFF professionnel avec interface moderne et Ghostscript intégré**- .NET 8 SDK

- Visual Studio 2022 ou VS Code

Outil de conversion par lots optimisé pour les professionnels, offrant une expérience utilisateur simplifiée avec des paramètres automatiques intelligents.

**Structure du projet :**

## ✨ Nouveautés v2.1```

Converter.sln

- **🚀 Ghostscript portable intégré** : Fonctionne immédiatement sans installation sur tout PC Windows├── 🎯 Converter.Gui/      # Interface Windows Forms

- **🎛️ Interface simplifiée** : Plus de profils complexes - 3 paramètres essentiels (DPI, Compression, Couleurs)├── ⚙️ Converter.Core/     # Moteur de conversion  

- **💡 Gestion intelligente des fenêtres** : Réutilise les fenêtres d'explorateur existantes au lieu d'en ouvrir de nouvelles├── 🖥️ Converter.Cli/      # Interface ligne de commande

- **🔧 Compression optimisée** : Support complet de tous les types de compression TIFF selon les standards Ghostscript└── 📦 Resources/          # Ghostscript portable

- **🎯 UX améliorée** : Messages plus clairs, navigation intuitive, feedback utilisateur détaillé```



## 🚀 Utilisation rapide**Commandes utiles :**

```bash

1. **Lancez** `Converter.Gui.exe` (aucune installation requise)dotnet build                    # Compilation

2. **Ajoutez** vos PDF (bouton ou glisser-déposer)dotnet run --project Converter.Gui  # Lancement en debug

3. **Choisissez** vos paramètres :```

   - **Résolution** : 150, 300, 600 DPI

   - **Compression** : Aucune, LZW, PackBits, G3/G4, JPEG, ZIP## 🔧 Dépannage

   - **Couleurs** : N&B, Gris, Couleurs

4. **Sélectionnez** le dossier de sortie| Problème | Solution |

5. **Lancez** la conversion !|----------|----------|

| **Ghostscript introuvable** | Vérifiez le dossier `Resources/Ghostscript/` |

> 💡 **Nouveau** : Les paramètres sont maintenant directement accessibles - fini les profils complexes !| **Prévisualisation vide** | Vérifiez les paramètres et relancez l'aperçu |

| **Surveillance inactive** | Contrôlez les permissions du dossier surveillé |

## 🎛️ Fonctionnalités principales

---

### 🔄 **Conversion par lots intelligente**

- Ajout multiple via dialogue ou glisser-déposer**🚀 Color File Converter v2.1** - Conversion PDF → TIFF simplifiée et portable !e

- Traitement parallèle optimisé3. Ghostscript dans le `PATH` système

- Arrêt propre à tout moment

## 🏗️ Structure du projetversion est enregistrée dans `%AppData%\ColorFileConverter\logs`

### 👁️ **Prévisualisation en temps réel**- **Statistiques détaillées** : nom, durée, tailles, statut

- Aperçu de la première page avec paramètres actuels- **Ouverture automatique** : option pour consulter le journal après conversion

- Zoom adaptatif pour vérifier la qualité

- Navigation entre les pages du PDF## 💼 Installation et déploiement



### 📊 **Journal détaillé automatique**### ✅ Version portable (recommandée)

- Sauvegarde dans `%AppData%\ColorFileConverter\logs`

- Statistiques complètes : durée, tailles, statuts**Ghostscript 10.05.1 déjà intégré !** Aucune installation requise.

- Option d'ouverture automatique

1. Téléchargez la version portable  

### 📁 **Mode surveillance automatique**2. Décompressez où vous voulez

- Conversion automatique des PDF déposés3. Lancez `Converter.Gui.exe`

- Surveillance en temps réel d'un dossier

- Traitement en arrière-plan### ⚙️ Installation personnalisée



### 🗂️ **Gestion intelligente des dossiers**Si vous souhaitez utiliser votre propre version de Ghostscript, le programme cherche dans l'ordre :

- **Nouveau** : Réutilise les fenêtres d'explorateur existantes

- Évite la multiplication des fenêtres1. Variable d'environnement `GHOSTSCRIPT_EXE`ur **(Avancé) Surveillance de dossier**

- Mise au premier plan automatique2. Sélectionnez le dossier à surveiller  

3. Tout PDF déposé sera automatiquement converti

## 🔧 Configuration automatique4. Le statut s'affiche en temps réel



L'interface simplifiée configure automatiquement les meilleurs paramètres selon vos besoins :### 👁️ Prévisualisation intelligente



| Type de couleur | Device Ghostscript | Compression | Usage optimal |- **Aperçu immédiat** : visualisez le résultat avant conversion

|-----------------|-------------------|-------------|---------------|- **Zoom adaptatif** : vérifiez la qualité en détail

| **N&B** | `tiffg4` | G4 | Documents, fax |- **Comparaison côte à côte** : PDF original vs TIFF converti1. **Lancez** l'application `Converter.Gui.exe`

| **Gris** | `tiffgray` | LZW | Scans, photos N&B |2. **Ajoutez** vos PDF (bouton **Ajouter des PDF** ou glisser-déposer)

| **Couleurs** | `tiff24nc` | LZW | Images, documents couleur |3. **Choisissez** le type de couleur : N&B, Gris ou Couleurs *(paramètres automatiques)*

4. **Sélectionnez** le dossier de sortie

## 📦 Installation et déploiement5. **Lancez** la conversion !



### ✅ Version portable (recommandée)> 💡 **Astuce** : Les paramètres sont automatiquement optimisés selon votre choix de couleur. Plus besoin de gérer les profils !ble** : intégration complète sans installation système requise  

- **🎨 Paramètres automatiques** : configuration intelligente selon le type de couleur choisi

**Ghostscript 10.05.1 déjà intégré !** Aucune installation requise.- **👁️ Prévisualisation en temps réel** : aperçu de la première page avec les paramètres actuels

- **📊 Journal détaillé** : suivi complet des conversions avec statistiques

1. Téléchargez l'archive portable- **🚫 Arrêt à tout moment** : annulation propre des conversions en cours

2. Décompressez où vous voulez- **🗂️ Gestion intelligente des dossiers** : réutilise les fenêtres d'explorateur existantes

3. Lancez `Converter.Gui.exe`- **📁 Surveillance automatique** : conversion automatique des PDF déposés dans un dossier

- **🖱️ Glisser-déposer** : interface intuitive pour ajouter vos fichiers2.1

### 📁 Structure portable

Application Windows Forms pour convertir rapidement des lots de PDF en TIFF, avec une interface moderne et intuitive. **Maintenant avec Ghostscript intégré - aucune installation requise !**

```

ColorFileConverter/## ✨ Nouveautés v2.1

├── 📄 Converter.Gui.exe          # Interface principale

├── 📄 Converter.Core.dll         # Moteur de conversion- **🚀 Ghostscript portable intégré** : Fonctionne immédiatement sans installation sur tout PC Windows

├── 📁 Resources/- **🎛️ Interface simplifiée** : Paramètres automatiques selon le type de couleur (N&B, Gris, Couleurs)

│   └── 📁 Ghostscript/           # Ghostscript portable intégré- **💡 Gestion intelligente des fenêtres** : Réutilise les fenêtres d'explorateur existantes

│       ├── gswin64c.exe          # Exécutable principal- **🔧 Compression optimisée** : Support complet de tous les types de compression TIFF

│       ├── gsdll64.dll           # Bibliothèque native- **🎯 Textes améliorés** : Messages plus clairs et intuitifs

│       └── ...                   # Fonts, ICC, etc.

└── 📁 (autres dépendances .NET)## Fonctionnalités principales

```

## Caractéristiques principales

### ⚙️ Installation personnalisée

- **🔄 Conversion par lots** : ajoutez plusieurs PDF via la boîte de dialogue ou en glisser-déposer

Pour utiliser votre propre Ghostscript, le programme cherche dans l'ordre :- **Profils personnalisables** : sélectionnez des profils Ghostscript prédéfinis (fax, niveaux de gris, couleur) ou créez les vôtres (device, compression, DPI, paramètres avancés).

- **Prévisualisation avant/après** : affiche la première page du PDF source et un aperçu TIFF généré avec le profil courant. Ajustez le zoom pour vérifier la qualité.

1. Variable d'environnement `GHOSTSCRIPT_EXE`- **Journal détaillé** : export automatique d’un fichier texte listant chaque conversion (durée, tailles d’entrée/sortie, statut). Option pour ouvrir le journal à la fin.

2. Dossier `Resources/Ghostscript/` (portable)- **Suivi de progression et annulation** : barre de progression globale et bouton « Arrêter » pour interrompre la conversion.

3. Ghostscript dans le `PATH` système- **Ouverture du fichier converti** : à l’issue du traitement, ouverture de l’explorateur Windows sur le premier fichier converti (désactivable).

- **Mode dossier surveillé** : conversion automatique de tout PDF déposé dans un dossier de veille.

## 🛠️ Développement- **Prise en charge du drag & drop** : glissez vos fichiers directement sur la liste pour les ajouter.



**Prérequis :**## Utilisation

- .NET 8 SDK

- Visual Studio 2022 ou VS Code1. Lancez l’application `Converter.Gui`.

2. Ajoutez un ou plusieurs PDF (bouton **Ajouter des PDF** ou glisser-déposer).

**Structure du projet :**3. Sélectionnez le profil de conversion souhaité ou créez-en un nouveau via **Gérer les profils**.

```4. Choisissez le dossier de sortie.

Converter.sln5. (Optionnel) Activez l’ouverture automatique de l’explorateur et/ou du journal.

├── 🎯 Converter.Gui/      # Interface Windows Forms6. Cliquez sur **Lancer la conversion**.

├── ⚙️ Converter.Core/     # Moteur de conversion  7. Surveillez la progression, prévisualisez la qualité, annulez si nécessaire.

├── 🖥️ Converter.Cli/      # Interface ligne de commande

└── 📦 Resources/          # Ghostscript portable### Mode dossier surveillé

```

1. Cochez **Activer le dossier surveillé**.

**Commandes utiles :**2. Sélectionnez le dossier à surveiller.

```bash3. Tout PDF ajouté dans ce dossier est converti automatiquement avec le profil actif.

dotnet build                         # Compilation4. Le statut du mode s’affiche sous la zone de sélection.

dotnet run --project Converter.Gui  # Lancement debug

```## Prévisualisation TIFF



## 🔧 Dépannage- La section de droite affiche la première page avant et après conversion.

- **Zoom adaptatif** : curseur pour inspecter les détails

| Problème | Solution |- **Actualisation manuelle** : bouton pour forcer un nouveau rendu

|----------|----------|

| **Ghostscript introuvable** | Vérifiez le dossier `Resources/Ghostscript/` |## 🔧 Configuration automatique

| **Prévisualisation vide** | Vérifiez les paramètres et relancez l'aperçu |

| **Surveillance inactive** | Contrôlez les permissions du dossier surveillé |L'application sélectionne automatiquement les meilleurs paramètres :

| **Erreur de compression** | Utilisez LZW ou G4 selon le type de document |

| **Fenêtres multiples** | La v2.1 réutilise automatiquement les fenêtres existantes || Type de couleur | Device | Compression | DPI | Optimisé pour |

|-----------------|--------|-------------|-----|---------------|

## 📈 Migration depuis v2.0| **N&B** | `tiffg4` | G4 | 150 | Documents texte, fax |

| **Gris** | `tiffgray` | LZW | 150 | Scans, photos N&B |

- **Interface simplifiée** : Les anciens profils sont remplacés par 3 paramètres directs| **Couleurs** | `tiff24nc` | LZW | 150 | Images, documents couleur |

- **Compatibilité** : Tous vos anciens PDF peuvent être traités avec les nouveaux paramètres

- **Performance** : Conversion plus rapide et gestion mémoire optimisée> 💡 Vous pouvez toujours ajuster manuellement via les options avancées



---## 📊 Journal et suivi



**🚀 Color File Converter v2.1** - Conversion PDF → TIFF simplifiée et professionnelle !Un fichier texte est généré dans `%AppData%\ColorFileConverter\logs`. Il liste :

- Nom du fichier
- Durée de conversion
- Taille d’entrée et de sortie
- Statut et message d’erreur éventuel

Activez l’ouverture automatique via la case **Afficher le journal détaillé**.

## Utilisation sans installation de Ghostscript

Le programme cherche Ghostscript dans l’ordre suivant :

1. Variable d’environnement `GHOSTSCRIPT_EXE`
2. Dossier `Ghostscript` situé à côté de l’exécutable (`Converter.Gui.exe`)
3. Exécutable présent dans le `PATH` (`gswin64c`, `gswin32c`, `gs`…)

Pour un poste Windows sans installation Ghostscript :

1. Téléchargez Ghostscript depuis <https://ghostscript.com/releases/index.html> (version 64 bits recommandée).
2. Copiez le contenu des dossiers `bin`, `lib` et `Resource` de Ghostscript dans un sous-dossier `Ghostscript` situé à côté de `Converter.Gui.exe`. Exemple d’arborescence :

```
ColorFileConverter/
├── 📄 Converter.Gui.exe          # Interface principale  
├── 📄 Converter.Core.dll         # Moteur de conversion
├── 📁 Resources/
│   └── 📁 Ghostscript/           # Ghostscript portable intégré
│       ├── gswin64c.exe          # Exécutable principal
│       ├── gsdll64.dll           # Bibliothèque native
│       └── (autres fichiers...)   # Fonts, ICC, etc.
└── 📁 (autres dépendances .NET)
```

## 🛠️ Développement

3. Vérifiez que `gswin64c.exe` et `gsdll64.dll` sont bien présents dans `Ghostscript`. L’application utilisera automatiquement cette copie portable.

Sur macOS/Linux, créez également un dossier `Ghostscript` contenant l’exécutable (`gs`) et ses ressources si nécessaire.

## Dépannage

- **Ghostscript introuvable** : assurez-vous que l’exécutable et les bibliothèques sont accessibles (voir section ci-dessus) ou définissez `GHOSTSCRIPT_EXE` vers le binaire.
- **Prévisualisation vide** : sélectionnez un profil valide, vérifiez que Ghostscript fonctionne et relancez l’aperçu.
- **Dossier surveillé inactif** : confirmez les permissions d’écriture/lecture sur le dossier et que l’option est bien cochée.

## Développement

- Les profils et paramètres utilisateurs sont enregistrés sous `%AppData%\ColorFileConverter`.
- Le projet cible .NET 8 et utilise Windows Forms.
