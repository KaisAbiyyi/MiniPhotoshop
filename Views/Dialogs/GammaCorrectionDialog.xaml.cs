using System;
using System.Windows;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class GammaCorrectionDialog : Window
    {
        public double Gamma { get; private set; } = 0.8;
        public double Gain { get; private set; } = 1.0;
        public bool WasApplied { get; private set; }

        public GammaCorrectionDialog()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(GammaBox.Text, out double gamma))
            {
                MessageBox.Show("Nilai gamma tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(GainBox.Text, out double gain))
            {
                MessageBox.Show("Nilai gain tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (double.IsNaN(gamma) || double.IsInfinity(gamma) || gamma <= 0)
            {
                MessageBox.Show("Gamma harus lebih besar dari 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (double.IsNaN(gain) || double.IsInfinity(gain) || gain <= 0)
            {
                MessageBox.Show("Gain harus lebih besar dari 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Gamma = gamma;
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
