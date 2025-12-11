using System;
using System.Windows;
using System.Windows.Input;

namespace MiniPhotoshop.Views.MainWindow
{
    /// <summary>
    /// Move Image handlers for MainWindow.
    /// Implements drag functionality to move image on canvas.
    /// Formula: Pos_baru = Pos_lama + Δ
    /// </summary>
    public partial class MainWindow
    {
        #region Move State

        private bool _isMoveImageActive;
        private bool _isDraggingImage;
        private Point _dragStartPoint;
        private Point _dragStartCanvasPoint;
        private int _dragStartOffsetX;
        private int _dragStartOffsetY;

        #endregion

        #region Toggle Handlers

        /*
        private void MoveImageToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.ImageObjects.Count == 0)
            {
                // MoveImageToggle.IsChecked = false;
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isMoveImageActive = true;
            
            // Change cursor to indicate move mode
            DisplayImage.Cursor = Cursors.SizeAll;
            
            // Subscribe to mouse events
            DisplayImage.MouseLeftButtonDown += MoveImage_MouseDown;
            DisplayImage.MouseMove += MoveImage_MouseMove;
            DisplayImage.MouseLeftButtonUp += MoveImage_MouseUp;
            DisplayImage.MouseLeave += MoveImage_MouseLeave;

            // Update status
            ImageInfoText.Text = "Mode Pindah Gambar: Klik dan seret untuk memindahkan gambar di kanvas";
        }
        */

        private void MoveImageToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _isMoveImageActive = false;
            _isDraggingImage = false;
            
            // Reset cursor
            DisplayImage.Cursor = Cursors.Arrow;
            
            // Unsubscribe from mouse events
            DisplayImage.MouseLeftButtonDown -= MoveImage_MouseDown;
            DisplayImage.MouseMove -= MoveImage_MouseMove;
            DisplayImage.MouseLeftButtonUp -= MoveImage_MouseUp;
            DisplayImage.MouseLeave -= MoveImage_MouseLeave;

            // Update status
            var selected = _imageObjectManager.GetSelectedImage();
            if (selected != null)
            {
                ImageInfoText.Text = $"Posisi gambar: ({selected.OffsetX}, {selected.OffsetY})";
            }
        }

        #endregion

        #region Mouse Event Handlers

        private void MoveImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isMoveImageActive || _state.ImageObjects.Count == 0)
                return;

            var selected = _imageObjectManager.GetSelectedImage();
            if (selected == null)
            {
                // Try to select image at click point
                // This logic is similar to Selection Tool but integrated here for convenience
                // For now, just return if nothing selected
                return;
            }

            _isDraggingImage = true;

            // Track start point in canvas coordinates (accounts for zoom + scrolling)
            Point viewportPoint = e.GetPosition(WorkspaceScrollViewer);
            Point contentPoint = new Point(
                WorkspaceScrollViewer.HorizontalOffset + viewportPoint.X,
                WorkspaceScrollViewer.VerticalOffset + viewportPoint.Y);

            _dragStartPoint = viewportPoint; // keep for reference
            _dragStartCanvasPoint = new Point(
                contentPoint.X / _currentZoom,
                contentPoint.Y / _currentZoom);
            
            // Get current offset
            _dragStartOffsetX = selected.OffsetX;
            _dragStartOffsetY = selected.OffsetY;
            
            // Capture mouse for tracking outside control
            DisplayImage.CaptureMouse();
            
            e.Handled = true;
        }

        private void MoveImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingImage || !_isMoveImageActive)
                return;

            // Compute cursor position in canvas space: (scroll offset + viewport point) / zoom
            Point viewportPoint = e.GetPosition(WorkspaceScrollViewer);
            Point contentPoint = new Point(
                WorkspaceScrollViewer.HorizontalOffset + viewportPoint.X,
                WorkspaceScrollViewer.VerticalOffset + viewportPoint.Y);

            double canvasX = contentPoint.X / _currentZoom;
            double canvasY = contentPoint.Y / _currentZoom;

            double deltaX = canvasX - _dragStartCanvasPoint.X;
            double deltaY = canvasY - _dragStartCanvasPoint.Y;
            
            // Calculate new position: Pos_baru = Pos_lama + Δ
            int newOffsetX = _dragStartOffsetX + (int)Math.Round(deltaX);
            int newOffsetY = _dragStartOffsetY + (int)Math.Round(deltaY);
            
            // Update offset and render canvas
            _imageObjectManager.SetSelectedImagePosition(newOffsetX, newOffsetY);
            UpdateCanvasDisplay();
            
            // Update status with current position
            ImageInfoText.Text = $"Mode Pindah: Posisi ({newOffsetX}, {newOffsetY})";
            
            e.Handled = true;
        }

        private void MoveImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingImage)
                return;

            _isDraggingImage = false;
            DisplayImage.ReleaseMouseCapture();
            
            // Show final position
            var selected = _imageObjectManager.GetSelectedImage();
            if (selected != null)
            {
                ImageInfoText.Text = $"Gambar dipindahkan ke posisi ({selected.OffsetX}, {selected.OffsetY})";
            }
            
            e.Handled = true;
        }

        private void MoveImage_MouseLeave(object sender, MouseEventArgs e)
        {
            // Don't stop dragging on leave - we have mouse capture
            // This allows dragging outside the control
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Centers the image on the canvas.
        /// </summary>
        private void CenterImageOnCanvas()
        {
            if (_state.ImageObjects.Count == 0 || _currentCanvasState == null)
                return;

            var selected = _imageObjectManager.GetSelectedImage();
            if (selected == null) return;

            var (canvasWidth, canvasHeight) = _canvasService.GetCanvasDimensions();
            int imageWidth = selected.Width;
            int imageHeight = selected.Height;

            if (imageWidth == 0 || imageHeight == 0)
                return;

            // Calculate center position
            int centerX = (canvasWidth - imageWidth) / 2;
            int centerY = (canvasHeight - imageHeight) / 2;

            // Update offset
            _imageObjectManager.SetSelectedImagePosition(centerX, centerY);

            UpdateCanvasDisplay();
        }

        /// <summary>
        /// Resets the image position to origin (0, 0).
        /// </summary>
        private void ResetImagePosition()
        {
            if (_currentCanvasState == null)
                return;

            _imageObjectManager.SetSelectedImagePosition(0, 0);

            UpdateCanvasDisplay();
        }

        #endregion
    }
}
