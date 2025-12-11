using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Views.MainWindow
{
    /// <summary>
    /// Edge Detection operations handlers for MainWindow.
    /// Handles Sobel, Prewitt, Robert, and Canny edge detection.
    /// </summary>
    public partial class MainWindow
    {
        #region Sobel Edge Detection

        private void EdgeDetectionSobelH_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _edgeDetectionService.ApplySobelHorizontalEdge();
                ApplyEdgeDetectionResult(result, "Sobel_Horizontal");
            }
            catch (Exception ex)
            {
                ShowEdgeDetectionError("Sobel Horizontal", ex);
            }
        }

        private void EdgeDetectionSobelV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _edgeDetectionService.ApplySobelVerticalEdge();
                ApplyEdgeDetectionResult(result, "Sobel_Vertical");
            }
            catch (Exception ex)
            {
                ShowEdgeDetectionError("Sobel Vertical", ex);
            }
        }

        private void EdgeDetectionSobelMagnitude_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _edgeDetectionService.ApplySobelMagnitude();
                ApplyEdgeDetectionResult(result, "Sobel_Magnitude");
            }
            catch (Exception ex)
            {
                ShowEdgeDetectionError("Sobel Magnitude", ex);
            }
        }

        #endregion

        #region Prewitt Edge Detection

        private void EdgeDetectionPrewitt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _edgeDetectionService.ApplyPrewitt();
                ApplyEdgeDetectionResult(result, "Prewitt");
            }
            catch (Exception ex)
            {
                ShowEdgeDetectionError("Prewitt", ex);
            }
        }

        #endregion

        #region Robert Cross Edge Detection

        private void EdgeDetectionRobert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _edgeDetectionService.ApplyRobert();
                ApplyEdgeDetectionResult(result, "Robert_Cross");
            }
            catch (Exception ex)
            {
                ShowEdgeDetectionError("Robert Cross", ex);
            }
        }

        #endregion

        #region Canny Edge Detection

        private void EdgeDetectionCanny_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Default thresholds: low=50, high=150
                var result = _edgeDetectionService.ApplyCanny(50, 150);
                ApplyEdgeDetectionResult(result, "Canny");
            }
            catch (Exception ex)
            {
                ShowEdgeDetectionError("Canny", ex);
            }
        }

        #endregion

        #region Helper Methods

        private void ApplyEdgeDetectionResult(BitmapSource result, string operationName)
        {
            ApplyImageProcessingResult(result, operationName);
        }

        private void ShowEdgeDetectionError(string operation, Exception ex)
        {
            MessageBox.Show(
                $"Gagal menerapkan {operation}: {ex.Message}",
                "Error Deteksi Tepi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        #endregion
    }
}
