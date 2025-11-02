using System;
using System.Windows;
using System.Windows.Controls;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
        private void ApplyFilter(ImageFilterMode mode, bool resetZoom = false)
        {
            try
            {
                DisplayImage.Source = _filterService.SetActiveFilter(mode);
                _filterService.SyncPreviewActivation();
                ResetBrightnessControl();

                if (resetZoom)
                {
                    QueueAutoFit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menerapkan filter: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterPreview_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is ImageFilterMode mode)
            {
                ApplyFilter(mode);
            }
        }

        private void ResetBrightnessControl()
        {
            _suppressBrightnessHandler = true;
            BrightnessSlider.Value = 0;
            _suppressBrightnessHandler = false;
            _brightnessService.Reset();
        }
    }
}
