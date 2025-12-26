using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Memastikan bitmap dalam format Bgra32 (4 byte per pixel: B,G,R,A)
        // agar akses byte konsisten di seluruh operasi (filter, histogram, rotasi, dll.).
        private static BitmapSource EnsureBgra32(BitmapSource source)
        {
            if (source.Format == PixelFormats.Bgra32)
            {
                return source;
            }

            var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            converted.Freeze();
            return converted;
        }

        // Mengekstrak semua pixel ke dalam array 3D: [x, y, channel].
        // Struktur channel:
        //   0 = R, 1 = G, 2 = B, 3 = Gray (luminance), 4 = A (alpha).
        // Method ini adalah dasar untuk fitur-fitur lain (filter, histogram, dll.).
        private static byte[,,] ExtractPixelCache(BitmapSource bitmap)
        {
            // Normalisasi format dulu supaya pasti Bgra32.
            BitmapSource normalized = EnsureBgra32(bitmap);
            int width = normalized.PixelWidth;
            int height = normalized.PixelHeight;
            int stride = width * 4; // 4 byte per pixel.
            byte[] buffer = new byte[stride * height];

            // Copy seluruh pixel ke buffer 1D (B,G,R,A berurutan per pixel).
            normalized.CopyPixels(buffer, stride, 0);

            // cache[x, y, channel] menyimpan data per pixel dalam bentuk 3D.
            var cache = new byte[width, height, 5];
            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = rowOffset + (x * 4);
                    byte b = buffer[offset];
                    byte g = buffer[offset + 1];
                    byte r = buffer[offset + 2];
                    byte a = buffer[offset + 3];

                    // Hitung luminance (gray) dari R,G,B dengan koefisien standar.
                    byte gray = (byte)Math.Clamp(0.2126 * r + 0.7152 * g + 0.0722 * b, 0, 255);

                    // Simpan R,G,B,Gray,A ke dalam PixelCache 3D.
                    cache[x, y, 0] = r;
                    cache[x, y, 1] = g;
                    cache[x, y, 2] = b;
                    cache[x, y, 3] = gray;
                    cache[x, y, 4] = a;
                }
            }

            return cache;
        }

        private static BitmapSource CreateBitmapFromBuffer(byte[] buffer, int width, int height)
        {
            int stride = width * 4;
            var writeable = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            writeable.WritePixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);
            writeable.Freeze();
            return writeable;
        }

        private static BitmapSource CreateThumbnail(BitmapSource source, int maxDimension)
        {
            if (source.PixelWidth <= maxDimension && source.PixelHeight <= maxDimension)
            {
                return source;
            }

            double scale = Math.Min((double)maxDimension / source.PixelWidth, (double)maxDimension / source.PixelHeight);
            if (scale <= 0 || Math.Abs(scale - 1.0) < 0.0001)
            {
                return source;
            }

            var transformed = new TransformedBitmap(source, new ScaleTransform(scale, scale));
            transformed.Freeze();
            return transformed;
        }
    }
}

