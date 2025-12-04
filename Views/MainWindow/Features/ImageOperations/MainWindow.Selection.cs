using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.MainWindow
{
    /// <summary>
    /// Selection tools handlers for MainWindow.
    /// Implements Rectangle and Lasso selection with drag support.
    /// </summary>
    public partial class MainWindow
    {
        #region Selection State

        private SelectionToolMode _currentSelectionTool = SelectionToolMode.None;
        private bool _isSelectingArea;
        private Point _selectionStartPoint;
        private List<Point> _lassoPoints = new();
        private bool _isMovingSelection;
        private Point _selectionMoveStartPoint;
        private int _selectionMoveStartOffsetX;
        private int _selectionMoveStartOffsetY;

        #endregion

        #region Toggle Handlers

        private void RectangleSelectToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                RectangleSelectToggle.IsChecked = false;
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Disable other tools
            DisableOtherSelectionTools(SelectionToolMode.Rectangle);
            _currentSelectionTool = SelectionToolMode.Rectangle;
            
            // Clear previous selection
            _selectionService.ClearSelection();
            _lassoPoints.Clear();
            
            // Change cursor
            DisplayImage.Cursor = Cursors.Cross;
            
            // Subscribe to mouse events
            DisplayImage.MouseLeftButtonDown += Selection_MouseDown;
            DisplayImage.MouseMove += Selection_MouseMove;
            DisplayImage.MouseLeftButtonUp += Selection_MouseUp;

            ImageInfoText.Text = "Mode Seleksi Kotak: Klik dan seret untuk membuat seleksi";
        }

        private void RectangleSelectToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_currentSelectionTool == SelectionToolMode.Rectangle)
            {
                _currentSelectionTool = SelectionToolMode.None;
                UnsubscribeSelectionEvents();
                DisplayImage.Cursor = Cursors.Arrow;
                
                // Clear selection
                _selectionService.ClearSelection();
                UpdateCanvasDisplay();
            }
        }

        private void LassoSelectToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                LassoSelectToggle.IsChecked = false;
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Disable other tools
            DisableOtherSelectionTools(SelectionToolMode.Lasso);
            _currentSelectionTool = SelectionToolMode.Lasso;
            
            // Clear previous selection
            _selectionService.ClearSelection();
            _lassoPoints.Clear();
            
            // Change cursor
            DisplayImage.Cursor = Cursors.Cross;
            
            // Subscribe to mouse events
            DisplayImage.MouseLeftButtonDown += Selection_MouseDown;
            DisplayImage.MouseMove += Selection_MouseMove;
            DisplayImage.MouseLeftButtonUp += Selection_MouseUp;

            ImageInfoText.Text = "Mode Seleksi Bebas (Lasso): Klik dan seret untuk menggambar area seleksi";
        }

        private void LassoSelectToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_currentSelectionTool == SelectionToolMode.Lasso)
            {
                _currentSelectionTool = SelectionToolMode.None;
                UnsubscribeSelectionEvents();
                DisplayImage.Cursor = Cursors.Arrow;
                
                // Clear selection
                _selectionService.ClearSelection();
                _lassoPoints.Clear();
                UpdateCanvasDisplay();
            }
        }

        private void MoveSelectionToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                MoveSelectionToggle.IsChecked = false;
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_selectionService.SelectionState.HasSelection)
            {
                MoveSelectionToggle.IsChecked = false;
                MessageBox.Show("Silakan buat seleksi terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Disable other tools
            DisableOtherSelectionTools(SelectionToolMode.MoveSelection);
            _currentSelectionTool = SelectionToolMode.MoveSelection;
            
            // Copy selected pixels for moving
            _selectionService.CopySelectedPixels();
            
            // Change cursor
            DisplayImage.Cursor = Cursors.SizeAll;
            
            // Subscribe to mouse events
            DisplayImage.MouseLeftButtonDown += MoveSelection_MouseDown;
            DisplayImage.MouseMove += MoveSelection_MouseMove;
            DisplayImage.MouseLeftButtonUp += MoveSelection_MouseUp;

            ImageInfoText.Text = "Mode Pindah Seleksi: Klik dan seret untuk memindahkan area terpilih";
        }

        private void MoveSelectionToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_currentSelectionTool == SelectionToolMode.MoveSelection)
            {
                _currentSelectionTool = SelectionToolMode.None;
                DisplayImage.Cursor = Cursors.Arrow;
                
                // Unsubscribe from mouse events
                DisplayImage.MouseLeftButtonDown -= MoveSelection_MouseDown;
                DisplayImage.MouseMove -= MoveSelection_MouseMove;
                DisplayImage.MouseLeftButtonUp -= MoveSelection_MouseUp;
            }
        }

        private void ApplySelectionMove_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectionService.SelectionState.HasSelection)
            {
                MessageBox.Show("Tidak ada seleksi untuk diterapkan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Apply the move
                var result = _selectionService.ApplySelectionMove();
                
                // Clear selection before applying
                _selectionService.ClearSelection();
                
                // Reset toggles
                RectangleSelectToggle.IsChecked = false;
                LassoSelectToggle.IsChecked = false;
                MoveSelectionToggle.IsChecked = false;

                ApplyImageProcessingResult(result, "Seleksi Dipindahkan");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menerapkan seleksi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            _selectionService.ClearSelection();
            _lassoPoints.Clear();
            
            RectangleSelectToggle.IsChecked = false;
            LassoSelectToggle.IsChecked = false;
            MoveSelectionToggle.IsChecked = false;
            
            UpdateCanvasDisplay();
            ImageInfoText.Text = "Seleksi dibersihkan";
        }

        #endregion

        #region Selection Mouse Events

        private void Selection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentSelectionTool == SelectionToolMode.None || _state.OriginalBitmap == null)
                return;

            _isSelectingArea = true;
            _selectionStartPoint = e.GetPosition(DisplayImage);
            
            // Convert to image coordinates
            var (imgX, imgY) = ConvertToImageCoordinates(_selectionStartPoint);

            if (_currentSelectionTool == SelectionToolMode.Lasso)
            {
                _lassoPoints.Clear();
                _lassoPoints.Add(new Point(imgX, imgY));
            }
            
            DisplayImage.CaptureMouse();
            e.Handled = true;
        }

        private void Selection_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelectingArea || _currentSelectionTool == SelectionToolMode.None)
                return;

            Point currentPoint = e.GetPosition(DisplayImage);
            var (imgX, imgY) = ConvertToImageCoordinates(currentPoint);

            if (_currentSelectionTool == SelectionToolMode.Rectangle)
            {
                // Update rectangle selection preview
                var (startImgX, startImgY) = ConvertToImageCoordinates(_selectionStartPoint);
                
                _selectionService.CreateRectangleSelection(
                    (int)startImgX, (int)startImgY,
                    (int)imgX, (int)imgY);
                
                // Render selection overlay
                RenderSelectionPreview();
            }
            else if (_currentSelectionTool == SelectionToolMode.Lasso)
            {
                // Add point to lasso path
                _lassoPoints.Add(new Point(imgX, imgY));
                
                // Update lasso selection
                if (_lassoPoints.Count >= 3)
                {
                    _selectionService.CreateLassoSelection(_lassoPoints);
                    RenderSelectionPreview();
                }
            }

            var selState = _selectionService.SelectionState;
            ImageInfoText.Text = $"Seleksi: {selState.SelectedWidth} x {selState.SelectedHeight} piksel";
            
            e.Handled = true;
        }

        private void Selection_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelectingArea)
                return;

            _isSelectingArea = false;
            DisplayImage.ReleaseMouseCapture();

            var selState = _selectionService.SelectionState;
            if (selState.HasSelection)
            {
                // Enable move selection button
                MoveSelectionToggle.IsEnabled = true;
                ApplySelectionButton.IsEnabled = true;
                ClearSelectionButton.IsEnabled = true;
                
                ImageInfoText.Text = $"Seleksi dibuat: {selState.SelectedWidth} x {selState.SelectedHeight} piksel. Gunakan 'Pindah Seleksi' untuk memindahkan.";
            }
            
            e.Handled = true;
        }

        #endregion

        #region Move Selection Mouse Events

        private void MoveSelection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentSelectionTool != SelectionToolMode.MoveSelection || 
                !_selectionService.SelectionState.HasSelection)
                return;

            _isMovingSelection = true;
            _selectionMoveStartPoint = e.GetPosition(DisplayImage);
            _selectionMoveStartOffsetX = _selectionService.SelectionState.OffsetX;
            _selectionMoveStartOffsetY = _selectionService.SelectionState.OffsetY;
            
            DisplayImage.CaptureMouse();
            e.Handled = true;
        }

        private void MoveSelection_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMovingSelection || _currentSelectionTool != SelectionToolMode.MoveSelection)
                return;

            Point currentPoint = e.GetPosition(DisplayImage);
            
            // Calculate delta in image coordinates
            double deltaX = (currentPoint.X - _selectionMoveStartPoint.X) / _currentZoom;
            double deltaY = (currentPoint.Y - _selectionMoveStartPoint.Y) / _currentZoom;
            
            // Calculate new position
            int newOffsetX = _selectionMoveStartOffsetX + (int)Math.Round(deltaX);
            int newOffsetY = _selectionMoveStartOffsetY + (int)Math.Round(deltaY);
            
            // Update selection offset
            _selectionService.SelectionState.OffsetX = newOffsetX;
            _selectionService.SelectionState.OffsetY = newOffsetY;
            _selectionService.SelectionState.Bounds = new Rect(
                newOffsetX, newOffsetY,
                _selectionService.SelectionState.SelectedWidth,
                _selectionService.SelectionState.SelectedHeight);
            
            // Render preview
            RenderSelectionPreview();
            
            ImageInfoText.Text = $"Memindahkan seleksi ke ({newOffsetX}, {newOffsetY})";
            
            e.Handled = true;
        }

        private void MoveSelection_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isMovingSelection)
                return;

            _isMovingSelection = false;
            DisplayImage.ReleaseMouseCapture();
            
            var selState = _selectionService.SelectionState;
            ImageInfoText.Text = $"Seleksi dipindahkan ke ({selState.OffsetX}, {selState.OffsetY}). Klik 'Terapkan' untuk konfirmasi.";
            
            e.Handled = true;
        }

        #endregion

        #region Helper Methods

        private void DisableOtherSelectionTools(SelectionToolMode activeMode)
        {
            _suppressSelectionToggleHandlers = true;
            
            if (activeMode != SelectionToolMode.Rectangle && RectangleSelectToggle.IsChecked == true)
            {
                RectangleSelectToggle.IsChecked = false;
            }
            if (activeMode != SelectionToolMode.Lasso && LassoSelectToggle.IsChecked == true)
            {
                LassoSelectToggle.IsChecked = false;
            }
            if (activeMode != SelectionToolMode.MoveSelection && MoveSelectionToggle.IsChecked == true)
            {
                MoveSelectionToggle.IsChecked = false;
            }
            if (activeMode != SelectionToolMode.None && MoveImageToggle.IsChecked == true)
            {
                MoveImageToggle.IsChecked = false;
            }
            
            _suppressSelectionToggleHandlers = false;
        }

        private void UnsubscribeSelectionEvents()
        {
            DisplayImage.MouseLeftButtonDown -= Selection_MouseDown;
            DisplayImage.MouseMove -= Selection_MouseMove;
            DisplayImage.MouseLeftButtonUp -= Selection_MouseUp;
        }

        private (double X, double Y) ConvertToImageCoordinates(Point screenPoint)
        {
            // Convert screen coordinates to image coordinates considering zoom
            double imgX = screenPoint.X / _currentZoom;
            double imgY = screenPoint.Y / _currentZoom;
            
            // Consider canvas offset if applicable
            if (_currentCanvasState != null)
            {
                imgX -= _currentCanvasState.ImageOffsetX;
                imgY -= _currentCanvasState.ImageOffsetY;
            }
            
            return (imgX, imgY);
        }

        private void RenderSelectionPreview()
        {
            var canvas = _canvasService.RenderCanvas();
            if (canvas != null)
            {
                var overlay = _selectionService.RenderSelectionOverlay(canvas);
                DisplayImage.Source = overlay;
            }
        }

        private void UpdateSelectionButtonsState()
        {
            bool hasImage = _state.OriginalBitmap != null;
            bool hasSelection = _selectionService.SelectionState.HasSelection;
            
            RectangleSelectToggle.IsEnabled = hasImage;
            LassoSelectToggle.IsEnabled = hasImage;
            MoveSelectionToggle.IsEnabled = hasImage && hasSelection;
            ApplySelectionButton.IsEnabled = hasImage && hasSelection;
            ClearSelectionButton.IsEnabled = hasImage && hasSelection;
        }

        #endregion
    }

    /// <summary>
    /// Selection tool modes.
    /// </summary>
    public enum SelectionToolMode
    {
        None,
        Rectangle,
        Lasso,
        MoveSelection
    }
}
