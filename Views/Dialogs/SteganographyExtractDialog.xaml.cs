using System.Windows;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class SteganographyExtractDialog : Window
    {
        public SteganographyExtractDialog(string message)
        {
            InitializeComponent();
            MessageTextBox.Text = message ?? string.Empty;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
