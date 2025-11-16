using System;
using System.Windows;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        // Toggle utama untuk mengaktifkan panel dan efek binary threshold.
        private void BinaryThresholdToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada gambar yang dimuat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                BinaryThresholdToggle.IsChecked = false;
                return;
            }

            // Tampilkan panel slider threshold di UI.
            BinaryThresholdPanel.Visibility = Visibility.Visible;
            try
            {
                // Beri tahu service bahwa mode threshold aktif dan minta processed bitmap terbaru.
                DisplayImage.Source = _binaryThresholdService.SetBinaryThresholdActive(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengaktifkan binary threshold: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BinaryThresholdToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            // Sembunyikan panel ketika fitur dinonaktifkan.
            BinaryThresholdPanel.Visibility = Visibility.Collapsed;
            try
            {
                // Matikan mode threshold dan kembalikan tampilan ke pipeline normal.
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

            // Jika belum ada gambar atau mode threshold belum aktif, tidak perlu update.
            if (_state.PixelCache == null || !_state.IsBinaryThresholdActive)
            {
                return;
            }

            try
            {
                // Kirim nilai threshold baru ke service untuk dihitung ulang.
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
