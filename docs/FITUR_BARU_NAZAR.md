# Dokumentasi Fitur Baru - Mini Photoshop

## Ringkasan

Dokumen ini menjelaskan tiga fitur baru yang telah diimplementasikan dalam aplikasi Mini Photoshop untuk presentasi Nazar.

---

## 1. Perkalian & Pembagian Citra dengan Normalisasi

### Deskripsi

Fitur ini memungkinkan operasi perkalian dan pembagian antara dua citra (A dan B) dengan normalisasi otomatis per kanal warna.

### Cara Penggunaan

1. **Muat Gambar Utama (A)**

   - Klik `File` → `Buka Gambar...`
   - Pilih gambar pertama yang akan diproses

2. **Pilih Gambar B**

   - Di panel "Aritmetika Citra", klik tombol `Pilih Gambar B`
   - Pilih gambar kedua untuk operasi

3. **Atur Offset (Opsional)**

   - `Offset X`: Geser gambar B secara horizontal
   - `Offset Y`: Geser gambar B secara vertikal
   - Default: (0, 0)

4. **Pilih Operasi**
   - Klik `Kali Citra (×)` untuk perkalian
   - Klik `Bagi Citra (÷)` untuk pembagian

### Algoritma

#### Perkalian Citra

```
Untuk setiap piksel (x, y) dalam area overlap:
    Untuk setiap kanal (R, G, B):
        hasil_float[x,y,kanal] = A[x,y,kanal] × B[x-offsetX, y-offsetY, kanal]
```

#### Pembagian Citra

```
EPSILON = 1e-10  // Untuk menghindari pembagian dengan nol

Untuk setiap piksel (x, y) dalam area overlap:
    Untuk setiap kanal (R, G, B):
        hasil_float[x,y,kanal] = A[x,y,kanal] / (B[x-offsetX, y-offsetY, kanal] + EPSILON)
```

#### Normalisasi Per Kanal

```
Untuk setiap kanal (R, G, B):
    1. Cari nilai minimum (min) dan maksimum (max) dalam kanal tersebut
    2. Untuk setiap piksel:
        nilai_normalized = ((nilai - min) / (max - min)) × 255
        hasil_akhir = clamp(nilai_normalized, 0, 255)
```

### Output

- **Citra Hasil**: Gambar hasil operasi dengan nilai pixel 0-255
- **Area Non-Overlap**: Diisi dengan warna hitam (0, 0, 0)

### Contoh

```
Normalisasi: R: [0.00, 65025.00] | G: [0.00, 52900.00] | B: [0.00, 48400.00]
```

---

## 2. Operasi Citra Biner (Boolean)

### Deskripsi

Fitur ini menyediakan operasi logika Boolean pada citra biner: AND, OR, NOT, XOR.

### Cara Penggunaan

1. **Muat Gambar Utama (A)**

   - Klik `File` → `Buka Gambar...`

2. **Konversi ke Biner (Opsional)**

   - Di panel "Citra Biner (Boolean)", klik `Convert to Binary`
   - Threshold: 128 (piksel ≥128 → putih, <128 → hitam)

3. **Untuk Operasi 2 Citra (AND, OR, XOR)**

   - Klik `Pilih Gambar B` di panel Binary
   - Pilih gambar kedua
   - Atur Offset X dan Y jika diperlukan
   - Klik tombol operasi: `AND`, `OR`, atau `XOR`

4. **Untuk Operasi 1 Citra (NOT)**
   - Tidak perlu Gambar B
   - Langsung klik tombol `NOT`

### Algoritma

#### Konversi ke Biner

```
Threshold = 128

Untuk setiap piksel (x, y):
    grayscale = 0.299×R + 0.587×G + 0.114×B

    jika grayscale ≥ threshold:
        hasil[x,y] = 255 (putih)
    jika tidak:
        hasil[x,y] = 0 (hitam)
```

#### Operasi AND

```
Untuk setiap piksel dalam area overlap:
    A_bit = (A[x,y] ≥ 128) ? 1 : 0
    B_bit = (B[x-offsetX, y-offsetY] ≥ 128) ? 1 : 0

    hasil = (A_bit AND B_bit) ? 255 : 0
```

#### Operasi OR

```
Untuk setiap piksel dalam area overlap:
    A_bit = (A[x,y] ≥ 128) ? 1 : 0
    B_bit = (B[x-offsetX, y-offsetY] ≥ 128) ? 1 : 0

    hasil = (A_bit OR B_bit) ? 255 : 0
```

#### Operasi NOT

```
Untuk setiap piksel (x, y):
    hasil[x,y] = 255 - A[x,y]

    // Contoh:
    // 0 (hitam) → 255 (putih)
    // 255 (putih) → 0 (hitam)
```

#### Operasi XOR

```
Untuk setiap piksel dalam area overlap:
    A_bit = (A[x,y] ≥ 128) ? 1 : 0
    B_bit = (B[x-offsetX, y-offsetY] ≥ 128) ? 1 : 0

    hasil = (A_bit XOR B_bit) ? 255 : 0
```

### Tabel Kebenaran

| A   | B   | AND | OR  | NOT A | XOR |
| --- | --- | --- | --- | ----- | --- |
| 0   | 0   | 0   | 0   | 1     | 0   |
| 0   | 1   | 0   | 1   | 1     | 1   |
| 1   | 0   | 0   | 1   | 0     | 1   |
| 1   | 1   | 1   | 1   | 0     | 0   |

### Output

- **Citra Hasil**: Gambar biner (hanya hitam/putih)
- **Area Non-Overlap**: Diisi dengan warna hitam (0, 0, 0)

---

## File-File yang Ditambahkan/Dimodifikasi

### File Baru

1. **`Services/Contracts/IBinaryImageService.cs`**

   - Interface untuk operasi citra biner

2. **`Services/ImageEditor/ImageEditor.BinaryImage.cs`**

   - Implementasi operasi biner: ToBinary, AND, OR, NOT, XOR

3. **`MainWindow.BinaryImage.cs`**

   - Event handlers untuk UI operasi biner

### File yang Dimodifikasi

1. **`Services/Contracts/IArithmeticService.cs`**

   - Menambahkan method `MultiplyImage` dan `DivideImage` dengan parameter `out string normalizationInfo`

2. **`Services/ImageEditor/ImageEditor.Arithmetic.cs`**

   - Implementasi perkalian dan pembagian citra dengan normalisasi

3. **`Services/ImageEditor/ImageEditor.cs`**

   - Menambahkan interface `IBinaryImageService`

4. **`MainWindow.xaml`**

   - Menambahkan button `Kali Citra (×)` dan `Bagi Citra (÷)`
   - Menambahkan panel "Citra Biner (Boolean)"

5. **`MainWindow.xaml.cs`**

   - Menambahkan field untuk binary operations
   - Menambahkan enum `BinaryToggleMode`
   - Memperluas enum `ArithmeticToggleMode`

6. **`MainWindow.Arithmetic.cs`**

   - Menambahkan event handlers untuk multiply/divide image

7. **`MainWindow.ImageLoading.cs`**
   - Menambahkan inisialisasi panel Binary Image

---

## Catatan Teknis

### Normalisasi

- Dilakukan **per kanal** (R, G, B terpisah)
- Menggunakan perhitungan `float` untuk presisi tinggi
- Formula: `((nilai - min) / (max - min)) × 255`
- Hasil di-clamp ke range [0, 255]

### Binary Operations

- Threshold default: 128
- Piksel dianggap `1` (true) jika nilai ≥ 128
- Piksel dianggap `0` (false) jika nilai < 128
- Hasil selalu hitam (0) atau putih (255)

### Overlap Processing

- Hanya area yang tumpang tindih yang diproses
- Area di luar overlap = hitam (0, 0, 0)
- Offset bisa negatif atau positif

### Performance

- Operasi dilakukan pada byte array untuk kecepatan
- Buffer sementara float untuk normalisasi
- Minimal object allocation

---

## Troubleshooting

### Masalah: Hasil gambar hitam semua

**Solusi**: Periksa offset X dan Y. Pastikan ada area overlap antara citra A dan B.

### Masalah: Status bar normalisasi tidak muncul

**Solusi**: Status bar hanya muncul saat operasi perkalian/pembagian citra dilakukan.

### Masalah: Tombol operasi disabled

**Solusi**:

- Pastikan gambar A sudah dimuat
- Untuk operasi 2 citra (×, ÷, AND, OR, XOR), pastikan gambar B sudah dipilih

---

## Tips Penggunaan

1. **Untuk Eksperimen**

   - Coba berbagai kombinasi offset
   - Bandingkan hasil normalisasi dengan operasi skalar
   - Eksperimen dengan threshold biner

2. **Untuk Hasil Optimal**
   - Gunakan gambar dengan kontras yang baik
   - Untuk operasi biner, konversi ke biner terlebih dahulu
   - Perhatikan nilai min/max normalisasi untuk memahami distribusi warna

---

## Kontributor

- **Nazar** - Requirement & Testing
- **GitHub Copilot** - Implementation

Versi: 1.0  
Tanggal: November 2025
