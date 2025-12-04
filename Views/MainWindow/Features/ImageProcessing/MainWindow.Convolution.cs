using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.MainWindow
{
    /// <summary>
    /// Convolution operations handlers for MainWindow.
    /// Handles local area image operations using kernel matrices.
    /// </summary>
    public partial class MainWindow
    {
        #region Event Handlers

        private void ConvolutionLowPass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int kernelSize = GetKernelSizeFromMenuItem(sender);
                var result = _convolutionService.ApplyLowPassFilter(kernelSize);
                ApplyConvolutionResult(result, $"LowPass_{kernelSize}x{kernelSize}");
            }
            catch (Exception ex)
            {
                ShowConvolutionError("Low Pass Filter", ex);
            }
        }

        private void ConvolutionHighPass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _convolutionService.ApplyHighPassFilter();
                ApplyConvolutionResult(result, "HighPass");
            }
            catch (Exception ex)
            {
                ShowConvolutionError("High Pass Filter", ex);
            }
        }

        private void ConvolutionGaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int kernelSize = GetKernelSizeFromMenuItem(sender);
                var result = _convolutionService.ApplyGaussianBlur(kernelSize);
                ApplyConvolutionResult(result, $"Gaussian_{kernelSize}x{kernelSize}");
            }
            catch (Exception ex)
            {
                ShowConvolutionError("Gaussian Blur", ex);
            }
        }

        private void ConvolutionSobelH_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _convolutionService.ApplySobelHorizontal();
                ApplyConvolutionResult(result, "Sobel_Horizontal");
            }
            catch (Exception ex)
            {
                ShowConvolutionError("Sobel Horizontal", ex);
            }
        }

        private void ConvolutionSobelV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _convolutionService.ApplySobelVertical();
                ApplyConvolutionResult(result, "Sobel_Vertical");
            }
            catch (Exception ex)
            {
                ShowConvolutionError("Sobel Vertical", ex);
            }
        }

        private void ConvolutionLaplacian_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _convolutionService.ApplyLaplacian();
                ApplyConvolutionResult(result, "Laplacian");
            }
            catch (Exception ex)
            {
                ShowConvolutionError("Laplacian", ex);
            }
        }

        private void ConvolutionEmboss_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = _convolutionService.ApplyEmboss();
                ApplyConvolutionResult(result, "Emboss");
            }
            catch (Exception ex)
            {
                ShowConvolutionError("Emboss", ex);
            }
        }

        private void ConvolutionCustom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Dialogs.CustomKernelDialog();
                dialog.Owner = this;
                
                if (dialog.ShowDialog() == true && dialog.Applied)
                {
                    var result = _convolutionService.ApplyConvolution(dialog.Kernel, dialog.Multiplier);
                    int size = dialog.Kernel.GetLength(0);
                    ApplyConvolutionResult(result, $"Custom_{size}x{size}");
                }
            }
            catch (Exception ex)
            {
                ShowConvolutionError("Custom Kernel", ex);
            }
        }

        #endregion

        #region Helper Methods

        private int GetKernelSizeFromMenuItem(object sender)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string tagStr)
            {
                if (int.TryParse(tagStr, out int size))
                {
                    return size;
                }
            }
            return 3; // Default kernel size
        }

        private void ApplyConvolutionResult(BitmapSource result, string operationName)
        {
            ApplyImageProcessingResult(result, operationName);
        }

        private void ShowConvolutionError(string operation, Exception ex)
        {
            MessageBox.Show(
                $"Gagal menerapkan {operation}: {ex.Message}",
                "Error Konvolusi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        #endregion
    }
}
