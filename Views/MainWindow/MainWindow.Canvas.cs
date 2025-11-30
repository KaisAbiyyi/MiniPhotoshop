using System.Windows;
using System.Windows.Media;
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
