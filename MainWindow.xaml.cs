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
        
        // Array 3 dimensi untuk menyimpan data pixel di memory
        // Format: [width][height][4] dimana [R, G, B, Gray]
        private byte[,,]? _pixelCache;
        private int _cachedWidth;
        private int _cachedHeight;

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

                    // Ekstrak pixel dan simpan ke memory cache
                    ExtractPixelsToCache(bitmap);

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
                    _pixelCache = null; // Hapus cache pixel
                    DisplayImage.Source = null;
                    FileNameText.Text = "Belum ada gambar dipilih";
                    FileNameText.Foreground = System.Windows.Media.Brushes.Gray;
                    ImageInfoText.Text = "Pilih gambar untuk melihat detail dan menyimpan data pixel.";
                }
            }
        }

        // Fungsi untuk ekstrak pixel dari gambar dan simpan ke memory cache
        // Format array: [width][height][4] dimana index 0=R, 1=G, 2=B, 3=Gray
        private void ExtractPixelsToCache(BitmapSource bitmap)
        {
            try
            {
                // Konversi ke format BGRA32 untuk konsistensi
                BitmapSource normalizedBitmap = EnsureBgra32(bitmap);
                
                int width = normalizedBitmap.PixelWidth;
                int height = normalizedBitmap.PixelHeight;
                int bytesPerPixel = normalizedBitmap.Format.BitsPerPixel / 8;
                int stride = width * bytesPerPixel;
                
                // Baca semua pixel dari gambar ke array byte sementara
                byte[] rawPixels = new byte[stride * height];
                normalizedBitmap.CopyPixels(rawPixels, stride, 0);
                
                // Buat array 3 dimensi untuk cache
                _pixelCache = new byte[width, height, 4];
                _cachedWidth = width;
                _cachedHeight = height;
                
                // Ekstrak setiap pixel dan hitung nilai gray
                for (int y = 0; y < height; y++)
                {
                    int rowOffset = y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int pixelOffset = rowOffset + (x * bytesPerPixel);
                        
                        // Ambil nilai RGB dari array byte (format BGRA)
                        byte b = rawPixels[pixelOffset];
                        byte g = rawPixels[pixelOffset + 1];
                        byte r = rawPixels[pixelOffset + 2];
                        
                        // Hitung gray sebagai rata-rata RGB
                        byte gray = (byte)((r + g + b) / 3);
                        
                        // Simpan ke cache dengan format [R, G, B, Gray]
                        _pixelCache[x, y, 0] = r;
                        _pixelCache[x, y, 1] = g;
                        _pixelCache[x, y, 2] = b;
                        _pixelCache[x, y, 3] = gray;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengekstrak pixel: {ex.Message}", 
                                "Error", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                _pixelCache = null;
            }
        }

        private void SavePixelsButton_Click(object sender, RoutedEventArgs e)
        {
            // Cek apakah ada data pixel di cache
            if (_pixelCache == null)
            {
                MessageBox.Show("Tidak ada data pixel yang dapat disimpan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
                // Ambil data dari cache memory (tidak perlu ekstrak ulang)
                int width = _cachedWidth;
                int height = _cachedHeight;

                using StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                writer.WriteLine($"# Data pixel untuk {FileNameText.Text}");
                writer.WriteLine($"# Dimensi: [{width}][{height}][4]");
                writer.WriteLine($"# Format: [x][y][channel] dimana channel: 0=Red, 1=Green, 2=Blue, 3=Grayscale");
                writer.WriteLine();

                // Tulis data dalam format array 3 dimensi native
                writer.WriteLine("[");
                for (int x = 0; x < width; x++)
                {
                    writer.WriteLine("  [");
                    for (int y = 0; y < height; y++)
                    {
                        // Ambil nilai dari cache [x, y, channel]
                        byte r = _pixelCache[x, y, 0];
                        byte g = _pixelCache[x, y, 1];
                        byte b = _pixelCache[x, y, 2];
                        byte gray = _pixelCache[x, y, 3];

                        // Format: [R, G, B, Gray]
                        string comma = (y < height - 1) ? "," : "";
                        writer.WriteLine($"    [{r}, {g}, {b}, {gray}]{comma}");
                    }
                    string arrayComma = (x < width - 1) ? "," : "";
                    writer.WriteLine($"  ]{arrayComma}");
                }
                writer.WriteLine("]");

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
