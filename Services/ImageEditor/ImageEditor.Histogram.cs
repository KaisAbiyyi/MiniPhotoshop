using System;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Menghitung histogram intensitas untuk setiap channel (R, G, B, Gray)
        // dari PixelCache 3D. Hasilnya dipakai oleh sidebar dan window Histogram.
        public HistogramData Build()
        {
            if (State.PixelCache == null)
            {
                // Histogram hanya bisa dibuat jika pixel sudah diekstrak ke PixelCache.
                throw new InvalidOperationException("Tidak ada data pixel untuk membuat histogram.");
            }

            // Setiap array berukuran 256: index = intensitas (0-255), value = jumlah pixel.
            var red = new int[256];
            var green = new int[256];
            var blue = new int[256];
            var gray = new int[256];
            var cache = State.PixelCache; // [x, y, channel]

            int width = State.CachedWidth;
            int height = State.CachedHeight;

            // Loop semua pixel, baca nilai intensitas per channel,
            // lalu tambahkan counter pada bucket yang sesuai (0-255).
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    red[cache[x, y, 0]]++;
                    green[cache[x, y, 1]]++;
                    blue[cache[x, y, 2]]++;
                    gray[cache[x, y, 3]]++;
                }
            }

            // Simpan hasil ke State.Histogram agar bisa diakses UI dan fitur lain.
            State.Histogram.SetChannels(red, green, blue, gray);
            return State.Histogram;
        }
    }
}

