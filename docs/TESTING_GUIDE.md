# Panduan Testing 5 Tugas Image Processing

Panduan ini menjelaskan langkah-langkah untuk testing setiap fitur yang telah diimplementasikan.

## Persiapan

1. **Build dan Run aplikasi:**
   ```bash
   cd c:\projects\MiniPhotoshop
   dotnet build
   dotnet run
   ```

2. **Siapkan gambar test:**
   - Gambar RGB berwarna (untuk testing warna)
   - Gambar dengan gradasi (untuk testing brightness & threshold)
   - Gambar dengan area warna solid (untuk testing seleksi warna)

## Test Case 1: Negasi Citra

### Langkah:
1. Klik menu **File** → **Buka Gambar...**
2. Pilih gambar RGB berwarna
3. Klik menu **Operations** → **1. Negasi Citra**

### Expected Result:
- Dialog konfirmasi muncul dengan pesan "Negasi citra berhasil diterapkan!"
- Gambar berubah dengan warna terbalik:
  - Merah → Cyan
  - Hijau → Magenta
  - Biru → Yellow
  - Hitam → Putih
  - Putih → Hitam

### Validation:
- Warna terang menjadi gelap
- Warna gelap menjadi terang
- Detail gambar tetap terjaga

---

## Test Case 2: Brightness Slider

### Langkah:
1. Buka gambar
2. Cari panel **"2. Brightness Adjustment"** di bawah info file
3. Geser slider ke kanan (+255)
4. Geser slider ke kiri (-255)
5. Geser slider kembali ke tengah (0)

### Expected Result:
- **+255**: Gambar menjadi sangat terang/putih
- **-255**: Gambar menjadi sangat gelap/hitam
- **0**: Gambar kembali ke brightness asli
- Nilai brightness ditampilkan di sebelah slider

### Validation:
- Slider responsif dan smooth
- Tidak ada data loss saat memundurkan slider
- Nilai di luar 0-255 di-clamp dengan benar saat ditampilkan

### Test Lanjutan (Data Loss Prevention):
1. Set brightness ke +100
2. Set brightness ke -100 (delta -200)
3. Set brightness kembali ke +100 (delta +200)
4. Hasil harus sama dengan langkah 1 (tidak blur/rusak)

---

## Test Case 3: Perbandingan Grayscale

### Langkah:
1. Buka gambar RGB berwarna (sebaiknya yang memiliki warna merah, hijau, biru)
2. Klik menu **Operations** → **3. Compare Grayscale (Avg vs Lum)**
3. Perhatikan window baru yang muncul dengan 2 gambar side-by-side

### Expected Result:
- Window baru terbuka dengan judul "Perbandingan Metode Grayscale"
- **Sisi Kiri (Average)**:
  - Label: "Average Method"
  - Formula ditampilkan: "(R + G + B) / 3"
- **Sisi Kanan (Luminance)**:
  - Label: "Luminance Method"
  - Formula ditampilkan: "0.299×R + 0.587×G + 0.114×B"

### Validation:
- Kedua gambar dalam grayscale
- **Perbedaan visual**: 
  - Area hijau pada Luminance lebih terang (bobot 0.587)
  - Area biru pada Luminance lebih gelap (bobot 0.114)
  - Average memberikan hasil lebih "rata"

### Test dengan Gambar Spesifik:
- **Pure Red (255,0,0):**
  - Average: 85
  - Luminance: ~76
- **Pure Green (0,255,0):**
  - Average: 85
  - Luminance: ~150
- **Pure Blue (0,0,255):**
  - Average: 85
  - Luminance: ~29

---

## Test Case 4: Seleksi Warna Interaktif

### Langkah:
1. Buka gambar dengan beberapa area warna solid (misal: logo, flat design)
2. Cari panel **"4. Seleksi Warna Interaktif"**
3. Centang checkbox **"Aktifkan"**
4. Klik pada area warna tertentu di gambar
5. Perhatikan perubahan
6. Klik warna lain untuk seleksi berbeda
7. Uncheck checkbox untuk reset

### Expected Result:
- Saat checkbox di-centang:
  - Text berubah menjadi biru: "Klik pada gambar untuk memilih warna"
  - Cursor dapat diklik pada gambar
- Setelah klik warna:
  - Hanya piksel dengan RGB yang **sama persis** ditampilkan berwarna
  - Piksel lain menjadi hitam
  - Text panel menunjukkan RGB value, contoh: "RGB(255, 128, 64)"
  - Warna text berubah sesuai warna yang dipilih
- Saat uncheck:
  - Gambar kembali normal

### Validation:
- Seleksi akurat (exact match)
- Info RGB benar
- Reset berfungsi

### Test Edge Case:
- Klik di luar gambar → tidak terjadi error
- Klik saat zoom → koordinat tetap benar

---

## Test Case 5: Binary Threshold + Negasi

### Langkah:
1. Buka gambar (sebaiknya yang memiliki gradasi abu-abu)
2. Klik menu **Operations** → **5. Binary Threshold (128) + Negasi**

### Expected Result:
- Dialog konfirmasi muncul dengan penjelasan 2 langkah:
  - "1. Threshold: if gray > 128 then 255 else 0"
  - "2. Negasi: 255 - binary_value"
- Gambar berubah menjadi hitam-putih:
  - Area yang **tadinya terang** (>128) → menjadi **hitam** (0)
  - Area yang **tadinya gelap** (≤128) → menjadi **putih** (255)

### Validation:
- Hanya ada 2 warna: hitam (0) dan putih (255)
- Threshold di 128 (titik tengah 0-255)
- Hasil ter-negasi (bukan langsung binary biasa)

### Perbandingan:
- **Binary Normal** (tanpa negasi):
  - >128 → putih
  - ≤128 → hitam
- **Binary + Negasi** (implementasi kita):
  - >128 → hitam
  - ≤128 → putih

---

## Test Kombinasi

### Scenario 1: Negasi → Brightness
1. Apply Negasi
2. Adjust Brightness slider
3. Validasi bahwa brightness bekerja pada hasil negasi

### Scenario 2: Brightness → Color Selection
1. Set brightness +100
2. Aktifkan color selection
3. Klik warna
4. Validasi bahwa seleksi bekerja pada gambar yang sudah di-adjust

### Scenario 3: Multiple Operations
1. Open image
2. Apply Negasi
3. Klik filter "Grayscale" di preview panel
4. Apply Binary Threshold
5. Validasi hasil akhir

---

## Regression Testing

Pastikan fitur lama masih berfungsi:

1. **Preview Filters** (bawah layar):
   - Normal, Red Only, Green Only, Blue Only, Grayscale
   
2. **Histogram** (sidebar kanan):
   - Red, Green, Blue, Grayscale histogram
   - Klik histogram untuk popup detail
   
3. **Save Pixels**:
   - Export pixel data ke .txt file
   - Validasi format 3D array

4. **Zoom & Pan**:
   - Ctrl + Mouse Wheel untuk zoom
   - Shift + Mouse Wheel untuk horizontal scroll

---

## Bug Report Template

Jika menemukan bug, catat:

```
**Fitur:** [Nama fitur]
**Langkah Reproduksi:**
1. ...
2. ...

**Expected:** [Apa yang seharusnya terjadi]
**Actual:** [Apa yang terjadi]
**Screenshot:** [Jika ada]
**Error Message:** [Jika ada]
```

---

## Performance Testing

Untuk gambar besar (>2000x2000 pixels):

1. Test responsiveness saat:
   - Apply Negasi
   - Geser Brightness slider
   - Color Selection click
   - Binary Threshold

2. Expected: Operasi selesai < 1 detik untuk gambar 4K

---

## Acceptance Criteria

✅ Semua 5 fitur berfungsi tanpa error
✅ UI responsif dan intuitif
✅ Data loss tidak terjadi pada brightness
✅ Seleksi warna akurat (exact RGB match)
✅ Binary threshold menggunakan threshold 128
✅ Semua dialog konfirmasi tampil dengan pesan yang benar
✅ Fitur lama tidak rusak (no regression)

---

**Testing Date:** _______________
**Tested By:** _______________
**Result:** ⬜ Pass  ⬜ Fail
**Notes:** _______________
