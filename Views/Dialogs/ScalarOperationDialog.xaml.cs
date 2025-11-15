using System;
using System.Windows;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class ScalarOperationDialog : Window
    {
        public bool IsMultiply { get; private set; }
        public double ScalarValue { get; private set; }
        public bool WasApplied { get; private set; }

        public ScalarOperationDialog()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(ScalarValueBox.Text, out double value))
            {
                MessageBox.Show("Nilai skalar tidak valid. Masukkan angka yang benar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (value == 0 && DivideRadio.IsChecked == true)
            {
                MessageBox.Show("Tidak dapat membagi dengan nol.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IsMultiply = MultiplyRadio.IsChecked == true;
            ScalarValue = value;
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
