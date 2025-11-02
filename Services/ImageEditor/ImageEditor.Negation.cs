using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public BitmapSource SetActive(bool isActive)
        {
            if (State.IsNegationActive == isActive)
            {
                return GetProcessedBitmap();
            }

            State.IsNegationActive = isActive;
            State.Brightness.Reset();
            return GetProcessedBitmap();
        }

        private BitmapSource ApplyNegationIfNeeded(BitmapSource source)
        {
            return State.IsNegationActive ? ApplyNegation(source) : source;
        }

        private static BitmapSource ApplyNegation(BitmapSource source)
        {
            BitmapSource normalized = EnsureBgra32(source);
            int width = normalized.PixelWidth;
            int height = normalized.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            normalized.CopyPixels(buffer, stride, 0);

            for (int i = 0; i < buffer.Length; i += 4)
            {
                buffer[i] = (byte)(255 - buffer[i]);         // B
                buffer[i + 1] = (byte)(255 - buffer[i + 1]); // G
                buffer[i + 2] = (byte)(255 - buffer[i + 2]); // R
                // Alpha untouched
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}

