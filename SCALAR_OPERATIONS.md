# Operasi Skalar pada Citra

## Deskripsi
Fitur ini memungkinkan pengguna untuk melakukan operasi perkalian dan pembagian citra dengan nilai skalar (angka).

## Fitur yang Ditambahkan

### 1. Button dan Input UI
- **Input Skalar (c)**: TextBox untuk memasukkan nilai skalar (default: 2)
- **Button Kali (A × c)**: Toggle button untuk perkalian citra dengan skalar
- **Button Bagi (A ÷ c)**: Toggle button untuk pembagian citra dengan skalar
- Lokasi: Di bawah button penambahan dan pengurangan citra

### 2. Fungsi Operasi

#### MultiplyByScalar(A, c)
- Setiap piksel pada gambar dikali dengan nilai c
- Formula: `pixel_baru = pixel_lama × c`
- Nilai pixel dibatasi antara 0-255 (clamping)
- Efek: Membuat gambar lebih terang

#### DivideByScalar(A, c)
- Setiap piksel pada gambar dibagi dengan nilai c
- Formula: `pixel_baru = pixel_lama ÷ c`
- Nilai pixel dibatasi antara 0-255 (clamping)
- Efek: Membuat gambar lebih gelap
- Validasi: Tidak boleh membagi dengan 0

### 3. Implementasi Teknis

#### File yang Dimodifikasi:
1. **MainWindow.xaml** - UI untuk input dan button
2. **IArithmeticService.cs** - Interface untuk method baru
3. **ImageEditor.Arithmetic.cs** - Implementasi algoritma
4. **MainWindow.Arithmetic.cs** - Event handlers
5. **MainWindow.xaml.cs** - Variabel state
6. **MainWindow.ImageLoading.cs** - Enable buttons saat load gambar
7. **MainWindow.Workspace.cs** - Reset state

#### Algoritma:
```csharp
// Untuk setiap pixel (B, G, R):
if (mode == Multiply)
{
    pixel = Clamp(pixel * scalar, 0, 255);
}
else if (mode == Divide)
{
    pixel = Clamp(pixel / scalar, 0, 255);
}
// Alpha channel tidak dimodifikasi
```

### 4. Cara Penggunaan

1. Buka gambar melalui menu File > Buka Gambar
2. Scroll ke bagian "Operasi Skalar"
3. Masukkan nilai skalar pada input "Skalar (c)"
4. Klik button "Kali (A × c)" untuk perkalian atau "Bagi (A ÷ c)" untuk pembagian
5. Gambar akan berubah sesuai operasi yang dipilih
6. Klik button yang sama lagi untuk mengembalikan gambar asli

### 5. Validasi

- ✅ Input harus berupa angka
- ✅ Tidak boleh membagi dengan 0
- ✅ Nilai pixel otomatis di-clamp (0-255)
- ✅ Hanya satu operasi aktif pada satu waktu
- ✅ Dapat dikembalikan ke gambar asli

### 6. Contoh Efek

| Operasi | Nilai c | Efek Visual |
|---------|---------|-------------|
| A × 1.5 | 1.5 | Gambar 50% lebih terang |
| A × 2   | 2 | Gambar 2x lebih terang |
| A ÷ 2   | 2 | Gambar 50% lebih gelap |
| A ÷ 4   | 4 | Gambar 75% lebih gelap |

## Catatan Penting

- Operasi skalar bekerja pada semua channel warna (R, G, B)
- Alpha channel (transparansi) tidak terpengaruh
- Nilai pixel yang melebihi 255 otomatis di-set ke 255
- Nilai pixel yang kurang dari 0 otomatis di-set ke 0
- Operasi dapat di-undo dengan uncheck button yang aktif
