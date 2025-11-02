using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
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

        private static byte[,,] ExtractPixelCache(BitmapSource bitmap)
        {
            BitmapSource normalized = EnsureBgra32(bitmap);
            int width = normalized.PixelWidth;
            int height = normalized.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            normalized.CopyPixels(buffer, stride, 0);

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
                    byte gray = (byte)Math.Clamp(0.2126 * r + 0.7152 * g + 0.0722 * b, 0, 255);

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

