using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Mengaktifkan atau menonaktifkan mode binary threshold.
        // Jika status tidak berubah, cukup kembalikan processed bitmap saat ini.
        public BitmapSource SetBinaryThresholdActive(bool isActive)
        {
            if (State.IsBinaryThresholdActive == isActive)
            {
                return GetProcessedBitmap();
            }

            State.IsBinaryThresholdActive = isActive;
            return GetProcessedBitmap();
        }

        // Mengubah nilai threshold (0–255) dan membangun ulang processed bitmap.
        public BitmapSource UpdateThreshold(int thresholdValue)
        {
            State.BinaryThresholdValue = Math.Clamp(thresholdValue, 0, 255);
            return GetProcessedBitmap();
        }

        // Menerapkan operasi binary threshold pada sebuah bitmap:
        // 1) konversi ke grayscale, 2) bandingkan dengan threshold,
        // 3) hasilkan citra hitam/putih (dengan efek inversi).
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
                    // Baca channel B,G,R,A dari buffer.
                    byte b = buffer[offset];
                    byte g = buffer[offset + 1];
                    byte r = buffer[offset + 2];
                    byte a = buffer[offset + 3];

                    // Konversi ke grayscale sederhana dengan rata-rata.
                    byte gray = (byte)((r + g + b) / 3);
                    // Jika gray > threshold → piksel dianggap putih, kalau tidak → hitam.
                    byte binary = (byte)(gray > threshold ? 255 : 0);
                    // Di sini hasilnya kemudian di-invers (255-binary) untuk efek visual tertentu.
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

