using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Snapshot bitmap asli sebelum operasi rotasi pertama,
        // supaya nanti bisa dikembalikan lewat RestoreOriginal.
        private BitmapSource? _rotationOriginalBitmap;
        private string? _rotationOriginalLabel;

        // Wrapper untuk rotasi 45°: simpan snapshot dulu, lalu delegasikan ke ApplyRotation.
        public BitmapSource Rotate45()
        {
            CaptureRotationSnapshot();
            return ApplyRotation(45);
        }

        // Wrapper untuk rotasi 90° (dipakai tombol preset di UI).
        public BitmapSource Rotate90()
        {
            CaptureRotationSnapshot();
            return ApplyRotation(90);
        }

        // Wrapper untuk rotasi 180°.
        public BitmapSource Rotate180()
        {
            CaptureRotationSnapshot();
            return ApplyRotation(180);
        }

        // Wrapper untuk rotasi 270°.
        public BitmapSource Rotate270()
        {
            CaptureRotationSnapshot();
            return ApplyRotation(270);
        }

        // Rotasi dengan sudut bebas (custom) yang dikirim dari UI.
        public BitmapSource RotateCustom(double degrees)
        {
            CaptureRotationSnapshot();
            return ApplyRotation(degrees);
        }

        // Inti logic pemilihan jenis rotasi:
        // - Normalisasi sudut ke range 0–360.
        // - Untuk 90/180/270 gunakan RotateOptimized (tanpa interpolasi, hanya remap index pixel).
        // - Untuk sudut lain gunakan RotateGeneral (bilinear interpolation).
        private BitmapSource ApplyRotation(double degrees)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk dirotasi.");
            }

            // Jika sudah ada snapshot rotasi, gunakan itu sebagai basis,
            // kalau belum, pakai State.OriginalBitmap saat ini.
            BitmapSource? baseBitmap = _rotationOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk rotasi.");
            }

            // Pastikan format Bgra32 agar akses buffer byte konsisten.
            BitmapSource source = EnsureBgra32(baseBitmap);
            
            // Normalize angle to 0-360
            double normalizedAngle = degrees % 360;
            if (normalizedAngle < 0) normalizedAngle += 360;

            // For 90, 180, 270 degrees, use optimized rotation
            if (Math.Abs(normalizedAngle - 90) < 0.01)
            {
                return RotateOptimized(source, 90);
            }
            else if (Math.Abs(normalizedAngle - 180) < 0.01)
            {
                return RotateOptimized(source, 180);
            }
            else if (Math.Abs(normalizedAngle - 270) < 0.01)
            {
                return RotateOptimized(source, 270);
            }
            else
            {
                // Untuk sudut selain 90/180/270, gunakan algoritma umum
                // dengan interpolasi bilinear untuk mengurangi jaggies.
                return RotateGeneral(source, degrees);
            }
        }

        // Rotasi teroptimasi untuk 90/180/270 derajat.
        // Di sini tidak ada interpolasi: hanya hitung ulang posisi (x,y)
        // ke koordinat baru dan copy 4 byte pixel apa adanya.
        private BitmapSource RotateOptimized(BitmapSource source, int degrees)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] sourceBuffer = new byte[stride * height];
            source.CopyPixels(sourceBuffer, stride, 0);

            byte[] resultBuffer;
            int resultWidth, resultHeight, resultStride;

            if (degrees == 90)
            {
                resultWidth = height;
                resultHeight = width;
                resultStride = resultWidth * 4;
                resultBuffer = new byte[resultStride * resultHeight];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Hitung index sumber (x,y) pada buffer Bgra32.
                        int srcIndex = y * stride + x * 4;
                        // Pemetaan koordinat baru untuk rotasi 90° searah jarum jam.
                        int dstX = height - 1 - y;
                        int dstY = x;
                        int dstIndex = dstY * resultStride + dstX * 4;

                        resultBuffer[dstIndex] = sourceBuffer[srcIndex];
                        resultBuffer[dstIndex + 1] = sourceBuffer[srcIndex + 1];
                        resultBuffer[dstIndex + 2] = sourceBuffer[srcIndex + 2];
                        resultBuffer[dstIndex + 3] = sourceBuffer[srcIndex + 3];
                    }
                }
            }
            else if (degrees == 180)
            {
                resultWidth = width;
                resultHeight = height;
                resultStride = resultWidth * 4;
                resultBuffer = new byte[resultStride * resultHeight];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex = y * stride + x * 4;
                        // Rotasi 180° = mirror horizontal + vertical.
                        int dstX = width - 1 - x;
                        int dstY = height - 1 - y;
                        int dstIndex = dstY * resultStride + dstX * 4;

                        resultBuffer[dstIndex] = sourceBuffer[srcIndex];
                        resultBuffer[dstIndex + 1] = sourceBuffer[srcIndex + 1];
                        resultBuffer[dstIndex + 2] = sourceBuffer[srcIndex + 2];
                        resultBuffer[dstIndex + 3] = sourceBuffer[srcIndex + 3];
                    }
                }
            }
            else // 270
            {
                resultWidth = height;
                resultHeight = width;
                resultStride = resultWidth * 4;
                resultBuffer = new byte[resultStride * resultHeight];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex = y * stride + x * 4;
                        // Pemetaan koordinat untuk rotasi 270° (atau 90° CCW).
                        int dstX = y;
                        int dstY = width - 1 - x;
                        int dstIndex = dstY * resultStride + dstX * 4;

                        resultBuffer[dstIndex] = sourceBuffer[srcIndex];
                        resultBuffer[dstIndex + 1] = sourceBuffer[srcIndex + 1];
                        resultBuffer[dstIndex + 2] = sourceBuffer[srcIndex + 2];
                        resultBuffer[dstIndex + 3] = sourceBuffer[srcIndex + 3];
                    }
                }
            }

            // Bungkus kembali buffer hasil ke BitmapSource dan update workspace.
            BitmapSource result = CreateBitmapFromBuffer(resultBuffer, resultWidth, resultHeight);
            string label = $"Rotasi_{degrees}.png";
            return ReplaceWorkspaceBitmapForRotation(result, label);
        }

        // Rotasi general untuk sudut bebas menggunakan backward mapping + bilinear interpolation.
        // Backward mapping: untuk setiap pixel di citra hasil, cari koordinat asal di citra sumber.
        // Bilinear: ambil 4 tetangga terdekat lalu rata-ratakan dengan bobot (fx, fy).
        private BitmapSource RotateGeneral(BitmapSource source, double degrees)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] sourceBuffer = new byte[stride * height];
            source.CopyPixels(sourceBuffer, stride, 0);

            // Convert degrees to radians
            double radians = degrees * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            // Hitung bounding box citra hasil setelah rotasi
            Point[] corners = new Point[]
            {
                new Point(0, 0),
                new Point(width, 0),
                new Point(width, height),
                new Point(0, height)
            };

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var corner in corners)
            {
                double rotX = corner.X * cos - corner.Y * sin;
                double rotY = corner.X * sin + corner.Y * cos;
                minX = Math.Min(minX, rotX);
                maxX = Math.Max(maxX, rotX);
                minY = Math.Min(minY, rotY);
                maxY = Math.Max(maxY, rotY);
            }

            int resultWidth = (int)Math.Ceiling(maxX - minX);
            int resultHeight = (int)Math.Ceiling(maxY - minY);
            int resultStride = resultWidth * 4;
            byte[] resultBuffer = new byte[resultStride * resultHeight];

            // Center of original image
            double centerX = width / 2.0;
            double centerY = height / 2.0;

            // Center of result image
            double resultCenterX = resultWidth / 2.0;
            double resultCenterY = resultHeight / 2.0;

            // Inisialisasi hasil dengan pixel transparan.
            for (int i = 0; i < resultBuffer.Length; i += 4)
            {
                resultBuffer[i + 3] = 0; // Transparent
            }

            // Backward mapping dengan bilinear interpolation
            for (int dstY = 0; dstY < resultHeight; dstY++)
            {
                for (int dstX = 0; dstX < resultWidth; dstX++)
                {
                    // Transform ke koordinat terpusat (origin di tengah gambar hasil).
                    double x = dstX - resultCenterX;
                    double y = dstY - resultCenterY;

                    // Inverse rotation: dari titik di citra hasil kembali ke posisi di citra sumber.
                    double srcX = x * cos + y * sin + centerX;
                    double srcY = -x * sin + y * cos + centerY;

                    // Pastikan koordinat sumber masih di dalam citra input.
                    if (srcX >= 0 && srcX < width - 1 && srcY >= 0 && srcY < height - 1)
                    {
                        // Bilinear interpolation
                        int x0 = (int)Math.Floor(srcX);
                        int y0 = (int)Math.Floor(srcY);
                        int x1 = x0 + 1;
                        int y1 = y0 + 1;

                        double fx = srcX - x0; // fraksi di sumbu X
                        double fy = srcY - y0; // fraksi di sumbu Y

                        int dstIndex = dstY * resultStride + dstX * 4;

                        for (int c = 0; c < 4; c++)
                        {
                            int idx00 = y0 * stride + x0 * 4 + c;
                            int idx10 = y0 * stride + x1 * 4 + c;
                            int idx01 = y1 * stride + x0 * 4 + c;
                            int idx11 = y1 * stride + x1 * 4 + c;

                            // Kombinasi linear dari 4 tetangga (00,10,01,11)
                            // dengan bobot sesuai jarak ke koordinat sumber sebenarnya.
                            double val = (1 - fx) * (1 - fy) * sourceBuffer[idx00] +
                                       fx * (1 - fy) * sourceBuffer[idx10] +
                                       (1 - fx) * fy * sourceBuffer[idx01] +
                                       fx * fy * sourceBuffer[idx11];

                            resultBuffer[dstIndex + c] = (byte)Math.Clamp(val, 0, 255);
                        }
                    }
                }
            }

            // Bungkus buffer hasil ke BitmapSource dan update workspace.
            BitmapSource result = CreateBitmapFromBuffer(resultBuffer, resultWidth, resultHeight);
            string label = $"Rotasi_{degrees:F1}.png";
            return ReplaceWorkspaceBitmapForRotation(result, label);
        }

        // Mengganti workspace dengan bitmap hasil rotasi:
        // - Reset state,
        // - set OriginalBitmap + FilterCache + PixelCache baru,
        // - rebuild previews dan histogram,
        // - lalu kembalikan processed bitmap (siap tampil di UI).
        private BitmapSource ReplaceWorkspaceBitmapForRotation(BitmapSource bitmap, string label)
        {
            State.Reset();

            State.OriginalBitmap = bitmap;
            State.FilterCache[Core.Enums.ImageFilterMode.Original] = bitmap;
            State.PixelCache = ExtractPixelCache(bitmap);
            State.CachedWidth = bitmap.PixelWidth;
            State.CachedHeight = bitmap.PixelHeight;
            State.CurrentFilePath = label;
            State.ActiveFilter = Core.Enums.ImageFilterMode.Original;

            BuildPreviews();
            Build();

            return GetProcessedBitmap();
        }

        // Mengembalikan gambar ke kondisi sebelum rotasi pertama kali dilakukan
        // menggunakan snapshot yang disimpan di _rotationOriginalBitmap.
        public BitmapSource RestoreOriginal()
        {
            if (_rotationOriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar awal yang tersimpan untuk dipulihkan.");
            }

            BitmapSource original = _rotationOriginalBitmap;
            string label = _rotationOriginalLabel ?? "Gambar.png";

            BitmapSource result = ReplaceWorkspaceBitmapForRotation(original, label);
            _rotationOriginalBitmap = null;
            _rotationOriginalLabel = null;
            return result;
        }

        // Menghapus snapshot rotasi tanpa mengubah gambar yang sedang tampil.
        public void ClearRotationSnapshot()
        {
            _rotationOriginalBitmap = null;
            _rotationOriginalLabel = null;
        }

        // Menyimpan sekali saja snapshot gambar sebelum rotasi,
        // supaya operasi rotasi berikutnya selalu merujuk ke gambar awal.
        private void CaptureRotationSnapshot()
        {
            if (_rotationOriginalBitmap != null)
            {
                return;
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk operasi rotasi.");
            }

            // Clone + Freeze supaya snapshot aman dipakai dari thread manapun
            // dan tidak terpengaruh perubahan berikutnya di workspace.
            BitmapSource snapshot = State.OriginalBitmap.Clone();
            snapshot.Freeze();
            _rotationOriginalBitmap = snapshot;
            _rotationOriginalLabel = State.CurrentFilePath;
        }
    }
}
