using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        // Toggle baru untuk mengaktifkan fitur seleksi warna berbasis klik pada gambar.
        private void ColorSelectionToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada gambar yang dimuat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                ColorSelectionToggle.IsChecked = false;
                return;
            }

            try
            {
                // Tampilkan panel informasi seleksi warna di UI.
                ColorSelectionPanel.Visibility = Visibility.Visible;
                // Aktifkan mode seleksi warna di service dan tampilkan gambar referensi.
                DisplayImage.Source = _colorSelectionService.SetColorSelectionActive(true);
                // Daftarkan handler klik di gambar untuk menangkap pixel yang dipilih.
                DisplayImage.MouseLeftButtonDown += DisplayImage_ColorSelection_Click;
                SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
                SelectedColorText.Foreground = Brushes.Blue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengaktifkan seleksi warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ColorSelectionToggle.IsChecked = false;
            }
        }

        private void ColorSelectionToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ColorSelectionPanel.Visibility = Visibility.Collapsed;
            // Hentikan penanganan klik ketika mode seleksi dimatikan.
            DisplayImage.MouseLeftButtonDown -= DisplayImage_ColorSelection_Click;
            try
            {
                // Kembalikan gambar ke kondisi sebelum seleksi warna diaktifkan.
                DisplayImage.Source = _colorSelectionService.SetColorSelectionActive(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menonaktifkan seleksi warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
            SelectedColorText.Foreground = Brushes.Gray;
        }

        // Legacy checkbox handlers (kept for backward compatibility if needed)
        private void ColorSelection_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada gambar yang dimuat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                DisplayImage.Source = _colorSelectionService.SetColorSelectionActive(true);
                DisplayImage.MouseLeftButtonDown += DisplayImage_ColorSelection_Click;
                SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
                SelectedColorText.Foreground = Brushes.Blue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengaktifkan seleksi warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ColorSelection_Unchecked(object sender, RoutedEventArgs e)
        {
            DisplayImage.MouseLeftButtonDown -= DisplayImage_ColorSelection_Click;
            try
            {
                DisplayImage.Source = _colorSelectionService.SetColorSelectionActive(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menonaktifkan seleksi warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
            SelectedColorText.Foreground = Brushes.Gray;
        }

        private void DisplayImage_ColorSelection_Click(object sender, MouseButtonEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                return;
            }

            try
            {
                // Ambil posisi klik relatif terhadap kontrol DisplayImage.
                Point clickPoint = e.GetPosition(DisplayImage);
                // Hitung faktor skala dari ukuran tampilan ke ukuran asli gambar.
                double scaleX = _state.CachedWidth / DisplayImage.ActualWidth;
                double scaleY = _state.CachedHeight / DisplayImage.ActualHeight;

                // Konversi koordinat klik ke koordinat pixel (x, y) dalam gambar asli.
                int pixelX = (int)Math.Clamp(clickPoint.X * scaleX, 0, _state.CachedWidth - 1);
                int pixelY = (int)Math.Clamp(clickPoint.Y * scaleY, 0, _state.CachedHeight - 1);

                // Minta service menerapkan seleksi berdasarkan pixel yang diklik.
                DisplayImage.Source = _colorSelectionService.ApplySelection(pixelX, pixelY);

                // Ambil warna target yang tersimpan di state untuk ditampilkan di teks UI.
                byte r = _state.ColorSelection.TargetR;
                byte g = _state.ColorSelection.TargetG;
                byte b = _state.ColorSelection.TargetB;
                SelectedColorText.Text = $"RGB({r}, {g}, {b})";
                SelectedColorText.Foreground = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memilih warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

