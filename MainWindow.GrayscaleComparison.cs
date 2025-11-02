using System;
using System.Windows;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
        private void ShowGrayscaleComparison_Click(object sender, RoutedEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada gambar yang dimuat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var average = _grayscaleService.CreateAverageGrayscale();
                var luminance = _grayscaleService.CreateLuminanceGrayscale();

                GrayscaleComparisonWindow comparisonWindow = new(average, luminance)
                {
                    Owner = this
                };
                comparisonWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal membuat perbandingan grayscale: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

