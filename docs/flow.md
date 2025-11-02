# Alur Kerja Fitur MiniPhotoshop

## 1. Memuat Gambar
- Klik File > Buka Gambar... lalu pilih berkas.
- Aplikasi memuat bitmap ke memori (_pixelCache) lengkap dengan kanal alpha sehingga PNG transparan tetap transparan.
- Sidebar menampilkan nama file, informasi resolusi, serta mengaktifkan fitur-fitur lanjutan.

## 2. Navigasi Workspace
- Gunakan roda mouse + Ctrl untuk zoom in/out (rentang 10% - 500%).
- Tahan Shift sambil scroll untuk menggeser horizontal; scroll biasa untuk vertikal.
- Posisi zoom dipertahankan secara relatif terhadap kursor sehingga fokus mudah diatur.

## 3. Filter Preview & Pemilihan Mode Warna
- Panel preview menampilkan pratinjau Normal, Red Only, Green Only, Blue Only, Grayscale.
- Klik salah satu pratinjau untuk menjadikannya filter aktif.
- Semua operasi lanjutan (brightness, negasi, threshold) selalu dimulai dari filter aktif ini.

## 4. Negasi Citra
- Aktifkan toggle Negasi Citra untuk membalik seluruh channel warna dari filter aktif.
- Menonaktifkan toggle langsung mengembalikan gambar ke kondisi sebelum negasi tanpa kehilangan perubahan lain.

## 5. Penyesuaian Brightness
- Panel Brightness Adjustment muncul otomatis setelah gambar dimuat.
- Slider memiliki rentang -255 sampai +255 dan bersifat relatif (nilai diingat selama sesi gambar).
- Perubahan brightness digabungkan dengan filter dan negasi, kemudian hasilnya dipakai sebagai dasar threshold/operasi lain.

## 6. Binary Threshold + Negasi
- Gunakan toggle Binary Threshold + Negasi untuk mengaktifkan mode biner.
- Saat aktif, panel slider threshold (0-255) muncul; nilai default 128 dan bisa diubah real-time.
- Algoritma: hitung grayscale rata-rata ? jika lebih besar dari nilai threshold maka 255, selain itu 0 ? hasilnya dinegasikan sehingga area terang menjadi hitam dan sebaliknya.
- Transparansi asli setiap piksel dipertahankan sehingga PNG tanpa latar tetap transparan.

## 7. Perbandingan Grayscale
- Tombol Grayscale Comparison membuka jendela baru yang menampilkan dua versi: average dan luminance.
- Setiap kali dibuka, gambar diambil dari cache sehingga selalu sesuai dengan gambar sumber terkini.

## 8. Seleksi Warna
- Aktifkan Color Selection Mode untuk menampilkan instruksi dan mengikat event klik.
- Klik piksel pada gambar ? aplikasi menyimpan warna target, menampilkan nilainya, dan membuat bitmap yang hanya mempertahankan piksel dengan warna persis sama (alpha tetap mengikuti sumber).
- Menonaktifkan mode mengembalikan gambar terakhir sebelum seleksi.

## 9. Histogram Sidebar
- Sidebar menampilkan histogram untuk kanal Red, Green, Blue, dan Grayscale.
- Klik kanvas histogram membuka jendela detail (jika diimplementasikan di tugas terkait) menggunakan data yang telah dihitung dari cache.
- Histogram otomatis diperbarui setiap kali gambar baru dimuat.

## 10. Simpan Data Pixel
- Menu File > Simpan Pixel menulis isi _pixelCache ke file teks.
- Struktur array yang disimpan: [width][height][5] dengan urutan kanal [R, G, B, Gray, Alpha].
- Cocok untuk dokumentasi tugas atau analisis lanjutan.

## Alur Demo yang Disarankan
1. Mulai dengan memuat PNG transparan untuk menonjolkan dukungan alpha.
2. Tunjukkan navigasi zoom/pan agar audiens paham workspace responsif.
3. Ganti-ganti filter pratinjau untuk memperlihatkan cache filter bekerja.
4. Aktifkan Negasi Citra lalu geser brightness, tunjukkan kombinasi efek.
5. Aktifkan Binary Threshold + Negasi, ubah nilai slider sambil sorot area hasil binarisasi.
6. Buka Grayscale Comparison untuk membandingkan dua metode grayscale.
7. Demonstrasikan Color Selection Mode dengan mengklik area tertentu.
8. Arahkan ke sidebar histogram untuk menegaskan analisis statistik tersedia.
9. Akhiri dengan menyimpan data pixel dan tunjukkan format file keluaran.

