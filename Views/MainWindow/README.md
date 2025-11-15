# MainWindow Structure Overview

## 📁 Folder Organization

This folder contains the main application window and all its partial class implementations, organized by functional responsibility.

```
MainWindow/
├── MainWindow.xaml              → UI Layout
├── MainWindow.xaml.cs           → Main initialization & state management
│
├── Features/                    → Feature-specific implementations
│   ├── ImageManagement/        → Loading, workspace, drag & drop
│   ├── ImageProcessing/        → Single-image operations
│   ├── ImageOperations/        → Multi-image operations (arithmetic, binary)
│   ├── ImageAnalysis/          → Histogram, grayscale comparison
│   └── ImageTransform/         → Rotation, geometric transforms
│
└── UserInteraction/            → User interface event handlers
    ├── ToolbarHandlers.cs      → Toolbar button/toggle handlers
    ├── FilterTab.cs            → Filter tab management
    ├── Zoom.cs                 → Zoom controls (Ctrl+Scroll, up to 50x)
    └── WindowInterop.cs        → Horizontal scroll (Shift+Scroll)
```

---

## 🎯 Feature Categories

### 📂 **ImageManagement** (3 files)
Handles image loading, workspace management, and file operations.

- **MainWindow.ImageLoading.cs**
  - Load image via button/dialog
  - Save processed images
  - Initialize UI after image load
  - Update zoom/workspace state

- **MainWindow.Workspace.cs**
  - Reset workspace to initial state
  - Clear all toggles and panels
  - Disable all features when no image
  - Sidebar show/hide management

- **MainWindow.DragDrop.cs**
  - Drag & drop file handling
  - Visual feedback during drag
  - Image A/B selection dialog for multi-image ops
  - File validation

### 🎨 **ImageProcessing** (5 files)
Single-image processing operations.

- **MainWindow.Brightness.cs**
  - Brightness slider (-255 to +255)
  - Real-time brightness adjustment
  - Apply to current image

- **MainWindow.Negation.cs**
  - Toggle negation (invert colors)
  - Fast pixel-wise inversion

- **MainWindow.Filtering.cs**
  - Blur, Sharpen, Edge Detection
  - Emboss, Gaussian Blur
  - Convolution-based filters
  - Filter tab integration

- **MainWindow.BinaryThreshold.cs**
  - Grayscale conversion
  - Binary threshold slider (0-255)
  - Black/white image output

- **MainWindow.ColorSelection.cs**
  - Interactive color picker
  - Click image to select color
  - Display RGB values

### ➕ **ImageOperations** (2 files)
Multi-image operations (requires Image A and Image B).

- **MainWindow.Arithmetic.cs**
  - **A + B**: Add with offset, clamping [0,255]
  - **A - B**: Subtract, clamp negative to 0
  - **A × B**: Multiply, normalize to [0,255]
  - **A ÷ B**: Divide, handle division by zero
  - Offset dialog (X, Y adjustments)
  - Right-click for offset configuration

- **MainWindow.BinaryImage.cs**
  - **AND**: Bitwise AND operation
  - **OR**: Bitwise OR operation
  - **NOT**: Bitwise NOT (inversion)
  - **XOR**: Bitwise XOR operation
  - Grayscale conversion before binary ops
  - Binary operation dialog for configuration

### 📊 **ImageAnalysis** (2 files)
Analysis and comparison tools.

- **MainWindow.Histogram.cs**
  - Show histogram windows (Red, Green, Blue, Grayscale)
  - Calculate pixel distribution
  - Render histogram charts
  - Separate window per channel

- **MainWindow.GrayscaleComparison.cs**
  - Open grayscale comparison window
  - Compare different grayscale algorithms
  - Side-by-side visualization

### 🔄 **ImageTransform** (1 file)
Geometric transformations.

- **MainWindow.Rotation.cs**
  - Rotate 90°, 180°, 270° (fast)
  - Custom angle rotation
  - Bilinear interpolation
  - Auto-resize canvas to fit rotated image

---

## 🖱️ User Interaction Layer

### **ToolbarHandlers.cs**
Central event handling for all toolbar buttons and toggles.

**Responsibilities:**
- Load/Add Image B button handlers
- Arithmetic toggle handlers (A+B, A-B, A×B, A÷B)
- Binary toggle handlers (AND, OR, NOT, XOR)
- Scalar operation toggle handler
- Mutual exclusion logic (only one toggle active at a time)
- Dialog spawning for operations
- Right-click handlers for offset configuration

**Key Pattern:**
```csharp
// Toggle checked → Show panel + Activate feature
// Toggle unchecked → Hide panel + Restore original
// DeactivateAllModes() → Uncheck all toggles (mutual exclusion)
```

### **FilterTab.cs**
Manages filter tab navigation and state.

**Responsibilities:**
- Tab switching logic
- Filter type selection
- Sync UI with current filter state

### **Zoom.cs**
Image zoom functionality (max 50x for pixel inspection).

**Responsibilities:**
- **Ctrl + Mouse Wheel**: Zoom in/out
- **Touch pinch**: Zoom on touch devices
- Scale transform management
- Scroll offset calculation (zoom to cursor point)
- Min zoom: 0.1x (10%)
- Max zoom: 50x (5000%) → see individual pixels

### **WindowInterop.cs**
Windows API integration for horizontal scrolling.

**Responsibilities:**
- **Shift + Mouse Wheel**: Horizontal scroll
- Windows message hook (WM_MOUSEHWHEEL)
- Native horizontal scroll support
- Cross-platform consideration

---

## 🔧 Technical Architecture

### Partial Class Pattern
All files are `partial class MainWindow` allowing code to be split by concern while maintaining single class compilation.

**Benefits:**
- ✅ **Separation of Concerns**: Each file has single responsibility
- ✅ **Maintainability**: Easy to find and modify specific features
- ✅ **Team Collaboration**: Reduces merge conflicts
- ✅ **Testability**: Clear boundaries for unit testing
- ✅ **Readability**: Smaller files, focused logic

### Namespace
All files use: `namespace MiniPhotoshop.Views.MainWindow`

### State Management
Centralized in `MainWindow.xaml.cs`:
```csharp
private readonly ImageWorkspaceState _state = new();
```

Shared across all partials for image data, pixel cache, histograms, etc.

### Service Dependencies
- `IImageLoaderService`: Image loading/saving
- `IGrayscaleComparisonService`: Grayscale algorithms
- `ImageEditor`: Business logic for image operations
- Services injected in `MainWindow.xaml.cs` constructor

---

## 🚀 Adding New Features

To add a new feature:

1. **Determine Category**
   - Image processing? → `Features/ImageProcessing/`
   - Multi-image operation? → `Features/ImageOperations/`
   - Analysis tool? → `Features/ImageAnalysis/`
   - UI interaction? → `UserInteraction/`

2. **Create Partial Class**
   ```csharp
   using System.Windows;
   
   namespace MiniPhotoshop.Views.MainWindow
   {
       public partial class MainWindow
       {
           // Your feature implementation
       }
   }
   ```

3. **Add UI in MainWindow.xaml**
   - Toggle button in toolbar
   - Panel in info section

4. **Implement Event Handlers**
   - Toggle_Checked: Show panel, activate feature
   - Toggle_Unchecked: Hide panel, restore original
   - Add to `DeactivateAllModes()` if mutually exclusive

5. **Update Workspace Reset**
   - Add toggle reset in `MainWindow.Workspace.cs`

6. **Test Integration**
   - Build, run, verify all features still work

---

## 📝 Coding Conventions

### Event Handler Naming
```csharp
private void FeatureToggle_Checked(object sender, RoutedEventArgs e)
private void FeatureToggle_Unchecked(object sender, RoutedEventArgs e)
private void FeatureButton_Click(object sender, RoutedEventArgs e)
```

### Panel Visibility Pattern
```csharp
// Show feature panel when toggle checked
FeaturePanel.Visibility = Visibility.Visible;

// Hide panel when toggle unchecked
FeaturePanel.Visibility = Visibility.Collapsed;
```

### State Restoration
```csharp
// Always restore original image on toggle unchecked
if (_state.OriginalBitmap != null)
{
    DisplayImage.Source = _state.OriginalBitmap;
}
```

### Null Checks
```csharp
// Guard against null state
if (_state.OriginalBitmap == null) return;
if (_state.PixelCache == null) return;
```

---

## 🎨 UI-Backend Separation

**UI Layer** (this folder):
- Event handlers
- User interaction logic
- Dialog spawning
- Panel visibility control

**Business Logic** (`Services/ImageEditor/`):
- Actual image processing algorithms
- Pixel manipulation
- Mathematical operations
- No UI dependencies

**Models** (`Core/Models/`):
- Data structures
- State objects
- Transfer objects

**Contracts** (`Services/Contracts/`):
- Interfaces
- Service abstractions

---

## 📚 Related Documentation

- **RESTRUCTURE_PLAN.md**: Full restructuring plan with all tasks
- **README.md**: Main project documentation
- **USAGE_GUIDE.md**: User manual
- **TESTING_GUIDE.md**: Testing procedures

---

**Last Updated**: 2025-11-15  
**Version**: 2.0 (Post-Restructure)
