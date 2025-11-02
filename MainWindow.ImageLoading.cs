using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
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

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                ImageLoadResult result = _imageLoader.Load(openFileDialog.FileName);
                ApplyLoadedImage(result);
            }
            catch (Exception ex)
            {
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
            DisplayImage.Source = _filterService.SetActiveFilter(ImageFilterMode.Original);
            _filterService.BuildPreviews();
            _filterService.SyncPreviewActivation();
            ResetBrightnessControl();

            FileNameText.Text = Path.GetFileName(result.FilePath);
            FileNameText.Foreground = System.Windows.Media.Brushes.Black;
            ImageInfoText.Text = $"Resolusi: {result.Width} x {result.Height} | Format: {result.PixelFormatDescription}";

            SavePixelsMenuItem.IsEnabled = true;
            NegationToggle.IsEnabled = true;
            NegationToggle.IsChecked = false;
            GrayscaleCompareButton.IsEnabled = true;
            BinaryThresholdToggle.IsEnabled = true;
            BinaryThresholdToggle.IsChecked = false;
            BinaryThresholdPanel.Visibility = Visibility.Collapsed;
            BinaryThresholdSlider.Value = ImageWorkspaceState.DefaultBinaryThreshold;
            UpdateBinaryThresholdLabel(ImageWorkspaceState.DefaultBinaryThreshold);

            BrightnessPanel.Visibility = Visibility.Visible;

            ColorSelectionPanel.Visibility = Visibility.Visible;
            ColorSelectionCheckBox.IsChecked = false;
            SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
            SelectedColorText.Foreground = System.Windows.Media.Brushes.Gray;
            DisplayImage.MouseLeftButtonDown -= DisplayImage_ColorSelection_Click;

            ShowSidebar();
            RenderHistograms();

            _state.CurrentZoom = 1.0;
            _currentZoom = 1.0;
            QueueAutoFit();
        }
    }
}
