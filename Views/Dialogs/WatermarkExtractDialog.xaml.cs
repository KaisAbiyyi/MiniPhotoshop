using System.Windows;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class WatermarkExtractDialog : Window
    {
        public WatermarkExtractDialog(WatermarkInfo info)
        {
            InitializeComponent();

            WatermarkTypeText.Text = $"Tipe: {info.Type}";
            WatermarkOpacityText.Text = $"Opacity: {(info.Opacity * 100):0}%";
            WatermarkPositionText.Text = $"Posisi: {info.Position}";

            if (info.Type == WatermarkType.Text)
            {
                TextPanel.Visibility = Visibility.Visible;
                ImagePanel.Visibility = Visibility.Collapsed;
                WatermarkTextBox.Text = info.Text ?? "-";
                WatermarkSizeText.Text = $"Ukuran Font: {info.FontSize:0}";
            }
            else
            {
                TextPanel.Visibility = Visibility.Collapsed;
                ImagePanel.Visibility = Visibility.Visible;
                WatermarkImage.Source = info.Image;
                WatermarkSizeText.Text = $"Scale: {(info.Scale * 100):0}%";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
