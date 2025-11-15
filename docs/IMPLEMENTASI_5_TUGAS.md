# Implementasi Image Processing Features

Dokumen ini menjelaskan implementasi fitur-fitur image processing yang telah ditambahkan ke aplikasi MiniPhotoshop.

## 1. Negasi Citra (RGB & Grayscale)

**Lokasi:** Toggle button di bawah filter preview panel

**Algoritma:**
```
Untuk setiap piksel (x,y):
  R' = 255 - R
  G' = 255 - G
  B' = 255 - B
```

**Kompleksitas:** O(W × H)

**Cara Pakai:**
1. Buka gambar
2. Pilih filter apapun (Normal, Red Only, Green Only, Blue Only, Grayscale)
3. Klik toggle button **"Negasi Citra"** di bawah filter preview
4. Gambar akan di-negasi secara instant
5. Negasi bekerja pada filter yang sedang aktif

**Keunggulan:** 
- Bekerja di semua mode filter
- Bisa dikombinasikan dengan brightness adjustment

**Kode:** `NegationToggle_Checked()`, `NegationToggle_Unchecked()`, `ApplyNegationToBitmap()`

---

## 2. Brightness dengan Slider Dinamis (-255 to +255)

**Lokasi:** Panel di bawah info gambar

**Algoritma:**
```
Inisialisasi:
  - Ambil gambar dari filter aktif (+ negasi jika aktif)
  - Salin ke temp[,,] (tipe int)

Saat slider berubah:
  - delta = nilai_sekarang - nilai_sebelumnya
  - temp[x,y,c] += delta (untuk semua piksel)
  - tampil = clamp(temp, 0, 255)
```

**Keunggulan:** 
- Menggunakan array temporer (int) untuk menghindari data loss akibat clipping
- Bekerja di **semua filter mode** termasuk saat negasi aktif
- Dapat dimundurkan tanpa kehilangan informasi

**Cara Pakai:**
1. Buka gambar
2. Pilih filter (Normal, Red Only, Green Only, Blue Only, atau Grayscale)
3. Opsional: Aktifkan Negasi
4. Geser slider Brightness di panel kontrol
5. Gambar akan update secara real-time
6. Nilai bisa dimundurkan tanpa kehilangan informasi

**Kode:** `BrightnessSlider_ValueChanged()`, `InitializeBrightnessTemp()`, `CreateBrightnessDisplayBitmap()`

---

## 3. Perbandingan Metode Grayscale (Average vs Luminance)

**Status:** Dihapus dari implementasi final

Fitur ini telah dihapus karena perubahan requirement. Filter Grayscale yang tersedia menggunakan metode Luminance weighted.

---

## 4. Seleksi Warna Interaktif

**Lokasi:** Panel di bawah Brightness slider

**Algoritma:**
```
onMouseDown(x,y):
  - target_RGB = piksel[x,y]
  
Untuk setiap piksel:
  - Jika RGB == target_RGB → tampilkan warna asli
  - Else → set ke hitam (0,0,0)
```

**Cara Pakai:**
1. Buka gambar
2. Centang checkbox "Aktifkan" di panel Seleksi Warna Interaktif
3. Klik pada area warna di gambar
4. Hanya piksel dengan warna yang sama persis yang akan ditampilkan
5. Info RGB warna terpilih muncul di panel
6. Uncheck untuk kembali ke gambar normal

**Kode:** `ColorSelection_Checked()`, `ColorSelection_Unchecked()`, `DisplayImage_ColorSelection_Click()`, `CreateColorSelectionBitmap()`

---

## 5. Konversi Biner (Threshold 128) → Negasi

**Status:** Dihapus dari implementasi final

Fitur ini telah dihapus karena perubahan requirement. Fungsi negasi sudah tersedia sebagai toggle yang dapat dikombinasikan dengan filter Grayscale untuk efek serupa.

---

## Struktur Implementasi Final

### Filter System
- **Normal** - Gambar asli
- **Red Only** - Hanya channel merah
- **Green Only** - Hanya channel hijau
- **Blue Only** - Hanya channel biru
- **Grayscale** - Konversi ke grayscale dengan luminance weighted

### Processing Pipeline
```
Gambar Asli
    ↓
[Filter Terpilih] (Normal/Red/Green/Blue/Grayscale)
    ↓
[Negasi] (Opsional - jika toggle aktif)
    ↓
[Brightness] (Opsional - jika slider digeser)
    ↓
Tampilan Akhir
```

### Data Flow untuk Brightness
1. `InitializeBrightnessTemp()` - Salin dari filter aktif + negasi (jika ada)
2. `BrightnessSlider_ValueChanged()` - Update dengan delta
3. `CreateBrightnessDisplayBitmap()` - Clamp dan tampilkan
4. Hasil tetap mengikuti filter dan negasi yang aktif

---

## Struktur Data

### Pixel Cache
```csharp
byte[,,] _pixelCache  // [width, height, 4]
// Index channel:
// 0 = Red
// 1 = Green  
// 2 = Blue
// 3 = Grayscale (pre-calculated average)
```

### Brightness Temporary Array
```csharp
int[,,] _brightnessTemp  // [width, height, 3]
// Menggunakan int untuk mencegah overflow/underflow
// Memungkinkan nilai di luar range 0-255
// Diinisialisasi dari: filter aktif + negasi (jika aktif)
```

### State Flags
```csharp
bool _isNegationActive     // Status toggle negasi
ImageFilterMode _activeFilter  // Filter yang sedang aktif
```

---

## Fungsi Bantu Utama

### GetProcessedBitmap()
```csharp
// Pipeline lengkap:
// 1. Ambil filter aktif
// 2. Apply negasi jika toggle aktif
// 3. Apply brightness jika ada adjustment
// Return: BitmapSource final
```

### ApplyNegationToBitmap(BitmapSource source)
```csharp
// Apply negasi ke BitmapSource manapun
// Rumus: 255 - nilai
```

### Clamp Function
```csharp
Math.Clamp(value, 0, 255)
// Membatasi nilai ke range 0-255
```

---

## Testing Checklist

- [x] Negasi bekerja di semua filter (Normal, Red, Green, Blue, Grayscale)
- [x] Brightness slider bekerja di semua filter
- [x] Brightness + Negasi dapat dikombinasikan
- [x] Brightness bisa dimundurkan tanpa data loss
- [x] Ganti filter mereset brightness ke 0
- [x] Toggle negasi mereset brightness ke 0
- [x] Color selection dapat memilih warna dengan klik
- [x] UI responsif dan intuitif

---

## File yang Dimodifikasi/Ditambahkan

1. **MainWindow.xaml** - UI: Toggle negasi, brightness slider, color selection
2. **MainWindow.xaml.cs** - Implementasi logika semua fitur
3. **GrayscaleComparisonWindow.xaml** - (Tidak digunakan di implementasi final)
4. **GrayscaleComparisonWindow.xaml.cs** - (Tidak digunakan di implementasi final)

---

## Cara Build & Run

```bash
# Build
dotnet build

# Run
dotnet run
```

## Dependencies

- .NET 8.0 Windows Desktop
- WPF (Windows Presentation Foundation)

---

**Catatan Perubahan dari Desain Awal:**
- Negasi diubah dari menu item menjadi toggle button yang bekerja di semua filter
- Brightness sekarang bekerja di filter apapun, termasuk kombinasi dengan negasi
- Binary Threshold dan Grayscale Comparison dihapus dari implementasi final
- Pipeline processing lebih fleksibel: Filter → Negasi → Brightness

## 1. Negasi Citra (RGB & Grayscale)

**Lokasi:** Menu `Operations` → `1. Negasi Citra`

**Algoritma:**
```
Untuk setiap piksel (x,y):
  R' = 255 - R
  G' = 255 - G
  B' = 255 - B
```

**Kompleksitas:** O(W × H)

**Cara Pakai:**
1. Buka gambar
2. Klik menu `Operations` → `1. Negasi Citra`
3. Gambar akan di-negasi secara instant

**Kode:** `ApplyNegation_Click()` dan `CreateNegationBitmap()`

---

## 2. Brightness dengan Slider Dinamis (-255 to +255)

**Lokasi:** Panel di bawah info gambar

**Algoritma:**
```
Inisialisasi:
  - Salin piksel asli ke temp[,,] (tipe int)

Saat slider berubah:
  - delta = nilai_sekarang - nilai_sebelumnya
  - temp[x,y,c] += delta (untuk semua piksel)
  - tampil = clamp(temp, 0, 255)
```

**Keunggulan:** Menggunakan array temporer (int) untuk menghindari data loss akibat clipping

**Cara Pakai:**
1. Buka gambar
2. Geser slider Brightness di panel kontrol
3. Gambar akan update secara real-time
4. Nilai bisa dimundurkan tanpa kehilangan informasi

**Kode:** `BrightnessSlider_ValueChanged()`, `InitializeBrightnessTemp()`, `CreateBrightnessDisplayBitmap()`

---

## 3. Perbandingan Metode Grayscale (Average vs Luminance)

**Lokasi:** Menu `Operations` → `3. Compare Grayscale (Avg vs Lum)`

**Algoritma:**

**Method 1 - Average:**
```
Gray = (R + G + B) / 3
```

**Method 2 - Luminance (Weighted):**
```
Gray = 0.299×R + 0.587×G + 0.114×B
```

**Perbedaan Visual:**
- **Average:** Lebih sederhana, memberikan bobot sama untuk semua channel
- **Luminance:** Lebih akurat secara perseptual, mata manusia lebih sensitif terhadap hijau

**Cara Pakai:**
1. Buka gambar RGB
2. Klik menu `Operations` → `3. Compare Grayscale (Avg vs Lum)`
3. Window baru akan muncul menampilkan kedua hasil side-by-side

**Kode:** `ShowGrayscaleComparison_Click()`, `CreateAverageGrayscale()`, `CreateLuminanceGrayscale()`

---

## 4. Seleksi Warna Interaktif

**Lokasi:** Panel di bawah Brightness slider

**Algoritma:**
```
onMouseDown(x,y):
  - target_RGB = piksel[x,y]
  
Untuk setiap piksel:
  - Jika RGB == target_RGB → tampilkan warna asli
  - Else → set ke hitam (0,0,0)
```

**Cara Pakai:**
1. Buka gambar
2. Centang checkbox "Aktifkan" di panel Seleksi Warna Interaktif
3. Klik pada area warna di gambar
4. Hanya piksel dengan warna yang sama persis yang akan ditampilkan
5. Info RGB warna terpilih muncul di panel
6. Uncheck untuk kembali ke gambar normal

**Kode:** `ColorSelection_Checked()`, `ColorSelection_Unchecked()`, `DisplayImage_ColorSelection_Click()`, `CreateColorSelectionBitmap()`

---

## 5. Konversi Biner (Threshold 128) → Negasi

**Lokasi:** Menu `Operations` → `5. Binary Threshold (128) + Negasi`

**Algoritma:**
```
Step 1 - Binary Threshold:
  Untuk setiap piksel:
    gray_value = nilai_grayscale
    binary = (gray_value > 128) ? 255 : 0

Step 2 - Negasi Biner:
  binary_negated = 255 - binary
  
Hasil:
  - Piksel terang (>128) → 0 (hitam)
  - Piksel gelap (≤128) → 255 (putih)
```

**Cara Pakai:**
1. Buka gambar
2. Klik menu `Operations` → `5. Binary Threshold (128) + Negasi`
3. Gambar akan dikonversi menjadi hitam-putih dengan threshold 128, kemudian di-negasi

**Kode:** `ApplyBinaryThresholdNegation_Click()`, `CreateBinaryThresholdNegation()`

---

## Struktur Data

### Pixel Cache
```csharp
byte[,,] _pixelCache  // [width, height, 4]
// Index channel:
// 0 = Red
// 1 = Green  
// 2 = Blue
// 3 = Grayscale (pre-calculated average)
```

### Brightness Temporary Array
```csharp
int[,,] _brightnessTemp  // [width, height, 3]
// Menggunakan int untuk mencegah overflow/underflow
// Memungkinkan nilai di luar range 0-255
```

## Fungsi Bantu Umum

### Clamp Function
```csharp
Math.Clamp(value, 0, 255)
// Membatasi nilai ke range 0-255
```

### WriteableBitmap Creation Pattern
```csharp
WriteableBitmap writable = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
writable.WritePixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);
writable.Freeze();
```

---

## Testing Checklist

- [x] Negasi Citra berfungsi untuk gambar RGB
- [x] Brightness slider dapat bergerak dari -255 to +255
- [x] Brightness bisa dimundurkan tanpa data loss
- [x] Grayscale comparison menampilkan 2 hasil berbeda
- [x] Color selection dapat memilih warna dengan klik
- [x] Binary threshold + negasi menghasilkan gambar hitam-putih terbalik

---

## File yang Dimodifikasi/Ditambahkan

1. **MainWindow.xaml** - UI controls untuk 5 fitur
2. **MainWindow.xaml.cs** - Implementasi logika semua fitur
3. **GrayscaleComparisonWindow.xaml** - Window untuk comparison grayscale
4. **GrayscaleComparisonWindow.xaml.cs** - Code-behind untuk comparison window

---

## Cara Build & Run

```bash
# Build
dotnet build

# Run
dotnet run
```

## Dependencies

- .NET 8.0 Windows Desktop
- WPF (Windows Presentation Foundation)

---

**Catatan:** Semua implementasi mengikuti algoritma yang paling sederhana sesuai requirement, dengan kompleksitas O(W×H) untuk setiap operasi.
