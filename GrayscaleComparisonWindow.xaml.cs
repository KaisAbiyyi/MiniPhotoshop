using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop
{
    public partial class GrayscaleComparisonWindow : Window
    {
        public GrayscaleComparisonWindow(BitmapSource averageGrayscale, BitmapSource luminanceGrayscale)
        {
            InitializeComponent();
            
            AverageImage.Source = averageGrayscale;
            LuminanceImage.Source = luminanceGrayscale;
        }
    }
}
