using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private bool _isSelectionToolActive;

        private bool _isDraggingSelection;
        private Point _selectionDragStartPoint;
        private int _selectionDragStartOffsetX;
        private int _selectionDragStartOffsetY;

        private void SelectionToolToggle_Checked(object sender, RoutedEventArgs e)
        {
            _isSelectionToolActive = true;
            if (DisplayImage != null)
            {
                DisplayImage.Cursor = Cursors.Hand;
                DisplayImage.MouseLeftButtonDown += ImageSelection_MouseLeftButtonDown;
                DisplayImage.MouseMove += ImageSelection_MouseMove;
                DisplayImage.MouseLeftButtonUp += ImageSelection_MouseLeftButtonUp;
                DisplayImage.MouseLeave += ImageSelection_MouseLeave;
            }
            
            // Disable other tools if needed
            // ...
        }

        private void SelectionToolToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _isSelectionToolActive = false;
            _isDraggingSelection = false;
            if (DisplayImage != null)
            {
                DisplayImage.Cursor = Cursors.Arrow;
                DisplayImage.MouseLeftButtonDown -= ImageSelection_MouseLeftButtonDown;
                DisplayImage.MouseMove -= ImageSelection_MouseMove;
                DisplayImage.MouseLeftButtonUp -= ImageSelection_MouseLeftButtonUp;
                DisplayImage.MouseLeave -= ImageSelection_MouseLeave;
            }
        }

        private void ImageSelection_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelectionToolActive || _state.MetadataCache == null) return;

            Point clickPoint = e.GetPosition(DisplayImage);
            
            // Convert UI coordinates to bitmap coordinates
            var source = DisplayImage.Source as System.Windows.Media.Imaging.BitmapSource;
            if (source == null) return;

            double scaleX = source.PixelWidth / DisplayImage.ActualWidth;
            double scaleY = source.PixelHeight / DisplayImage.ActualHeight;

            int x = (int)(clickPoint.X * scaleX);
            int y = (int)(clickPoint.Y * scaleY);

            int cacheWidth = _state.MetadataCache.GetLength(0);
            int cacheHeight = _state.MetadataCache.GetLength(1);

            if (x >= 0 && x < cacheWidth && y >= 0 && y < cacheHeight)
            {
                var metadata = _state.MetadataCache[x, y];
                if (metadata.ImageObjectId.HasValue)
                {
                    _imageObjectManager.SelectImage(metadata.ImageObjectId.Value);
                    
                    // Start dragging
                    _isDraggingSelection = true;
                    _selectionDragStartPoint = e.GetPosition(WorkspaceScrollViewer);
                    var selected = _imageObjectManager.GetSelectedImage();
                    if (selected != null)
                    {
                        _selectionDragStartOffsetX = selected.OffsetX;
                        _selectionDragStartOffsetY = selected.OffsetY;
                    }
                    DisplayImage.CaptureMouse();
                    
                    UpdateCanvasDisplay();
                    UpdateToolbarState();
                }
                else
                {
                    _imageObjectManager.DeselectAll();
                    UpdateCanvasDisplay();
                    UpdateToolbarState();
                }
                e.Handled = true;
            }
        }

        private void ImageSelection_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingSelection || !_isSelectionToolActive) return;

            Point currentPoint = e.GetPosition(WorkspaceScrollViewer);
            double deltaX = (currentPoint.X - _selectionDragStartPoint.X) / _currentZoom;
            double deltaY = (currentPoint.Y - _selectionDragStartPoint.Y) / _currentZoom;

            int newX = _selectionDragStartOffsetX + (int)deltaX;
            int newY = _selectionDragStartOffsetY + (int)deltaY;

            _imageObjectManager.SetSelectedImagePosition(newX, newY);
            UpdateCanvasDisplay();
        }

        private void ImageSelection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingSelection)
            {
                _isDraggingSelection = false;
                DisplayImage.ReleaseMouseCapture();
            }
        }

        private void ImageSelection_MouseLeave(object sender, MouseEventArgs e)
        {
            // Optional: Stop dragging if mouse leaves window, but CaptureMouse usually handles this
        }

        private void WorkspaceGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelectionToolActive) return;
            
            // If we reached here, it means the click was NOT handled by the Image
            // (i.e., clicked outside the image bounds)
            _imageObjectManager.DeselectAll();
            UpdateCanvasDisplay();
            UpdateToolbarState();
        }
    }
}
