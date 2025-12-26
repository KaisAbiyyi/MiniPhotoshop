using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Views.MainWindow
{
    /// <summary>
    /// Contrast enhancement handlers for MainWindow.
    /// </summary>
    public partial class MainWindow
    {
        private void ContrastLinear_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.LinearContrastDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.WasApplied)
            {
                try
                {
                    BitmapSource result = _contrastService.ApplyLinearContrast(dialog.Slope, dialog.Intercept);
                    ApplyImageProcessingResult(result, "Linear Contrast");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal menerapkan linear contrast: {ex.Message}", "Error Kontras", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ContrastGamma_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.GammaCorrectionDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.WasApplied)
            {
                try
                {
                    BitmapSource result = _contrastService.ApplyGammaCorrection(dialog.Gamma, dialog.Gain);
                    ApplyImageProcessingResult(result, "Gamma Correction");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal menerapkan gamma correction: {ex.Message}", "Error Kontras", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ContrastAdaptive_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.AdaptiveContrastDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.WasApplied)
            {
                try
                {
                    BitmapSource result = _contrastService.ApplyAdaptiveContrast(3, dialog.Gain);
                    ApplyImageProcessingResult(result, "Adaptive Contrast");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal menerapkan adaptive contrast: {ex.Message}", "Error Kontras", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ContrastStretch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapSource result = _contrastService.ApplyGlobalContrastStretching();
                ApplyImageProcessingResult(result, "Contrast Stretching");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menerapkan contrast stretching: {ex.Message}", "Error Kontras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
