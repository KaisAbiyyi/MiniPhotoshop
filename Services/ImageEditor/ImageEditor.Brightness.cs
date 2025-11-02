using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public void Reset()
        {
            State.Brightness.Reset();
        }

        public BitmapSource Update(double newValue)
        {
            if (State.PixelCache == null || State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar yang dimuat.");
            }

            if (State.Brightness.Buffer == null)
            {
                InitializeBrightnessBuffer();
            }

            double delta = newValue - State.Brightness.PreviousValue;

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            var buffer = State.Brightness.Buffer!;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    buffer[x, y, 0] += (int)delta;
                    buffer[x, y, 1] += (int)delta;
                    buffer[x, y, 2] += (int)delta;
                }
            }

            State.Brightness.PreviousValue = newValue;
            return GetProcessedBitmap();
        }

        private void InitializeBrightnessBuffer()
        {
            BitmapSource current = GetFilteredBitmap(State.ActiveFilter);

            if (State.IsNegationActive)
            {
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
                    buffer[x, y, 0] = raw[offset + 2];
                    buffer[x, y, 1] = raw[offset + 1];
                    buffer[x, y, 2] = raw[offset];
                }
            }

            State.Brightness.Buffer = buffer;
            State.Brightness.PreviousValue = 0;
        }

        private BitmapSource ApplyBrightnessIfNeeded(BitmapSource source)
        {
            if (State.Brightness.Buffer == null || Math.Abs(State.Brightness.PreviousValue) < 0.0001)
            {
                return source;
            }

            return CreateBrightnessBitmap();
        }

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
                    int r = Math.Clamp(brightness[x, y, 0], 0, 255);
                    int g = Math.Clamp(brightness[x, y, 1], 0, 255);
                    int b = Math.Clamp(brightness[x, y, 2], 0, 255);

                    buffer[offset] = (byte)b;
                    buffer[offset + 1] = (byte)g;
                    buffer[offset + 2] = (byte)r;
                    buffer[offset + 3] = pixelCache![x, y, 4];
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}

