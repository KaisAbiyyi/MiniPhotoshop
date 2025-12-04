using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        // Handler untuk tombol "Pilih Gambar" di UI.
        // Membuka dialog file, memanggil service loader, lalu menerapkan hasilnya ke workspace.
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Konfigurasi dialog open file untuk memilih gambar.
            OpenFileDialog openFileDialog = new()
            {
                Title = "Pilih Gambar",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|" +
                         "JPEG Files|*.jpg;*.jpeg|" +
                         "PNG Files|*.png|" +
                         "Bitmap Files|*.bmp|" +
                         "GIF Files|*.gif|" +
                         "TIFF Files|*.tiff|" +
                         "All Files|*.*",
                FilterIndex = 1
            };

            // Jika user membatalkan dialog, tidak ada aksi lanjutan.
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                // Panggil service loader (ImageEditor.Load) untuk benar-benar memuat gambar
                // dan mengisi state internal (bitmap, pixel cache, histogram, dsb.).
                ImageLoadResult result = _imageLoader.Load(openFileDialog.FileName);

                // Setiap kali load gambar baru, mode aritmatika di-reset ke None
                // dan snapshot aritmatika dibersihkan supaya tidak tercampur dengan gambar sebelumnya.
                _currentArithmeticMode = ArithmeticToggleMode.None;
                _arithmeticService.ClearArithmeticSnapshot();

                // Sementara menonaktifkan handler toggle agar perubahan IsChecked
                // tidak memicu logika aritmatika secara tidak sengaja.
                _suppressArithmeticToggleHandlers = true;
                ArithmeticAddToggle.IsChecked = false;
                ArithmeticSubtractToggle.IsChecked = false;
                _suppressArithmeticToggleHandlers = false;

                // Terapkan hasil load (update DisplayImage, teks informasi, enable fitur, dsb.).
                ApplyLoadedImage(result);
            }
            catch (Exception ex)
            {
                // Jika terjadi error saat load, tampilkan pesan dan reset workspace
                // agar UI kembali ke kondisi aman.
                MessageBox.Show($"Gagal memuat gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetWorkspaceState();
            }
        }

        private void SavePixelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada data pixel untuk disimpan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Simpan Data Pixel",
                Filter = "Text File|*.txt|All Files|*.*",
                FileName = $"{Path.GetFileNameWithoutExtension(_state.CurrentFilePath)}_pixels.txt"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                _pixelExporter.Export(saveFileDialog.FileName);
                MessageBox.Show("Data pixel berhasil disimpan.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data pixel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyLoadedImage(ImageLoadResult result)
        {
            // Apply filter first to update state
            _filterService.SetActiveFilter(ImageFilterMode.Original);
            _filterService.BuildPreviews();
            _filterService.SyncPreviewActivation();
            ResetBrightnessControl();
            
            // Clear cached canvas pixels to force refresh with new image
            _canvasService.RefreshImagePixels();
            
            // Render image through canvas (image will be clipped to canvas bounds)
            UpdateCanvasDisplay();

            FileNameText.Text = Path.GetFileName(result.FilePath);
            FileNameText.Foreground = System.Windows.Media.Brushes.Black;
            ImageInfoText.Text = $"Resolusi: {result.Width} x {result.Height} | Format: {result.PixelFormatDescription}";

            SavePixelsMenuItem.IsEnabled = true;
            ResetImageMenuItem.IsEnabled = true;
            NegationToggle.IsEnabled = true;
            NegationToggle.IsChecked = false;
            NegationButton.IsEnabled = true;
            UpdateNegationButtonStyle(false);
            GrayscaleCompareButton.IsEnabled = true;
            BinaryThresholdToggle.IsEnabled = true;
            BinaryThresholdToggle.IsChecked = false;
            BinaryThresholdPanel.Visibility = Visibility.Collapsed;
            BinaryThresholdSlider.Value = ImageWorkspaceState.DefaultBinaryThreshold;

            BrightnessToggle.IsEnabled = true;
            BrightnessToggle.IsChecked = false;
            BrightnessPanel.Visibility = Visibility.Collapsed;
            
            // Enable Binary NOT toggle (doesn't need image B)
            BinaryNotToggle.IsEnabled = true;

            // Enable Color Selection toggle
            ColorSelectionToggle.IsEnabled = true;
            ColorSelectionToggle.IsChecked = false;
            ColorSelectionPanel.Visibility = Visibility.Collapsed;

            SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
            SelectedColorText.Foreground = System.Windows.Media.Brushes.Gray;
            DisplayImage.MouseLeftButtonDown -= DisplayImage_ColorSelection_Click;

            // Enable toolbar buttons
            ScalarOperationToggle.IsEnabled = true;
            ScalarOperationToggle.IsChecked = false;
            ConvolutionMenu.IsEnabled = true;
            MoveImageToggle.IsEnabled = true;
            MoveImageToggle.IsChecked = false;
            RectangleSelectToggle.IsEnabled = true;
            RectangleSelectToggle.IsChecked = false;
            LassoSelectToggle.IsEnabled = true;
            LassoSelectToggle.IsChecked = false;
            MoveSelectionToggle.IsEnabled = false; // Enabled after selection is made
            ApplySelectionButton.IsEnabled = false;
            ClearSelectionButton.IsEnabled = false;
            UpdateRotationButtonsState();
            UpdateDistortionButtonState();
            // UpdateArithmeticButtonsState();
            // UpdateBinaryButtonsState();

            ShowSidebar();
            RenderHistograms();

            _state.CurrentZoom = 1.0;
            _currentZoom = 1.0;
            QueueAutoFit();
        }
    }
}
