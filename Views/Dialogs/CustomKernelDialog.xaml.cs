using System;
using System.Windows;
using System.Windows.Controls;

namespace MiniPhotoshop.Views.Dialogs
{
    /// <summary>
    /// Dialog untuk input kernel konvolusi custom.
    /// </summary>
    public partial class CustomKernelDialog : Window
    {
        private int _kernelSize = 3;
        private TextBox[,] _kernelCells;

        /// <summary>
        /// Gets the custom kernel matrix.
        /// </summary>
        public double[,] Kernel { get; private set; }

        /// <summary>
        /// Gets the multiplier value.
        /// </summary>
        public double Multiplier { get; private set; } = 1.0;

        /// <summary>
        /// Gets whether the user clicked Apply.
        /// </summary>
        public bool Applied { get; private set; }

        public CustomKernelDialog()
        {
            InitializeComponent();
            _kernelCells = new TextBox[_kernelSize, _kernelSize];
            CreateKernelGrid(_kernelSize);
        }

        /// <summary>
        /// Creates the kernel input grid.
        /// </summary>
        private void CreateKernelGrid(int size)
        {
            _kernelSize = size;
            _kernelCells = new TextBox[size, size];
            
            KernelGrid.Children.Clear();
            KernelGrid.RowDefinitions.Clear();
            KernelGrid.ColumnDefinitions.Clear();

            // Create rows and columns
            for (int i = 0; i < size; i++)
            {
                KernelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                KernelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            // Create textboxes
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    var textBox = new TextBox
                    {
                        Width = 50,
                        Height = 30,
                        Margin = new Thickness(2),
                        TextAlignment = TextAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Text = "0"
                    };
                    
                    Grid.SetRow(textBox, row);
                    Grid.SetColumn(textBox, col);
                    
                    KernelGrid.Children.Add(textBox);
                    _kernelCells[row, col] = textBox;
                }
            }

            // Set center value to 1 (identity)
            int center = size / 2;
            _kernelCells[center, center].Text = "1";
        }

        /// <summary>
        /// Parses kernel values from UI.
        /// </summary>
        private bool TryParseKernel(out double[,] kernel)
        {
            kernel = new double[_kernelSize, _kernelSize];

            for (int row = 0; row < _kernelSize; row++)
            {
                for (int col = 0; col < _kernelSize; col++)
                {
                    string text = _kernelCells[row, col].Text.Trim();
                    
                    if (!double.TryParse(text, out double value))
                    {
                        MessageBox.Show($"Nilai tidak valid pada baris {row + 1}, kolom {col + 1}: '{text}'",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    
                    kernel[row, col] = value;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets kernel values to UI.
        /// </summary>
        private void SetKernelValues(double[,] values)
        {
            int rows = Math.Min(values.GetLength(0), _kernelSize);
            int cols = Math.Min(values.GetLength(1), _kernelSize);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    _kernelCells[row, col].Text = values[row, col].ToString("G4");
                }
            }
        }

        #region Event Handlers

        private void KernelSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard against early calls before grid is initialized
            if (KernelGrid == null) return;
            
            if (KernelSizeCombo.SelectedItem is ComboBoxItem item)
            {
                string content = item.Content?.ToString() ?? "3x3";
                int size = content switch
                {
                    "5x5" => 5,
                    "7x7" => 7,
                    _ => 3
                };
                
                CreateKernelGrid(size);
            }
        }

        private void ResetKernel_Click(object sender, RoutedEventArgs e)
        {
            // Set all values to 0
            for (int row = 0; row < _kernelSize; row++)
            {
                for (int col = 0; col < _kernelSize; col++)
                {
                    _kernelCells[row, col].Text = "0";
                }
            }
        }

        private void IdentityKernel_Click(object sender, RoutedEventArgs e)
        {
            // Reset all to 0
            for (int row = 0; row < _kernelSize; row++)
            {
                for (int col = 0; col < _kernelSize; col++)
                {
                    _kernelCells[row, col].Text = "0";
                }
            }
            
            // Set center to 1
            int center = _kernelSize / 2;
            _kernelCells[center, center].Text = "1";
            MultiplierTextBox.Text = "1.0";
        }

        private void SharpenKernel_Click(object sender, RoutedEventArgs e)
        {
            if (_kernelSize >= 3)
            {
                // Reset to 0
                ResetKernel_Click(sender, e);
                
                int center = _kernelSize / 2;
                
                // Standard sharpen kernel
                _kernelCells[center - 1, center].Text = "-1";
                _kernelCells[center + 1, center].Text = "-1";
                _kernelCells[center, center - 1].Text = "-1";
                _kernelCells[center, center + 1].Text = "-1";
                _kernelCells[center, center].Text = "5";
                
                MultiplierTextBox.Text = "1.0";
            }
        }

        private void BlurKernel_Click(object sender, RoutedEventArgs e)
        {
            // Set all values to 1
            for (int row = 0; row < _kernelSize; row++)
            {
                for (int col = 0; col < _kernelSize; col++)
                {
                    _kernelCells[row, col].Text = "1";
                }
            }
            
            // Set multiplier to 1/(size*size)
            double multiplier = 1.0 / (_kernelSize * _kernelSize);
            MultiplierTextBox.Text = multiplier.ToString("G4");
        }

        private void EdgeKernel_Click(object sender, RoutedEventArgs e)
        {
            if (_kernelSize >= 3)
            {
                // Reset to 0
                ResetKernel_Click(sender, e);
                
                int center = _kernelSize / 2;
                
                // Laplacian edge detection
                _kernelCells[center - 1, center].Text = "-1";
                _kernelCells[center + 1, center].Text = "-1";
                _kernelCells[center, center - 1].Text = "-1";
                _kernelCells[center, center + 1].Text = "-1";
                _kernelCells[center, center].Text = "4";
                
                MultiplierTextBox.Text = "1.0";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Applied = false;
            DialogResult = false;
            Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            // Parse multiplier
            if (!double.TryParse(MultiplierTextBox.Text, out double multiplier))
            {
                MessageBox.Show("Nilai multiplier tidak valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Parse kernel
            if (!TryParseKernel(out double[,] kernel))
            {
                return;
            }

            Kernel = kernel;
            Multiplier = multiplier;
            Applied = true;
            DialogResult = true;
            Close();
        }

        #endregion
    }
}
