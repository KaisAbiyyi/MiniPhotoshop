using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Mengubah filter aktif di workspace (Original/RedOnly/GreenOnly/BlueOnly/Grayscale)
        // lalu rebuild bitmap hasil akhir (sudah termasuk efek lain seperti negasi, brightness, dll.).
        public BitmapSource SetActiveFilter(ImageFilterMode mode)
        {
            if (State.ImageObjects.Count == 0)
            {
                // Tidak boleh ganti filter kalau belum ada gambar yang dimuat.
                throw new InvalidOperationException("Tidak ada gambar yang dimuat.");
            }

            // Simpan pilihan filter ke state global workspace.
            State.ActiveFilter = mode;
            // Reset state brightness supaya filter baru tidak mewarisi adjustment sebelumnya.
            State.Brightness.Reset();
            // Kembalikan bitmap hasil akhir dengan filter baru + efek lain yang aktif.
            return GetProcessedBitmap();
        }

        // Pipeline utama untuk membentuk "processed image" yang tampil di UI.
        // 1) Mulai dari bitmap dasar sesuai filter (GetFilteredBitmap).
        // 2) Jika negasi aktif → ApplyNegationIfNeeded.
        // 3) Jika brightness diset → ApplyBrightnessIfNeeded.
        // 4) Jika binary threshold aktif → ApplyBinaryThreshold.
        // 5) Jika color selection aktif → ApplyColorSelection.
        public BitmapSource GetProcessedBitmap()
        {
            // Langkah pertama: ambil bitmap hasil filter warna (Original/RedOnly/...)
            var source = GetFilteredBitmap(State.ActiveFilter);
            // Layer berikutnya: efek negasi (jika aktif).
            source = ApplyNegationIfNeeded(source);
            // Lalu efek brightness (penyesuaian kecerahan).
            source = ApplyBrightnessIfNeeded(source);

            // Jika mode binary threshold aktif, ubah gambar jadi hitam/putih berdasarkan nilai threshold.
            if (State.IsBinaryThresholdActive)
            {
                source = ApplyBinaryThreshold(source, State.BinaryThresholdValue);
            }

            // Jika mode seleksi warna aktif, hanya warna tertentu yang dipertahankan.
            if (State.ColorSelection.IsActive)
            {
                source = ApplyColorSelection(source);
            }

            return source;
        }

        // Membangun daftar thumbnail kecil untuk setiap mode filter
        // yang ditampilkan di sidebar Filter (PreviewItems di-bind ke UI).
        public IReadOnlyList<PreviewItem> BuildPreviews()
        {
            State.PreviewItems.Clear();
            if (State.ImageObjects.Count == 0)
            {
                // Kalau belum ada gambar, tidak ada preview yang perlu dibuat.
                return State.PreviewItems;
            }

            // Tambah preview untuk setiap jenis filter warna.
            AddPreview(ImageFilterMode.Original, "Normal");
            AddPreview(ImageFilterMode.RedOnly, "Red Only");
            AddPreview(ImageFilterMode.GreenOnly, "Green Only");
            AddPreview(ImageFilterMode.BlueOnly, "Blue Only");
            AddPreview(ImageFilterMode.Grayscale, "Grayscale");
            // Tandai mana preview yang sedang aktif (sesuai ActiveFilter).
            SyncPreviewActivation();
            return State.PreviewItems;
        }

        // Menyesuaikan flag IsActive di setiap preview agar cocok dengan filter yang sedang dipakai.
        public void SyncPreviewActivation()
        {
            foreach (var item in State.PreviewItems)
            {
                item.IsActive = item.Mode == State.ActiveFilter;
            }
        }

        // Helper untuk menambahkan satu item preview:
        // 1) buat bitmap full-size sesuai mode filter,
        // 2) perkecil jadi thumbnail,
        // 3) simpan ke daftar PreviewItems.
        private void AddPreview(ImageFilterMode mode, string title)
        {
            BitmapSource full = GetFilteredBitmap(mode);
            BitmapSource preview = CreateThumbnail(full, PreviewThumbnailSize);
            State.PreviewItems.Add(new PreviewItem(mode, title, preview, mode == State.ActiveFilter));
        }

        // Mengembalikan bitmap yang sudah difilter sesuai mode.
        // Jika sudah pernah dihitung sebelumnya, gunakan cache di State.FilterCache.
        private BitmapSource GetFilteredBitmap(ImageFilterMode mode)
        {
            // Use selected image or first image
            var selectedImage = GetSelectedImage() ?? State.ImageObjects.FirstOrDefault();
            if (selectedImage == null)
            {
                throw new InvalidOperationException("Tidak ada gambar yang dimuat.");
            }

            if (State.FilterCache.TryGetValue(mode, out var cached))
            {
                // Sudah pernah dibuat sebelumnya → pakai bitmap dari cache.
                return cached;
            }

            // Kalau belum ada di cache, buat bitmap baru berdasarkan mode filter.
            BitmapSource result = mode switch
            {
                // RedOnly: ambil channel R dari pixel asli, nol-kan G dan B.
                ImageFilterMode.RedOnly => CreateFilteredBitmap((r, _, _, _) => (r, (byte)0, (byte)0)),
                // GreenOnly: ambil channel G saja.
                ImageFilterMode.GreenOnly => CreateFilteredBitmap((_, g, _, _) => ((byte)0, g, (byte)0)),
                // BlueOnly: ambil channel B saja.
                ImageFilterMode.BlueOnly => CreateFilteredBitmap((_, _, b, _) => ((byte)0, (byte)0, b)),
                // Grayscale: gunakan nilai gray (luminance) yang sudah disimpan di PixelCache untuk R,G,B.
                ImageFilterMode.Grayscale => CreateFilteredBitmap((r, g, b, gray) => (gray, gray, gray)),
                // Mode Original: pakai bitmap asli tanpa modifikasi.
                _ => selectedImage.Bitmap
            };

            // Simpan ke cache supaya pemanggilan berikutnya lebih cepat.
            State.FilterCache[mode] = result;
            return result;
        }

        // Membuat bitmap baru berbasis PixelCache 3D dengan fungsi selector custom.
        // selector menerima (r,g,b,gray) dari pixel asli lalu mengembalikan (R,G,B) baru.
        private BitmapSource CreateFilteredBitmap(Func<byte, byte, byte, byte, (byte R, byte G, byte B)> selector)
        {
            if (State.PixelCache == null)
            {
                throw new InvalidOperationException("Pixel cache belum diinisialisasi.");
            }

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            int stride = width * 4; // 4 byte per pixel: B,G,R,A
            byte[] buffer = new byte[stride * height];
            var cache = State.PixelCache; // Struktur [x, y, channel] dari ExtractPixelCache.

            // Loop 2D ke seluruh pixel dengan membaca nilai dari PixelCache
            // kemudian menulis nilai baru ke buffer 1D sesuai selector.
            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = rowOffset + (x * 4);
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];
                    byte gray = cache[x, y, 3];

                    // selector menentukan bagaimana R,G,B baru dibentuk dari data asli.
                    (byte rr, byte gg, byte bb) = selector(r, g, b, gray);
                    // Urutan penulisan ke buffer: B,G,R,A (format Bgra32).
                    buffer[offset] = bb;
                    buffer[offset + 1] = gg;
                    buffer[offset + 2] = rr;
                    buffer[offset + 3] = cache[x, y, 4]; // alpha tetap sama.
                }
            }

            // Ubah buffer Bgra32 1D menjadi BitmapSource baru.
            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}
