# Color File Conv- **⚡ Ghostscri## 🚀 Utilisatio### 📁 Mode surveillance automatique
- **Journal automatique** : cha2. Dossier `Ghostscript` **Prérequis :**
- .NET 8 SDK
- Visual Studio 2022 ou VS Code

**Structure du projet :**
```
Converter.sln
├── 🎯 Converter.Gui/      # Interface Windows Forms
├── ⚙️ Converter.Core/     # Moteur de conversion  
├── 🖥️ Converter.Cli/      # Interface ligne de commande
└── 📦 Resources/          # Ghostscript portable
```

**Commandes utiles :**
```bash
dotnet build                    # Compilation
dotnet run --project Converter.Gui  # Lancement en debug
```

## 🔧 Dépannage

| Problème | Solution |
|----------|----------|
| **Ghostscript introuvable** | Vérifiez le dossier `Resources/Ghostscript/` |
| **Prévisualisation vide** | Vérifiez les paramètres et relancez l'aperçu |
| **Surveillance inactive** | Contrôlez les permissions du dossier surveillé |

---

**🚀 Color File Converter v2.1** - Conversion PDF → TIFF simplifiée et portable !e
3. Ghostscript dans le `PATH` système

## 🏗️ Structure du projetversion est enregistrée dans `%AppData%\ColorFileConverter\logs`
- **Statistiques détaillées** : nom, durée, tailles, statut
- **Ouverture automatique** : option pour consulter le journal après conversion

## 💼 Installation et déploiement

### ✅ Version portable (recommandée)

**Ghostscript 10.05.1 déjà intégré !** Aucune installation requise.

1. Téléchargez la version portable  
2. Décompressez où vous voulez
3. Lancez `Converter.Gui.exe`

### ⚙️ Installation personnalisée

Si vous souhaitez utiliser votre propre version de Ghostscript, le programme cherche dans l'ordre :

1. Variable d'environnement `GHOSTSCRIPT_EXE`ur **(Avancé) Surveillance de dossier**
2. Sélectionnez le dossier à surveiller  
3. Tout PDF déposé sera automatiquement converti
4. Le statut s'affiche en temps réel

### 👁️ Prévisualisation intelligente

- **Aperçu immédiat** : visualisez le résultat avant conversion
- **Zoom adaptatif** : vérifiez la qualité en détail
- **Comparaison côte à côte** : PDF original vs TIFF converti1. **Lancez** l'application `Converter.Gui.exe`
2. **Ajoutez** vos PDF (bouton **Ajouter des PDF** ou glisser-déposer)
3. **Choisissez** le type de couleur : N&B, Gris ou Couleurs *(paramètres automatiques)*
4. **Sélectionnez** le dossier de sortie
5. **Lancez** la conversion !

> 💡 **Astuce** : Les paramètres sont automatiquement optimisés selon votre choix de couleur. Plus besoin de gérer les profils !ble** : intégration complète sans installation système requise  
- **🎨 Paramètres automatiques** : configuration intelligente selon le type de couleur choisi
- **👁️ Prévisualisation en temps réel** : aperçu de la première page avec les paramètres actuels
- **📊 Journal détaillé** : suivi complet des conversions avec statistiques
- **🚫 Arrêt à tout moment** : annulation propre des conversions en cours
- **🗂️ Gestion intelligente des dossiers** : réutilise les fenêtres d'explorateur existantes
- **📁 Surveillance automatique** : conversion automatique des PDF déposés dans un dossier
- **🖱️ Glisser-déposer** : interface intuitive pour ajouter vos fichiers2.1

Application Windows Forms pour convertir rapidement des lots de PDF en TIFF, avec une interface moderne et intuitive. **Maintenant avec Ghostscript intégré - aucune installation requise !**

## ✨ Nouveautés v2.1

- **🚀 Ghostscript portable intégré** : Fonctionne immédiatement sans installation sur tout PC Windows
- **🎛️ Interface simplifiée** : Paramètres automatiques selon le type de couleur (N&B, Gris, Couleurs)
- **💡 Gestion intelligente des fenêtres** : Réutilise les fenêtres d'explorateur existantes
- **🔧 Compression optimisée** : Support complet de tous les types de compression TIFF
- **🎯 Textes améliorés** : Messages plus clairs et intuitifs

## Fonctionnalités principales

## Caractéristiques principales

- **🔄 Conversion par lots** : ajoutez plusieurs PDF via la boîte de dialogue ou en glisser-déposer
- **Profils personnalisables** : sélectionnez des profils Ghostscript prédéfinis (fax, niveaux de gris, couleur) ou créez les vôtres (device, compression, DPI, paramètres avancés).
- **Prévisualisation avant/après** : affiche la première page du PDF source et un aperçu TIFF généré avec le profil courant. Ajustez le zoom pour vérifier la qualité.
- **Journal détaillé** : export automatique d’un fichier texte listant chaque conversion (durée, tailles d’entrée/sortie, statut). Option pour ouvrir le journal à la fin.
- **Suivi de progression et annulation** : barre de progression globale et bouton « Arrêter » pour interrompre la conversion.
- **Ouverture du fichier converti** : à l’issue du traitement, ouverture de l’explorateur Windows sur le premier fichier converti (désactivable).
- **Mode dossier surveillé** : conversion automatique de tout PDF déposé dans un dossier de veille.
- **Prise en charge du drag & drop** : glissez vos fichiers directement sur la liste pour les ajouter.

## Utilisation

1. Lancez l’application `Converter.Gui`.
2. Ajoutez un ou plusieurs PDF (bouton **Ajouter des PDF** ou glisser-déposer).
3. Sélectionnez le profil de conversion souhaité ou créez-en un nouveau via **Gérer les profils**.
4. Choisissez le dossier de sortie.
5. (Optionnel) Activez l’ouverture automatique de l’explorateur et/ou du journal.
6. Cliquez sur **Lancer la conversion**.
7. Surveillez la progression, prévisualisez la qualité, annulez si nécessaire.

### Mode dossier surveillé

1. Cochez **Activer le dossier surveillé**.
2. Sélectionnez le dossier à surveiller.
3. Tout PDF ajouté dans ce dossier est converti automatiquement avec le profil actif.
4. Le statut du mode s’affiche sous la zone de sélection.

## Prévisualisation TIFF

- La section de droite affiche la première page avant et après conversion.
- **Zoom adaptatif** : curseur pour inspecter les détails
- **Actualisation manuelle** : bouton pour forcer un nouveau rendu

## 🔧 Configuration automatique

L'application sélectionne automatiquement les meilleurs paramètres :

| Type de couleur | Device | Compression | DPI | Optimisé pour |
|-----------------|--------|-------------|-----|---------------|
| **N&B** | `tiffg4` | G4 | 150 | Documents texte, fax |
| **Gris** | `tiffgray` | LZW | 150 | Scans, photos N&B |
| **Couleurs** | `tiff24nc` | LZW | 150 | Images, documents couleur |

> 💡 Vous pouvez toujours ajuster manuellement via les options avancées

## 📊 Journal et suivi

Un fichier texte est généré dans `%AppData%\ColorFileConverter\logs`. Il liste :

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
