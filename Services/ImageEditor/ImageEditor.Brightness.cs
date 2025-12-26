using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Mereset seluruh state brightness (buffer dan nilai sebelumnya)
        // ke kondisi awal (tidak ada penyesuaian kecerahan).
        public void Reset()
        {
            State.Brightness.Reset();
        }

        // Mengupdate brightness berdasarkan nilai slider baru.
        // Perbedaan (delta) dari nilai sebelumnya ditambahkan ke buffer brightness,
        // lalu pipeline processed bitmap dihitung ulang.
        public BitmapSource Update(double newValue)
        {
            if (State.PixelCache == null || State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar yang dimuat.");
            }

            // Inisialisasi buffer brightness sekali, berisi nilai R,G,B awal.
            if (State.Brightness.Buffer == null)
            {
                InitializeBrightnessBuffer();
            }

            // Hitung selisih dari nilai sebelumnya sehingga slider bisa digeser
            // secara relatif tanpa menghitung ulang dari nol setiap kali.
            double delta = newValue - State.Brightness.PreviousValue;

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            var buffer = State.Brightness.Buffer!;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Tambahkan delta ke setiap channel R,G,B di buffer.
                    buffer[x, y, 0] += (int)delta;
                    buffer[x, y, 1] += (int)delta;
                    buffer[x, y, 2] += (int)delta;
                }
            }

            // Simpan nilai slider terakhir agar delta berikutnya bisa dihitung.
            State.Brightness.PreviousValue = newValue;
            return GetProcessedBitmap();
        }

        // Membuat buffer brightness awal berdasarkan gambar saat ini
        // (sudah termasuk filter dan negasi jika aktif).
        private void InitializeBrightnessBuffer()
        {
            // Mulai dari bitmap yang sudah terfilter sesuai ActiveFilter.
            BitmapSource current = GetFilteredBitmap(State.ActiveFilter);

            if (State.IsNegationActive)
            {
                // Jika negasi aktif, gunakan versi yang sudah ter-invert.
                current = ApplyNegation(current);
            }

            BitmapSource normalized = EnsureBgra32(current);
            int width = normalized.PixelWidth;
            int height = normalized.PixelHeight;
            int stride = width * 4;
            byte[] raw = new byte[stride * height];
            normalized.CopyPixels(raw, stride, 0);

            var buffer = new int[width, height, 3];
            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = rowOffset + (x * 4);
                    // Simpan nilai awal R,G,B sebagai integer untuk menampung penjumlahan delta.
                    buffer[x, y, 0] = raw[offset + 2];
                    buffer[x, y, 1] = raw[offset + 1];
                    buffer[x, y, 2] = raw[offset];
                }
            }

            State.Brightness.Buffer = buffer;
            State.Brightness.PreviousValue = 0;
        }

        // Menerapkan efek brightness hanya jika buffer sudah ada dan
        // nilai brightness berbeda signifikan dari 0.
        private BitmapSource ApplyBrightnessIfNeeded(BitmapSource source)
        {
            if (State.Brightness.Buffer == null || Math.Abs(State.Brightness.PreviousValue) < 0.0001)
            {
                return source;
            }

            return CreateBrightnessBitmap();
        }

        // Menghasilkan bitmap baru berdasarkan buffer brightness yang
        // sudah disesuaikan, sambil mempertahankan alpha dari PixelCache.
        private BitmapSource CreateBrightnessBitmap()
        {
            if (State.Brightness.Buffer == null || State.PixelCache == null)
            {
                throw new InvalidOperationException("Brightness buffer belum siap.");
            }

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            var brightness = State.Brightness.Buffer;
            var pixelCache = State.PixelCache;

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = rowOffset + (x * 4);
                    // Clamp nilai R,G,B agar tetap pada rentang valid 0–255.
                    int r = Math.Clamp(brightness[x, y, 0], 0, 255);
                    int g = Math.Clamp(brightness[x, y, 1], 0, 255);
                    int b = Math.Clamp(brightness[x, y, 2], 0, 255);

                    buffer[offset] = (byte)b;
                    buffer[offset + 1] = (byte)g;
                    buffer[offset + 2] = (byte)r;
                    // Ambil alpha dari PixelCache agar transparansi tidak berubah.
                    buffer[offset + 3] = pixelCache![x, y, 4];
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}

