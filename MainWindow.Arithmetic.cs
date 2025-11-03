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

                if (_currentArithmeticMode != ArithmeticToggleMode.None)
                {
                    DeactivateArithmeticMode();
                }
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

        private void ArithmeticAddToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleArithmeticToggleChecked(isAddition: true);
        }

        private void ArithmeticAddToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleArithmeticToggleUnchecked();
        }

        private void ArithmeticSubtractToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleArithmeticToggleChecked(isAddition: false);
        }

        private void ArithmeticSubtractToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleArithmeticToggleUnchecked();
        }

        private void HandleArithmeticToggleChecked(bool isAddition)
        {
            if (_suppressArithmeticToggleHandlers)
            {
                return;
            }

            if (!EnsureArithmeticReady())
            {
                SuppressAndUncheckToggle(isAddition);
                return;
            }

            // Ensure only one toggle stays active at a time.
            if (isAddition && ArithmeticSubtractToggle.IsChecked == true)
            {
                _suppressArithmeticToggleHandlers = true;
                ArithmeticSubtractToggle.IsChecked = false;
                _suppressArithmeticToggleHandlers = false;
            }
            else if (!isAddition && ArithmeticAddToggle.IsChecked == true)
            {
                _suppressArithmeticToggleHandlers = true;
                ArithmeticAddToggle.IsChecked = false;
                _suppressArithmeticToggleHandlers = false;
            }

            if (!ApplyArithmeticOperation(isAddition))
            {
                SuppressAndUncheckToggle(isAddition);
            }
        }

        private void HandleArithmeticToggleUnchecked()
        {
            if (_suppressArithmeticToggleHandlers)
            {
                return;
            }

            if (ArithmeticAddToggle.IsChecked == true || ArithmeticSubtractToggle.IsChecked == true)
            {
                return;
            }

            if (_currentArithmeticMode == ArithmeticToggleMode.None)
            {
                return;
            }

            RestoreArithmeticBaseImage();
        }

        private bool ApplyArithmeticOperation(bool isAddition)
        {
            if (_arithmeticOverlayBitmap == null)
            {
                MessageBox.Show("Silakan pilih gambar B terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (!TryParseOffset(ArithmeticOffsetXTextBox.Text, "Offset X", out int offsetX))
            {
                return false;
            }

            if (!TryParseOffset(ArithmeticOffsetYTextBox.Text, "Offset Y", out int offsetY))
            {
                return false;
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

                _currentArithmeticMode = isAddition ? ArithmeticToggleMode.Addition : ArithmeticToggleMode.Subtraction;

                _suppressArithmeticToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressArithmeticToggleHandlers = false;
                UpdateArithmeticButtonsState();
                return true;
            }
            catch (Exception ex)
            {
                string action = isAddition ? "menjumlahkan" : "mengurangkan";
                MessageBox.Show($"Gagal {action} gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentArithmeticMode = ArithmeticToggleMode.None;
                _arithmeticService.ClearArithmeticSnapshot();
                return false;
            }
        }

        private void RestoreArithmeticBaseImage()
        {
            try
            {
                BitmapSource restored = _arithmeticService.RestoreArithmeticBase();
                _currentArithmeticMode = ArithmeticToggleMode.None;

                string fileLabel = _state.CurrentFilePath ?? "Gambar.png";
                var resultInfo = new ImageLoadResult(
                    restored,
                    fileLabel,
                    restored.PixelWidth,
                    restored.PixelHeight,
                    restored.Format.ToString()
                );

                _suppressArithmeticToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressArithmeticToggleHandlers = false;
                UpdateArithmeticButtonsState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengembalikan gambar awal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _suppressArithmeticToggleHandlers = true;
                ArithmeticAddToggle.IsChecked = false;
                ArithmeticSubtractToggle.IsChecked = false;
                _suppressArithmeticToggleHandlers = false;
                _currentArithmeticMode = ArithmeticToggleMode.None;
                UpdateArithmeticButtonsState();
            }
        }

        private void DeactivateArithmeticMode()
        {
            if (_currentArithmeticMode == ArithmeticToggleMode.None)
            {
                return;
            }

            _suppressArithmeticToggleHandlers = true;
            ArithmeticAddToggle.IsChecked = false;
            ArithmeticSubtractToggle.IsChecked = false;
            _suppressArithmeticToggleHandlers = false;

            RestoreArithmeticBaseImage();
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
            bool isEnabled = hasBase && hasOverlay;
            ArithmeticAddToggle.IsEnabled = isEnabled;
            ArithmeticSubtractToggle.IsEnabled = isEnabled;
        }

        private bool EnsureArithmeticReady()
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar utama terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (_arithmeticOverlayBitmap == null)
            {
                MessageBox.Show("Silakan pilih gambar B terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        private void SuppressAndUncheckToggle(bool isAddition)
        {
            _suppressArithmeticToggleHandlers = true;
            if (isAddition)
            {
                ArithmeticAddToggle.IsChecked = false;
            }
            else
            {
                ArithmeticSubtractToggle.IsChecked = false;
            }
            _suppressArithmeticToggleHandlers = false;
        }
    }
}
