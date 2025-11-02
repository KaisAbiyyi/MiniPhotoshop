using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public BitmapSource CreateAverageGrayscale()
        {
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
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];
                    byte a = cache[x, y, 4];

                    byte gray = (byte)((r + g + b) / 3);
                    buffer[offset] = gray;
                    buffer[offset + 1] = gray;
                    buffer[offset + 2] = gray;
                    buffer[offset + 3] = a;
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }

        public BitmapSource CreateLuminanceGrayscale()
        {
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
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];
                    byte a = cache[x, y, 4];

                    double luminance = 0.299 * r + 0.587 * g + 0.114 * b;
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

