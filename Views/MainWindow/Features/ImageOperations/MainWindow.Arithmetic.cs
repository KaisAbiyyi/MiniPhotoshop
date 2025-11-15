using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MiniPhotoshop.Core.Models;
using MiniPhotoshop.Views.Dialogs;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        // DEPRECATED: Method ini sudah diganti dengan AddImageB_Click di MainWindow.ToolbarHandlers.cs
        // Dikomen agar tidak error tapi tetap ada untuk referensi
        /*
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
                // ArithmeticInfoText sudah tidak ada di UI baru
                // UpdateArithmeticButtonsState();

                if (_currentArithmeticMode != ArithmeticToggleMode.None)
                {
                    DeactivateArithmeticMode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat gambar B: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _arithmeticOverlayBitmap = null;
                // UpdateArithmeticButtonsState();
            }
        }
        */

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

            // Use fields from ToolbarHandlers instead of TextBox controls
            int offsetX = _arithmeticOffsetX;
            int offsetY = _arithmeticOffsetY;

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

                _currentArithmeticMode = isAddition ? ArithmeticToggleMode.Add : ArithmeticToggleMode.Subtract;

                _suppressArithmeticToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressArithmeticToggleHandlers = false;
                // UpdateArithmeticButtonsState is now in ToolbarHandlers
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
                // UpdateArithmeticButtonsState(); // Moved to ToolbarHandlers
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengembalikan gambar awal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _suppressArithmeticToggleHandlers = true;
                // ArithmeticAddToggle/SubtractToggle no longer exist in new UI
                _suppressArithmeticToggleHandlers = false;
                _currentArithmeticMode = ArithmeticToggleMode.None;
                // UpdateArithmeticButtonsState(); // Moved to ToolbarHandlers
            }
        }

        private void DeactivateArithmeticMode()
        {
            if (_currentArithmeticMode == ArithmeticToggleMode.None)
            {
                return;
            }

            _suppressArithmeticToggleHandlers = true;
            // ArithmeticAddToggle/SubtractToggle no longer exist in new UI
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

        // DEPRECATED: UpdateArithmeticButtonsState moved to MainWindow.ToolbarHandlers.cs
        /*
        private void UpdateArithmeticButtonsState()
        {
            bool hasBase = _state.OriginalBitmap != null;
            bool hasOverlay = _arithmeticOverlayBitmap != null;
            bool isEnabled = hasBase && hasOverlay;
            ArithmeticAddToggle.IsEnabled = isEnabled;
            ArithmeticSubtractToggle.IsEnabled = isEnabled;
            // ArithmeticMultiplyToggle.IsEnabled = isEnabled;
            // ArithmeticDivideToggle.IsEnabled = isEnabled;
        }
        */

        private void ArithmeticMultiplyToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleImageArithmeticToggleChecked(true);
        }

        private void ArithmeticMultiplyToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleImageArithmeticToggleUnchecked();
        }

        private void ArithmeticDivideToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleImageArithmeticToggleChecked(false);
        }

        private void ArithmeticDivideToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleImageArithmeticToggleUnchecked();
        }

        private void HandleImageArithmeticToggleChecked(bool isMultiply)
        {
            if (_suppressArithmeticToggleHandlers)
            {
                return;
            }

            if (!EnsureArithmeticReady())
            {
                SuppressAndUncheckImageArithmeticToggle(isMultiply);
                return;
            }

            // Ensure only one toggle stays active at a time
            if (isMultiply && ArithmeticDivideToggle.IsChecked == true)
            {
                _suppressArithmeticToggleHandlers = true;
                ArithmeticDivideToggle.IsChecked = false;
                _suppressArithmeticToggleHandlers = false;
            }
            else if (!isMultiply && ArithmeticMultiplyToggle.IsChecked == true)
            {
                _suppressArithmeticToggleHandlers = true;
                ArithmeticMultiplyToggle.IsChecked = false;
                _suppressArithmeticToggleHandlers = false;
            }

            // Also uncheck add/subtract toggles
            _suppressArithmeticToggleHandlers = true;
            ArithmeticAddToggle.IsChecked = false;
            ArithmeticSubtractToggle.IsChecked = false;
            _suppressArithmeticToggleHandlers = false;

            if (!ApplyImageMultiplyDivideOperation(isMultiply))
            {
                SuppressAndUncheckImageArithmeticToggle(isMultiply);
            }
        }

        private void HandleImageArithmeticToggleUnchecked()
        {
            if (_suppressArithmeticToggleHandlers)
            {
                return;
            }

            // if (ArithmeticMultiplyToggle.IsChecked == true || ArithmeticDivideToggle.IsChecked == true)
            // {
            //     return;
            // }

            if (_currentArithmeticMode != ArithmeticToggleMode.Multiply && 
                _currentArithmeticMode != ArithmeticToggleMode.Divide)
            {
                return;
            }

            RestoreArithmeticBaseImage();
        }

        private bool ApplyImageMultiplyDivideOperation(bool isMultiply)
        {
            if (_arithmeticOverlayBitmap == null)
            {
                MessageBox.Show("Silakan pilih gambar B terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (!TryParseOffset(_arithmeticOffsetX.ToString(), "Offset X", out int offsetX))
            {
                return false;
            }

            if (!TryParseOffset(_arithmeticOffsetY.ToString(), "Offset Y", out int offsetY))
            {
                return false;
            }

            try
            {
                string normInfo;
                BitmapSource result = isMultiply
                    ? _arithmeticService.MultiplyImage(_arithmeticOverlayBitmap, offsetX, offsetY, out normInfo)
                    : _arithmeticService.DivideImage(_arithmeticOverlayBitmap, offsetX, offsetY, out normInfo);

                string fallbackName = isMultiply ? "Hasil_Perkalian_Citra.png" : "Hasil_Pembagian_Citra.png";
                string fileLabel = _state.CurrentFilePath ?? fallbackName;

                var resultInfo = new ImageLoadResult(
                    result,
                    fileLabel,
                    result.PixelWidth,
                    result.PixelHeight,
                    result.Format.ToString()
                );

                _currentArithmeticMode = isMultiply ? ArithmeticToggleMode.Multiply : ArithmeticToggleMode.Divide;

                _suppressArithmeticToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressArithmeticToggleHandlers = false;
                // UpdateArithmeticButtonsState();

                return true;
            }
            catch (Exception ex)
            {
                string action = isMultiply ? "mengalikan" : "membagi";
                MessageBox.Show($"Gagal {action} gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentArithmeticMode = ArithmeticToggleMode.None;
                _arithmeticService.ClearArithmeticSnapshot();
                return false;
            }
        }

        private void SuppressAndUncheckImageArithmeticToggle(bool isMultiply)
        {
            _suppressArithmeticToggleHandlers = true;
            if (isMultiply)
            {
                // ArithmeticMultiplyToggle.IsChecked = false;
            }
            else
            {
                // ArithmeticDivideToggle.IsChecked = false;
            }
            _suppressArithmeticToggleHandlers = false;
        }

        private void UpdateScalarButtonsState()
        {
            bool hasBase = _state.OriginalBitmap != null;
            // // ScalarMultiplyToggle.IsEnabled = hasBase;
            // // ScalarDivideToggle.IsEnabled = hasBase;
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

        // Deprecated: Scalar toggle handlers removed (now using ScalarOperationToggle dialog)
        // private void ScalarMultiplyToggle_Checked(object sender, RoutedEventArgs e)
        // {
        //     HandleScalarToggleChecked(isMultiply: true);
        // }

        // private void ScalarMultiplyToggle_Unchecked(object sender, RoutedEventArgs e)
        // {
        //     HandleScalarToggleUnchecked();
        // }

        // private void ScalarDivideToggle_Checked(object sender, RoutedEventArgs e)
        // {
        //     HandleScalarToggleChecked(isMultiply: false);
        // }

        // private void ScalarDivideToggle_Unchecked(object sender, RoutedEventArgs e)
        // {
        //     HandleScalarToggleUnchecked();
        // }

        // Deprecated: HandleScalarToggleChecked now handled by ScalarOperationToggle_Checked in ToolbarHandlers
        // private void HandleScalarToggleChecked(bool isMultiply)
        // {
        //     if (_suppressScalarToggleHandlers)
        //     {
        //         return;
        //     }
        //
        //     if (_state.OriginalBitmap == null)
        //     {
        //         MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        //         SuppressAndUncheckScalarToggle(isMultiply);
        //         return;
        //     }
        //
        //     if (!TryParseScalar(ScalarValueTextBox.Text, out double scalar))
        //     {
        //         SuppressAndUncheckScalarToggle(isMultiply);
        //         return;
        //     }
        //
        //     if (!isMultiply && scalar == 0)
        //     {
        //         MessageBox.Show("Tidak dapat membagi dengan nol.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //         SuppressAndUncheckScalarToggle(isMultiply);
        //         return;
        //     }
        //
        //     // Ensure only one scalar toggle stays active at a time
        //     if (isMultiply && ScalarDivideToggle.IsChecked == true)
        //     {
        //         _suppressScalarToggleHandlers = true;
        //         ScalarDivideToggle.IsChecked = false;
        //         _suppressScalarToggleHandlers = false;
        //     }
        //     else if (!isMultiply && ScalarMultiplyToggle.IsChecked == true)
        //     {
        //         _suppressScalarToggleHandlers = true;
        //         ScalarMultiplyToggle.IsChecked = false;
        //         _suppressScalarToggleHandlers = false;
        //     }
        //
        //     if (!ApplyScalarOperation(isMultiply, scalar))
        //     {
        //         SuppressAndUncheckScalarToggle(isMultiply);
        //     }
        // }

        // Deprecated: HandleScalarToggleUnchecked now handled by ScalarOperationToggle_Unchecked in ToolbarHandlers
        // private void HandleScalarToggleUnchecked()
        // {
        //     if (_suppressScalarToggleHandlers)
        //     {
        //         return;
        //     }
        //
        //     if (ScalarMultiplyToggle.IsChecked == true || ScalarDivideToggle.IsChecked == true)
        //     {
        //         return;
        //     }
        //
        //     if (_currentScalarMode == ScalarToggleMode.None)
        //     {
        //         return;
        //     }
        //
        //     RestoreScalarBaseImage();
        // }

        private bool ApplyScalarOperation(bool isMultiply, double scalar)
        {
            try
            {
                BitmapSource result = isMultiply
                    ? _arithmeticService.MultiplyByScalar(scalar)
                    : _arithmeticService.DivideByScalar(scalar);

                string fallbackName = isMultiply ? "Hasil_Perkalian.png" : "Hasil_Pembagian.png";
                string fileLabel = _state.CurrentFilePath ?? fallbackName;

                var resultInfo = new ImageLoadResult(
                    result,
                    fileLabel,
                    result.PixelWidth,
                    result.PixelHeight,
                    result.Format.ToString()
                );

                _currentScalarMode = isMultiply ? ScalarToggleMode.Multiply : ScalarToggleMode.Divide;

                _suppressScalarToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressScalarToggleHandlers = false;
                return true;
            }
            catch (Exception ex)
            {
                string action = isMultiply ? "mengalikan" : "membagi";
                MessageBox.Show($"Gagal {action} gambar dengan skalar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentScalarMode = ScalarToggleMode.None;
                _arithmeticService.ClearArithmeticSnapshot();
                return false;
            }
        }

        private void RestoreScalarBaseImage()
        {
            try
            {
                BitmapSource restored = _arithmeticService.RestoreArithmeticBase();
                _currentScalarMode = ScalarToggleMode.None;

                string fileLabel = _state.CurrentFilePath ?? "Gambar.png";
                var resultInfo = new ImageLoadResult(
                    restored,
                    fileLabel,
                    restored.PixelWidth,
                    restored.PixelHeight,
                    restored.Format.ToString()
                );

                _suppressScalarToggleHandlers = true;
                ApplyLoadedImage(resultInfo);
                _suppressScalarToggleHandlers = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengembalikan gambar awal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _suppressScalarToggleHandlers = true;
                // // ScalarMultiplyToggle.IsChecked = false;
                // // ScalarDivideToggle.IsChecked = false;
                _suppressScalarToggleHandlers = false;
                _currentScalarMode = ScalarToggleMode.None;
            }
        }

        private bool TryParseScalar(string? input, out double value)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Nilai skalar tidak boleh kosong.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                value = 0;
                return false;
            }

            if (!double.TryParse(input, out value))
            {
                MessageBox.Show("Nilai skalar harus berupa angka.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        private void SuppressAndUncheckScalarToggle(bool isMultiply)
        {
            _suppressScalarToggleHandlers = true;
            if (isMultiply)
            {
                // // ScalarMultiplyToggle.IsChecked = false;
            }
            else
            {
                // // ScalarDivideToggle.IsChecked = false;
            }
            _suppressScalarToggleHandlers = false;
        }
    }
}
