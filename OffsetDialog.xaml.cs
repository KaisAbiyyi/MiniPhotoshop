using System;
using System.Windows;

namespace MiniPhotoshop
{
    public partial class OffsetDialog : Window
    {
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        public OffsetDialog(int currentX, int currentY)
        {
            InitializeComponent();
            OffsetXBox.Text = currentX.ToString();
            OffsetYBox.Text = currentY.ToString();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(OffsetXBox.Text, out int x))
            {
                MessageBox.Show("Offset X tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(OffsetYBox.Text, out int y))
            {
                MessageBox.Show("Offset Y tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OffsetX = x;
            OffsetY = y;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
