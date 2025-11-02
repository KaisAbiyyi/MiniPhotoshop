using System;
using System.Windows;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
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
                ResetBrightnessControl();
                DisplayImage.Source = _negationService.SetNegationActive(isActive);
                _filterService.SyncPreviewActivation();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menerapkan negasi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

