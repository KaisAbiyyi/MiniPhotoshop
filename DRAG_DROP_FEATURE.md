# Fitur Drag and Drop untuk Gambar

## Deskripsi

Fitur drag and drop memungkinkan pengguna untuk memuat gambar ke aplikasi MiniPhotoshop dengan cara drag file gambar dari file explorer dan drop ke window aplikasi.

## Cara Penggunaan

1. **Buka File Explorer** dan navigasi ke folder yang berisi gambar
2. **Pilih file gambar** (klik dan tahan)
3. **Drag file** ke window aplikasi MiniPhotoshop
4. **Drop file** di area aplikasi
5. Gambar akan otomatis dimuat dan ditampilkan

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
   - `Window_Drop`: Load gambar yang di-drop
   - `IsImageFile`: Validasi ekstensi file
   - `LoadImageFromFile`: Load gambar menggunakan existing service
   - `ShowDragOverlay`: Tampilkan visual feedback
   - `HideDragOverlay`: Sembunyikan visual feedback

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
Load gambar
    ↓
Sembunyikan visual feedback
```

## Keunggulan

✅ **User-friendly**: Tidak perlu klik menu File → Open  
✅ **Cepat**: Langsung drag and drop  
✅ **Visual feedback**: Jelas kapan bisa drop file  
✅ **Validasi otomatis**: Hanya menerima file gambar yang valid  
✅ **Error handling**: Pesan error yang informatif  

## Catatan

- Fitur ini bekerja bersamaan dengan menu File → Open yang sudah ada
- Multiple files drag tidak didukung (hanya file pertama yang akan dimuat)
- Drag and drop berfungsi di seluruh area window aplikasi

---

**Branch**: `feature/drag-drop-image`  
**Status**: ✅ Implemented and tested  
**Tanggal**: November 6, 2025
