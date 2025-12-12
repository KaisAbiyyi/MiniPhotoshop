using System;
using System.Windows;

namespace MiniPhotoshop.Views.Dialogs
{
    /// <summary>
    /// Dialog for Canny Edge Detection parameters.
    /// Allows user to configure Low/High Threshold, Gaussian Kernel Size, and Sigma.
    /// </summary>
    public partial class CannyEdgeDetectionDialog : Window
    {
        public double LowThreshold { get; private set; } = 50;
        public double HighThreshold { get; private set; } = 150;
        public int GaussianKernelSize { get; private set; } = 5;
        public double Sigma { get; private set; } = 1.4;

        public CannyEdgeDetectionDialog()
        {
            InitializeComponent();
        }

        private void LowThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LowThresholdValue != null)
            {
                LowThreshold = (int)e.NewValue;
                LowThresholdValue.Text = ((int)e.NewValue).ToString();
                
                // Ensure low threshold is always less than high threshold
                if (HighThresholdSlider != null && LowThreshold >= HighThresholdSlider.Value)
                {
                    HighThresholdSlider.Value = LowThreshold + 1;
                }
            }
        }

        private void HighThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (HighThresholdValue != null)
            {
                HighThreshold = (int)e.NewValue;
                HighThresholdValue.Text = ((int)e.NewValue).ToString();
                
                // Ensure high threshold is always greater than low threshold
                if (LowThresholdSlider != null && HighThreshold <= LowThresholdSlider.Value)
                {
                    LowThresholdSlider.Value = HighThreshold - 1;
                }
            }
        }

        private void KernelSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (KernelSizeValue != null)
            {
                // Ensure kernel size is always odd
                int value = (int)e.NewValue;
                if (value % 2 == 0) value++;
                GaussianKernelSize = value;
                KernelSizeValue.Text = value.ToString();
            }
        }

        private void SigmaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SigmaValue != null)
            {
                Sigma = Math.Round(e.NewValue, 1);
                SigmaValue.Text = Sigma.ToString("F1");
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
