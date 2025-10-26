using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using IOPath = System.IO.Path;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Windows.Shapes;
using Microsoft.Win32;

#nullable enable

namespace MiniPhotoshop
{
    public partial class MainWindow : Window
    {
        private BitmapSource? _loadedBitmap;
        private readonly Dictionary<ImageFilterMode, BitmapSource> _filterCache = new();
        private readonly ObservableCollection<PreviewItem> _previewItems = new();
        private ImageFilterMode _activeFilter = ImageFilterMode.Original;
        private double _currentZoom = 1.0;
        private bool _pendingAutoFit;
        private HwndSource? _hwndSource;

        private const double MinZoom = 0.1;
        private const double MaxZoom = 5.0;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const double HorizontalWheelStep = 48.0;

        // Array 3 dimensi untuk menyimpan data pixel di memory
        // Format: [width][height][4] dimana [R, G, B, Gray]
        private byte[,,]? _pixelCache;
        private int _cachedWidth;
        private int _cachedHeight;
        
        // Menyimpan path file untuk info sidebar
        private string? _currentFilePath;
        
        // Menyimpan data histogram untuk popup
        private int[]? _histogramRed;
        private int[]? _histogramGreen;
        private int[]? _histogramBlue;
        private int[]? _histogramGray;

        public MainWindow()
        {
            InitializeComponent();
            FilterPreviewList.ItemsSource = _previewItems;
            DisplayImage.RenderTransformOrigin = new Point(0.5, 0.5);
            WorkspaceScrollViewer.SizeChanged += WorkspaceScrollViewer_SizeChanged;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hwndSource = (HwndSource?)PresentationSource.FromVisual(this);
            _hwndSource?.AddHook(WndProc);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource = null;
            }

            base.OnClosed(e);
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
                    _loadedBitmap = bitmap;
                    _filterCache.Clear();
                    _filterCache[ImageFilterMode.Original] = bitmap;

                    // Simpan path file
                    _currentFilePath = openFileDialog.FileName;

                    // Ekstrak pixel dan simpan ke memory cache
                    ExtractPixelsToCache(bitmap);

                    _activeFilter = ImageFilterMode.Original;
                    GenerateFilterPreviews();
                    ApplyFilter(ImageFilterMode.Original, resetZoom: true);

                    // Update text untuk menampilkan nama file
                    FileNameText.Text = IOPath.GetFileName(openFileDialog.FileName);
                    FileNameText.Foreground = System.Windows.Media.Brushes.Black;
                    ImageInfoText.Text = $"Resolusi: {bitmap.PixelWidth} x {bitmap.PixelHeight} | Format: {bitmap.Format}";
                    SavePixelsMenuItem.IsEnabled = true;
                    
                    // Tampilkan sidebar dan update histogram
                    ShowSidebar();
                    UpdateHistograms();
                }
                catch (Exception ex)
                {
                    // Tampilkan pesan error jika gagal memuat gambar
                    MessageBox.Show($"Gagal memuat gambar: {ex.Message}", 
                                    "Error", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);
                    ResetWorkspaceState();
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
                FileName = IOPath.ChangeExtension(FileNameText.Text, ".txt")
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

        private void ApplyFilter(ImageFilterMode mode, bool resetZoom = false)
        {
            if (_loadedBitmap == null)
            {
                return;
            }

            BitmapSource source = GetFilteredBitmap(mode);
            DisplayImage.Source = source;
            _activeFilter = mode;
            UpdatePreviewSelection();

            if (resetZoom)
            {
                QueueAutoFit();
            }
        }

        private BitmapSource GetFilteredBitmap(ImageFilterMode mode)
        {
            if (_loadedBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar yang dimuat.");
            }

            if (_filterCache.TryGetValue(mode, out BitmapSource? cached))
            {
                return cached;
            }

            BitmapSource result = mode switch
            {
                ImageFilterMode.RedOnly => CreateFilteredBitmap((r, g, b, gray) => (r, 0, 0)),
                ImageFilterMode.GreenOnly => CreateFilteredBitmap((r, g, b, gray) => (0, g, 0)),
                ImageFilterMode.BlueOnly => CreateFilteredBitmap((r, g, b, gray) => (0, 0, b)),
                ImageFilterMode.Grayscale => CreateFilteredBitmap((r, g, b, gray) =>
                {
                    byte luminance = (byte)Math.Clamp(0.2126 * r + 0.7152 * g + 0.0722 * b, 0, 255);
                    return (luminance, luminance, luminance);
                }),
                _ => _loadedBitmap
            };

            _filterCache[mode] = result;
            return result;
        }

        private BitmapSource CreateFilteredBitmap(Func<byte, byte, byte, byte, (byte R, byte G, byte B)> channelSelector)
        {
            if (_pixelCache == null)
            {
                return _loadedBitmap ?? throw new InvalidOperationException("Tidak ada bitmap untuk diproses.");
            }

            int width = _cachedWidth;
            int height = _cachedHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = rowOffset + (x * 4);
                    byte r = _pixelCache[x, y, 0];
                    byte g = _pixelCache[x, y, 1];
                    byte b = _pixelCache[x, y, 2];
                    byte gray = _pixelCache[x, y, 3];

                    (byte R, byte G, byte B) = channelSelector(r, g, b, gray);

                    buffer[offset] = B;
                    buffer[offset + 1] = G;
                    buffer[offset + 2] = R;
                    buffer[offset + 3] = 255;
                }
            }

            WriteableBitmap writable = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            writable.WritePixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);
            writable.Freeze();
            return writable;
        }

        private void GenerateFilterPreviews()
        {
            _previewItems.Clear();
            if (_loadedBitmap == null)
            {
                return;
            }

            AddPreview(ImageFilterMode.Original, "Normal");
            AddPreview(ImageFilterMode.RedOnly, "Red Only");
            AddPreview(ImageFilterMode.GreenOnly, "Green Only");
            AddPreview(ImageFilterMode.BlueOnly, "Blue Only");
            AddPreview(ImageFilterMode.Grayscale, "Grayscale");

            UpdatePreviewSelection();
        }

        private void AddPreview(ImageFilterMode mode, string title)
        {
            BitmapSource full = GetFilteredBitmap(mode);
            BitmapSource preview = CreateThumbnail(full, 160);
            _previewItems.Add(new PreviewItem(mode, title, preview, mode == _activeFilter));
        }

        private static BitmapSource CreateThumbnail(BitmapSource source, int maxDimension)
        {
            if (source.PixelWidth <= maxDimension && source.PixelHeight <= maxDimension)
            {
                return source;
            }

            double scale = Math.Min((double)maxDimension / source.PixelWidth, (double)maxDimension / source.PixelHeight);
            if (scale <= 0 || scale >= 1)
            {
                return source;
            }

            TransformedBitmap transformed = new TransformedBitmap(source, new ScaleTransform(scale, scale));
            transformed.Freeze();
            return transformed;
        }

        private void UpdatePreviewSelection()
        {
            foreach (PreviewItem item in _previewItems)
            {
                item.IsActive = item.Mode == _activeFilter;
            }
        }

        private void FilterPreview_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is ImageFilterMode mode)
            {
                ApplyFilter(mode);
            }
        }

        private void WorkspaceScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((_loadedBitmap == null))
            {
                return;
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
                Point focus = e.GetPosition(WorkspaceScrollViewer);
                ZoomImage(zoomFactor, focus);
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                double newOffset = WorkspaceScrollViewer.HorizontalOffset - e.Delta;
                newOffset = Math.Clamp(newOffset, 0, WorkspaceScrollViewer.ScrollableWidth);
                WorkspaceScrollViewer.ScrollToHorizontalOffset(newOffset);
            }
        }

        private void ZoomImage(double zoomFactor, Point focusPoint)
        {
            double targetZoom = Math.Clamp(_currentZoom * zoomFactor, MinZoom, MaxZoom);
            zoomFactor = targetZoom / _currentZoom;

            if (Math.Abs(zoomFactor - 1.0) < 0.0001)
            {
                return;
            }

            double absoluteX = WorkspaceScrollViewer.HorizontalOffset + focusPoint.X;
            double absoluteY = WorkspaceScrollViewer.VerticalOffset + focusPoint.Y;

            _currentZoom = targetZoom;
            ImageScaleTransform.ScaleX = targetZoom;
            ImageScaleTransform.ScaleY = targetZoom;

            WorkspaceScrollViewer.UpdateLayout();

            WorkspaceScrollViewer.ScrollToHorizontalOffset(Math.Clamp(absoluteX * zoomFactor - focusPoint.X, 0, WorkspaceScrollViewer.ScrollableWidth));
            WorkspaceScrollViewer.ScrollToVerticalOffset(Math.Clamp(absoluteY * zoomFactor - focusPoint.Y, 0, WorkspaceScrollViewer.ScrollableHeight));
        }

        private void DisplayImage_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            if (_loadedBitmap == null)
            {
                return;
            }

            e.ManipulationContainer = WorkspaceScrollViewer;
            e.Mode = ManipulationModes.Scale;
            e.Handled = true;
        }

        private void DisplayImage_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_loadedBitmap == null)
            {
                return;
            }

            double scaleDelta = e.DeltaManipulation.Scale.X;
            if (!double.IsNaN(scaleDelta) && !double.IsInfinity(scaleDelta) && Math.Abs(scaleDelta - 1.0) > 0.0001)
            {
                Point focus = e.ManipulationOrigin;
                ZoomImage(scaleDelta, focus);
            }

            Vector translation = e.DeltaManipulation.Translation;
            if (!double.IsNaN(translation.X) && !double.IsInfinity(translation.X) &&
                !double.IsNaN(translation.Y) && !double.IsInfinity(translation.Y))
            {
                double targetHorizontal = WorkspaceScrollViewer.HorizontalOffset - translation.X;
                double targetVertical = WorkspaceScrollViewer.VerticalOffset - translation.Y;

                targetHorizontal = Math.Clamp(targetHorizontal, 0, WorkspaceScrollViewer.ScrollableWidth);
                targetVertical = Math.Clamp(targetVertical, 0, WorkspaceScrollViewer.ScrollableHeight);

                WorkspaceScrollViewer.ScrollToHorizontalOffset(targetHorizontal);
                WorkspaceScrollViewer.ScrollToVerticalOffset(targetVertical);
            }

            e.Handled = true;
        }

        private void WorkspaceScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_pendingAutoFit)
            {
                ResetZoomToFit();
            }
        }

        private void ResetZoomToFit()
        {
            if (_loadedBitmap == null)
            {
                return;
            }

            double viewportWidth = WorkspaceScrollViewer.ViewportWidth;
            double viewportHeight = WorkspaceScrollViewer.ViewportHeight;

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }

            double scaleX = viewportWidth / _loadedBitmap.PixelWidth;
            double scaleY = viewportHeight / _loadedBitmap.PixelHeight;
            double fitZoom = Math.Min(scaleX, scaleY);
            fitZoom = Math.Min(1.0, fitZoom);
            fitZoom = Math.Clamp(fitZoom, MinZoom, MaxZoom);

            _currentZoom = fitZoom;
            ImageScaleTransform.ScaleX = fitZoom;
            ImageScaleTransform.ScaleY = fitZoom;

            WorkspaceScrollViewer.ScrollToHorizontalOffset(0);
            WorkspaceScrollViewer.ScrollToVerticalOffset(0);

            _pendingAutoFit = false;
        }

        private void QueueAutoFit()
        {
            _pendingAutoFit = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (_pendingAutoFit)
                {
                    ResetZoomToFit();
                }
            }));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEHWHEEL && WorkspaceScrollViewer.IsLoaded)
            {
                Point devicePoint = new Point((short)((long)lParam & 0xFFFF), (short)(((long)lParam >> 16) & 0xFFFF));
                Point screenPoint = devicePoint;

                if (_hwndSource?.CompositionTarget != null)
                {
                    screenPoint = _hwndSource.CompositionTarget.TransformFromDevice.Transform(devicePoint);
                }

                Point viewerPoint = WorkspaceScrollViewer.PointFromScreen(screenPoint);

                if (viewerPoint.X >= 0 && viewerPoint.X <= WorkspaceScrollViewer.ActualWidth &&
                    viewerPoint.Y >= 0 && viewerPoint.Y <= WorkspaceScrollViewer.ActualHeight)
                {
                    int wheelDelta = (short)(((long)wParam >> 16) & 0xFFFF);
                    double deltaMultiplier = wheelDelta / (double)Mouse.MouseWheelDeltaForOneLine;
                    double horizontalChange = deltaMultiplier * HorizontalWheelStep;

                    double target = WorkspaceScrollViewer.HorizontalOffset + horizontalChange;
                    target = Math.Clamp(target, 0, WorkspaceScrollViewer.ScrollableWidth);
                    WorkspaceScrollViewer.ScrollToHorizontalOffset(target);

                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        private void ResetWorkspaceState()
        {
            _loadedBitmap = null;
            _pixelCache = null;
            _currentFilePath = null;
            _filterCache.Clear();
            _previewItems.Clear();
            DisplayImage.Source = null;
            SavePixelsMenuItem.IsEnabled = false;
            _currentZoom = 1.0;
            _pendingAutoFit = false;
            ImageScaleTransform.ScaleX = 1.0;
            ImageScaleTransform.ScaleY = 1.0;
            WorkspaceScrollViewer.ScrollToHorizontalOffset(0);
            WorkspaceScrollViewer.ScrollToVerticalOffset(0);
            FileNameText.Text = "Belum ada gambar dipilih";
            FileNameText.Foreground = System.Windows.Media.Brushes.Gray;
            ImageInfoText.Text = "Pilih gambar untuk melihat detail dan menyimpan data pixel.";
            
            // Reset histogram data
            _histogramRed = null;
            _histogramGreen = null;
            _histogramBlue = null;
            _histogramGray = null;
            
            // Sembunyikan sidebar
            HideSidebar();
        }

        // Fungsi untuk menampilkan sidebar
        private void ShowSidebar()
        {
            SidebarColumn.Width = new GridLength(280);
            SidebarPanel.Visibility = Visibility.Visible;
        }

        // Fungsi untuk menyembunyikan sidebar
        private void HideSidebar()
        {
            SidebarColumn.Width = new GridLength(0);
            SidebarPanel.Visibility = Visibility.Collapsed;
        }

        // Fungsi untuk update semua histogram
        private void UpdateHistograms()
        {
            if (_pixelCache == null) return;

            // Hitung dan simpan histogram untuk setiap channel (0=Red, 1=Green, 2=Blue, 3=Gray)
            _histogramRed = CalculateHistogram(0);
            _histogramGreen = CalculateHistogram(1);
            _histogramBlue = CalculateHistogram(2);
            _histogramGray = CalculateHistogram(3);
            
            // Gambar histogram di sidebar
            DrawHistogramOnCanvas(HistogramRed, _histogramRed, Colors.Red);
            DrawHistogramOnCanvas(HistogramGreen, _histogramGreen, Colors.Green);
            DrawHistogramOnCanvas(HistogramBlue, _histogramBlue, Colors.Blue);
            DrawHistogramOnCanvas(HistogramGray, _histogramGray, Colors.Gray);
        }

        // Fungsi untuk menghitung histogram dari pixel cache
        private int[] CalculateHistogram(int channel)
        {
            int[] histogram = new int[256];
            
            for (int x = 0; x < _cachedWidth; x++)
            {
                for (int y = 0; y < _cachedHeight; y++)
                {
                    byte value = _pixelCache![x, y, channel];
                    // Byte otomatis 0-255, tapi pastikan tetap dalam range
                    if (value <= 255)
                    {
                        histogram[value]++;
                    }
                }
            }
            
            return histogram;
        }

        // Fungsi untuk menggambar histogram di canvas sidebar
        private void DrawHistogramOnCanvas(Canvas canvas, int[] histogram, Color color)
        {
            canvas.Children.Clear();
            
            if (histogram == null || histogram.Length != 256) return;
            
            // Cari nilai maksimum untuk normalisasi (hanya dari 0-255)
            int maxCount = 0;
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] > maxCount)
                    maxCount = histogram[i];
            }
            
            if (maxCount == 0) return;
            
            // Gambar histogram - 255 di ujung kanan
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 248;
            double height = canvas.Height;
            // Bagi dengan 255 agar bar 255 tepat di ujung (0 di awal, 255 di akhir)
            double barWidth = width / 255.0;
            
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] > 0)
                {
                    double barHeight = (histogram[i] / (double)maxCount) * height;
                    double xPosition = i * barWidth;
                    
                    Rectangle bar = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = new SolidColorBrush(color)
                    };
                    
                    Canvas.SetLeft(bar, xPosition);
                    Canvas.SetBottom(bar, 0);
                    canvas.Children.Add(bar);
                }
            }
        }

        // Event handler untuk klik histogram Red
        private void HistogramRed_Click(object sender, MouseButtonEventArgs e)
        {
            if (_histogramRed != null)
            {
                HistogramWindow window = new HistogramWindow("Red Channel", _histogramRed, Colors.Red);
                window.Owner = this;
                window.ShowDialog();
            }
        }

        // Event handler untuk klik histogram Green
        private void HistogramGreen_Click(object sender, MouseButtonEventArgs e)
        {
            if (_histogramGreen != null)
            {
                HistogramWindow window = new HistogramWindow("Green Channel", _histogramGreen, Colors.Green);
                window.Owner = this;
                window.ShowDialog();
            }
        }

        // Event handler untuk klik histogram Blue
        private void HistogramBlue_Click(object sender, MouseButtonEventArgs e)
        {
            if (_histogramBlue != null)
            {
                HistogramWindow window = new HistogramWindow("Blue Channel", _histogramBlue, Colors.Blue);
                window.Owner = this;
                window.ShowDialog();
            }
        }

        // Event handler untuk klik histogram Grayscale
        private void HistogramGray_Click(object sender, MouseButtonEventArgs e)
        {
            if (_histogramGray != null)
            {
                HistogramWindow window = new HistogramWindow("Grayscale", _histogramGray, Colors.Gray);
                window.Owner = this;
                window.ShowDialog();
            }
        }

        private enum ImageFilterMode
        {
            Original,
            RedOnly,
            GreenOnly,
            BlueOnly,
            Grayscale
        }

        private sealed class PreviewItem : INotifyPropertyChanged
        {
            private bool _isActive;

            public PreviewItem(ImageFilterMode mode, string title, ImageSource previewSource, bool isActive)
            {
                Mode = mode;
                Title = title;
                PreviewSource = previewSource;
                _isActive = isActive;
            }

            public ImageFilterMode Mode { get; }
            public string Title { get; }
            public ImageSource PreviewSource { get; }

            public bool IsActive
            {
                get => _isActive;
                set
                {
                    if (_isActive != value)
                    {
                        _isActive = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
