using System;
using System.Windows;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class LinearContrastDialog : Window
    {
        public double Slope { get; private set; } = 1.2;
        public double Intercept { get; private set; } = 0.0;
        public bool WasApplied { get; private set; }

        public LinearContrastDialog()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(SlopeBox.Text, out double slope))
            {
                MessageBox.Show("Nilai slope tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(InterceptBox.Text, out double intercept))
            {
                MessageBox.Show("Nilai intercept tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (double.IsNaN(slope) || double.IsInfinity(slope))
            {
                MessageBox.Show("Nilai slope tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (double.IsNaN(intercept) || double.IsInfinity(intercept))
            {
                MessageBox.Show("Nilai intercept tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Slope = slope;
            Intercept = intercept;
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
