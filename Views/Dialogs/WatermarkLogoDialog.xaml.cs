using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class WatermarkLogoDialog : Window
    {
        public BitmapSource? WatermarkImage { get; private set; }
        public double WatermarkOpacity { get; private set; } = 0.3;
        public double Scale { get; private set; } = 0.2;
        public WatermarkPosition Position { get; private set; } = WatermarkPosition.BottomRight;
        public bool WasApplied { get; private set; }

        public WatermarkLogoDialog()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "Pilih Logo",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|" +
                         "JPEG Files|*.jpg;*.jpeg|" +
                         "PNG Files|*.png|" +
                         "Bitmap Files|*.bmp|" +
                         "GIF Files|*.gif|" +
                         "TIFF Files|*.tiff|" +
                         "All Files|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                WatermarkImage = bitmap;
                LogoPathBox.Text = dialog.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat logo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (WatermarkImage == null)
            {
                MessageBox.Show("Logo belum dipilih.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(ScaleBox.Text, out double scalePercent) || scalePercent <= 0)
            {
                MessageBox.Show("Scale tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var comboItem = PositionCombo.SelectedItem as ComboBoxItem;
            string? tag = comboItem?.Tag?.ToString();
            if (!Enum.TryParse(tag, out WatermarkPosition position))
            {
                position = WatermarkPosition.BottomRight;
            }

            Scale = Math.Clamp(scalePercent / 100.0, 0.05, 1.0);
            WatermarkOpacity = Math.Clamp(OpacitySlider.Value / 100.0, 0.0, 1.0);
            Position = position;
            WasApplied = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            WasApplied = false;
            DialogResult = false;
            Close();
        }
    }
}
