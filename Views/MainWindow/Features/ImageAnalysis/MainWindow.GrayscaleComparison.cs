using System;
using System.Windows;
using MiniPhotoshop.Views.Windows;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        // Handler tombol untuk menampilkan window perbandingan dua metode grayscale
        // (average vs luminance) berdasar pixel dari gambar yang sedang dimuat.
        private void ShowGrayscaleComparison_Click(object sender, RoutedEventArgs e)
        {
            // Pastikan sudah ada gambar yang dimuat sebelum melakukan perbandingan.
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada gambar yang dimuat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Minta service untuk membuat dua bitmap grayscale dengan metode berbeda.
                var average = _grayscaleService.CreateAverageGrayscale();
                var luminance = _grayscaleService.CreateLuminanceGrayscale();

                // Buka window perbandingan dan kirim dua versi grayscale untuk ditampilkan berdampingan.
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

