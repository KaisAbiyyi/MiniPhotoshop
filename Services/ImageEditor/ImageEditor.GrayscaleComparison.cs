using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Membuat citra grayscale dengan metode rata-rata sederhana: gray = (R + G + B) / 3.
        public BitmapSource CreateAverageGrayscale()
        {
            // Pastikan sudah ada pixel cache yang diisi oleh proses load.
            if (State.PixelCache == null)
            {
                throw new InvalidOperationException("Tidak ada data pixel yang dimuat.");
            }

            // Ambil ukuran gambar dari state.
            int width = State.CachedWidth;
            int height = State.CachedHeight;
            int stride = width * 4;
            // Buffer byte 1D: [B,G,R,A] berulang untuk setiap pixel.
            byte[] buffer = new byte[stride * height];
            var cache = State.PixelCache;

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = rowOffset + (x * 4);
                    // Baca channel dari cache 3D agar tidak perlu memanggil CopyPixels lagi.
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];
                    byte a = cache[x, y, 4];

                    // Hitung nilai grayscale dengan rata-rata aritmatika.
                    byte gray = (byte)((r + g + b) / 3);
                    // Tulis nilai gray yang sama ke B, G, dan R.
                    buffer[offset] = gray;
                    buffer[offset + 1] = gray;
                    buffer[offset + 2] = gray;
                    buffer[offset + 3] = a;
                }
            }

            // Konversi buffer byte menjadi BitmapSource baru.
            return CreateBitmapFromBuffer(buffer, width, height);
        }

        // Membuat citra grayscale dengan metode luminance (persepsi manusia),
        // menggunakan koefisien 0.299R + 0.587G + 0.114B.
        public BitmapSource CreateLuminanceGrayscale()
        {
            // Sama seperti metode rata-rata, tetap butuh PixelCache.
            if (State.PixelCache == null)
            {
                throw new InvalidOperationException("Tidak ada data pixel yang dimuat.");
            }

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            var cache = State.PixelCache;

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = rowOffset + (x * 4);
                    // Ambil R,G,B,A dari cache.
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];
                    byte a = cache[x, y, 4];

                    // Hitung luminance dengan bobot berbeda per channel.
                    double luminance = 0.299 * r + 0.587 * g + 0.114 * b;
                    // Clamp ke rentang 0–255 dan bulatkan ke byte.
                    byte gray = (byte)Math.Round(Math.Clamp(luminance, 0, 255));

                    buffer[offset] = gray;
                    buffer[offset + 1] = gray;
                    buffer[offset + 2] = gray;
                    buffer[offset + 3] = a;
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}

