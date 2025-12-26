using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Models;
using MiniPhotoshop.Views.Dialogs;

namespace MiniPhotoshop.Views.MainWindow
{
    /// <summary>
    /// Partial class MainWindow untuk fitur Canvas Settings.
    /// Mengikuti Single Responsibility Principle - hanya menangani interaksi UI terkait kanvas.
    /// </summary>
    public partial class MainWindow
    {
        #region Canvas State

        /// <summary>
        /// State kanvas saat ini.
        /// </summary>
        private CanvasState? _currentCanvasState;

        #endregion

        #region Apply Image Processing Result

        /// <summary>
        /// Menerapkan hasil pemrosesan gambar ke canvas.
        /// Digunakan oleh semua fitur seperti konvolusi, distorsi, rotasi, dll.
        /// </summary>
        /// <param name="result">Hasil bitmap dari pemrosesan</param>
        /// <param name="operationName">Nama operasi untuk status text</param>
        private void ApplyImageProcessingResult(BitmapSource result, string operationName)
        {
            if (result == null) return;

            // Update the original bitmap and refresh caches with the processing result
            var oldBitmap = _state.OriginalBitmap;
            _editor.RefreshWorkspaceCaches(result);
            var updatedBitmap = _state.OriginalBitmap;

            // Sync with ImageObjects (Canvas Rendering)
            // We need to update the ImageObject that corresponds to the processed bitmap
            // so that the canvas renderer (which uses ImageObjects) shows the change.
            var imageObj = System.Linq.Enumerable.FirstOrDefault(_state.ImageObjects, x => x.Bitmap == oldBitmap);
            if (imageObj != null)
            {
                imageObj.Bitmap = updatedBitmap!;
            }
            else
            {
                // Fallback: Update selected or first if exact match not found
                var selected = _imageObjectManager.GetSelectedImage();
                if (selected != null)
                {
                    selected.Bitmap = updatedBitmap!;
                }
                else if (_state.ImageObjects.Count > 0)
                {
                    _state.ImageObjects[0].Bitmap = updatedBitmap!;
                }
            }
            
            // Clear cached pixels so canvas re-reads the updated bitmap
            _canvasService.RefreshImagePixels();
            
            // Rebuild filter previews with new image
            _filterService.BuildPreviews();
            
            // Update histogram
            RenderHistograms();
            
            // Refresh canvas display
            UpdateCanvasDisplay();
            
            // Update info
            ImageInfoText.Text = $"{operationName} diterapkan | Resolusi: {updatedBitmap!.PixelWidth} x {updatedBitmap!.PixelHeight}";
        }

        #endregion

        #region Canvas Initialization

        /// <summary>
        /// Menampilkan dialog pengaturan kanvas saat aplikasi dimulai.
        /// Dipanggil dari event Loaded pada MainWindow.
        /// </summary>
        private void ShowCanvasSettingsOnStartup()
        {
            var dialog = new CanvasSettingsDialog();
            bool? result = dialog.ShowDialog();

            if (result == true && dialog.CreateCanvas)
            {
                InitializeCanvas(dialog.CanvasWidth, dialog.CanvasHeight, dialog.BackgroundColor);
            }
            else
            {
                // Gunakan default kanvas jika user melewati dialog
                InitializeCanvas(
                    CanvasState.DefaultWidth, 
                    CanvasState.DefaultHeight, 
                    CanvasState.DefaultBackgroundColor);
            }
        }

        /// <summary>
        /// Menginisialisasi kanvas dengan ukuran dan warna yang ditentukan.
        /// </summary>
        private void InitializeCanvas(int width, int height, Color backgroundColor)
        {
            _currentCanvasState = new CanvasState
            {
                Width = width,
                Height = height,
                BackgroundColor = backgroundColor,
                ImageOffsetX = CanvasState.DefaultImageOffsetX,
                ImageOffsetY = CanvasState.DefaultImageOffsetY,
                IsInitialized = true
            };

            _canvasService.InitializeCanvas(width, height, backgroundColor);
            UpdateCanvasDisplay();
            
            // Queue auto fit after canvas is initialized
            QueueAutoFit();
        }

        #endregion

        #region Canvas UI Handlers

        /// <summary>
        /// Handler untuk menu item Pengaturan Kanvas di File dropdown.
        /// Membuka dialog yang sama seperti startup untuk edit kanvas.
        /// </summary>
        private void OpenCanvasSettingsDialog_Click(object sender, RoutedEventArgs e)
        {
            // Buat dialog dengan nilai saat ini
            var dialog = new CanvasSettingsDialog();
            
            // Set nilai awal dari state saat ini
            if (_currentCanvasState != null)
            {
                dialog.SetCurrentValues(
                    _currentCanvasState.Width, 
                    _currentCanvasState.Height, 
                    _currentCanvasState.BackgroundColor);
            }
            
            bool? result = dialog.ShowDialog();

            if (result == true && dialog.CreateCanvas)
            {
                // Update kanvas dengan nilai baru
                UpdateCanvasSettings(dialog.CanvasWidth, dialog.CanvasHeight, dialog.BackgroundColor);
            }
        }

        #endregion

        #region Canvas Operations

        /// <summary>
        /// Mengubah pengaturan kanvas (ukuran dan warna).
        /// </summary>
        private void UpdateCanvasSettings(int width, int height, Color backgroundColor)
        {
            if (_currentCanvasState == null)
            {
                _currentCanvasState = new CanvasState();
            }

            _currentCanvasState.Width = width;
            _currentCanvasState.Height = height;
            _currentCanvasState.BackgroundColor = backgroundColor;
            _currentCanvasState.IsInitialized = true;

            _canvasService.UpdateCanvasSize(width, height);
            _canvasService.UpdateCanvasBackground(backgroundColor);
            UpdateCanvasDisplay();
            QueueAutoFit();
        }

        /// <summary>
        /// Memperbarui tampilan kanvas di Image control.
        /// </summary>
        private void UpdateCanvasDisplay()
        {
            var canvasBitmap = _canvasService.RenderCanvas();
            if (canvasBitmap != null && DisplayImage != null)
            {
                DisplayImage.Source = canvasBitmap;
            }
        }

        #endregion
    }
}
