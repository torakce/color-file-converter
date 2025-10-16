# Release Notes - v2.2.0

## 🎉 Enhanced GUI with Advanced Features

This release brings major improvements to the GUI with a focus on performance, usability, and user experience.

---

## ✨ New Features

### 🚀 Performance Optimization
- **Preview Cache System**: Intelligent caching of converted previews (up to 3 files)
  - Instant preview reload when switching between files
  - No unnecessary Ghostscript reconversions
  - Automatic memory management with FIFO eviction
  - Status indicator: "Aperçu (depuis cache)" when using cached preview

### 📑 Multi-Page Navigation
- **ThumbnailStripPanel**: Horizontal scrollable thumbnail strip for multi-page PDFs
  - Clickable thumbnails (100×135px) for instant page navigation
  - Horizontal scrollbar with mouse wheel support
  - Synchronized selection with current page
  - Auto-hide for single-page documents

### 📊 Progress Tracking
- **ConversionProgressPanel**: Enhanced progress display during conversion
  - Current file name display
  - Color-coded progress bar (blue → orange → green)
  - Real-time conversion speed (pages/sec)
  - Estimated time remaining (ETA)
  - Auto-hide after 2 seconds upon completion

### 🔍 Advanced Zoom
- **Ctrl+MouseWheel Zoom**: Zoom centered on cursor position
  - 10% increment per scroll
  - Smooth pan adjustment to keep focus
  - Synchronized with zoom trackbar

### 🎨 Visual Improvements
- **Enhanced PresetCards**: Custom GDI+ rendered icons
  - Black & White: Solid black square
  - Grayscale: Gradient vertical bars
  - Color: RGB vertical bars (Red/Green/Blue)
- **Fixed DropZonePanel**: Corrected border calculations for drag & drop

---

## 🔧 Improvements

### Simplified Workflow
- **Auto-suffix for duplicates**: No more file overwrite prompts
  - Automatic naming: `file.tif` → `file_1.tif` → `file_2.tif`
  - Removed "overwrite files" checkbox (no longer needed)
  
- **Smart folder opening**: Always opens output folder after conversion
  - Removed "open folder" checkbox (always active)
  - Intelligent folder detection (reuses existing Explorer window)

### Better UX
- **Removed "Custom" preset**: Simplified interface
  - Parameters always visible
  - Direct control over conversion settings
  
- **Improved memory management**
  - Automatic cache cleanup on application close
  - Proper disposal of cached preview images
  - FIFO eviction when cache limit reached (3 files)

---

## 🛠️ Technical Details

### New Components
- `Converter.Gui/Controls/ThumbnailStripPanel.cs` (353 lines)
- `Converter.Gui/Controls/ConversionProgressPanel.cs` (278 lines)
- `Converter.Gui/Controls/PresetCard.cs` (276 lines)
- `Converter.Gui/Controls/DropZonePanel.cs`

### Core Enhancements
- `BatchConversionService.GetUniqueOutputPath()`: Auto-suffix logic
- `MainForm._previewPagesCache`: Dictionary-based preview cache with max 3 entries
- `GhostscriptRunner`: Enhanced with improved error handling

### Key Algorithms
- **Cache Key**: `"{filepath}_{dpi}_{compression}_{device}_{monopages}"`
- **FIFO Eviction**: Oldest cache entry removed when limit exceeded
- **Image Cloning**: Cached images are clones to prevent disposal issues
- **Zoom Calculation**: Centered on cursor with pan offset adjustment

---

## 📋 Files Modified/Added

### Modified Files
- `Converter.Gui/MainForm.cs` (2,690 lines)
  - Added preview cache system
  - Integrated new controls
  - Enhanced zoom and navigation
  
- `Converter.Core/BatchConversionService.cs`
  - Added `GetUniqueOutputPath()` method
  
- `Converter.Gui/MainForm.Presets.cs`
  - Preset management logic

### New Files
- 4 custom controls (ThumbnailStripPanel, ConversionProgressPanel, PresetCard, DropZonePanel)
- 13 documentation files (IMPROVEMENTS.md, GUIDE-TEST-GUI.md, etc.)

---

## 🧪 Testing

All features have been tested and validated:
- ✅ Thumbnail strip with horizontal scrolling
- ✅ Page navigation (buttons + thumbnails)
- ✅ Synchronized thumbnail selection
- ✅ Zoom trackbar and Ctrl+MouseWheel
- ✅ Preview cache (file switching and parameter changes)
- ✅ ConversionProgressPanel animations
- ✅ Drag & drop border animations
- ✅ All presets (Black & White, Grayscale, Color)

---

## 🔄 Migration Notes

No breaking changes. This release is fully backward compatible with v2.1.

---

## 📦 Installation

1. Download the release package
2. Extract to desired location
3. Run `Converter.Gui.exe`

**Requirements:**
- .NET 8.0 Runtime
- Windows 10/11
- Ghostscript (included in portable version)

---

## 🙏 Acknowledgments

Built with .NET 8.0, WinForms, and Ghostscript.

---

## 🐛 Known Issues

None reported. Please report any issues on GitHub.

---

**Full Changelog**: https://github.com/torakce/color-file-converter/compare/v2.1.0...v2.2.0
