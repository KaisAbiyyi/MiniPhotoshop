using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Mengaktifkan / menonaktifkan mode seleksi warna.
        // Saat aktif, snapshot gambar sebelum seleksi disimpan agar bisa dipulihkan.
        public BitmapSource SetColorSelectionActive(bool isActive)
        {
            if (State.ColorSelection.IsActive == isActive)
            {
                return GetProcessedBitmap();
            }

            State.ColorSelection.IsActive = isActive;

            if (isActive)
            {
                // Simpan bitmap saat ini sebagai referensi sebelum seleksi.
                State.ColorSelection.OriginalBeforeSelection = GetProcessedBitmap();
                State.ColorSelection.HasTarget = false;
                return State.ColorSelection.OriginalBeforeSelection;
            }

            // Jika dimatikan, kembalikan ke gambar sebelum seleksi (jika ada).
            var original = State.ColorSelection.OriginalBeforeSelection ?? GetProcessedBitmap();
            State.ColorSelection.Reset();
            return original;
        }

        // Menerapkan seleksi dengan cara memilih warna target di koordinat (pixelX, pixelY)
        // lalu menghasilkan citra baru yang hanya menampilkan pixel dengan warna tersebut.
        public BitmapSource ApplySelection(int pixelX, int pixelY)
        {
            if (!State.ColorSelection.IsActive || State.PixelCache == null)
            {
                throw new InvalidOperationException("Mode seleksi warna belum aktif.");
            }

            if (pixelX < 0 || pixelX >= State.CachedWidth || pixelY < 0 || pixelY >= State.CachedHeight)
            {
                throw new ArgumentOutOfRangeException("Koordinat piksel berada di luar gambar.");
            }

            // Ambil warna target dari PixelCache pada posisi yang diklik.
            State.ColorSelection.TargetR = State.PixelCache[pixelX, pixelY, 0];
            State.ColorSelection.TargetG = State.PixelCache[pixelX, pixelY, 1];
            State.ColorSelection.TargetB = State.PixelCache[pixelX, pixelY, 2];
            State.ColorSelection.HasTarget = true;

            return ApplyColorSelection(GetProcessedBitmap());
        }

        // Melakukan masking: pixel yang warnanya sama dengan target akan ditampilkan,
        // sisanya dibuat hitam (0,0,0) dengan alpha dipertahankan.
        private BitmapSource ApplyColorSelection(BitmapSource source)
        {
            if (!State.ColorSelection.IsActive || State.PixelCache == null)
            {
                return source;
            }

            byte targetR = State.ColorSelection.TargetR;
            byte targetG = State.ColorSelection.TargetG;
            byte targetB = State.ColorSelection.TargetB;

            if (State.ColorSelection.OriginalBeforeSelection == null || !State.ColorSelection.HasTarget)
            {
                return source;
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
                    // Baca R,G,B,A dari cache.
                    byte r = cache![x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];
                    byte a = cache[x, y, 4];

                    // Jika sama dengan warna target → tampilkan warna asli.
                    if (r == targetR && g == targetG && b == targetB)
                    {
                        buffer[offset] = b;
                        buffer[offset + 1] = g;
                        buffer[offset + 2] = r;
                        buffer[offset + 3] = a;
                    }
                    else
                    {
                        // Jika berbeda → jadikan hitam (mask).
                        buffer[offset] = 0;
                        buffer[offset + 1] = 0;
                        buffer[offset + 2] = 0;
                        buffer[offset + 3] = a;
                    }
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}
