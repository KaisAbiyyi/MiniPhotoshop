using System;
using System.Windows;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private void DistortionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DistortionPanel.Visibility = DistortionPanel.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private void DistortionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Slider value changed - no action needed until Apply button is clicked
        }

        private void ApplyDistortionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                MessageBox.Show("Silakan muat gambar terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                int level = (int)DistortionSlider.Value;
                var result = _distortionService.ApplyDistortion(level);
                
                ApplyImageProcessingResult(result, $"Distorsi Level {level}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menerapkan distorsi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _distortionService.ClearDistortionSnapshot();
            }
        }

        private void RestoreDistortionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var restored = _distortionService.RestoreDistortion();
                
                ApplyImageProcessingResult(restored, "Gambar Dikembalikan");
                DistortionSlider.Value = 5; // Reset slider ke default
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengembalikan gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDistortionButtonState()
        {
            bool hasImage = _state.OriginalBitmap != null;
            DistortionButton.IsEnabled = hasImage;
        }
    }
}
