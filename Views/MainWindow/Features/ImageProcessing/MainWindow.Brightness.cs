using System;
using System.Windows;
using System.Windows;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Jika handler sementara dinonaktifkan (misalnya saat reset UI),
            // abaikan perubahan slider agar tidak memicu proses brightness.
            if (_suppressBrightnessHandler || _state.PixelCache == null)
            {
                return;
            }

            try
            {
                // Pastikan sudah ada gambar yang dimuat sebelum mengubah brightness.
                DisplayImage.Source = _brightnessService.Update(e.NewValue);
                // Minta service brightness menghitung bitmap baru berdasarkan
                // nilai slider terkini (e.NewValue).
                // Tampilkan hasil gambar yang sudah disesuaikan kecerahannya.
            }
            catch (Exception ex)
            {
                // Jika terjadi error saat memproses brightness, tampilkan pesan
                // agar user tahu tanpa membuat aplikasi crash.
                MessageBox.Show($"Gagal menerapkan brightness: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
