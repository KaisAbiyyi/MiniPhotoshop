using System;
using System.Windows;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            RotationPanel.Visibility = RotationPanel.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private void Rotate45Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyRotation(45);
        }

        private void Rotate90Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyRotation(90);
        }

        private void Rotate180Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyRotation(180);
        }

        private void Rotate270Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyRotation(270);
        }

        private void RotateLeft1Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyIncrementalRotation(-1);
        }

        private void RotateRight1Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyIncrementalRotation(1);
        }

        private void RestoreRotationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var restored = _rotationService.RestoreOriginal();
                _currentRotationMode = RotationMode.None;
                _cumulativeRotationAngle = 0; // Reset cumulative angle

                string fileLabel = _state.CurrentFilePath ?? "Gambar.png";
                var resultInfo = new ImageLoadResult(
                    restored,
                    fileLabel,
                    restored.PixelWidth,
                    restored.PixelHeight,
                    restored.Format.ToString()
                );

                ApplyLoadedImage(resultInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengembalikan gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyRotation(double degrees)
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Set cumulative angle to this degree (untuk preset rotation)
                _cumulativeRotationAngle = degrees;
                
                // Normalize to 0-360 range
                var normalizedAngle = _cumulativeRotationAngle % 360;
                if (normalizedAngle < 0) normalizedAngle += 360;

                var result = normalizedAngle switch
                {
                    45 => _rotationService.Rotate45(),
                    90 => _rotationService.Rotate90(),
                    180 => _rotationService.Rotate180(),
                    270 => _rotationService.Rotate270(),
                    _ => _rotationService.RotateCustom(normalizedAngle)
                };

                _currentRotationMode = RotationMode.Rotated;

                string fileLabel = $"Rotasi_{normalizedAngle:F1}°.png";
                var resultInfo = new ImageLoadResult(
                    result,
                    fileLabel,
                    result.PixelWidth,
                    result.PixelHeight,
                    result.Format.ToString()
                );

                ApplyLoadedImage(resultInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal merotasi gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentRotationMode = RotationMode.None;
                _rotationService.ClearRotationSnapshot();
            }
        }

        private void ApplyIncrementalRotation(double deltaDegrees)
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Accumulate rotation angle
                _cumulativeRotationAngle += deltaDegrees;
                
                // Normalize to 0-360 range
                var normalizedAngle = _cumulativeRotationAngle % 360;
                if (normalizedAngle < 0) normalizedAngle += 360;

                // Apply cumulative rotation
                var result = _rotationService.RotateCustom(normalizedAngle);
                _currentRotationMode = RotationMode.Rotated;

                string fileLabel = $"Rotasi_{normalizedAngle:F1}°.png";
                var resultInfo = new ImageLoadResult(
                    result,
                    fileLabel,
                    result.PixelWidth,
                    result.PixelHeight,
                    result.Format.ToString()
                );

                ApplyLoadedImage(resultInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal merotasi gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentRotationMode = RotationMode.None;
                _rotationService.ClearRotationSnapshot();
            }
        }

        private void ApplyCustomRotationButton_Click(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Parse input value
            if (double.TryParse(CustomRotationInput.Text, out double angle))
            {
                // Normalize to 0-360 range
                angle = angle % 360;
                if (angle < 0) angle += 360;

                ApplyRotation(angle);
            }
            else
            {
                MessageBox.Show("Masukkan nilai sudut yang valid (angka).", "Input Tidak Valid", MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomRotationInput.Focus();
                CustomRotationInput.SelectAll();
            }
        }

        private void CustomRotationInput_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only numbers, minus sign, and decimal point
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            
            // Check if the resulting text is a valid number format
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(fullText, @"^-?\d*\.?\d*$");
        }

        private void UpdateRotationButtonsState()
        {
            bool hasImage = _state.OriginalBitmap != null;
            RotateButton.IsEnabled = hasImage;
        }
    }
}
