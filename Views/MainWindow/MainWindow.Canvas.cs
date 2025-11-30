using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            UpdateCanvasInfoDisplay();
            
            // Queue auto fit after canvas is initialized
            QueueAutoFit();
        }

        #endregion

        #region Canvas UI Handlers

        /// <summary>
        /// Handler untuk tombol Canvas Settings di toolbar.
        /// Membuka panel pengaturan kanvas.
        /// </summary>
        private void CanvasSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideCanvasSettingsPanel(true);
        }

        /// <summary>
        /// Handler untuk tombol tutup panel canvas settings.
        /// </summary>
        private void CloseCanvasSettingsPanel_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideCanvasSettingsPanel(false);
        }

        /// <summary>
        /// Menampilkan atau menyembunyikan panel canvas settings.
        /// </summary>
        private void ShowOrHideCanvasSettingsPanel(bool show)
        {
            if (CanvasSettingsPanel != null)
            {
                CanvasSettingsPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handler untuk tombol Apply Canvas Size.
        /// Mengubah ukuran kanvas sesuai input.
        /// </summary>
        private void ApplyCanvasSize_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCanvasInputs())
            {
                return;
            }

            int width = int.Parse(CanvasWidthInput.Text);
            int height = int.Parse(CanvasHeightInput.Text);

            UpdateCanvasSize(width, height);
        }

        /// <summary>
        /// Handler untuk perubahan warna kanvas dari ComboBox.
        /// </summary>
        private void CanvasColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanvasColorComboBox?.SelectedItem is ComboBoxItem selectedItem && 
                selectedItem.Tag is string colorTag &&
                _currentCanvasState != null)
            {
                try
                {
                    Color selectedColor = (Color)ColorConverter.ConvertFromString(colorTag);
                    UpdateCanvasBackgroundColor(selectedColor);
                }
                catch
                {
                    // Abaikan jika parsing gagal
                }
            }
        }

        /// <summary>
        /// Validasi input hanya angka untuk ukuran kanvas.
        /// </summary>
        private void CanvasSizeInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        #endregion

        #region Canvas Operations

        /// <summary>
        /// Mengubah ukuran kanvas.
        /// </summary>
        private void UpdateCanvasSize(int width, int height)
        {
            if (_currentCanvasState == null)
            {
                _currentCanvasState = new CanvasState();
            }

            _currentCanvasState.Width = width;
            _currentCanvasState.Height = height;

            _canvasService.UpdateCanvasSize(width, height);
            UpdateCanvasDisplay();
            UpdateCanvasInfoDisplay();
        }

        /// <summary>
        /// Mengubah warna latar belakang kanvas.
        /// </summary>
        private void UpdateCanvasBackgroundColor(Color color)
        {
            if (_currentCanvasState == null)
            {
                _currentCanvasState = new CanvasState();
            }

            _currentCanvasState.BackgroundColor = color;

            _canvasService.UpdateCanvasBackground(color);
            UpdateCanvasDisplay();
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

        /// <summary>
        /// Memperbarui label info kanvas di UI.
        /// </summary>
        private void UpdateCanvasInfoDisplay()
        {
            if (_currentCanvasState != null && CanvasInfoLabel != null)
            {
                CanvasInfoLabel.Text = $"Kanvas: {_currentCanvasState.Width} × {_currentCanvasState.Height} px";
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Memvalidasi input ukuran kanvas.
        /// </summary>
        private bool ValidateCanvasInputs()
        {
            if (CanvasWidthInput == null || CanvasHeightInput == null)
            {
                return false;
            }

            if (!int.TryParse(CanvasWidthInput.Text, out int width) || width < 1 || width > 10000)
            {
                MessageBox.Show(
                    "Lebar kanvas harus berupa angka antara 1 dan 10000 pixel.",
                    "Validasi Gagal",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CanvasWidthInput.Focus();
                return false;
            }

            if (!int.TryParse(CanvasHeightInput.Text, out int height) || height < 1 || height > 10000)
            {
                MessageBox.Show(
                    "Tinggi kanvas harus berupa angka antara 1 dan 10000 pixel.",
                    "Validasi Gagal",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CanvasHeightInput.Focus();
                return false;
            }

            return true;
        }

        #endregion
    }
}
