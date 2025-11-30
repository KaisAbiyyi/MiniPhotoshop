using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MiniPhotoshop.Views.Dialogs
{
    /// <summary>
    /// Dialog untuk pengaturan kanvas saat aplikasi dimulai.
    /// Mengikuti Single Responsibility Principle - hanya menangani input pengaturan kanvas.
    /// </summary>
    public partial class CanvasSettingsDialog : Window
    {
        #region Properties

        /// <summary>
        /// Lebar kanvas yang dipilih user (dalam pixel).
        /// </summary>
        public int CanvasWidth { get; private set; } = 800;

        /// <summary>
        /// Tinggi kanvas yang dipilih user (dalam pixel).
        /// </summary>
        public int CanvasHeight { get; private set; } = 600;

        /// <summary>
        /// Warna latar belakang kanvas yang dipilih user.
        /// </summary>
        public Color BackgroundColor { get; private set; } = Colors.White;

        /// <summary>
        /// Apakah user memilih untuk membuat kanvas atau melewatinya.
        /// </summary>
        public bool CreateCanvas { get; private set; } = false;

        #endregion

        #region Constructor

        public CanvasSettingsDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set nilai awal dialog dari kanvas yang sudah ada (untuk edit).
        /// </summary>
        public void SetCurrentValues(int width, int height, Color backgroundColor)
        {
            CanvasWidth = width;
            CanvasHeight = height;
            BackgroundColor = backgroundColor;

            WidthInput.Text = width.ToString();
            HeightInput.Text = height.ToString();
            ColorPreview.Background = new SolidColorBrush(backgroundColor);

            // Select matching color in ComboBox
            SelectColorInComboBox(backgroundColor);
            
            // Change button text for edit mode
            CreateButton.Content = "Terapkan";
            Title = "Edit Pengaturan Kanvas";
        }

        /// <summary>
        /// Pilih warna yang sesuai di ComboBox.
        /// </summary>
        private void SelectColorInComboBox(Color color)
        {
            string colorHex = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            
            foreach (ComboBoxItem item in ColorComboBox.Items)
            {
                if (item.Tag is string tag && tag.Equals(colorHex, System.StringComparison.OrdinalIgnoreCase))
                {
                    ColorComboBox.SelectedItem = item;
                    return;
                }
            }
            
            // If no exact match, keep first selected
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Memvalidasi input agar hanya menerima angka.
        /// </summary>
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        /// <summary>
        /// Handler untuk perubahan pilihan warna.
        /// </summary>
        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorComboBox.SelectedItem is ComboBoxItem selectedItem && 
                selectedItem.Tag is string colorTag)
            {
                try
                {
                    Color selectedColor = (Color)ColorConverter.ConvertFromString(colorTag);
                    BackgroundColor = selectedColor;
                    ColorPreview.Background = new SolidColorBrush(selectedColor);
                }
                catch
                {
                    // Fallback ke putih jika parsing gagal
                    BackgroundColor = Colors.White;
                    ColorPreview.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        /// <summary>
        /// Handler untuk tombol preset ukuran.
        /// </summary>
        private void PresetSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sizeTag)
            {
                string[] parts = sizeTag.Split(',');
                if (parts.Length == 2 && 
                    int.TryParse(parts[0], out int width) && 
                    int.TryParse(parts[1], out int height))
                {
                    WidthInput.Text = width.ToString();
                    HeightInput.Text = height.ToString();
                }
            }
        }

        /// <summary>
        /// Handler untuk tombol Skip/Lewati.
        /// Menutup dialog tanpa membuat kanvas.
        /// </summary>
        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            CreateCanvas = false;
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handler untuk tombol Buat Kanvas.
        /// Memvalidasi input dan menutup dialog dengan hasil positif.
        /// </summary>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            CanvasWidth = int.Parse(WidthInput.Text);
            CanvasHeight = int.Parse(HeightInput.Text);
            CreateCanvas = true;
            DialogResult = true;
            Close();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Memvalidasi semua input sebelum membuat kanvas.
        /// </summary>
        private bool ValidateInputs()
        {
            // Validasi lebar
            if (!int.TryParse(WidthInput.Text, out int width) || width < 1 || width > 10000)
            {
                MessageBox.Show(
                    "Lebar kanvas harus berupa angka antara 1 dan 10000 pixel.",
                    "Validasi Gagal",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                WidthInput.Focus();
                return false;
            }

            // Validasi tinggi
            if (!int.TryParse(HeightInput.Text, out int height) || height < 1 || height > 10000)
            {
                MessageBox.Show(
                    "Tinggi kanvas harus berupa angka antara 1 dan 10000 pixel.",
                    "Validasi Gagal",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                HeightInput.Focus();
                return false;
            }

            return true;
        }

        #endregion
    }
}
