using System;
using System.Windows;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class AdaptiveContrastDialog : Window
    {
        public double Gain { get; private set; } = 1.5;
        public bool WasApplied { get; private set; }

        public AdaptiveContrastDialog()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(GainBox.Text, out double gain))
            {
                MessageBox.Show("Nilai gain tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (double.IsNaN(gain) || double.IsInfinity(gain) || gain <= 0)
            {
                MessageBox.Show("Gain harus lebih besar dari 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Gain = gain;
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
