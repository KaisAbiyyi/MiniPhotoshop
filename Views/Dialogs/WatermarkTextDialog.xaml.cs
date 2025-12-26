using System;
using System.Windows;
using System.Windows.Controls;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class WatermarkTextDialog : Window
    {
        public string WatermarkText { get; private set; } = "MiniPhotoshop";
        public double WatermarkOpacity { get; private set; } = 0.3;
        public double WatermarkFontSize { get; private set; } = 24;
        public WatermarkPosition Position { get; private set; } = WatermarkPosition.BottomRight;
        public bool WasApplied { get; private set; }

        public WatermarkTextDialog()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            string text = WatermarkTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Teks watermark tidak boleh kosong.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(FontSizeBox.Text, out double fontSize) || fontSize <= 0)
            {
                MessageBox.Show("Ukuran font tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var comboItem = PositionCombo.SelectedItem as ComboBoxItem;
            string? tag = comboItem?.Tag?.ToString();
            if (!Enum.TryParse(tag, out WatermarkPosition position))
            {
                position = WatermarkPosition.BottomRight;
            }

            WatermarkText = text;
            WatermarkFontSize = fontSize;
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
