# Fitur Rotasi Citra

## Deskripsi
Fitur ini memungkinkan pengguna untuk merotasi gambar dengan sudut preset (45°, 90°, 180°, 270°) atau sudut bebas (Free Degree).

## File-File yang Dibuat

### 1. **IRotationService.cs** - Interface Service
Lokasi: `Services/Contracts/IRotationService.cs`

Interface untuk operasi rotasi citra dengan method:
- `Rotate45()` - Rotasi 45 derajat
- `Rotate90()` - Rotasi 90 derajat
- `Rotate180()` - Rotasi 180 derajat
- `Rotate270()` - Rotasi 270 derajat
- `RotateCustom(double degrees)` - Rotasi dengan sudut bebas
- `RestoreOriginal()` - Kembalikan ke gambar asli
- `ClearRotationSnapshot()` - Hapus snapshot rotasi

### 2. **ImageEditor.Rotation.cs** - Implementasi Service
Lokasi: `Services/ImageEditor/ImageEditor.Rotation.cs`

Implementasi algoritma rotasi dengan fitur:
- **Rotasi Optimal** untuk 90°, 180°, 270° (tanpa interpolasi)
- **Rotasi Umum** untuk sudut lainnya dengan bilinear interpolation
- **Snapshot Management** untuk restore ke gambar asli

#### Algoritma Rotasi Optimal (90°, 180°, 270°)
```csharp
// Rotasi 90°: (x, y) → (height - 1 - y, x)
// Rotasi 180°: (x, y) → (width - 1 - x, height - 1 - y)
// Rotasi 270°: (x, y) → (y, width - 1 - x)
```

#### Algoritma Rotasi Umum (45°, Free Degree)
- Menggunakan **backward mapping** untuk menghindari lubang pada hasil
- Implementasi **bilinear interpolation** untuk hasil yang halus
- Menghitung **bounding box** otomatis untuk sudut rotasi
- Formula transformasi:
  ```
  srcX = x * cos + y * sin + centerX
  srcY = -x * sin + y * cos + centerY
  ```

### 3. **MainWindow.Rotation.cs** - Event Handlers
Lokasi: `MainWindow.Rotation.cs`

Event handlers untuk UI:
- `RotateButton_Click` - Toggle panel rotasi
- `Rotate45Button_Click` - Rotasi 45°
- `Rotate90Button_Click` - Rotasi 90°
- `Rotate180Button_Click` - Rotasi 180°
- `Rotate270Button_Click` - Rotasi 270°
- `RotateCustomButton_Click` - Rotasi dengan input derajat
- `RestoreRotationButton_Click` - Kembalikan ke asli
- `ApplyRotation(double degrees)` - Method helper untuk apply rotasi
- `UpdateRotationButtonsState()` - Update state button

## UI Components

### Button Rotasi di Toolbar
Lokasi: `MainWindow.xaml` - Grid Row 0

Button "Rotasi" yang membuka/tutup panel rotasi.

### Panel Rotasi
Lokasi: `MainWindow.xaml` - Info Panel (Grid Row 1)

Panel yang berisi:
1. **Preset Buttons**: 45°, 90°, 180°, 270°
2. **Free Degree Input**: TextBox untuk input sudut bebas + Button Rotasi
3. **Restore Button**: Kembalikan ke gambar asli

## Cara Penggunaan

1. **Buka gambar** melalui menu File > Buka Gambar
2. **Klik button "Rotasi"** di toolbar untuk membuka panel rotasi
3. **Pilih rotasi**:
   - Klik button preset (45°, 90°, 180°, 270°) untuk rotasi cepat
   - Atau masukkan sudut bebas di input "Free Degree" dan klik "Rotasi"
4. **Kembalikan ke asli** dengan klik "Kembalikan ke Asli"

## Fitur Teknis

### 1. Rotasi Optimal (90°, 180°, 270°)
- ✅ Tidak ada interpolasi (pixel perfect)
- ✅ Sangat cepat
- ✅ Tidak ada loss quality
- ✅ Cocok untuk rotasi tegak lurus

### 2. Rotasi Umum (45°, Free Degree)
- ✅ Bilinear interpolation untuk hasil halus
- ✅ Backward mapping (tidak ada lubang)
- ✅ Bounding box otomatis
- ✅ Transparan background untuk area kosong
- ✅ Cocok untuk sudut arbitrary

### 3. State Management
- ✅ Snapshot gambar asli sebelum rotasi
- ✅ Dapat dikembalikan ke asli kapan saja
- ✅ Snapshot otomatis di-clear saat load gambar baru

## Contoh Hasil

| Sudut | Deskripsi | Algoritma |
|-------|-----------|-----------|
| 45° | Diagonal | Bilinear Interpolation |
| 90° | Putar kanan | Optimal (pixel mapping) |
| 180° | Terbalik | Optimal (pixel mapping) |
| 270° | Putar kiri | Optimal (pixel mapping) |
| 30° | Custom angle | Bilinear Interpolation |
| 135° | Custom angle | Bilinear Interpolation |

## Validasi

- ✅ Input sudut harus berupa angka
- ✅ Button hanya enabled saat ada gambar
- ✅ Snapshot otomatis sebelum rotasi
- ✅ Dapat restore ke gambar asli
- ✅ Panel dapat dibuka/tutup

## Performa

### Rotasi Optimal (90°, 180°, 270°)
- Kompleksitas: O(n) dimana n = jumlah pixel
- Memory: 2x ukuran gambar (source + result)
- Kecepatan: Sangat cepat (direct pixel mapping)

### Rotasi Umum (45°, Free Degree)
- Kompleksitas: O(n × 4) untuk bilinear interpolation
- Memory: 2x ukuran gambar
- Kecepatan: Lebih lambat karena interpolasi

## Catatan Penting

1. **Ukuran Gambar**: Rotasi selain 180° akan mengubah dimensi gambar
2. **Transparansi**: Area kosong pada rotasi umum akan transparan
3. **Quality**: Rotasi optimal (90°, 180°, 270°) tidak ada loss quality
4. **Interpolation**: Rotasi umum menggunakan bilinear untuk hasil halus
5. **Snapshot**: Hanya satu snapshot disimpan, rotasi bertingkat akan hilang

## Integrasi dengan Fitur Lain

- ✅ Compatible dengan semua filter (grayscale, sepia, dll)
- ✅ Compatible dengan brightness adjustment
- ✅ Compatible dengan negasi
- ✅ Compatible dengan operasi aritmetika
- ✅ Histogram otomatis update setelah rotasi
- ✅ Preview filter otomatis update

## Troubleshooting

**Q: Rotasi 45° hasilnya pecah?**
A: Gunakan gambar dengan resolusi lebih tinggi untuk hasil lebih baik.

**Q: Rotasi lambat?**
A: Rotasi dengan interpolasi (45°, custom) lebih lambat. Gunakan rotasi optimal jika memungkinkan.

**Q: Area kosong hitam?**
A: Itu adalah area transparan. Save sebagai PNG untuk preserve transparansi.

**Q: Tidak bisa restore?**
A: Pastikan sudah melakukan rotasi sebelumnya. Restore hanya bekerja setelah rotasi.
