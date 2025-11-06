using System.Windows;
using System.Windows.Controls;

namespace MiniPhotoshop
{
    public enum BinaryOperationType
    {
        And,
        Or,
        Not,
        Xor
    }

    public partial class BinaryOperationDialog : Window
    {
        public BinaryOperationType OperationType { get; private set; }
        public bool WasApplied { get; private set; }

        public BinaryOperationDialog()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (OperationComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string tag = selectedItem.Tag?.ToString() ?? "And";
                OperationType = tag switch
                {
                    "And" => BinaryOperationType.And,
                    "Or" => BinaryOperationType.Or,
                    "Not" => BinaryOperationType.Not,
                    "Xor" => BinaryOperationType.Xor,
                    _ => BinaryOperationType.And
                };
            }

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
