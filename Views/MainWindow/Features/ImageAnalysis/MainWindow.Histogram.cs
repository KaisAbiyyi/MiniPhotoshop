using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MiniPhotoshop.Views.Windows;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private void RenderHistograms()
        {
            if (_state.PixelCache == null)
            {
                return;
            }

            var data = _histogramService.Build();
            DrawHistogramOnCanvas(HistogramRed, data.Red, Colors.Red);
            DrawHistogramOnCanvas(HistogramGreen, data.Green, Colors.Green);
            DrawHistogramOnCanvas(HistogramBlue, data.Blue, Colors.Blue);
            DrawHistogramOnCanvas(HistogramGray, data.Gray, Colors.Gray);
        }

        private static void DrawHistogramOnCanvas(Canvas canvas, int[] histogram, Color color)
        {
            canvas.Children.Clear();
            if (histogram == null || histogram.Length != 256)
            {
                return;
            }

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            int maxCount = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                if (histogram[i] > maxCount)
                {
                    maxCount = histogram[i];
                }
            }

            if (maxCount == 0)
            {
                return;
            }

            double barWidth = width / 255.0;

            for (int i = 0; i < histogram.Length; i++)
            {
                if (histogram[i] == 0)
                {
                    continue;
                }

                double barHeight = (histogram[i] / (double)maxCount) * height;
                Rectangle rectangle = new()
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new SolidColorBrush(color)
                };

                Canvas.SetLeft(rectangle, i * barWidth);
                Canvas.SetTop(rectangle, height - barHeight);
                canvas.Children.Add(rectangle);
            }
        }

        private void HistogramRed_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowHistogramWindow("Red Channel", _state.Histogram.Red, Colors.Red);
        }

        private void HistogramGreen_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowHistogramWindow("Green Channel", _state.Histogram.Green, Colors.Green);
        }

        private void HistogramBlue_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowHistogramWindow("Blue Channel", _state.Histogram.Blue, Colors.Blue);
        }

        private void HistogramGray_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowHistogramWindow("Grayscale Channel", _state.Histogram.Gray, Colors.Gray);
        }

        private void ShowHistogramWindow(string channelName, int[] histogram, Color color)
        {
            if (histogram == null || histogram.Length != 256)
            {
                MessageBox.Show("Histogram tidak tersedia.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            HistogramWindow window = new(channelName, histogram, color)
            {
                Owner = this
            };
            window.ShowDialog();
        }
    }
}
