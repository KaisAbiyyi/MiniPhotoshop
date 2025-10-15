using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace MiniPhotoshop
{
    public partial class MainWindow : Window
    {
        private BitmapSource? _loadedBitmap;

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
                    bitmap.Freeze();

                    // Tampilkan gambar di control Image
                    DisplayImage.Source = bitmap;

                    _loadedBitmap = bitmap;

                    // Update text untuk menampilkan nama file
                    FileNameText.Text = Path.GetFileName(openFileDialog.FileName);
                    FileNameText.Foreground = System.Windows.Media.Brushes.Black;
                    ImageInfoText.Text = $"Resolusi: {bitmap.PixelWidth} x {bitmap.PixelHeight} | Format: {bitmap.Format}";
                    SavePixelsButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    // Tampilkan pesan error jika gagal memuat gambar
                    MessageBox.Show($"Gagal memuat gambar: {ex.Message}", 
                                    "Error", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);
                    SavePixelsButton.IsEnabled = false;
                    _loadedBitmap = null;
                    DisplayImage.Source = null;
                    FileNameText.Text = "Belum ada gambar dipilih";
                    FileNameText.Foreground = System.Windows.Media.Brushes.Gray;
                    ImageInfoText.Text = "Pilih gambar untuk melihat detail dan menyimpan data pixel.";
                }
            }
        }

        private void SavePixelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loadedBitmap == null)
            {
                MessageBox.Show("Tidak ada gambar yang dapat disimpan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Simpan Data Pixel",
                Filter = "Text Files|*.txt|All Files|*.*",
                DefaultExt = "txt",
                FileName = Path.ChangeExtension(FileNameText.Text, ".txt")
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                BitmapSource bitmap = EnsureBgra32(_loadedBitmap);
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                int bytesPerPixel = bitmap.Format.BitsPerPixel / 8;
                int stride = width * bytesPerPixel;
                byte[] pixels = new byte[stride * height];
                bitmap.CopyPixels(pixels, stride, 0);

                using StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                writer.WriteLine($"# Data pixel untuk {FileNameText.Text} ({width} x {height})");
                writer.WriteLine("# Format baris: x,y | R,G,B | Gray");

                for (int y = 0; y < height; y++)
                {
                    int rowOffset = y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int pixelOffset = rowOffset + (x * bytesPerPixel);
                        byte b = pixels[pixelOffset];
                        byte g = pixels[pixelOffset + 1];
                        byte r = pixels[pixelOffset + 2];

                        int gray = (int)Math.Round((r * 0.299) + (g * 0.587) + (b * 0.114));

                        writer.WriteLine($"{x},{y} | {r},{g},{b} | {gray}");
                    }
                }

                writer.Flush();

                MessageBox.Show("Data pixel berhasil disimpan.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data pixel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static BitmapSource EnsureBgra32(BitmapSource source)
        {
            if (source.Format == PixelFormats.Bgra32)
            {
                return source;
            }

            FormatConvertedBitmap converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            converted.Freeze();
            return converted;
        }
    }
}
