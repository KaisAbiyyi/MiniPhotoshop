using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
        private void ArithmeticSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "Pilih Gambar B",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|" +
                         "JPEG Files|*.jpg;*.jpeg|" +
                         "PNG Files|*.png|" +
                         "Bitmap Files|*.bmp|" +
                         "GIF Files|*.gif|" +
                         "TIFF Files|*.tiff|" +
                         "All Files|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                _arithmeticOverlayBitmap = bitmap;
                ArithmeticInfoText.Text = $"{Path.GetFileName(dialog.FileName)} ({bitmap.PixelWidth} x {bitmap.PixelHeight})";
                ArithmeticInfoText.Foreground = Brushes.Black;
                UpdateArithmeticButtonsState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat gambar B: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _arithmeticOverlayBitmap = null;
                ArithmeticInfoText.Text = "Belum ada gambar B";
                ArithmeticInfoText.Foreground = Brushes.Gray;
                UpdateArithmeticButtonsState();
            }
        }

        private void ArithmeticAddButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyArithmeticOperation(true);
        }

        private void ArithmeticSubtractButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyArithmeticOperation(false);
        }

        private void ApplyArithmeticOperation(bool isAddition)
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar utama terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_arithmeticOverlayBitmap == null)
            {
                MessageBox.Show("Silakan pilih gambar B terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!TryParseOffset(ArithmeticOffsetXTextBox.Text, "Offset X", out int offsetX))
            {
                return;
            }

            if (!TryParseOffset(ArithmeticOffsetYTextBox.Text, "Offset Y", out int offsetY))
            {
                return;
            }

            try
            {
                BitmapSource result = isAddition
                    ? _arithmeticService.AddImage(_arithmeticOverlayBitmap, offsetX, offsetY)
                    : _arithmeticService.SubtractImage(_arithmeticOverlayBitmap, offsetX, offsetY);

                string fallbackName = isAddition ? "Hasil_Penjumlahan.png" : "Hasil_Pengurangan.png";
                string fileLabel = _state.CurrentFilePath ?? fallbackName;

                var resultInfo = new ImageLoadResult(
                    result,
                    fileLabel,
                    result.PixelWidth,
                    result.PixelHeight,
                    result.Format.ToString()
                );

                ApplyLoadedImage(resultInfo);
                UpdateArithmeticButtonsState();
            }
            catch (Exception ex)
            {
                string action = isAddition ? "menjumlahkan" : "mengurangkan";
                MessageBox.Show($"Gagal {action} gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryParseOffset(string? input, string fieldLabel, out int value)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                value = 0;
                return true;
            }

            if (!int.TryParse(input, out value))
            {
                MessageBox.Show($"{fieldLabel} harus berupa angka.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        private void UpdateArithmeticButtonsState()
        {
            bool hasBase = _state.OriginalBitmap != null;
            bool hasOverlay = _arithmeticOverlayBitmap != null;
            ArithmeticAddButton.IsEnabled = hasBase && hasOverlay;
            ArithmeticSubtractButton.IsEnabled = hasBase && hasOverlay;
        }
    }
}
