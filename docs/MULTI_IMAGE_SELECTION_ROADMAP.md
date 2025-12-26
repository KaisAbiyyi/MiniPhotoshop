# Multi-Image Selection System - Implementation Roadmap

## Overview
Transformasi dari single-image editor menjadi multi-image canvas system dengan selection-based workflow mirip Photoshop. Sistem ini memungkinkan multiple images pada canvas, selection tool untuk memilih image, dan fitur editing yang hanya aktif untuk selected image.

---

## Architecture Changes

### Core Concepts
1. **Image Object System**: Setiap gambar di canvas memiliki unique identifier dan metadata
2. **Selection-Based Workflow**: User harus select image dulu sebelum bisa apply fitur
3. **Visual Feedback**: Selected image ditandai dengan border (hitam atau negasi warna canvas)
4. **Locked Features**: Semua fitur image processing disabled sampai ada selection

### Data Structure
```csharp
// New: ImageObject model
class ImageObject
{
    Guid Id;
    BitmapSource Bitmap;
    int OffsetX, OffsetY;
    int Width, Height;
    bool IsSelected;
    string Name;
}

// Canvas akan menyimpan List<ImageObject>
// Pixel array akan punya metadata tentang ImageObject ownership
```

---

## Implementation Phases

### **Phase 1: Foundation - Canvas Resolution Presets**
**Branch**: `feature/canvas-resolution-presets`

**Scope**:
- Add resolution preset dropdown to Canvas Settings Dialog
- Common presets: A4, Letter, Social Media (Instagram, Facebook, etc.)
- Store preset configurations
- Update canvas initialization to use presets

**Files to Create/Modify**:
- `Views/Dialogs/CanvasSettingsDialog.xaml` - Add preset UI
- `Views/Dialogs/CanvasSettingsDialog.xaml.cs` - Preset logic
- `Core/Models/CanvasPreset.cs` - Preset data model

**Deliverables**:
- Canvas dialog dengan preset resolusi yang lengkap
- Quick-select untuk ukuran canvas populer
- Custom size tetap available

**Duration**: 1-2 jam

---

### **Phase 2: Image Object Model**
**Branch**: `feature/image-object-model`

**Scope**:
- Create ImageObject model dengan metadata
- Refactor canvas state untuk support multiple images
- Create ImageObjectManager service
- Update pixel rendering untuk track image ownership

**Files to Create/Modify**:
- `Core/Models/ImageObject.cs` - Image object data model
- `Core/Models/ImageObjectMetadata.cs` - Metadata structure
- `Services/ImageObjectManager/ImageObjectManager.cs` - Manage image collection
- `Services/Contracts/IImageObjectManager.cs` - Interface
- `Services/ImageEditor/ImageEditor.Canvas.cs` - Update render untuk multi-image

**Technical Details**:
```csharp
// Pixel metadata array structure
class PixelMetadata
{
    Guid? ImageObjectId; // null = canvas background
    int LocalX, LocalY;   // Coordinate dalam image object
}
```

**Deliverables**:
- ImageObject model yang robust
- Canvas bisa render multiple images
- Pixel-level tracking untuk image ownership

**Duration**: 3-4 jam

---

### **Phase 3: Selection Tool Implementation**
**Branch**: `feature/selection-tool`

**Scope**:
- Implement selection tool di toolbar
- Click detection untuk select image
- Visual highlight untuk selected image (border)
- Border color logic (hitam atau negasi warna canvas)
- Deselection mechanism

**Files to Create/Modify**:
- `Views/MainWindow/Features/ImageOperations/MainWindow.SelectionTool.cs` - Selection handlers
- `Services/ImageEditor/ImageEditor.Selection.cs` - Selection logic
- `Services/Contracts/ISelectionService.cs` - Interface
- Update MainWindow.xaml untuk selection tool button

**Technical Details**:
```csharp
// Selection detection
1. Get click position on canvas
2. Check pixel metadata at that position
3. Get ImageObjectId from pixel
4. Mark that ImageObject as selected
5. Render border around selected image
```

**Border Rendering**:
- Get canvas background color
- Calculate negasi: `Color.FromArgb(255, 255-R, 255-G, 255-B)`
- If too similar to background, use black
- Draw 2-3px border around image bounds

**Deliverables**:
- Working selection tool
- Visual feedback untuk selected images
- Clean deselection mechanism

**Duration**: 4-5 jam

---

### **Phase 4: Selection-Integrated Move Tool**
**Branch**: `feature/selection-move-integration`

**Scope**:
- Integrate selection dengan move tool
- Only selected image can be moved
- Remove standalone move image toggle
- Seamless drag experience setelah selection

**Files to Modify**:
- `Views/MainWindow/Features/ImageTransform/MainWindow.MoveImage.cs` - Update untuk work dengan selection
- `Services/ImageEditor/ImageEditor.Canvas.cs` - Update offset untuk specific ImageObject
- Remove atau hide old move toggle

**Workflow**:
1. User click selection tool
2. Click image → image selected + visual border
3. Drag selected image → move it
4. Click canvas background → deselect
5. Click different image → select that one instead

**Deliverables**:
- Move tool terintegrasi dengan selection
- Smooth workflow seperti Photoshop
- Proper offset tracking per image

**Duration**: 2-3 jam

---

### **Phase 5: Feature Locking System**
**Branch**: `feature/selection-based-locking`

**Scope**:
- Disable all image processing features by default
- Enable features only when image selected
- Apply features to selected image only
- Update feature result untuk replace selected image bitmap

**Files to Modify**:
- All feature handlers (Convolution, Brightness, etc.)
- MainWindow initialization - set all features disabled
- Selection tool - enable features on select, disable on deselect

**Features to Lock**:
- ✅ Convolution (Low Pass, High Pass, Gaussian, etc.)
- ✅ Brightness/Contrast
- ✅ Color Selection
- ✅ Binary Threshold
- ✅ Negation
- ✅ Filters (R, G, B channels)
- ✅ Rotation
- ✅ Distortion
- ✅ Arithmetic Operations
- ✅ Binary Operations

**Deliverables**:
- Clean locked state for all features
- Features unlock hanya dengan selection
- Processing result apply ke selected image only

**Duration**: 3-4 jam

---

### **Phase 6: Multi-Image Canvas Rendering**
**Branch**: `feature/multi-image-rendering`

**Scope**:
- Update RenderCanvas untuk composite multiple images
- Z-order management (stacking)
- Selection border rendering
- Optimized rendering dengan caching

**Technical Details**:
```csharp
// Rendering order
1. Fill canvas background
2. Sort images by Z-order
3. For each image:
   - Composite pixels at offset position
   - Clip to canvas bounds
4. For selected image:
   - Draw selection border
```

**Files to Modify**:
- `Services/ImageEditor/ImageEditor.Canvas.cs` - Major refactor
- Add Z-order property to ImageObject
- Implement bring-to-front / send-to-back

**Deliverables**:
- Canvas bisa render multiple images correctly
- Proper layering/stacking
- Selection border rendering
- Performance optimizations

**Duration**: 4-5 jam

---

### **Phase 7: Image Management UI**
**Branch**: `feature/image-management-ui`

**Scope**:
- Panel untuk list all images di canvas
- Add/Remove image buttons
- Rename image functionality
- Layer visibility toggle
- Z-order controls (up/down/top/bottom)

**UI Location**: 
- New tab di sidebar (alongside Histogram, Filter)
- Or separate panel di side

**Files to Create**:
- `Views/MainWindow/Panels/ImageLayersPanel.xaml`
- `Views/MainWindow/Panels/ImageLayersPanel.xaml.cs`

**Features**:
- List view dengan image thumbnails
- Selected image highlighted di list
- Right-click context menu (Duplicate, Delete, Rename)
- Drag-and-drop untuk Z-order

**Deliverables**:
- Complete image management UI
- Layer-style workflow
- Thumbnail previews

**Duration**: 5-6 jam

---

## Testing Strategy

### Phase 1 Testing
- Test all preset resolutions
- Verify custom size still works
- Check dialog validation

### Phase 2 Testing
- Load multiple images
- Verify pixel metadata accuracy
- Test render dengan 2-3 images

### Phase 3 Testing
- Click various positions untuk select
- Test border rendering di berbagai canvas colors
- Deselection scenarios

### Phase 4 Testing
- Move selected image at various zoom levels
- Test smooth drag
- Edge cases (image partially off-canvas)

### Phase 5 Testing
- Verify all features locked initially
- Test feature unlock pada selection
- Apply convolution, brightness, etc. ke selected image
- Verify results replace selected image bitmap

### Phase 6 Testing
- Render 3+ images dengan overlapping regions
- Z-order correctness
- Performance dengan 5+ images

### Phase 7 Testing
- UI responsiveness
- Layer controls functionality
- Thumbnail accuracy

---

## Migration Notes

### Breaking Changes
- `State.OriginalBitmap` akan deprecated
- Canvas akan store `List<ImageObject>` instead
- All feature handlers perlu update untuk work dengan selected image

### Backward Compatibility
- Phase 1-2: Existing code tetap work (single image mode)
- Phase 3+: Gradual transition to multi-image

### Data Migration
- No user data affected (canvas settings hanya runtime)
- Code migration via refactoring

---

## Risk Assessment

### High Risk
- **Phase 6**: Complex rendering logic, potential performance issues
- **Phase 5**: Many feature handlers to update, risk of regression

### Medium Risk
- **Phase 2**: Core architecture change
- **Phase 4**: Integration dengan existing move code

### Low Risk
- **Phase 1**: Simple UI addition
- **Phase 3**: Self-contained feature
- **Phase 7**: UI-only, no business logic impact

---

## Timeline Estimate

| Phase | Duration | Cumulative |
|-------|----------|------------|
| Phase 1 | 1-2 hrs | 2 hrs |
| Phase 2 | 3-4 hrs | 6 hrs |
| Phase 3 | 4-5 hrs | 11 hrs |
| Phase 4 | 2-3 hrs | 14 hrs |
| Phase 5 | 3-4 hrs | 18 hrs |
| Phase 6 | 4-5 hrs | 23 hrs |
| Phase 7 | 5-6 hrs | 29 hrs |

**Total**: ~29 hours of development time

**Recommended Approach**: 
- Implement 1-2 phases per day
- Complete testing after each phase before moving to next
- Create PR per phase untuk review

---

## Success Criteria

### Phase 1
✅ Canvas dialog memiliki 10+ preset resolusi
✅ Preset dapat dipilih dan applied

### Phase 2
✅ Canvas bisa hold 3+ ImageObjects
✅ Pixel metadata tracking accurate

### Phase 3
✅ User bisa select image dengan click
✅ Selected image memiliki visible border
✅ Border color logic correct

### Phase 4
✅ Move tool terintegrasi dengan selection
✅ Drag selected image smooth dan accurate
✅ Non-selected images tidak bisa di-drag

### Phase 5
✅ All features locked on startup
✅ Features unlock ketika image selected
✅ Feature processing apply ke selected image only

### Phase 6
✅ Multiple images rendered correctly
✅ Z-order management works
✅ Performance acceptable dengan 5+ images

### Phase 7
✅ Image layers panel functional
✅ User bisa manage all images via UI
✅ Thumbnail previews accurate

---

## Next Steps

1. **Review roadmap** dengan stakeholder
2. **Start Phase 1** - Canvas Resolution Presets
3. **Iterate** phase by phase dengan testing
4. **Document** learnings dan adjustments per phase

---

**Document Version**: 1.0
**Date**: December 8, 2025
**Author**: GitHub Copilot
**Status**: Draft - Awaiting Approval
