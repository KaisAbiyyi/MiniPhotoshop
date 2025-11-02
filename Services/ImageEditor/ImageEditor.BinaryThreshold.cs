using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public BitmapSource SetActive(bool isActive)
        {
            if (State.IsBinaryThresholdActive == isActive)
            {
                return GetProcessedBitmap();
            }

            State.IsBinaryThresholdActive = isActive;
            return GetProcessedBitmap();
        }

        public BitmapSource UpdateThreshold(int thresholdValue)
        {
            State.BinaryThresholdValue = Math.Clamp(thresholdValue, 0, 255);
            return GetProcessedBitmap();
        }

        private static BitmapSource ApplyBinaryThreshold(BitmapSource source, int threshold)
        {
            BitmapSource normalized = EnsureBgra32(source);
            int width = normalized.PixelWidth;
            int height = normalized.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            normalized.CopyPixels(buffer, stride, 0);

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

                    byte gray = (byte)((r + g + b) / 3);
                    byte binary = (byte)(gray > threshold ? 255 : 0);
                    byte negated = (byte)(255 - binary);

                    buffer[offset] = negated;
                    buffer[offset + 1] = negated;
                    buffer[offset + 2] = negated;
                    buffer[offset + 3] = a;
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}

