# 📖 Guide de Distribution - GitHub Release

## 🎯 **MÉTHODE RECOMMANDÉE : GitHub Releases**

### 🔧 **Prérequis**
- Projet compilable localement
- Accès push sur GitHub
- PowerShell disponible

---

## 🚀 **Méthode 1 : Automatique (Recommandée)**

### ✅ **Avec GitHub Actions (Configuré)**

1. **Créer un tag version :**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **GitHub Actions fera automatiquement :**
   - ✅ Compilation optimisée 
   - ✅ Création du ZIP
   - ✅ Publication de la release
   - ✅ Upload du binaire

3. **Résultat :** Release disponible sur `https://github.com/torakce/color-file-converter/releases`

---

## 📋 **Méthode 2 : Manuelle**

### 🔨 **Étape 1 : Compilation locale**
```powershell
# Aller dans le projet
cd C:\Code\color-file-converter

# Compiler la version optimisée
powershell -ExecutionPolicy Bypass -File "scripts\Build-Portable-Light.ps1" -CleanBuild
```

### 📦 **Étape 2 : Créer le ZIP**
```powershell
# Créer le ZIP de distribution
Compress-Archive -Path "Release\ColorFileConverter\*" -DestinationPath "ColorFileConverter-v1.0.0.zip" -Force
```

### 🌐 **Étape 3 : Créer la Release GitHub**

1. **Aller sur GitHub :** `https://github.com/torakce/color-file-converter`

2. **Cliquer "Releases" → "Create a new release"**

3. **Remplir les informations :**

   **Tag version :** `v1.0.0`
   
   **Release title :** `Color File Converter v1.0.0`
   
   **Description :**
   ```markdown
   ## 🎉 Color File Converter v1.0.0
   
   ### 📥 Téléchargement :
   
   **ColorFileConverter-v1.0.0.zip** (~194 MB)
   
   ### ✨ Fonctionnalités :
   - ✅ **Conversion PDF → TIFF** (mono-page et multi-page)  
   - ✅ **Aperçu avec navigation par pages** (◀ ▶)
   - ✅ **Interface intuitive** avec zoom et pan
   - ✅ **Profils personnalisables** pour différents besoins
   - ✅ **Mode portable** - aucune installation requise
   - ✅ **Structure propre** - exe facilement accessible
   
   ### 🚀 Installation :
   1. **Télécharger** le fichier ZIP
   2. **Extraire** dans un dossier
   3. **Double-cliquer** sur `Converter.Gui.exe` (visible à la racine!)
   4. **C'est tout !** - Pas besoin de chercher dans des sous-dossiers
   
   ### 📂 Structure :
   ```
   ColorFileConverter/
   ├── Converter.Gui.exe     ← Lancez directement ce fichier !
   ├── README.txt           
   └── Runtime/             ← Toutes les dépendances
       ├── *.dll
       └── Ghostscript/
   ```
   ```

4. **Uploader le ZIP** dans "Attach binaries"

5. **Cliquer "Publish release"**

---

## 🎯 **Résultat Final**

### 👤 **Pour les utilisateurs :**

1. **Accès facile :** `https://github.com/torakce/color-file-converter/releases`

2. **Téléchargement simple :** 
   - Clic sur le fichier ZIP
   - Téléchargement automatique

3. **Installation intuitive :**
   - Extraire le ZIP
   - Double-clic sur `Converter.Gui.exe` (visible à la racine)
   - **Aucune recherche dans des sous-dossiers !**

### 📊 **Avantages de cette approche :**

- ✅ **Exe principal visible immédiatement**
- ✅ **Structure propre et professionnelle**  
- ✅ **Dépendances organisées dans Runtime/**
- ✅ **Taille optimisée (~194 MB)**
- ✅ **Statistiques de téléchargement GitHub**
- ✅ **Gestion des versions automatique**
- ✅ **Interface utilisateur familière (GitHub)**

---

## 🔄 **Mises à jour futures**

Pour publier une nouvelle version :

```bash
# Méthode automatique
git tag v1.1.0
git push origin v1.1.0

# Ou méthode manuelle
# 1. Recompiler avec Build-Portable-Light.ps1
# 2. Créer nouveau ZIP  
# 3. Créer nouvelle release GitHub
```

---

## ⚡ **Notes importantes**

- **Structure finale :** `Converter.Gui.exe` à la racine = **facilement trouvable**
- **Dépendances :** Toutes dans `Runtime/` = **organisation claire**
- **Taille :** ~194 MB (optimisé avec Ghostscript minimal)
- **Compatibilité :** Windows 10/11 x64 uniquement
- **Installation :** Aucune - portable complet