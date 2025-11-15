# Quick Reference Guide - Fitur Baru MiniPhotoshop

## 🚀 Quick Start

### 1️⃣ Perkalian/Pembagian Citra

```
1. File → Buka Gambar (Gambar A)
2. Pilih Gambar B → Pilih file kedua
3. Klik "Kali Citra (×)" atau "Bagi Citra (÷)"
4. Lihat hasil + info normalisasi di status bar
```

### 2️⃣ Operasi Boolean (AND/OR/NOT/XOR)

```
1. File → Buka Gambar (Gambar A)
2. [Opsional] Klik "Convert to Binary"
3. Untuk AND/OR/XOR: Pilih Gambar B → Klik operasi
4. Untuk NOT: Langsung klik "NOT"
```

---

## 📋 Tombol & Fungsi

| Tombol                | Fungsi                         | Memerlukan Gambar B |
| --------------------- | ------------------------------ | ------------------- |
| **Kali Citra (×)**    | Perkalian piksel + normalisasi | ✅ Ya               |
| **Bagi Citra (÷)**    | Pembagian piksel + normalisasi | ✅ Ya               |
| **Convert to Binary** | Konversi ke hitam-putih        | ❌ Tidak            |
| **AND**               | Operasi logika AND             | ✅ Ya               |
| **OR**                | Operasi logika OR              | ✅ Ya               |
| **NOT**               | Operasi logika NOT (inversi)   | ❌ Tidak            |
| **XOR**               | Operasi logika XOR             | ✅ Ya               |

---

## 🎯 Workflow Rekomendasi Presentasi

### Skenario 1: Demonstrasi Normalisasi

```
1. Buka 2 gambar yang mirip (misal: foto sama beda exposure)
2. Kali Citra (×)
3. Tunjukkan nilai normalisasi di status bar
4. Jelaskan perbedaan min/max per kanal RGB
```

### Skenario 2: Demonstrasi Boolean

```
1. Buka gambar dengan objek jelas (misal: logo hitam-putih)
2. Convert to Binary (threshold 128)
3. Pilih Gambar B (misal: mask atau pola)
4. Coba AND → lihat irisan
5. Coba OR → lihat gabungan
6. Coba XOR → lihat selisih
7. Coba NOT → lihat inversi
```

---

## 💡 Tips Cepat

### Untuk Hasil Terbaik:

- ✅ Gunakan gambar dengan ukuran yang sama
- ✅ Untuk Boolean: konversi ke biner dulu
- ✅ Perhatikan offset jika gambar beda ukuran
- ✅ Lihat status bar untuk info normalisasi

### Troubleshooting:

- ❌ **Gambar hitam semua?** → Cek offset X/Y
- ❌ **Tombol disabled?** → Pastikan gambar A dan B sudah dimuat
- ❌ **Status bar kosong?** → Status bar hanya untuk operasi ×/÷

---

## 🔢 Nilai Default

| Parameter        | Default |
| ---------------- | ------- |
| Offset X         | 0       |
| Offset Y         | 0       |
| Binary Threshold | 128     |
| EPSILON (÷)      | 1e-10   |

---

## ⌨️ Keyboard Shortcuts

_Belum ada shortcuts khusus untuk fitur baru_

---

## 📊 Formula Penting

### Normalisasi

```
normalized = ((value - min) / (max - min)) × 255
```

### Grayscale (untuk Binary)

```
gray = 0.299×R + 0.587×G + 0.114×B
```

### Binary Threshold

```
result = (gray ≥ 128) ? 255 : 0
```

---

## 📁 File Output

| Operasi   | Nama File Default           |
| --------- | --------------------------- |
| Perkalian | `Hasil_Perkalian_Citra.png` |
| Pembagian | `Hasil_Pembagian_Citra.png` |
| Binary    | `Citra_Biner.png`           |
| AND       | `Hasil_AND.png`             |
| OR        | `Hasil_OR.png`              |
| NOT       | `Hasil_NOT.png`             |
| XOR       | `Hasil_XOR.png`             |

---

## 📞 Support

Untuk pertanyaan atau bug report, hubungi tim development.

**Happy Editing! 🎉**
