# LEARNING_ROADMAP

Roadmap ini menjelaskan, minggu demi minggu, di file mana kamu bisa belajar setiap fitur (logic di `Services/ImageEditor/*` dan view di `Views/MainWindow/*`), lengkap dengan rentang line dan penjelasan singkat.

Semua nomor line di bawah ini diambil dari kode yang ada saat ini di branch `dev`.

---

## Minggu 1 – Upload gambar dan tampilkan

### 1.1 Logic: Load gambar dari disk

- **File**: `Services/ImageEditor/ImageEditor.Load.cs`
- **Fungsi utama**: `Load(string filePath)`
- **Line**: 9–41
- **Kode**:
	```csharp
	public ImageLoadResult Load(string filePath)
	{
			if (string.IsNullOrWhiteSpace(filePath))
			{
					throw new ArgumentException("File path must be provided.", nameof(filePath));
			}

			ClearArithmeticSnapshot();
			State.Reset();

			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.EndInit();
			bitmap.Freeze();

			State.OriginalBitmap = bitmap;
			State.FilterCache[ImageFilterMode.Original] = bitmap;
			State.PixelCache = ExtractPixelCache(bitmap);
			State.CachedWidth = bitmap.PixelWidth;
			State.CachedHeight = bitmap.PixelHeight;
			State.CurrentFilePath = filePath;
			State.ActiveFilter = ImageFilterMode.Original;

			BuildPreviews();
			Build();

			return new ImageLoadResult(
					bitmap,
					filePath,
					bitmap.PixelWidth,
					bitmap.PixelHeight,
					bitmap.Format.ToString()
			);
	}
	```
- **Penjelasan**:
	- Validasi path, hapus snapshot aritmatika dan reset `State` workspace.
	- Buat `BitmapImage`, load from disk (`UriSource`), `OnLoad` supaya file tidak di-lock, lalu `Freeze()` supaya thread-safe.
	- Simpan bitmap ke `State.OriginalBitmap`, isi cache filter (`FilterCache`) dan cache pixel (`PixelCache` lewat `ExtractPixelCache`).
	- Set metadata ukuran dan file path, aktifkan filter `Original` lalu build preview dan histogram (`BuildPreviews()`, `Build()`).
	- Kembalikan `ImageLoadResult` berisi bitmap + info (width, height, pixel format).

### 1.2 Logic pendukung: Ekstrak pixel ke array 3D

- **File**: `Services/ImageEditor/ImageEditor.Helpers.cs`
- **Fungsi utama**: `ExtractPixelCache(BitmapSource bitmap)`
- **Line**: 17–48
- **Kode**:
	```csharp
	private static byte[,,] ExtractPixelCache(BitmapSource bitmap)
	{
			BitmapSource normalized = EnsureBgra32(bitmap);
			int width = normalized.PixelWidth;
			int height = normalized.PixelHeight;
			int stride = width * 4;
			byte[] buffer = new byte[stride * height];
			normalized.CopyPixels(buffer, stride, 0);

			var cache = new byte[width, height, 5];
			for (int y = 0; y < height; y++)
			{
					int rowOffset = y * stride;
					for (int x = 0; x < width; x++)
					{
							int offset = rowOffset + (x * 4);
							byte b = buffer[offset];
							byte g = buffer[offset + 1];
							byte r = buffer[offset + 2];
							byte a = buffer[offset + 3];
							byte gray = (byte)Math.Clamp(0.2126 * r + 0.7152 * g + 0.0722 * b, 0, 255);

							cache[x, y, 0] = r;
							cache[x, y, 1] = g;
							cache[x, y, 2] = b;
							cache[x, y, 3] = gray;
							cache[x, y, 4] = a;
					}
			}

			return cache;
	}
	```
- **Penjelasan**:
	- Normalisasi format ke `Bgra32` (4 byte per pixel) agar akses byte konsisten.
	- `CopyPixels` menyalin semua pixel ke buffer 1D.
	- Loop 2D `(x,y)` mengubah buffer 1D jadi array 3D `[width,height,channel]`.
	- Simpan R,G,B, grayscale (pakai koefisien luminance), dan alpha.

### 1.3 View: Tombol "Pilih Gambar" dan menampilkan gambar

- **File (view event handler)**: `Views/MainWindow/Features/ImageManagement/MainWindow.ImageLoading.cs`
- **Fungsi**: `SelectImageButton_Click`
- **Line**: 11–51
- **Kode**:
	```csharp
	private void SelectImageButton_Click(object sender, RoutedEventArgs e)
	{
			OpenFileDialog openFileDialog = new()
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

			if (openFileDialog.ShowDialog() != true)
			{
					return;
			}

			try
			{
					ImageLoadResult result = _imageLoader.Load(openFileDialog.FileName);
					_currentArithmeticMode = ArithmeticToggleMode.None;
					_arithmeticService.ClearArithmeticSnapshot();
					_suppressArithmeticToggleHandlers = true;
					ArithmeticAddToggle.IsChecked = false;
					ArithmeticSubtractToggle.IsChecked = false;
					_suppressArithmeticToggleHandlers = false;
					ApplyLoadedImage(result);
			}
			catch (Exception ex)
			{
					MessageBox.Show($"Gagal memuat gambar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					ResetWorkspaceState();
			}
	}
	```
- **Penjelasan**:
	- Buka dialog file, batalkan jika user klik Cancel.
	- Panggil service `_imageLoader.Load` (implementasinya di `ImageEditor.Load.cs`).
	- Reset mode aritmatika, matikan toggle, lalu panggil `ApplyLoadedImage(result)` untuk update UI.
	- Jika terjadi error (file rusak, dll), tampilkan `MessageBox` dan reset workspace.

- **File (view – menerapkan hasil load ke UI)**: `Views/MainWindow/Features/ImageManagement/MainWindow.ImageLoading.cs`
- **Fungsi**: `ApplyLoadedImage(ImageLoadResult result)`
- **Line**: 80–151
- **Penjelasan singkat**:
	- Set `DisplayImage.Source` menggunakan `_filterService.SetActiveFilter(ImageFilterMode.Original)`.
	- Build preview filter, reset brightness/threshold/negasi/seleksi warna.
	- Update label nama file dan informasi resolusi.
	- Enable semua tombol fitur (save pixels, negasi, grayscale compare, binary threshold, dsb).
	- Panggil `ShowSidebar()` dan `RenderHistograms()` lalu reset zoom ke autofit.

### 1.4 View XAML: Area gambar dan zoom

- **File**: `Views/MainWindow/MainWindow.xaml`
- **Bagian penting**: ScrollViewer dan Image utama
- **Line**: 454–477
- **Kode** (potongan):
	```xaml
	<ScrollViewer x:Name="WorkspaceScrollViewer"
								VerticalScrollBarVisibility="Auto"
								HorizontalScrollBarVisibility="Auto"
								PanningMode="Both"
								Focusable="True"
								PreviewMouseWheel="WorkspaceScrollViewer_PreviewMouseWheel">
			<Grid Background="Transparent">
					<Image Name="DisplayImage"
								 Stretch="None"
								 HorizontalAlignment="Center"
								 VerticalAlignment="Center"
								 RenderOptions.BitmapScalingMode="HighQuality"
								 IsManipulationEnabled="True"
								 ManipulationStarting="DisplayImage_ManipulationStarting"
								 ManipulationDelta="DisplayImage_ManipulationDelta">
							<Image.RenderTransform>
									<ScaleTransform x:Name="ImageScaleTransform" ScaleX="1" ScaleY="1"/>
							</Image.RenderTransform>
					</Image>
			</Grid>
	</ScrollViewer>
	```
- **Penjelasan**:
	- `WorkspaceScrollViewer` membungkus `DisplayImage` agar bisa di-scroll dan di-zoom.
	- Zoom/pan di-handle di `MainWindow.Zoom.cs` (lihat Minggu 7).

---

## Minggu 3 – Ekstrak pixel

### 3.1 Logic: Ekstrak dan simpan pixel ke file

- **Logic ekstraksi pixel**: sama seperti 1.2 (`ExtractPixelCache`).

- **File (view – Save Pixels)**: `Views/MainWindow/Features/ImageManagement/MainWindow.ImageLoading.cs`
- **Fungsi**: `SavePixelsButton_Click`
- **Line**: 53–89
- **Kode**:
	```csharp
	private void SavePixelsButton_Click(object sender, RoutedEventArgs e)
	{
			if (_state.PixelCache == null)
			{
					MessageBox.Show("Tidak ada data pixel untuk disimpan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
					return;
			}

			SaveFileDialog saveFileDialog = new()
			{
					Title = "Simpan Data Pixel",
					Filter = "Text File|*.txt|All Files|*.*",
					FileName = $"{Path.GetFileNameWithoutExtension(_state.CurrentFilePath)}_pixels.txt"
			};

			if (saveFileDialog.ShowDialog() != true)
			{
					return;
			}

			try
			{
					_pixelExporter.Export(saveFileDialog.FileName);
					MessageBox.Show("Data pixel berhasil disimpan.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
					MessageBox.Show($"Gagal menyimpan data pixel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
	}
	```
- **Penjelasan**:
	- Cek apakah `PixelCache` sudah ada.
	- Buka `SaveFileDialog` dengan nama default `<nama>_pixels.txt`.
	- Panggil `_pixelExporter.Export(path)` untuk menulis file (lihat interface `IPixelExportService`).

---

## Minggu 4 – Array 3D, Filter, Histogram Chart

### 4.1 Array 3D pixel

- Lihat lagi `ExtractPixelCache` di Minggu 1 (struktur `[x, y, channel]`).

### 4.2 Logic Filter (Red/Green/Blue/Grayscale)

- **File**: `Services/ImageEditor/ImageEditor.Filters.cs`
- **Fungsi**: `GetProcessedBitmap`, `GetFilteredBitmap`, `CreateFilteredBitmap`, `BuildPreviews`.
- **Line utama**:
	- `GetProcessedBitmap` – line 18–38
	- `BuildPreviews` – line 40–62
	- `GetFilteredBitmap` – line 80–108
	- `CreateFilteredBitmap` – line 110–145
- **Inti logic**:
	- `GetProcessedBitmap`: ambil bitmap dasar sesuai `ActiveFilter`, lalu chaining efek lain (negasi, brightness, binary threshold, color selection).
	- `GetFilteredBitmap`: jika cache belum ada, generate berdasarkan mode:
		- `RedOnly` → hanya channel R yang diambil, G dan B = 0.
		- `GreenOnly`, `BlueOnly`, `Grayscale` mirip, tapi fungsi selector berbeda.
	- `CreateFilteredBitmap`: loop semua pixel di `PixelCache`, panggil `selector(r,g,b,gray)` dan tulis ke buffer baru, lalu buat `BitmapSource`.
	- `BuildPreviews`: buat thumbnail kecil untuk tiap filter dan simpan di `State.PreviewItems`.

### 4.3 View Filter dan Histogram sidebar

- **File**: `Views/MainWindow/MainWindow.xaml`
- **Tab Histogram** – line 492–541 (lihat potongan di Minggu 1 untuk canvas `HistogramRed/Green/Blue/Gray`).
- **Tab Filter** – line 543–620
	- `ItemsControl` `FilterPreviewList` menampilkan preview filter memakai binding ke `PreviewItems`.
	- Tombol `NegationButton` ada di bagian bawah tab Filter.

- **File (view code-behind: Histogram)**: `Views/MainWindow/Features/ImageAnalysis/MainWindow.Histogram.cs`
- **Fungsi utama**:
	- `RenderHistograms()` – line 11–24: ambil `HistogramData` dari `_histogramService.Build()` dan gambar ke 4 canvas.
	- `DrawHistogramOnCanvas` – line 25–74: gambar bar histogram ke `Canvas`.
	- `HistogramRed/Green/Blue/Gray_Click` – line 77–96: buka window detail.
	- `ShowHistogramWindow` – line 97–114: instansiasi `HistogramWindow`.

- **File (logic histogram)**: `Services/ImageEditor/ImageEditor.Histogram.cs`
	- `Build()` – menghitung jumlah pixel pada tiap intensitas untuk R,G,B,Gray menggunakan `PixelCache`.

- **File (window histogram detail)**:
	- View: `Views/Windows/HistogramWindow.xaml`
	- Code-behind: `Views/Windows/HistogramWindow.xaml.cs` – konstruktor + `DrawHistogram` untuk menampilkan satu channel sebagai chart.

---

## Minggu 5 – Grayscale Comparison, Binary Threshold, Color Selection, Negasi Citra

### 5.1 Grayscale Comparison

- **Logic**: `Services/ImageEditor/ImageEditor.GrayscaleComparison.cs`
	- `CreateAverageGrayscale()` – line 9–34: gray = rata-rata `(r+g+b)/3`.
	- `CreateLuminanceGrayscale()` – line 36–63: gray = `0.299*r + 0.587*g + 0.114*b`.

- **View**: `Views/MainWindow/Features/ImageAnalysis/MainWindow.GrayscaleComparison.cs`
	- `ShowGrayscaleComparison_Click` – line 9–33: cek gambar, panggil dua fungsi di atas, lalu tampilkan `GrayscaleComparisonWindow` dengan dua bitmap.

### 5.2 Binary Threshold

- **Logic**: `Services/ImageEditor/ImageEditor.BinaryThreshold.cs`
	- `SetBinaryThresholdActive(bool)` – line 9–18: mengaktifkan/menonaktifkan mode threshold.
	- `UpdateThreshold(int)` – line 20–25: set nilai threshold ke state dan rebuild processed bitmap.
	- `ApplyBinaryThreshold(BitmapSource,int)` – line 27–59: konversi ke gray, bandingkan dengan threshold, hasilnya 0 atau 255 (putih/hitam terbalik) lalu buat bitmap baru.

- **View**: `Views/MainWindow/Features/ImageProcessing/MainWindow.BinaryThreshold.cs`
	- `BinaryThresholdToggle_Checked/Unchecked` – line 9–32: show/hide panel, aktifkan mode di service.
	- `BinaryThresholdSlider_ValueChanged` – line 34–57: baca slider (0–255), update label dan panggil `_binaryThresholdService.UpdateThreshold`.

### 5.3 Color Selection (Seleksi warna)

- **Logic**: `Services/ImageEditor/ImageEditor.ColorSelection.cs`
	- `SetColorSelectionActive(bool)` – line 9–29: aktifkan mode seleksi warna, simpan gambar sebelum seleksi untuk nanti dipulihkan.
	- `ApplySelection(int pixelX, int pixelY)` – line 31–57: baca warna target dari `PixelCache` pada posisi klik, simpan ke state, lalu panggil `ApplyColorSelection`.
	- `ApplyColorSelection(BitmapSource)` – line 59–102: loop semua pixel, jika warna sama dengan target → tampilkan warna asli, kalau beda → jadikan hitam (masking by color).

- **View**: `Views/MainWindow/Features/ImageProcessing/MainWindow.ColorSelection.cs`
	- `ColorSelectionToggle_Checked/Unchecked` – line 10–43: mengaktifkan panel dan event click pada gambar.
	- `DisplayImage_ColorSelection_Click` – line 82–118: hitung koordinat pixel dari posisi klik di UI ke koordinat asli gambar, panggil `_colorSelectionService.ApplySelection`, update teks RGB dan warna teks.

### 5.4 Negasi Citra

- **Logic**: `Services/ImageEditor/ImageEditor.Negation.cs`
	- `SetNegationActive(bool)` – line 9–20: toggle flag negasi dan reset brightness.
	- `ApplyNegation(BitmapSource)` – line 28–49: untuk setiap pixel, `new = 255 - old` di channel B,G,R.

- **View**:
	- `Views/MainWindow/Features/ImageProcessing/MainWindow.Negation.cs` → toggle tersembunyi `NegationToggle_Checked/Unchecked` memanggil `HandleNegationToggle` yang panggil `_negationService.SetNegationActive`.
	- `Views/MainWindow/UserInteraction/MainWindow.FilterTab.cs` → tombol UI `NegationButton_Click` (line 9–27) hanya mengubah toggle dan tampilan tombol.

---

## Minggu 6 – Penambahan, Pengurangan, Perkalian, Pembagian, Binary

### 6.1 Operasi Aritmatika Antar Gambar (Add/Subtract/Multiply/Divide)

- **Logic**: `Services/ImageEditor/ImageEditor.Arithmetic.cs`
	- Metode publik:
		- `AddImage(BitmapSource overlay, int offsetX, int offsetY)` – line 9–15.
		- `SubtractImage(BitmapSource overlay, int offsetX, int offsetY)` – line 17–23.
		- `MultiplyByScalar(double scalar)` / `DivideByScalar(double scalar)` – line 25–45.
		- `MultiplyImage(..., out string normalizationInfo)` / `DivideImage(..., out string normalizationInfo)` – line 47–65.
	- Implementasi inti:
		- `ApplyArithmeticOperation` – line 133–221: operasi penjumlahan/pengurangan antar dua citra dengan offset (bisa beda ukuran).
		- `ApplyImageMultiplicationDivision` – line 67–131: operasi perkalian/pembagian antar citra dengan normalisasi hasil per channel.
		- `ApplyScalarOperation` – line 67–115: kalikan/bagi semua pixel dengan bilangan skalar.
		- `ReplaceWorkspaceBitmap` – line 223–258: mengganti workspace dengan hasil operasi (reset state + rebuild preview & histogram).

- **View**: `Views/MainWindow/Features/ImageOperations/MainWindow.Arithmetic.cs`
	- Pemilihan gambar B (deprecated handler dikomentari) digantikan oleh tombol di toolbar (`ToolbarHandlers`).
	- `ArithmeticAddToggle_Checked/Unchecked`, `ArithmeticSubtractToggle_Checked/Unchecked` – line 54–86: mengatur toggle add/subtract.
	- `HandleArithmeticToggleChecked` – line 88–135: validasi state & overlay, memastikan hanya satu toggle aktif, lalu panggil `ApplyArithmeticOperation`.
	- `ApplyArithmeticOperation(bool isAddition)` – line 171–214: panggil `_arithmeticService.AddImage/SubtractImage`, bungkus hasil ke `ImageLoadResult`, dan kirim ke `ApplyLoadedImage`.
	- Bagian kedua (line 260–376): `ArithmeticMultiplyToggle_*`, `ArithmeticDivideToggle_*`, dan `ApplyImageMultiplyDivideOperation` untuk operasi perkalian/pembagian citra.

### 6.2 Operasi Boolean/Biner (AND, OR, NOT, XOR)

- **Logic**: `Services/ImageEditor/ImageEditor.BinaryImage.cs`
	- `ToBinary(int threshold)` – line 9–43: ubah gambar menjadi citra biner berdasarkan threshold.
	- `AndImage/OrImage/NotImage/XorImage` – line 45–86: wrapper untuk `ApplyBinaryOperation` atau operasi NOT langsung.
	- `ApplyBinaryOperation` – line 88–165: implementasi AND/OR/XOR per pixel pada area overlap, hasil 0 atau 255.
	- `ReplaceBinaryWorkspaceBitmap`, `RestoreBinaryBase`, `CaptureBinarySnapshot` – line 167–222: mengelola snapshot dan workspace saat operasi biner.

- **View**: `Views/MainWindow/Features/ImageOperations/MainWindow.BinaryImage.cs`
	- `BinarySelectButton_Click` – line 11–49: pilih gambar B untuk operasi biner.
	- `ConvertToBinaryButton_Click` – line 51–79: panggil `_binaryImageService.ToBinary(128)` dan terapkan hasil.
	- `BinaryAnd/Or/Not/XorToggle_Checked/Unchecked` – line 81–128: mengatur toggle operasi biner.
	- `HandleBinaryToggleChecked` – line 130–175: validasi state dan overlay, pastikan hanya satu toggle aktif, lalu panggil `ApplyBinaryOperation`.
	- `ApplyBinaryOperation(BinaryToggleMode)` – line 196–252: panggil service (`AndImage/OrImage/NotImage/XorImage`), bungkus hasil ke `ImageLoadResult` dan `ApplyLoadedImage`.

---

## Minggu 7 – Rotasi, Distorsi, Zoom, Pan

### 7.1 Rotasi

- **Logic**: `Services/ImageEditor/ImageEditor.Rotation.cs`
	- Metode publik: `Rotate45/90/180/270/RotateCustom` – line 9–33.
	- `ApplyRotation(double)` – line 35–69: pilih antara rotasi optimized (90/180/270) dan general.
	- `RotateOptimized` – line 71–137: rotasi 90/180/270 menggunakan perhitungan index pixel (tanpa interpolasi).
	- `RotateGeneral` – line 139–232: rotasi sudut bebas dengan bilinear interpolation.
	- `ReplaceWorkspaceBitmapForRotation` + snapshot (`CaptureRotationSnapshot`, `RestoreOriginal`, `ClearRotationSnapshot`) – line 234–302.

- **View**: `Views/MainWindow/Features/ImageTransform/MainWindow.Rotation.cs`
	- `RotateButton_Click` – line 9–22: menampilkan/menyembunyikan panel rotasi.
	- `Rotate45/90/180/270Button_Click` – line 24–39: memanggil `ApplyRotation` dengan preset sudut.
	- `ApplyRotation(double)` – line 63–110: normalisasi sudut, pilih metode di service, bungkus hasil ke `ImageLoadResult`, lalu `ApplyLoadedImage`.
	- `ApplyIncrementalRotation(double)` – line 112–151: menambah sudut sedikit demi sedikit (misal tombol ±1°) menggunakan `_cumulativeRotationAngle`.
	- `RestoreRotationButton_Click` – line 41–61: memanggil `_rotationService.RestoreOriginal`.

### 7.2 Zoom dan Pan

- **File (view)**: `Views/MainWindow/UserInteraction/MainWindow.Zoom.cs`
- **Fungsi utama**:
	- `WorkspaceScrollViewer_PreviewMouseWheel` – line 11–35: Ctrl + Wheel → zoom, Shift + Wheel → horizontal pan.
	- `ZoomImage(double zoomFactor, Point focusPoint)` – line 37–71: hitung zoom baru, update `ImageScaleTransform`, dan jaga fokus di sekitar posisi mouse.
	- `DisplayImage_ManipulationStarting/Delta` – line 73–114: gesture touch pinch untuk zoom dan drag untuk pan.
	- `ResetZoomToFit` + `QueueAutoFit` – line 120–167: menyesuaikan zoom agar gambar pas di viewport setelah load.

### 7.3 Distorsi

- Untuk saat ini belum ada file khusus "Distorsi". Jika nanti kamu menambah efek distorsi geometrik atau intensitas:
	- Logic sebaiknya masuk ke `Services/ImageEditor/ImageEditor.<NamaDistorsi>.cs`.
	- View event handler sebaiknya masuk ke folder `Views/MainWindow/Features/ImageTransform/` atau `ImageProcessing/` tergantung jenisnya.

---

## Ringkasan Peran File

### Core

- `Core/Models/ImageWorkspaceState.cs` – menyimpan seluruh state workspace (bitmap asli, filter cache, pixel cache, histogram, zoom, dsb.).
- `Core/Models/HistogramData.cs` – model untuk data histogram (array 256 nilai per channel).
- `Core/Models/ImageLoadResult.cs` – hasil operasi load/transform gambar yang siap dikirim ke UI.
- `Core/Models/BrightnessState.cs`, `ColorSelectionState.cs` – menyimpan state spesifik fitur.
- `Core/Enums/ImageFilterMode.cs` – enum jenis filter (Original, RedOnly, GreenOnly, BlueOnly, Grayscale).

### Services (logic)

- `Services/ImageEditor/ImageEditor.*.cs` – satu kelas `ImageEditor` yang dipecah per fitur:
	- `Load`, `Helpers` – load gambar, ekstraksi pixel, pembuatan bitmap/thumbnail.
	- `Filters` – filter warna dan preview.
	- `Brightness` – logic penyesuaian kecerahan dengan buffer `int[,,]`.
	- `Negation` – invert warna.
	- `BinaryThreshold` – threshold biner di atas grayscale.
	- `ColorSelection` – masking berdasarkan warna yang diklik.
	- `Histogram` – hitung histogram R,G,B,Gray.
	- `GrayscaleComparison` – dua metode grayscale.
	- `Arithmetic` – penjumlahan/pengurangan/perkalian/pembagian (gambar dan skalar).
	- `BinaryImage` – operasi boolean AND/OR/NOT/XOR dan citra biner.
	- `Rotation` – rotasi 45/90/180/270/custom deg.
	- `Workspace`, `Export`, `Display`, dll – helper lain untuk sinkronisasi state dan ekspor.

### Views (UI)

- `Views/MainWindow/MainWindow.xaml` – definisi layout utama (toolbar, workspace, sidebar histogram & filter, panel-panel fitur).
- `Views/MainWindow/MainWindow.xaml.cs` – konstruktor, inisialisasi service, wiring event handler.
- `Views/MainWindow/Features/*` – event handler per fitur, dibagi per kategori:
	- `ImageManagement` – load/save pixel, workspace & drag-drop.
	- `ImageProcessing` – brightness, grayscale/binary threshold, color selection, negasi, filter.
	- `ImageOperations` – operasi aritmatika & biner antar dua gambar.
	- `ImageAnalysis` – histogram dan grayscale comparison.
	- `ImageTransform` – rotasi.
- `Views/MainWindow/UserInteraction/*` – interaksi umum (toolbar, zoom, tab filter, window interop).
- `Views/Dialogs/*` – window kecil untuk input (scalar operation, pilih gambar, offset, dll.).
- `Views/Windows/*` – window terpisah untuk histogram detail dan grayscale comparison.

Dengan roadmap ini, kamu bisa belajar per minggu dengan fokus: mulai dari alur load gambar → memahami struktur `PixelCache` 3D → filter & histogram → operasi grayscale/biner → operasi aritmatika & boolean → rotasi dan zoom/pan.

