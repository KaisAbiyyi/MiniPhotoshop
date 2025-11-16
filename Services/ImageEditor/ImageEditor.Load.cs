using System;
using System.IO;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Memuat gambar dari path yang diberikan, mengisi seluruh state workspace,
        // dan mengembalikan objek hasil load yang berisi info dasar gambar.
        public ImageLoadResult Load(string filePath)
        {
            // Validasi: path tidak boleh null/kosong/whitespace.
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path must be provided.", nameof(filePath));
            }

            // Setiap kali load gambar baru, bersihkan snapshot aritmatika sebelumnya
            // dan reset seluruh state workspace ke kondisi awal.
            ClearArithmeticSnapshot();
            State.Reset();

            // Buat BitmapImage kosong, lalu inisialisasi properti-propertinya.
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            // Ubah path relatif ke absolut dan set sebagai sumber gambar.
            bitmap.UriSource = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            // OnLoad: baca semua data ke memori supaya file tidak di-lock di disk.
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            // Freeze: menjadikan bitmap immutable dan aman dipakai lintas thread.
            bitmap.Freeze();

            // Simpan bitmap asli ke state utama.
            State.OriginalBitmap = bitmap;
            // Cache versi "Original" di kamus filter, supaya bisa diakses cepat.
            State.FilterCache[ImageFilterMode.Original] = bitmap;
            // Ekstrak semua pixel ke bentuk array 3D agar operasi lanjutan lebih cepat.
            State.PixelCache = ExtractPixelCache(bitmap);
            // Simpan lebar/tinggi untuk referensi fitur lain (zoom, histogram, dsb.).
            State.CachedWidth = bitmap.PixelWidth;
            State.CachedHeight = bitmap.PixelHeight;
            // Simpan path file saat ini untuk keperluan tampilan dan ekspor.
            State.CurrentFilePath = filePath;
            // Setelah load, mode filter aktif default adalah Original.
            State.ActiveFilter = ImageFilterMode.Original;

            // Bangun daftar preview filter (Original, R-only, G-only, dst.).
            BuildPreviews();
            // Bangun data turunan lain (misalnya histogram) dari pixel cache.
            Build();

            // Bungkus hasil load ke dalam ImageLoadResult agar UI punya semua info penting.
            return new ImageLoadResult(
                bitmap,
                filePath,
                bitmap.PixelWidth,
                bitmap.PixelHeight,
                bitmap.Format.ToString()
            );
        }
    }
}

