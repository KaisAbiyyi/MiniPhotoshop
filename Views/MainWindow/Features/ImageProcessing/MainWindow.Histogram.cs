using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Views.MainWindow
{
    /// <summary>
    /// Histogram operations handlers for MainWindow (stubs).
    /// </summary>
    public partial class MainWindow
    {
        private void HistogramEqualization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _editor.ApplyHistogramEqualization();
                ApplyImageProcessingResult(result, "Histogram Equalization");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menjalankan Histogram Equalization: {ex.Message}", "Error Histogram", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HistogramLinearStretch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _editor.ApplyLinearStretchEqualization();
                ApplyImageProcessingResult(result, "Linear Stretch Equalization");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menjalankan Linear Stretch: {ex.Message}", "Error Histogram", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void HistogramAdaptiveEqualization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Run heavy operation on background thread to avoid freezing UI
                System.Windows.Input.Cursor? previous = System.Windows.Input.Mouse.OverrideCursor;
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                try
                {
                    var tuple = await System.Threading.Tasks.Task.Run(() => _editor.ComputeAdaptiveHistogramBuffer(63));
                    var result = _editor.ApplyAdaptiveHistogramFromBuffer(tuple.buffer, tuple.width, tuple.height);
                    ApplyImageProcessingResult(result, "Adaptive Histogram Equalization");
                }
                finally
                {
                    System.Windows.Input.Mouse.OverrideCursor = previous;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menjalankan Adaptive Histogram Equalization: {ex.Message}", "Error Histogram", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
