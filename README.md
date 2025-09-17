# Color File Converter

Application Windows Forms pour convertir rapidement des lots de PDF en TIFF, avec une interface moderne inspirée des outils PDF24.

## Fonctionnalités principales

- **Conversion par lots** : ajoutez plusieurs PDF via la boîte de dialogue ou en glisser-déposer et convertissez-les en une seule opération.
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
- Utilisez le curseur de zoom pour inspecter rapidement les détails.
- Le bouton **Actualiser la prévisualisation** force un rendu avec les paramètres courants.

## Profils de conversion

Chaque profil contient :

- `Device` Ghostscript (ex. `tiffg4`, `tiffgray`, `tiff24nc`)
- Option de compression (`lzw`, `jpeg`, `packbits`, …)
- Résolution en DPI
- Paramètres additionnels (un par ligne, ex. `-dDownScaleFactor=2`)

Les profils sont stockés dans `%AppData%\ColorFileConverter\profiles.json`.

## Journal et rapports

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
ColorFileConverter
├─ Converter.Gui.exe
└─ Ghostscript
   ├─ gswin64c.exe
   ├─ gsdll64.dll
   ├─ gsdll64.lib (optionnel)
   ├─ fonts\*
   ├─ iccprofiles\*
   ├─ lib\*
   └─ Resource\*
```

3. Vérifiez que `gswin64c.exe` et `gsdll64.dll` sont bien présents dans `Ghostscript`. L’application utilisera automatiquement cette copie portable.

Sur macOS/Linux, créez également un dossier `Ghostscript` contenant l’exécutable (`gs`) et ses ressources si nécessaire.

## Dépannage

- **Ghostscript introuvable** : assurez-vous que l’exécutable et les bibliothèques sont accessibles (voir section ci-dessus) ou définissez `GHOSTSCRIPT_EXE` vers le binaire.
- **Prévisualisation vide** : sélectionnez un profil valide, vérifiez que Ghostscript fonctionne et relancez l’aperçu.
- **Dossier surveillé inactif** : confirmez les permissions d’écriture/lecture sur le dossier et que l’option est bien cochée.

## Développement

- Les profils et paramètres utilisateurs sont enregistrés sous `%AppData%\ColorFileConverter`.
- Le projet cible .NET 8 et utilise Windows Forms.
