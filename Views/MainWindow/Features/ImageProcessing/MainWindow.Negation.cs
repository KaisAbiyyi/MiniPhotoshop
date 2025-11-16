using System;
using System.Windows;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        // Toggle tersembunyi yang mengontrol aktif/non-aktifnya efek negasi
        // (dipicu oleh tombol UI NegationButton di tab Filter).
        private void NegationToggle_Checked(object sender, RoutedEventArgs e)
        {
            HandleNegationToggle(true);
        }

        private void NegationToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleNegationToggle(false);
        }

        private void HandleNegationToggle(bool isActive)
        {
            if (_state.OriginalBitmap == null)
            {
                return;
            }

            try
            {
                // Reset kontrol brightness agar tidak bercampur dengan efek negasi.
                ResetBrightnessControl();
                // Beri tahu service untuk mengubah flag negasi dan kembalikan processed bitmap.
                DisplayImage.Source = _negationService.SetNegationActive(isActive);
                // Sinkronkan status preview filter di sidebar dengan filter aktif saat ini.
                _filterService.SyncPreviewActivation();
                // Update tampilan tombol UI (warna background, dsb.) supaya pengguna tahu status negasi.
                UpdateNegationButtonStyle(isActive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menerapkan negasi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

