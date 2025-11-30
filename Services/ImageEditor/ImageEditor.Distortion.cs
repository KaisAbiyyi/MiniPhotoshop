using System;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Snapshot bitmap asli sebelum operasi distorsi pertama,
        // supaya nanti bisa dikembalikan lewat RestoreDistortion.
        private BitmapSource? _distortionOriginalBitmap;
        private string? _distortionOriginalLabel;

        /// <summary>
        /// Menerapkan distorsi pada citra dengan level tertentu (1-10).
        /// Algoritma: Pixel Displacement dengan Random Offset.
        /// 
        /// Rumus:
        ///   newX = x + random(-level, level)
        ///   newY = y + random(-level, level)
        ///   result[x, y] = source[newX, newY] (jika dalam batas)
        /// 
        /// Level menentukan seberapa jauh pixel bisa bergeser dari posisi aslinya.
        /// </summary>
        public BitmapSource ApplyDistortion(int level)
        {
            CaptureDistortionSnapshot();

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk distorsi.");
            }

            // Gunakan snapshot distorsi jika sudah ada, kalau tidak pakai gambar asli.
            BitmapSource baseBitmap = _distortionOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk distorsi.");
            }

            // Clamp level antara 1-50
            level = Math.Clamp(level, 1, 50);

            BitmapSource source = EnsureBgra32(baseBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] sourceBuffer = new byte[stride * height];
            byte[] resultBuffer = new byte[stride * height];
            source.CopyPixels(sourceBuffer, stride, 0);

            Random random = new Random();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int resultIndex = y * stride + x * 4;

                    // Hitung offset acak berdasarkan level
                    int offsetX = random.Next(-level, level + 1);
                    int offsetY = random.Next(-level, level + 1);

                    // Hitung posisi sumber baru
                    int srcX = x + offsetX;
                    int srcY = y + offsetY;

                    // Clamp ke dalam batas gambar
                    srcX = Math.Clamp(srcX, 0, width - 1);
                    srcY = Math.Clamp(srcY, 0, height - 1);

                    int sourceIndex = srcY * stride + srcX * 4;

                    // Copy pixel dari posisi sumber ke posisi hasil
                    resultBuffer[resultIndex] = sourceBuffer[sourceIndex];         // B
                    resultBuffer[resultIndex + 1] = sourceBuffer[sourceIndex + 1]; // G
                    resultBuffer[resultIndex + 2] = sourceBuffer[sourceIndex + 2]; // R
                    resultBuffer[resultIndex + 3] = sourceBuffer[sourceIndex + 3]; // A
                }
            }

            BitmapSource result = CreateBitmapFromBuffer(resultBuffer, width, height);
            string label = $"Distorsi_Level{level}.png";
            return ReplaceDistortionWorkspaceBitmap(result, label);
        }

        // Mengganti workspace dengan bitmap hasil distorsi.
        private BitmapSource ReplaceDistortionWorkspaceBitmap(BitmapSource bitmap, string label)
        {
            State.Reset();

            State.OriginalBitmap = bitmap;
            State.FilterCache[ImageFilterMode.Original] = bitmap;
            State.PixelCache = ExtractPixelCache(bitmap);
            State.CachedWidth = bitmap.PixelWidth;
            State.CachedHeight = bitmap.PixelHeight;
            State.CurrentFilePath = label;
            State.ActiveFilter = ImageFilterMode.Original;

            BuildPreviews();
            Build();

            return GetProcessedBitmap();
        }

        // Mengembalikan gambar ke kondisi sebelum distorsi pertama kali dilakukan.
        public BitmapSource RestoreDistortion()
        {
            if (_distortionOriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar awal yang tersimpan untuk dipulihkan.");
            }

            BitmapSource original = _distortionOriginalBitmap;
            string label = _distortionOriginalLabel ?? "Gambar.png";

            BitmapSource result = ReplaceDistortionWorkspaceBitmap(original, label);
            _distortionOriginalBitmap = null;
            _distortionOriginalLabel = null;
            return result;
        }

        // Menghapus snapshot distorsi tanpa mengubah gambar yang sedang tampil.
        public void ClearDistortionSnapshot()
        {
            _distortionOriginalBitmap = null;
            _distortionOriginalLabel = null;
        }

        // Menyimpan sekali saja snapshot gambar sebelum distorsi.
        private void CaptureDistortionSnapshot()
        {
            if (_distortionOriginalBitmap != null)
            {
                return;
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk operasi distorsi.");
            }

            BitmapSource snapshot = State.OriginalBitmap.Clone();
            snapshot.Freeze();
            _distortionOriginalBitmap = snapshot;
            _distortionOriginalLabel = State.CurrentFilePath;
        }
    }
}
