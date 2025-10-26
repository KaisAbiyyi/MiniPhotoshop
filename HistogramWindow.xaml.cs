using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

#nullable enable

namespace MiniPhotoshop
{
    public partial class HistogramWindow : Window
    {
        public HistogramWindow(string channelName, int[] histogram, Color color)
        {
            InitializeComponent();
            
            TitleText.Text = $"Histogram - {channelName}";
            TitleText.Foreground = new SolidColorBrush(color);
            
            Loaded += (s, e) => DrawHistogram(histogram, color);
        }

        private void DrawHistogram(int[] histogram, Color color)
        {
            HistogramCanvas.Children.Clear();
            
            // Validasi: histogram harus 256 elemen (0-255)
            if (histogram == null || histogram.Length != 256) return;
            
            // Cari nilai maksimum dan total (hanya dari 0-255)
            int maxCount = 0;
            long totalPixels = 0;
            
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] > maxCount) maxCount = histogram[i];
                totalPixels += histogram[i];
            }
            
            if (maxCount == 0) return;
            
            // Update labels
            MaxLabel.Text = maxCount.ToString("N0");
            MidLabel.Text = (maxCount / 2).ToString("N0");
            InfoText.Text = $"Total pixels: {totalPixels:N0}  |  Max: {maxCount:N0}";
            
            // Gambar histogram - 255 di ujung kanan
            double width = HistogramCanvas.ActualWidth;
            double height = HistogramCanvas.ActualHeight;
            // Bagi dengan 255 agar bar 255 tepat di ujung (0 di awal, 255 di akhir)
            double barWidth = width / 255.0;
            
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] > 0)
                {
                    double barHeight = (histogram[i] / (double)maxCount) * height;
                    double xPosition = i * barWidth;
                    
                    Rectangle bar = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = new SolidColorBrush(color),
                        ToolTip = $"Intensitas: {i}\nPixels: {histogram[i]:N0}"
                    };
                    
                    Canvas.SetLeft(bar, xPosition);
                    Canvas.SetBottom(bar, 0);
                    HistogramCanvas.Children.Add(bar);
                }
            }
        }
    }
}
