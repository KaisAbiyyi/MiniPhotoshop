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
        private int _dragStartOffsetX;
        private int _dragStartOffsetY;

        #endregion

        #region Toggle Handlers

        private void MoveImageToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                MoveImageToggle.IsChecked = false;
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
            if (_state.OriginalBitmap != null)
            {
                var (offsetX, offsetY) = _canvasService.GetImageOffset();
                ImageInfoText.Text = $"Posisi gambar: ({offsetX}, {offsetY})";
            }
        }

        #endregion

        #region Mouse Event Handlers

        private void MoveImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isMoveImageActive || _state.OriginalBitmap == null)
                return;

            _isDraggingImage = true;
            _dragStartPoint = e.GetPosition(DisplayImage);
            
            // Get current offset
            var (currentOffsetX, currentOffsetY) = _canvasService.GetImageOffset();
            _dragStartOffsetX = currentOffsetX;
            _dragStartOffsetY = currentOffsetY;
            
            // Capture mouse for tracking outside control
            DisplayImage.CaptureMouse();
            
            e.Handled = true;
        }

        private void MoveImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingImage || !_isMoveImageActive)
                return;

            Point currentPoint = e.GetPosition(DisplayImage);
            
            // Calculate delta based on current zoom level
            // Δx = (currentX - startX) / zoom
            // Δy = (currentY - startY) / zoom
            double deltaX = (currentPoint.X - _dragStartPoint.X) / _currentZoom;
            double deltaY = (currentPoint.Y - _dragStartPoint.Y) / _currentZoom;
            
            // Calculate new position: Pos_baru = Pos_lama + Δ
            int newOffsetX = _dragStartOffsetX + (int)Math.Round(deltaX);
            int newOffsetY = _dragStartOffsetY + (int)Math.Round(deltaY);
            
            // Update offset and render canvas
            _canvasService.SetImageOffset(newOffsetX, newOffsetY);
            
            // Also update local canvas state
            if (_currentCanvasState != null)
            {
                _currentCanvasState.ImageOffsetX = newOffsetX;
                _currentCanvasState.ImageOffsetY = newOffsetY;
            }
            
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
            var (finalOffsetX, finalOffsetY) = _canvasService.GetImageOffset();
            ImageInfoText.Text = $"Gambar dipindahkan ke posisi ({finalOffsetX}, {finalOffsetY})";
            
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
            if (_state.OriginalBitmap == null || _currentCanvasState == null)
                return;

            var (canvasWidth, canvasHeight) = _canvasService.GetCanvasDimensions();
            var (imageWidth, imageHeight) = _canvasService.GetOriginalImageDimensions();

            if (imageWidth == 0 || imageHeight == 0)
                return;

            // Calculate center position
            int centerX = (canvasWidth - imageWidth) / 2;
            int centerY = (canvasHeight - imageHeight) / 2;

            // Update offset
            _canvasService.SetImageOffset(centerX, centerY);
            _currentCanvasState.ImageOffsetX = centerX;
            _currentCanvasState.ImageOffsetY = centerY;

            UpdateCanvasDisplay();
        }

        /// <summary>
        /// Resets the image position to origin (0, 0).
        /// </summary>
        private void ResetImagePosition()
        {
            if (_currentCanvasState == null)
                return;

            _canvasService.SetImageOffset(0, 0);
            _currentCanvasState.ImageOffsetX = 0;
            _currentCanvasState.ImageOffsetY = 0;

            UpdateCanvasDisplay();
        }

        #endregion
    }
}
