# Cara Penggunaan Fitur Image Processing

## Quick Start Guide

### 1. Negasi Citra (Image Negation)
**Cara:**
1. Buka gambar (File → Buka Gambar...)
2. Pilih filter yang diinginkan di bagian bawah (Normal, Red Only, Green Only, Blue Only, Grayscale)
3. Klik toggle button **"Negasi Citra"** di bawah filter preview
4. Gambar akan langsung ter-negasi (warna terbalik)

**Efek:**
- Warna terang → gelap
- Warna gelap → terang
- Merah → Cyan, Hijau → Magenta, Biru → Yellow

**Kombinasi:**
- Negasi + Filter Red Only = Cyan only
- Negasi + Grayscale = Inverted grayscale
- Negasi + Brightness = Brightness pada gambar ter-negasi

---

### 2. Brightness Adjustment
**Cara:**
1. Buka gambar
2. Pilih filter apapun (opsional: aktifkan negasi juga)
3. Geser slider **"Brightness Adjustment"** di panel atas
   - Geser ke kanan: Lebih terang (+255 max)
   - Geser ke kiri: Lebih gelap (-255 min)
   - Tengah: Brightness asli (0)

**Keunggulan:**
- Tidak ada data loss saat dimundurkan
- Bekerja di filter apapun
- Bekerja pada gambar dengan negasi aktif

**Contoh Workflow:**
```
1. Pilih filter "Grayscale"
2. Aktifkan "Negasi Citra" (inverted grayscale)
3. Brightness +100 (terangkan sedikit)
```

---

### 3. Seleksi Warna Interaktif
**Cara:**
1. Buka gambar berwarna
2. Centang checkbox **"Aktifkan"** di panel Seleksi Warna
3. Klik pada warna yang ingin diseleksi di gambar
4. Hanya piksel dengan RGB yang **sama persis** yang ditampilkan
5. Piksel lain menjadi hitam
6. Uncheck untuk kembali normal

**Info RGB:**
- Panel akan menampilkan nilai RGB yang dipilih
- Contoh: "RGB(255, 128, 64)"
- Warna text berubah sesuai warna yang dipilih

**Tips:**
- Gunakan untuk isolasi warna tertentu
- Bekerja bagus pada flat design / logo
- Exact match RGB (tidak ada toleransi)

---

## Kombinasi Fitur

### Skenario 1: High Contrast Grayscale
```
1. Filter: Grayscale
2. Brightness: +50
3. Negasi: OFF
Result: Bright grayscale image
```

### Skenario 2: Inverted Blue Channel
```
1. Filter: Blue Only
2. Negasi: ON (menjadi Yellow-only)
3. Brightness: -30 (gelap sedikit)
Result: Dark yellow channel
```

### Skenario 3: Color Isolation pada Negated Image
```
1. Filter: Normal
2. Negasi: ON
3. Color Selection: Pilih warna tertentu
Result: Hanya warna terpilih (ter-negasi) yang muncul
```

---

## UI Layout

```
┌─────────────────────────────────────────┐
│ Menu: File                              │
│   - Buka Gambar                         │
│   - Simpan Pixel                        │
├─────────────────────────────────────────┤
│ Filename: image.jpg                     │
│ Info: 1920 x 1080 | Bgra32              │
│                                         │
│ Brightness Adjustment (-255 to +255)   │
│ [========●=========] 0                  │
│                                         │
│ Seleksi Warna Interaktif               │
│ ☐ Aktifkan  Klik pada gambar...        │
├─────────────────────────────────────────┤
│                                         │
│         GAMBAR UTAMA                    │
│                                         │
├─────────────────────────────────────────┤
│ ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐    │
│ │Norm│ │Red │ │Gren│ │Blue│ │Gray│    │
│ └────┘ └────┘ └────┘ └────┘ └────┘    │
│                                         │
│      [   Negasi Citra   ]              │
└─────────────────────────────────────────┘
```

---

## Shortcut Workflow

**Reset semua:**
1. Pilih filter "Normal"
2. Negasi OFF
3. Brightness ke 0
4. Color Selection uncheck

**Quick negative:**
1. Klik toggle "Negasi Citra"
(Bekerja di filter apapun yang sedang aktif)

**Ganti filter:**
- Otomatis mereset Brightness ke 0
- Negasi tetap aktif/nonaktif sesuai state

---

## Troubleshooting

**Q: Brightness tidak muncul?**
A: Pastikan sudah buka gambar terlebih dahulu

**Q: Negasi button disabled?**
A: Buka gambar dulu, button akan aktif otomatis

**Q: Color selection tidak akurat?**
A: Fitur menggunakan exact RGB match. Jika warna sedikit berbeda (misal karena JPEG compression), tidak akan terdeteksi.

**Q: Brightness "lompat" saat ganti filter?**
A: Normal behavior. Brightness direset ke 0 saat ganti filter untuk konsistensi.

**Q: Kombinasi Negasi + Brightness lambat?**
A: Untuk gambar >4K, proses mungkin memakan waktu 1-2 detik. Ini normal.

---

## Advanced Tips

1. **Untuk Analisis Warna:**
   - Gunakan filter Red/Green/Blue Only
   - Aktifkan Color Selection untuk isolasi warna spesifik
   - Lihat histogram di sidebar kanan

2. **Untuk Kontras Tinggi:**
   - Grayscale filter
   - Brightness adjustment
   - Optional: Negasi untuk inverted contrast

3. **Untuk Efek Artistik:**
   - Kombinasi Negasi dengan filter warna
   - Blue Only + Negasi = Yellow dominant
   - Red Only + Negasi = Cyan dominant

---

**Dokumentasi Lengkap:** Lihat `IMPLEMENTASI_5_TUGAS.md`  
**Testing Guide:** Lihat `TESTING_GUIDE.md`
