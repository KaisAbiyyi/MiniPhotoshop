# Fitur Drag and Drop untuk Gambar

## Deskripsi

Fitur drag and drop memungkinkan pengguna untuk memuat gambar ke aplikasi MiniPhotoshop dengan cara drag file gambar dari file explorer dan drop ke window aplikasi.

## Cara Penggunaan

### Skenario 1: Memuat Gambar A (Pertama Kali)

1. **Buka File Explorer** dan navigasi ke folder yang berisi gambar
2. **Pilih file gambar** (klik dan tahan)
3. **Drag file** ke window aplikasi MiniPhotoshop
4. **Drop file** di area aplikasi
5. Gambar akan otomatis dimuat sebagai **Gambar A** (gambar utama)

### Skenario 2: Memuat Gambar Ketika Gambar A Sudah Ada

1. **Drag file gambar** dari file explorer
2. **Drop file** ke window aplikasi
3. **Dialog pilihan akan muncul** dengan 2 opsi:
   - **Ganti Gambar A**: Mengganti gambar utama yang sedang ditampilkan
   - **Set sebagai Gambar B**: Menetapkan gambar sebagai gambar kedua untuk operasi aritmatika/biner
4. **Pilih opsi** yang diinginkan dan klik **OK**
5. Gambar akan dimuat sesuai pilihan Anda

### Fitur Dialog Pilihan

![Dialog Pilihan](docs/dialog-selection.png)

**Opsi yang Tersedia:**

- ✅ **Ganti Gambar A (default)**: Mengganti gambar utama
  - Gambar lama akan hilang
  - Gambar baru akan ditampilkan di layar utama
  - Cocok untuk membuka gambar baru

- ✅ **Set sebagai Gambar B**: Menyimpan sebagai gambar operasi
  - Gambar A tetap ditampilkan di layar
  - Gambar B tersimpan untuk operasi aritmatika (tambah, kurang, kali, bagi)
  - Gambar B tersimpan untuk operasi biner (AND, OR, NOT, XOR)
  - Pesan konfirmasi akan ditampilkan dengan informasi ukuran gambar

## Format Gambar yang Didukung

- `.jpg` / `.jpeg` - JPEG Image
- `.png` - Portable Network Graphics
- `.bmp` - Bitmap Image
- `.gif` - Graphics Interchange Format
- `.tiff` / `.tif` - Tagged Image File Format

## Fitur Visual Feedback

Saat melakukan drag over window aplikasi:

- **Border** akan berubah warna menjadi **biru** (#4C8BF5)
- **Background** akan memiliki highlight **transparan biru**
- **Border thickness** akan bertambah untuk indikasi yang lebih jelas

Feedback visual ini membantu pengguna mengetahui bahwa aplikasi siap menerima file yang di-drop.

## Validasi File

Aplikasi akan otomatis memvalidasi:

- Apakah file yang di-drop adalah gambar
- Apakah format file didukung
- Apakah file dapat dibaca

Jika file tidak valid, akan muncul pesan error yang informatif.

## Error Handling

### Format Tidak Valid

```
File yang dipilih bukan gambar.

Format yang didukung: .jpg, .jpeg, .png, .bmp, .gif, .tiff
```

### Gagal Memuat Gambar

```
Gagal memuat gambar: [pesan error detail]
```

### Konfirmasi Gambar B Berhasil Dimuat

```
Gambar B berhasil dimuat:
[nama_file.jpg]
(1920 x 1080)
```

## Implementasi Teknis

### File yang Dimodifikasi/Ditambahkan

1. **MainWindow.xaml**

   - Menambahkan `AllowDrop="True"` pada Window
   - Menambahkan event handlers: `Drop`, `DragEnter`, `DragOver`, `DragLeave`
   - Menambahkan `x:Name="DisplayImageBorder"` untuk visual feedback

2. **MainWindow.DragDrop.cs** (File Baru)
   - `Window_DragEnter`: Validasi file dan tampilkan visual feedback
   - `Window_DragOver`: Maintain drag state
   - `Window_DragLeave`: Sembunyikan visual feedback
   - `Window_Drop`: Load gambar yang di-drop dengan dialog pilihan
   - `IsImageFile`: Validasi ekstensi file
   - `LoadImageFromFile`: Load gambar A menggunakan existing service
   - `LoadImageBFromFile`: Load gambar B untuk operasi aritmatika/biner
   - `ShowDragOverlay`: Tampilkan visual feedback
   - `HideDragOverlay`: Sembunyikan visual feedback

3. **ImageSelectionDialog.xaml** (File Baru)
   - Dialog UI untuk memilih tipe gambar (A atau B)
   - Radio button untuk pilihan
   - Button OK dan Cancel

4. **ImageSelectionDialog.xaml.cs** (File Baru)
   - Logic untuk dialog pilihan
   - Enum `ImageTarget`: ImageA, ImageB, Cancel
   - Event handlers untuk button OK dan Cancel

### Event Flow

```
File di-drag ke window
    ↓
Window_DragEnter
    ↓
Validasi format file
    ↓
Tampilkan visual feedback (jika valid)
    ↓
Window_DragOver (maintain state)
    ↓
[User drop file]
    ↓
Window_Drop
    ↓
Cek apakah Image A sudah ada?
    ↓
┌─────────┴─────────┐
│ Ya                │ Tidak
↓                   ↓
Tampilkan dialog    Load langsung sebagai Image A
pilihan             
│
├─ User pilih "Ganti Image A"
│  └─> Load sebagai Image A (replace)
│
├─ User pilih "Set sebagai Image B"
│  └─> Load sebagai Image B
│      └─> Deactivate active modes
│      └─> Tampilkan konfirmasi
│
└─ User klik "Batal"
   └─> Batalkan operasi
    ↓
Sembunyikan visual feedback
```

## Keunggulan

✅ **User-friendly**: Tidak perlu klik menu File → Open  
✅ **Cepat**: Langsung drag and drop  
✅ **Fleksibel**: Bisa pilih Image A atau Image B  
✅ **Visual feedback**: Jelas kapan bisa drop file  
✅ **Validasi otomatis**: Hanya menerima file gambar yang valid  
✅ **Error handling**: Pesan error yang informatif  
✅ **Smart detection**: Otomatis tampilkan dialog jika Image A sudah ada  
✅ **Multi-purpose**: Support untuk operasi aritmatika dan biner

## Catatan

- Fitur ini bekerja bersamaan dengan menu File → Open yang sudah ada
- Multiple files drag tidak didukung (hanya file pertama yang akan dimuat)
- Drag and drop berfungsi di seluruh area window aplikasi
- Dialog pilihan hanya muncul jika sudah ada Image A sebelumnya
- Saat memilih "Set sebagai Image B", mode operasi yang sedang aktif akan di-deactivate
- Image B dapat digunakan untuk operasi aritmatika (tambah, kurang, kali, bagi) dan operasi biner (AND, OR, XOR)

---

**Branch**: `feature/drag-drop-image`  
**Status**: ✅ Implemented and tested  
**Tanggal**: November 6, 2025
