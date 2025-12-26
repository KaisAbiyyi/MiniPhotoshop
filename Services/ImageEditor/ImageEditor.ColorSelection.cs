using System;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Models;

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
                State.ColorSelection.TolerancePercent = ColorSelectionState.DefaultTolerancePercent;
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

            return GetProcessedBitmap();
        }

        // Melakukan masking: pixel yang warnanya dekat dengan target (sesuai toleransi)
        // ditampilkan, sisanya diubah menjadi grayscale yang digelapkan (spotlight).
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

            double tolerance = Math.Clamp(State.ColorSelection.TolerancePercent, 0, 100);
            double maxDistanceSquared = 255.0 * 255.0 * 3.0;
            double thresholdSquared = maxDistanceSquared * (tolerance / 100.0) * (tolerance / 100.0);
            const double dimFactor = 0.4;

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

                    int dr = r - targetR;
                    int dg = g - targetG;
                    int db = b - targetB;
                    double distSquared = (dr * dr) + (dg * dg) + (db * db);

                    // Jika berada dalam toleransi → tampilkan warna asli.
                    if (distSquared <= thresholdSquared)
                    {
                        buffer[offset] = b;
                        buffer[offset + 1] = g;
                        buffer[offset + 2] = r;
                        buffer[offset + 3] = a;
                    }
                    else
                    {
                        // Jika di luar toleransi → ubah ke grayscale dan gelapkan.
                        byte gray = cache[x, y, 3];
                        byte dimmed = (byte)Math.Clamp((int)Math.Round(gray * dimFactor), 0, 255);
                        buffer[offset] = dimmed;
                        buffer[offset + 1] = dimmed;
                        buffer[offset + 2] = dimmed;
                        buffer[offset + 3] = a;
                    }
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }

        // Mengubah nilai toleransi seleksi warna (0-100%) dan menghitung ulang hasil seleksi.
        public BitmapSource UpdateTolerance(double tolerancePercent)
        {
            State.ColorSelection.TolerancePercent = Math.Clamp(tolerancePercent, 0, 100);

            if (!State.ColorSelection.IsActive)
            {
                return GetProcessedBitmap();
            }

            return GetProcessedBitmap();
        }
    }
}
