# Dokumentasi Fitur Distorsi dan Rotasi Citra

## 1. Fitur Distorsi Citra

### Deskripsi
Fitur distorsi menggunakan algoritma **Pixel Displacement dengan Random Offset** untuk menciptakan efek distorsi pada citra. Semakin tinggi level distorsi, semakin besar pergeseran pixel dari posisi aslinya.

### Rumus Algoritma

Untuk setiap pixel pada posisi $(x, y)$ di citra hasil:

$$
\text{newX} = x + \text{random}(-\text{level}, \text{level})
$$

$$
\text{newY} = y + \text{random}(-\text{level}, \text{level})
$$

$$
\text{result}[x, y] = \text{source}[\text{clamp}(\text{newX}, 0, \text{width}-1), \text{clamp}(\text{newY}, 0, \text{height}-1)]
$$

Di mana:
- `level` adalah tingkat distorsi (1-50)
- `random(-level, level)` menghasilkan bilangan acak dalam rentang tersebut
- `clamp(value, min, max)` memastikan nilai tetap dalam batas gambar

### Cara Kerja
1. Untuk setiap pixel di citra hasil, hitung offset acak berdasarkan level
2. Tentukan posisi sumber baru dengan menambahkan offset
3. Clamp posisi sumber agar tetap dalam batas gambar
4. Copy nilai pixel dari posisi sumber ke posisi hasil

### Penggunaan di UI
1. Klik tombol **"Distorsi"** di toolbar
2. Atur **Level Distorsi** menggunakan slider (1-50)
3. Klik **"Terapkan Distorsi"** untuk menerapkan efek
4. Klik **"Kembalikan ke Asli"** untuk mengembalikan gambar ke kondisi sebelum distorsi

---

## 2. Fitur Rotasi Citra

### Deskripsi
Fitur rotasi mendukung dua mode:
- **Rotasi Optimized** (90°, 180°, 270°): Tanpa interpolasi, hanya pemetaan ulang koordinat
- **Rotasi General** (sudut bebas): Dengan interpolasi bilinear untuk mengurangi jaggies

### Rumus Rotasi Optimized

#### Rotasi 90° (searah jarum jam)
$$
\text{dstX} = \text{height} - 1 - y
$$
$$
\text{dstY} = x
$$

#### Rotasi 180°
$$
\text{dstX} = \text{width} - 1 - x
$$
$$
\text{dstY} = \text{height} - 1 - y
$$

#### Rotasi 270° (atau 90° berlawanan arah jarum jam)
$$
\text{dstX} = y
$$
$$
\text{dstY} = \text{width} - 1 - x
$$

### Rumus Rotasi General (Sudut Bebas)

#### Transformasi Koordinat
Konversi sudut ke radian:
$$
\theta = \text{degrees} \times \frac{\pi}{180}
$$

Untuk setiap pixel di citra hasil pada posisi $(\text{dstX}, \text{dstY})$, hitung koordinat sumber menggunakan **backward mapping**:

$$
x = \text{dstX} - \text{resultCenterX}
$$
$$
y = \text{dstY} - \text{resultCenterY}
$$

**Inverse rotation** (dari hasil ke sumber):
$$
\text{srcX} = x \cdot \cos(\theta) + y \cdot \sin(\theta) + \text{centerX}
$$
$$
\text{srcY} = -x \cdot \sin(\theta) + y \cdot \cos(\theta) + \text{centerY}
$$

#### Interpolasi Bilinear
Jika $(\text{srcX}, \text{srcY})$ berada dalam batas gambar sumber, ambil 4 tetangga terdekat:

$$
x_0 = \lfloor \text{srcX} \rfloor, \quad x_1 = x_0 + 1
$$
$$
y_0 = \lfloor \text{srcY} \rfloor, \quad y_1 = y_0 + 1
$$

Bobot fraksi:
$$
f_x = \text{srcX} - x_0
$$
$$
f_y = \text{srcY} - y_0
$$

Nilai akhir (per channel R, G, B, A):
$$
\text{value} = (1 - f_x)(1 - f_y) \cdot P_{00} + f_x(1 - f_y) \cdot P_{10} + (1 - f_x)f_y \cdot P_{01} + f_x f_y \cdot P_{11}
$$

Di mana $P_{ij}$ adalah nilai pixel pada posisi $(x_i, y_j)$.

### Bounding Box Hasil
Untuk menghitung ukuran citra hasil setelah rotasi:

1. Rotasi 4 sudut gambar asli: $(0,0)$, $(w,0)$, $(w,h)$, $(0,h)$
2. Untuk setiap sudut $(x, y)$:
$$
\text{rotX} = x \cdot \cos(\theta) - y \cdot \sin(\theta)
$$
$$
\text{rotY} = x \cdot \sin(\theta) + y \cdot \cos(\theta)
$$
3. Hitung bounding box dari titik-titik hasil rotasi

### Penggunaan di UI
1. Klik tombol **"Rotasi"** di toolbar
2. Pilih preset (45°, 90°, 180°, 270°) atau:
   - Gunakan **Fine Control** (← 1°, 1° →) untuk rotasi bertahap
   - Masukkan nilai sudut manual dan klik **"Rotasi"**
3. Klik **"Kembalikan ke Asli"** untuk mengembalikan gambar

---

## Catatan Teknis

### Format Pixel
Semua operasi menggunakan format **BGRA32** (4 byte per pixel):
- Byte 0: Blue
- Byte 1: Green
- Byte 2: Red
- Byte 3: Alpha

### Snapshot Mechanism
Kedua fitur menyimpan snapshot gambar asli sebelum operasi pertama, memungkinkan restore ke kondisi awal tanpa kehilangan kualitas.
