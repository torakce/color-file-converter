# Comment inclure Ghostscript portable

## Méthode recommandée : Binaires pré-intégrés dans le repository

### Étapes pour intégrer Ghostscript :

1. **Télécharger Ghostscript** depuis https://www.ghostscript.com/releases/gsdnld.html
   - Choisir "GPL Ghostscript 10.03.1 for Windows (64 bit)"

2. **Installer temporairement** sur votre machine de développement

3. **Copier les binaires** depuis le dossier d'installation :
   ```
   C:\Program Files\gs\gs10.03.1\bin\
   ```

4. **Fichiers essentiels à copier dans ce dossier** :
   - `gswin64c.exe` (exécutable principal)
   - `gsdll64.dll` (bibliothèque principale)
   - `gsdll64.lib` (si présent)
   - Autres DLLs de dépendance

5. **Structure finale souhaitée** :
   ```
   Resources/
   └── Ghostscript/
       ├── gswin64c.exe
       ├── gsdll64.dll
       └── (autres DLLs)
   ```

### Avantages de cette approche :

✅ **Aucune installation** nécessaire lors du build
✅ **Builds reproductibles** - mêmes binaires à chaque fois  
✅ **Pas de dépendance réseau** lors de la compilation
✅ **Contrôle de version** des binaires Ghostscript
✅ **Builds hors ligne** possibles

### Alternative : Téléchargement automatique

Si vous préférez télécharger automatiquement, utilisez les scripts :
- `Build-with-Ghostscript.ps1` (téléchargement automatique)
- `Build-Simple-Portable.ps1` (binaires pré-intégrés)

### Licence

Ghostscript est distribué sous licence GPL. Assurez-vous de respecter les termes de la licence GPL si vous redistribuez les binaires.