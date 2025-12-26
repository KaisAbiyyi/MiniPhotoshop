using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        // Mengaktifkan / menonaktifkan efek negasi (invert warna) pada pipeline.
        public BitmapSource SetNegationActive(bool isActive)
        {
            if (State.IsNegationActive == isActive)
            {
                return GetProcessedBitmap();
            }

            State.IsNegationActive = isActive;
            // Saat status negasi berubah, buffer brightness di-reset supaya
            // tidak ada kombinasi brightness lama dengan gambar ter-invert.
            State.Brightness.Reset();
            return GetProcessedBitmap();
        }

        // Jika flag IsNegationActive true, terapkan negasi pada bitmap sumber.
        private BitmapSource ApplyNegationIfNeeded(BitmapSource source)
        {
            return State.IsNegationActive ? ApplyNegation(source) : source;
        }

        // Fungsi inti negasi: tiap channel warna diganti menjadi 255 - nilai_lama,
        // alpha dibiarkan apa adanya.
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
                // Urutan byte: [B, G, R, A], hanya B,G,R yang di-invert.
                buffer[i] = (byte)(255 - buffer[i]);         // B
                buffer[i + 1] = (byte)(255 - buffer[i + 1]); // G
                buffer[i + 2] = (byte)(255 - buffer[i + 2]); // R
                // Alpha untouched
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}

