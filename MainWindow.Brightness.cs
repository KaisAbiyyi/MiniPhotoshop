using System;
using System.Windows;
using System.Windows;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_suppressBrightnessHandler || _state.PixelCache == null)
            {
                return;
            }

            try
            {
                DisplayImage.Source = _brightnessService.Update(e.NewValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menerapkan brightness: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
