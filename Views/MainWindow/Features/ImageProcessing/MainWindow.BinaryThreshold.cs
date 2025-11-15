using System;
using System.Windows;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private void BinaryThresholdToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada gambar yang dimuat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                BinaryThresholdToggle.IsChecked = false;
                return;
            }

            BinaryThresholdPanel.Visibility = Visibility.Visible;
            try
            {
                DisplayImage.Source = _binaryThresholdService.SetBinaryThresholdActive(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengaktifkan binary threshold: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BinaryThresholdToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            BinaryThresholdPanel.Visibility = Visibility.Collapsed;
            try
            {
                DisplayImage.Source = _binaryThresholdService.SetBinaryThresholdActive(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menonaktifkan binary threshold: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BinaryThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int thresholdValue = (int)Math.Round(e.NewValue);
            UpdateBinaryThresholdLabel(thresholdValue);

            if (_state.PixelCache == null || !_state.IsBinaryThresholdActive)
            {
                return;
            }

            try
            {
                DisplayImage.Source = _binaryThresholdService.UpdateThreshold(thresholdValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memperbarui binary threshold: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateBinaryThresholdLabel(int value)
        {
            // BinaryThresholdValueText control removed from UI
            // if (BinaryThresholdValueText == null)
            // {
            //     return;
            // }
            //
            // BinaryThresholdValueText.Text = $"Nilai Threshold: {value}";
        }
    }
}
