using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        private BitmapSource _binaryOriginalBitmap;
        private string _binaryOriginalLabel;

        // Mengonversi gambar (base atau snapshot binary) menjadi citra biner
        // menggunakan threshold: gray >= threshold → putih, selain itu hitam.
        public BitmapSource ToBinary(int threshold)
        {
            CaptureBinarySnapshot();

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk konversi biner.");
            }

            BitmapSource baseBitmap = _binaryOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk konversi biner.");
            }

            // Pastikan format pixel konsisten (Bgra32) untuk mempermudah akses buffer.
            BitmapSource source = EnsureBgra32(baseBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            source.CopyPixels(buffer, stride, 0);

            for (int i = 0; i < buffer.Length; i += 4)
            {
                // Konversi ke grayscale terlebih dahulu dengan bobot luminance.
                byte gray = (byte)((buffer[i + 2] * 0.299) + (buffer[i + 1] * 0.587) + (buffer[i] * 0.114));
                
                // Terapkan threshold: jika gray >= threshold, pixel menjadi putih, jika tidak hitam.
                byte binary = (byte)(gray >= threshold ? 255 : 0);
                
                buffer[i] = binary;      // B
                buffer[i + 1] = binary;  // G
                buffer[i + 2] = binary;  // R
                // Alpha channel (i + 3) remains unchanged
            }

            BitmapSource result = CreateBitmapFromBuffer(buffer, width, height);
            return ReplaceBinaryWorkspaceBitmap(result, "Citra_Biner.png");
        }

        // Operasi AND biner antar gambar base vs overlay.
        public BitmapSource AndImage(BitmapSource overlay, int offsetX, int offsetY)
        {
            CaptureBinarySnapshot();
            return ApplyBinaryOperation(overlay, offsetX, offsetY, BinaryOperation.And);
        }

        // Operasi OR biner antar gambar base vs overlay.
        public BitmapSource OrImage(BitmapSource overlay, int offsetX, int offsetY)
        {
            CaptureBinarySnapshot();
            return ApplyBinaryOperation(overlay, offsetX, offsetY, BinaryOperation.Or);
        }

        // Operasi NOT biner pada gambar base (menggunakan gambar yang sudah di-threshold).
        public BitmapSource NotImage()
        {
            CaptureBinarySnapshot();

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk operasi NOT.");
            }

            // Gunakan snapshot binary jika sudah ada, kalau tidak pakai gambar asli.
            BitmapSource baseBitmap = _binaryOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk operasi NOT.");
            }

            BitmapSource source = EnsureBgra32(baseBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            source.CopyPixels(buffer, stride, 0);

            for (int i = 0; i < buffer.Length; i += 4)
            {
                // Konversi ke boolean berdasarkan threshold (>=128 → true, <128 → false)
                bool val = buffer[i] >= 128;
                // Invert boolean
                bool resultVal = !val;
                // Konversi kembali ke byte (true → 255, false → 0)
                byte resultByte = (byte)(resultVal ? 255 : 0);
                
                buffer[i] = resultByte;         // B
                buffer[i + 1] = resultByte;     // G
                buffer[i + 2] = resultByte;     // R
                buffer[i + 3] = 255;            // A
            }

            BitmapSource result = CreateBitmapFromBuffer(buffer, width, height);
            return ReplaceBinaryWorkspaceBitmap(result, "Hasil_NOT.png");
        }

        // Operasi XOR biner antar gambar base vs overlay.
        public BitmapSource XorImage(BitmapSource overlay, int offsetX, int offsetY)
        {
            CaptureBinarySnapshot();
            return ApplyBinaryOperation(overlay, offsetX, offsetY, BinaryOperation.Xor);
        }

        // Implementasi inti operasi AND/OR/XOR biner dengan memperhitungkan offset dan overlap.
        private BitmapSource ApplyBinaryOperation(BitmapSource overlay, int offsetX, int offsetY, BinaryOperation operation)
        {
            if (overlay == null)
            {
                throw new ArgumentNullException(nameof(overlay));
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar utama untuk operasi biner.");
            }

            // Pilih gambar dasar: snapshot binary jika ada, kalau tidak gambar asli.
            BitmapSource baseBitmap = _binaryOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk operasi biner.");
            }

            BitmapSource baseSource = EnsureBgra32(baseBitmap);
            BitmapSource overlaySource = EnsureBgra32(overlay);

            int baseWidth = baseSource.PixelWidth;
            int baseHeight = baseSource.PixelHeight;
            int overlayWidth = overlaySource.PixelWidth;
            int overlayHeight = overlaySource.PixelHeight;

            int baseStride = baseWidth * 4;
            int overlayStride = overlayWidth * 4;

            byte[] baseBuffer = new byte[baseStride * baseHeight];
            byte[] overlayBuffer = new byte[overlayStride * overlayHeight];
            baseSource.CopyPixels(baseBuffer, baseStride, 0);
            overlaySource.CopyPixels(overlayBuffer, overlayStride, 0);

            // Hitung area overlap antara base dan overlay berdasarkan offset.
            int overlapX1 = Math.Max(0, offsetX);
            int overlapY1 = Math.Max(0, offsetY);
            int overlapX2 = Math.Min(baseWidth, offsetX + overlayWidth);
            int overlapY2 = Math.Min(baseHeight, offsetY + overlayHeight);

            int resultWidth = baseWidth;
            int resultHeight = baseHeight;
            int resultStride = resultWidth * 4;
            byte[] resultBuffer = new byte[resultStride * resultHeight];

            // Proses semua pixel hasil.
            for (int y = 0; y < resultHeight; y++)
            {
                for (int x = 0; x < resultWidth; x++)
                {
                    int baseIndex = y * baseStride + x * 4;
                    int resultIndex = y * resultStride + x * 4;

                    // Cek apakah (x,y) berada di area overlap.
                    if (x >= overlapX1 && x < overlapX2 && y >= overlapY1 && y < overlapY2)
                    {
                        int overlayX = x - offsetX;
                        int overlayY = y - offsetY;
                        int overlayIndex = overlayY * overlayStride + overlayX * 4;

                        // Konversi nilai intensitas (0–255) menjadi boolean (>=128 → true).
                        bool baseVal = baseBuffer[baseIndex] >= 128;
                        bool overlayVal = overlayBuffer[overlayIndex] >= 128;
                        bool resultVal;

                        switch (operation)
                        {
                            case BinaryOperation.And:
                                resultVal = baseVal && overlayVal;
                                break;
                            case BinaryOperation.Or:
                                resultVal = baseVal || overlayVal;
                                break;
                            case BinaryOperation.Xor:
                                resultVal = baseVal ^ overlayVal;
                                break;
                            default:
                                resultVal = false;
                                break;
                        }

                        // Konversi kembali boolean ke byte 0 atau 255.
                        byte resultByte = (byte)(resultVal ? 255 : 0);
                        resultBuffer[resultIndex] = resultByte;     // B
                        resultBuffer[resultIndex + 1] = resultByte; // G
                        resultBuffer[resultIndex + 2] = resultByte; // R
                        resultBuffer[resultIndex + 3] = 255;        // A
                    }
                    else
                    {
                        // Di luar overlap: set ke hitam (0,0,0) dengan alpha penuh.
                        resultBuffer[resultIndex] = 0;
                        resultBuffer[resultIndex + 1] = 0;
                        resultBuffer[resultIndex + 2] = 0;
                        resultBuffer[resultIndex + 3] = 255;
                    }
                }
            }

            BitmapSource result = CreateBitmapFromBuffer(resultBuffer, resultWidth, resultHeight);
            string label = operation switch
            {
                BinaryOperation.And => "Hasil_AND.png",
                BinaryOperation.Or => "Hasil_OR.png",
                BinaryOperation.Xor => "Hasil_XOR.png",
                _ => "Hasil_Boolean.png"
            };

            return ReplaceBinaryWorkspaceBitmap(result, label);
        }

        private BitmapSource ReplaceBinaryWorkspaceBitmap(BitmapSource bitmap, string label)
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

        public BitmapSource RestoreBinaryBase()
        {
            if (_binaryOriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar awal yang tersimpan untuk dipulihkan.");
            }

            BitmapSource original = _binaryOriginalBitmap;
            string label = _binaryOriginalLabel ?? "Gambar.png";

            BitmapSource result = ReplaceBinaryWorkspaceBitmap(original, label);
            _binaryOriginalBitmap = null;
            _binaryOriginalLabel = null;
            return result;
        }

        public void ClearBinarySnapshot()
        {
            _binaryOriginalBitmap = null;
            _binaryOriginalLabel = null;
        }

        private void CaptureBinarySnapshot()
        {
            if (_binaryOriginalBitmap != null)
            {
                return;
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar utama untuk operasi biner.");
            }

            BitmapSource snapshot = State.OriginalBitmap.Clone();
            snapshot.Freeze();
            _binaryOriginalBitmap = snapshot;
            _binaryOriginalLabel = State.CurrentFilePath;
        }

        private enum BinaryOperation
        {
            And,
            Or,
            Xor
        }
    }
}
