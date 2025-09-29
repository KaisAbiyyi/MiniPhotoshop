using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace MiniPhotoshop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Buat dialog untuk memilih file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Pilih Gambar",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|" +
                         "JPEG Files|*.jpg;*.jpeg|" +
                         "PNG Files|*.png|" +
                         "Bitmap Files|*.bmp|" +
                         "GIF Files|*.gif|" +
                         "TIFF Files|*.tiff|" +
                         "All Files|*.*",
                FilterIndex = 1
            };

            // Tampilkan dialog dan proses jika user memilih file
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Buat BitmapImage dari file yang dipilih
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // Tampilkan gambar di control Image
                    DisplayImage.Source = bitmap;

                    // Update text untuk menampilkan nama file
                    FileNameText.Text = Path.GetFileName(openFileDialog.FileName);
                    FileNameText.Foreground = System.Windows.Media.Brushes.Black;
                }
                catch (Exception ex)
                {
                    // Tampilkan pesan error jika gagal memuat gambar
                    MessageBox.Show($"Gagal memuat gambar: {ex.Message}", 
                                    "Error", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);
                }
            }
        }
    }
}
