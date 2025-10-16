# 🧪 Guide de Test - Interface Graphique

> Comment tester rapidement l'interface WinForms de Color File Converter

---

## 🚀 Démarrage Rapide (2 minutes)

### Option 1: Depuis Visual Studio

```powershell
# 1. Ouvrir la solution
cd c:\Code\color-file-converter
start Converter.sln

# 2. Dans Visual Studio:
# - Cliquer droit sur "Converter.Gui" dans l'Explorateur de solutions
# - "Définir comme projet de démarrage"
# - Appuyer sur F5 (ou cliquer sur ▶ Démarrer)
```

### Option 2: Build et Run depuis PowerShell

```powershell
# 1. Build le projet GUI
cd c:\Code\color-file-converter
dotnet build Converter.Gui\Converter.Gui.csproj -c Debug

# 2. Lancer l'application
.\Converter.Gui\bin\Debug\net8.0-windows\Converter.Gui.exe
```

### Option 3: Build Release et Test

```powershell
# Build en mode Release (plus rapide)
dotnet build Converter.Gui\Converter.Gui.csproj -c Release

# Lancer
.\Converter.Gui\bin\Release\net8.0-windows\Converter.Gui.exe
```

---

## 🎯 Checklist de Test UX

### Test 1: Premier Lancement (30 secondes)
- [ ] L'application démarre sans erreur
- [ ] L'interface est visible et responsive
- [ ] Tous les contrôles sont bien positionnés
- [ ] Pas de texte tronqué ou qui déborde

**Comment tester**:
```powershell
# Lancer et observer
.\Converter.Gui\bin\Debug\net8.0-windows\Converter.Gui.exe
```

---

### Test 2: Drag & Drop (1 minute)

**Étapes**:
1. Trouver un fichier PDF sur votre bureau
2. Glisser-déposer dans la fenêtre de l'application
3. Observer le feedback visuel

**Points à vérifier**:
- [ ] La zone de drop est clairement visible
- [ ] Indication visuelle quand on survole (couleur change)
- [ ] Le fichier apparaît dans la liste après drop
- [ ] Message de confirmation ou statut visible

**Créer un PDF de test**:
```powershell
# Si vous n'avez pas de PDF, en télécharger un simple
Invoke-WebRequest -Uri "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf" -OutFile "$env:USERPROFILE\Desktop\test.pdf"
```

---

### Test 3: Sélection de Paramètres (30 secondes)

**Étapes**:
1. Regarder la section "Paramètres"
2. Changer la résolution (DPI)
3. Changer le type de compression
4. Changer le type de couleurs

**Points à vérifier**:
- [ ] Les combos/contrôles sont faciles à trouver
- [ ] Les labels sont clairs (pas de jargon technique brut)
- [ ] Les changements sont visibles
- [ ] Des tooltips apparaissent au survol (si implémenté)

---

### Test 4: Prévisualisation (1 minute)

**Étapes**:
1. Charger un PDF avec plusieurs pages
2. Observer la zone de prévisualisation
3. Essayer de naviguer entre les pages (si implémenté)
4. Essayer le zoom (si implémenté)

**Points à vérifier**:
- [ ] L'aperçu se charge
- [ ] La qualité est acceptable
- [ ] Navigation entre pages fonctionne
- [ ] Zoom est fluide

---

### Test 5: Conversion (2 minutes)

**Étapes**:
1. Charger un fichier PDF
2. Sélectionner un dossier de sortie
3. Cliquer sur "Convertir"
4. Observer la progression

**Points à vérifier**:
- [ ] Le bouton "Convertir" est bien visible
- [ ] Une barre de progression apparaît
- [ ] Le statut est mis à jour (fichier actuel, etc.)
- [ ] Un message de succès s'affiche à la fin
- [ ] Le fichier TIFF est créé dans le bon dossier

**Où chercher le résultat**:
```powershell
# Par défaut, les fichiers vont probablement dans le même dossier que le PDF
# ou dans un dossier "Output" configurable
```

---

### Test 6: Gestion d'Erreurs (1 minute)

**Étapes**:
1. Essayer de convertir sans fichier sélectionné
2. Essayer avec un fichier non-PDF
3. Essayer avec un fichier PDF corrompu

**Points à vérifier**:
- [ ] Message d'erreur clair (pas de crash)
- [ ] Indication de ce qui ne va pas
- [ ] Suggestion de comment corriger
- [ ] L'app reste utilisable après l'erreur

**Créer un fichier invalide pour tester**:
```powershell
# Créer un faux PDF
echo "Ce n'est pas un vrai PDF" > "$env:USERPROFILE\Desktop\fake.pdf"
```

---

## 🎨 Test de l'Apparence (UI/UX)

### Points à Vérifier Visuellement

```
┌─────────────────────────────────────────────┐
│ Checklist Visuelle                          │
├─────────────────────────────────────────────┤
│ [ ] Police lisible (pas trop petite)        │
│ [ ] Couleurs agréables (pas de clash)       │
│ [ ] Espacement suffisant entre éléments     │
│ [ ] Icônes/emojis visibles (si présents)    │
│ [ ] Bordures et séparateurs clairs          │
│ [ ] Pas de pixels mal alignés               │
│ [ ] Boutons avec taille tactile (>30px)     │
│ [ ] Feedback visuel sur hover               │
│ [ ] Curseur change au survol (Cursor.Hand)  │
└─────────────────────────────────────────────┘
```

---

## 🔍 Tests Avancés

### Test de Performance

**Charger Plusieurs Fichiers**:
```powershell
# Créer 10 copies du même PDF pour tester
for ($i=1; $i -le 10; $i++) {
    Copy-Item "$env:USERPROFILE\Desktop\test.pdf" "$env:USERPROFILE\Desktop\test$i.pdf"
}
```

**Points à vérifier**:
- [ ] L'interface reste responsive
- [ ] Le drag & drop de 10+ fichiers fonctionne
- [ ] La liste défile si trop de fichiers
- [ ] La conversion en batch fonctionne

---

### Test de Redimensionnement

**Étapes**:
1. Lancer l'application
2. Redimensionner la fenêtre (petit → grand → petit)
3. Observer le comportement

**Points à vérifier**:
- [ ] Les contrôles se réorganisent correctement
- [ ] Pas de texte coupé
- [ ] Barres de défilement apparaissent si nécessaire
- [ ] Proportions gardées

---

### Test Multi-Écrans (si applicable)

**Étapes**:
1. Déplacer l'app sur un deuxième écran
2. Observer si tout s'affiche correctement

**Points à vérifier**:
- [ ] Pas de problème de DPI/scaling
- [ ] Dialogs s'ouvrent sur le bon écran
- [ ] Pas de contrôles invisibles

---

## 🐛 Tests de Robustesse

### Scénarios Edge Cases

```powershell
# Test 1: PDF très gros (>100 pages)
# Télécharger un gros PDF de test
Invoke-WebRequest -Uri "https://www.adobe.com/content/dam/acom/en/devnet/pdf/pdfs/PDF32000_2008.pdf" -OutFile "$env:USERPROFILE\Desktop\big.pdf"

# Test 2: PDF protégé par mot de passe
# (Créer ou télécharger un PDF protégé)

# Test 3: Plusieurs instances de l'app
# Lancer 2x l'application et voir si ça pose problème
```

**Points à vérifier**:
- [ ] Pas de crash avec gros fichiers
- [ ] Message clair si PDF protégé
- [ ] Pas de conflit entre instances

---

## 📊 Checklist Complète de Test

### Fonctionnel
- [ ] Charger fichiers (bouton)
- [ ] Charger fichiers (drag & drop)
- [ ] Retirer fichier sélectionné
- [ ] Vider la liste
- [ ] Changer paramètres
- [ ] Sélectionner dossier sortie
- [ ] Lancer conversion
- [ ] Annuler conversion en cours
- [ ] Voir les logs
- [ ] Fermer l'application

### UX
- [ ] Interface intuitive (utilisable sans aide)
- [ ] Feedback visuel immédiat
- [ ] Messages clairs (pas de jargon)
- [ ] Shortcuts clavier fonctionnent
- [ ] Tooltips informatifs

### Performance
- [ ] Démarrage rapide (<3 secondes)
- [ ] Interface réactive (pas de freeze)
- [ ] Conversion progresse en temps réel
- [ ] Mémoire raisonnable

### Stabilité
- [ ] Pas de crash
- [ ] Erreurs gérées proprement
- [ ] Récupération après erreur
- [ ] Pas de fuites mémoire

---

## 🛠️ Debug Si Problème

### L'application ne démarre pas

```powershell
# Vérifier les erreurs de compilation
dotnet build Converter.Gui\Converter.Gui.csproj -c Debug

# Voir les erreurs détaillées
dotnet run --project Converter.Gui\Converter.Gui.csproj
```

### L'interface est vide/bizarre

```powershell
# Vérifier que tous les packages sont restaurés
dotnet restore Converter.sln

# Rebuild complet
dotnet clean
dotnet build
```

### Erreur au runtime

```powershell
# Lancer avec logs détaillés
$env:DOTNET_ENVIRONMENT="Development"
dotnet run --project Converter.Gui\Converter.Gui.csproj --verbosity detailed
```

### Voir les logs de l'application

```powershell
# Ouvrir le fichier de logs (si configuré)
notepad "$env:APPDATA\ColorFileConverter\converter.log"

# Ou dans le dossier de l'app
Get-Content ".\Converter.Gui\bin\Debug\net8.0-windows\*.log" -Wait
```

---

## 🎯 Tests Visuels avec Screenshots

### Créer un Rapport Visuel

```powershell
# Script pour capturer des screenshots pendant le test
# (Utiliser Snipping Tool ou Win+Shift+S)

# États à capturer:
# 1. Interface au démarrage
# 2. Zone de drop au survol
# 3. Liste avec fichiers chargés
# 4. Prévisualisation active
# 5. Conversion en cours
# 6. Message de succès
```

---

## 📝 Template de Rapport de Test

```markdown
# Rapport de Test GUI - [Date]

## Configuration
- OS: Windows 11
- .NET: 8.0
- Résolution: 1920x1080

## Tests Effectués

### ✅ Réussis
- [x] Démarrage de l'app
- [x] Drag & drop de fichier
- [x] Conversion simple

### ❌ Échecs
- [ ] Prévisualisation ne charge pas
- [ ] Zoom non fonctionnel

### 💡 Suggestions
- Agrandir la zone de drop
- Ajouter icônes dans les boutons
- Améliorer contraste des couleurs

## Screenshots
[Insérer ici]

## Conclusion
Interface fonctionnelle mais quelques améliorations UX recommandées.
```

---

## 🚀 Quick Test (30 secondes)

Pour un test ultra-rapide, exécutez juste ça :

```powershell
# Build et run en une commande
dotnet run --project Converter.Gui\Converter.Gui.csproj
```

Ensuite :
1. Glissez un PDF → OK ?
2. Cliquez "Convertir" → OK ?
3. Fichier créé ? → OK ?

Si les 3 fonctionnent → **Interface OK** ✅

---

## 💡 Conseils de Test

### Pour Tester l'UX Réellement
1. **Donner à quelqu'un d'autre** - Quelqu'un qui ne connaît pas l'app
2. **Observer sans aider** - Voir où il clique, cherche
3. **Noter les hésitations** - Si l'utilisateur hésite, c'est un problème UX
4. **Demander "pense à voix haute"** - Comprendre son raisonnement

### Red Flags UX
- ❌ "Je ne sais pas où cliquer"
- ❌ "C'est quoi ce paramètre ?"
- ❌ "Ça a marché ?"
- ❌ "Comment j'annule ?"

### Good Signs UX
- ✅ Utilise sans lire la doc
- ✅ Trouve tout du premier coup
- ✅ "Ah c'est simple !"
- ✅ "C'est intuitif"

---

**Prochain test** : Lancez l'app maintenant et faites les 5 premiers tests (10 minutes) ! 🎯
