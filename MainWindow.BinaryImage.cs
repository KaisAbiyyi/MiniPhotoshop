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
        private void BinarySelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "Pilih Gambar B untuk Operasi Biner",
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

                _binaryOverlayBitmap = bitmap;
                BinaryInfoText.Text = $"{Path.GetFileName(dialog.FileName)} ({bitmap.PixelWidth} x {bitmap.PixelHeight})";
                BinaryInfoText.Foreground = Brushes.Black;
                UpdateBinaryButtonsState();

                if (_currentBinaryMode != BinaryToggleMode.None && _currentBinaryMode != BinaryToggleMode.Not)
                {
                    DeactivateBinaryMode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat gambar B: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _binaryOverlayBitmap = null;
                BinaryInfoText.Text = "Belum ada gambar B";
                BinaryInfoText.Foreground = Brushes.Gray;
                UpdateBinaryButtonsState();
            }
        }

        private void ConvertToBinaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                BitmapSource result = _binaryImageService.ToBinary(128);

                var resultInfo = new ImageLoadResult(
                    result,
                    "Citra_Biner.png",
                    result.PixelWidth,
                    result.PixelHeight,
                    result.Format.ToString()
                );

                _suppressBinaryToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressBinaryToggleHandlers = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengkonversi ke biner: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BinaryAndToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleBinaryToggleChecked(BinaryToggleMode.And);
        }

        private void BinaryAndToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleBinaryToggleUnchecked();
        }

        private void BinaryOrToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleBinaryToggleChecked(BinaryToggleMode.Or);
        }

        private void BinaryOrToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleBinaryToggleUnchecked();
        }

        private void BinaryNotToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleBinaryToggleChecked(BinaryToggleMode.Not);
        }

        private void BinaryNotToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleBinaryToggleUnchecked();
        }

        private void BinaryXorToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleBinaryToggleChecked(BinaryToggleMode.Xor);
        }

        private void BinaryXorToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleBinaryToggleUnchecked();
        }

        private void HandleBinaryToggleChecked(BinaryToggleMode mode)
        {
            if (_suppressBinaryToggleHandlers)
            {
                return;
            }

            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                SuppressAndUncheckBinaryToggle(mode);
                return;
            }

            // NOT operation doesn't require overlay
            if (mode != BinaryToggleMode.Not && _binaryOverlayBitmap == null)
            {
                MessageBox.Show("Silakan pilih gambar B terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                SuppressAndUncheckBinaryToggle(mode);
                return;
            }

            // Ensure only one toggle stays active at a time
            _suppressBinaryToggleHandlers = true;
            if (mode != BinaryToggleMode.And) BinaryAndToggle.IsChecked = false;
            if (mode != BinaryToggleMode.Or) BinaryOrToggle.IsChecked = false;
            if (mode != BinaryToggleMode.Not) BinaryNotToggle.IsChecked = false;
            if (mode != BinaryToggleMode.Xor) BinaryXorToggle.IsChecked = false;
            _suppressBinaryToggleHandlers = false;

            if (!ApplyBinaryOperation(mode))
            {
                SuppressAndUncheckBinaryToggle(mode);
            }
        }

        private void HandleBinaryToggleUnchecked()
        {
            if (_suppressBinaryToggleHandlers)
            {
                return;
            }

            if (BinaryAndToggle.IsChecked == true || BinaryOrToggle.IsChecked == true ||
                BinaryNotToggle.IsChecked == true || BinaryXorToggle.IsChecked == true)
            {
                return;
            }

            if (_currentBinaryMode == BinaryToggleMode.None)
            {
                return;
            }

            RestoreBinaryBaseImage();
        }

        private bool ApplyBinaryOperation(BinaryToggleMode mode)
        {
            try
            {
                BitmapSource result;

                if (mode == BinaryToggleMode.Not)
                {
                    result = _binaryImageService.NotImage();
                }
                else
                {
                    if (!TryParseOffset(BinaryOffsetXTextBox.Text, "Offset X", out int offsetX))
                    {
                        return false;
                    }

                    if (!TryParseOffset(BinaryOffsetYTextBox.Text, "Offset Y", out int offsetY))
                    {
                        return false;
                    }

                    result = mode switch
                    {
                        BinaryToggleMode.And => _binaryImageService.AndImage(_binaryOverlayBitmap, offsetX, offsetY),
                        BinaryToggleMode.Or => _binaryImageService.OrImage(_binaryOverlayBitmap, offsetX, offsetY),
                        BinaryToggleMode.Xor => _binaryImageService.XorImage(_binaryOverlayBitmap, offsetX, offsetY),
                        _ => throw new InvalidOperationException("Unknown binary operation")
                    };
                }

                string fallbackName = mode switch
                {
                    BinaryToggleMode.And => "Hasil_AND.png",
                    BinaryToggleMode.Or => "Hasil_OR.png",
                    BinaryToggleMode.Not => "Hasil_NOT.png",
                    BinaryToggleMode.Xor => "Hasil_XOR.png",
                    _ => "Hasil_Boolean.png"
                };

                string fileLabel = _state.CurrentFilePath ?? fallbackName;

                var resultInfo = new ImageLoadResult(
                    result,
                    fileLabel,
                    result.PixelWidth,
                    result.PixelHeight,
                    result.Format.ToString()
                );

                _currentBinaryMode = mode;

                _suppressBinaryToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressBinaryToggleHandlers = false;
                UpdateBinaryButtonsState();

                return true;
            }
            catch (Exception ex)
            {
                string operation = mode.ToString();
                MessageBox.Show($"Gagal melakukan operasi {operation}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentBinaryMode = BinaryToggleMode.None;
                _binaryImageService.ClearBinarySnapshot();
                return false;
            }
        }

        private void RestoreBinaryBaseImage()
        {
            try
            {
                BitmapSource restored = _binaryImageService.RestoreBinaryBase();
                _currentBinaryMode = BinaryToggleMode.None;

                string fileLabel = _state.CurrentFilePath ?? "Gambar.png";
                var resultInfo = new ImageLoadResult(
                    restored,
                    fileLabel,
                    restored.PixelWidth,
                    restored.PixelHeight,
                    restored.Format.ToString()
                );

                _suppressBinaryToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressBinaryToggleHandlers = false;
                UpdateBinaryButtonsState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengembalikan gambar awal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _suppressBinaryToggleHandlers = true;
                BinaryAndToggle.IsChecked = false;
                BinaryOrToggle.IsChecked = false;
                BinaryNotToggle.IsChecked = false;
                BinaryXorToggle.IsChecked = false;
                _suppressBinaryToggleHandlers = false;
                _currentBinaryMode = BinaryToggleMode.None;
                UpdateBinaryButtonsState();
            }
        }

        private void DeactivateBinaryMode()
        {
            if (_currentBinaryMode == BinaryToggleMode.None)
            {
                return;
            }

            _suppressBinaryToggleHandlers = true;
            BinaryAndToggle.IsChecked = false;
            BinaryOrToggle.IsChecked = false;
            BinaryNotToggle.IsChecked = false;
            BinaryXorToggle.IsChecked = false;
            _suppressBinaryToggleHandlers = false;

            RestoreBinaryBaseImage();
        }

        private void UpdateBinaryButtonsState()
        {
            bool hasBase = _state.OriginalBitmap != null;
            bool hasOverlay = _binaryOverlayBitmap != null;
            
            ConvertToBinaryButton.IsEnabled = hasBase;
            BinaryNotToggle.IsEnabled = hasBase;
            
            bool isBinaryOperationEnabled = hasBase && hasOverlay;
            BinaryAndToggle.IsEnabled = isBinaryOperationEnabled;
            BinaryOrToggle.IsEnabled = isBinaryOperationEnabled;
            BinaryXorToggle.IsEnabled = isBinaryOperationEnabled;
        }

        private void SuppressAndUncheckBinaryToggle(BinaryToggleMode mode)
        {
            _suppressBinaryToggleHandlers = true;
            switch (mode)
            {
                case BinaryToggleMode.And:
                    BinaryAndToggle.IsChecked = false;
                    break;
                case BinaryToggleMode.Or:
                    BinaryOrToggle.IsChecked = false;
                    break;
                case BinaryToggleMode.Not:
                    BinaryNotToggle.IsChecked = false;
                    break;
                case BinaryToggleMode.Xor:
                    BinaryXorToggle.IsChecked = false;
                    break;
            }
            _suppressBinaryToggleHandlers = false;
        }
    }
}
