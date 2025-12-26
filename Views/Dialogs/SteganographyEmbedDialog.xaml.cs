using System.Text;
using System.Windows;

namespace MiniPhotoshop.Views.Dialogs
{
    public partial class SteganographyEmbedDialog : Window
    {
        private readonly int _maxBytes;

        public string Message { get; private set; } = string.Empty;
        public bool WasApplied { get; private set; }

        public SteganographyEmbedDialog(int maxBytes)
        {
            _maxBytes = maxBytes;
            InitializeComponent();
            CapacityText.Text = $"Maksimum: {maxBytes} bytes";
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Pesan tidak boleh kosong.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int byteCount = Encoding.UTF8.GetByteCount(message);
            if (byteCount > _maxBytes)
            {
                MessageBox.Show($"Pesan terlalu panjang. Maksimum {_maxBytes} bytes.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Message = message;
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
