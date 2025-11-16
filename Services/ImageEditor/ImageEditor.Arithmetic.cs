using System;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        private BitmapSource? _arithmeticOriginalBitmap;
        private string? _arithmeticOriginalLabel;

        // Menjumlahkan gambar overlay ke gambar dasar dengan offset tertentu.
        // Menyimpan snapshot gambar awal sebelum operasi pertama kali dilakukan.
        public BitmapSource AddImage(BitmapSource overlay, int offsetX, int offsetY)
        {
            CaptureArithmeticSnapshot();
            return ApplyArithmeticOperation(overlay, offsetX, offsetY, ArithmeticMode.Add);
        }

        // Mengurangkan gambar overlay dari gambar dasar dengan offset tertentu.
        public BitmapSource SubtractImage(BitmapSource overlay, int offsetX, int offsetY)
        {
            CaptureArithmeticSnapshot();
            return ApplyArithmeticOperation(overlay, offsetX, offsetY, ArithmeticMode.Subtract);
        }

        // Mengalikan semua pixel gambar dasar dengan nilai skalar.
        public BitmapSource MultiplyByScalar(double scalar)
        {
            CaptureArithmeticSnapshot();
            return ApplyScalarOperation(scalar, ArithmeticMode.Multiply);
        }

        // Membagi semua pixel gambar dasar dengan nilai skalar (tidak boleh 0).
        public BitmapSource DivideByScalar(double scalar)
        {
            if (scalar == 0)
            {
                throw new ArgumentException("Tidak dapat membagi dengan nol.", nameof(scalar));
            }

            CaptureArithmeticSnapshot();
            return ApplyScalarOperation(scalar, ArithmeticMode.Divide);
        }

        public BitmapSource MultiplyImage(BitmapSource overlay, int offsetX, int offsetY, out string normalizationInfo)
        {
            CaptureArithmeticSnapshot();
            return ApplyImageMultiplicationDivision(overlay, offsetX, offsetY, true, out normalizationInfo);
        }

        public BitmapSource DivideImage(BitmapSource overlay, int offsetX, int offsetY, out string normalizationInfo)
        {
            CaptureArithmeticSnapshot();
            return ApplyImageMultiplicationDivision(overlay, offsetX, offsetY, false, out normalizationInfo);
        }

        // Implementasi inti operasi skalar pada citra (perkalian/pembagian per channel).
        private BitmapSource ApplyScalarOperation(double scalar, ArithmeticMode mode)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk operasi skalar.");
            }

            BitmapSource? baseBitmap = _arithmeticOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk operasi skalar.");
            }

            BitmapSource source = EnsureBgra32(baseBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            source.CopyPixels(buffer, stride, 0);

            for (int i = 0; i < buffer.Length; i += 4)
            {
                if (mode == ArithmeticMode.Multiply)
                {
                    // Kalikan B,G,R dengan skalar lalu clamp ke 0–255.
                    buffer[i] = (byte)Math.Clamp((int)(buffer[i] * scalar), 0, 255);         // B
                    buffer[i + 1] = (byte)Math.Clamp((int)(buffer[i + 1] * scalar), 0, 255); // G
                    buffer[i + 2] = (byte)Math.Clamp((int)(buffer[i + 2] * scalar), 0, 255); // R
                }
                else if (mode == ArithmeticMode.Divide)
                {
                    // Bagi B,G,R dengan skalar lalu clamp ke 0–255.
                    buffer[i] = (byte)Math.Clamp((int)(buffer[i] / scalar), 0, 255);         // B
                    buffer[i + 1] = (byte)Math.Clamp((int)(buffer[i + 1] / scalar), 0, 255); // G
                    buffer[i + 2] = (byte)Math.Clamp((int)(buffer[i + 2] / scalar), 0, 255); // R
                }
                // Alpha channel (i + 3) tetap tidak berubah
            }

            BitmapSource result = CreateBitmapFromBuffer(buffer, width, height);
            string label = mode == ArithmeticMode.Multiply ? "Hasil_Perkalian.png" : "Hasil_Pembagian.png";
            return ReplaceWorkspaceBitmap(result, label);
        }

        // Operasi perkalian/pembagian antar dua citra (base vs overlay) dengan offset,
        // dihitung dalam float lalu dinormalisasi ke 0–255 per channel.
        private BitmapSource ApplyImageMultiplicationDivision(BitmapSource overlay, int offsetX, int offsetY, bool isMultiply, out string normalizationInfo)
        {
            const double EPSILON = 1e-10;

            if (overlay == null)
            {
                throw new ArgumentNullException(nameof(overlay));
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar utama untuk operasi.");
            }

            BitmapSource? baseBitmap = _arithmeticOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk operasi.");
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
            
            // Buffer sementara (float) untuk menyimpan hasil sebelum dinormalisasi.
            float[] tempR = new float[resultWidth * resultHeight];
            float[] tempG = new float[resultWidth * resultHeight];
            float[] tempB = new float[resultWidth * resultHeight];
            
            // Menyimpan nilai minimum & maksimum per channel untuk keperluan normalisasi.
            float minR = float.MaxValue, maxR = float.MinValue;
            float minG = float.MaxValue, maxG = float.MinValue;
            float minB = float.MaxValue, maxB = float.MinValue;

            // Proses semua pixel pada citra hasil.
            for (int y = 0; y < resultHeight; y++)
            {
                for (int x = 0; x < resultWidth; x++)
                {
                    int resultIndex = y * resultWidth + x;
                    int baseIndex = y * baseStride + x * 4;

                    // Cek apakah koordinat (x,y) berada di area overlap.
                    if (x >= overlapX1 && x < overlapX2 && y >= overlapY1 && y < overlapY2)
                    {
                        int overlayX = x - offsetX;
                        int overlayY = y - offsetY;
                        int overlayIndex = overlayY * overlayStride + overlayX * 4;

                        float baseB = baseBuffer[baseIndex];
                        float baseG = baseBuffer[baseIndex + 1];
                        float baseR = baseBuffer[baseIndex + 2];

                        float overlayB = overlayBuffer[overlayIndex];
                        float overlayG = overlayBuffer[overlayIndex + 1];
                        float overlayR = overlayBuffer[overlayIndex + 2];

                        if (isMultiply)
                        {
                            // Perkalian langsung per channel.
                            tempR[resultIndex] = baseR * overlayR;
                            tempG[resultIndex] = baseG * overlayG;
                            tempB[resultIndex] = baseB * overlayB;
                        }
                        else // Divide
                        {
                            // Pembagian per channel, tambahkan EPSILON untuk menghindari pembagian 0.
                            tempR[resultIndex] = baseR / (overlayR + (float)EPSILON);
                            tempG[resultIndex] = baseG / (overlayG + (float)EPSILON);
                            tempB[resultIndex] = baseB / (overlayB + (float)EPSILON);
                        }

                        // Update nilai min dan max untuk normalisasi.
                        minR = Math.Min(minR, tempR[resultIndex]);
                        maxR = Math.Max(maxR, tempR[resultIndex]);
                        minG = Math.Min(minG, tempG[resultIndex]);
                        maxG = Math.Max(maxG, tempG[resultIndex]);
                        minB = Math.Min(minB, tempB[resultIndex]);
                        maxB = Math.Max(maxB, tempB[resultIndex]);
                    }
                    else
                    {
                        // Di luar overlap: hasil di-set hitam (0,0,0).
                        tempR[resultIndex] = 0;
                        tempG[resultIndex] = 0;
                        tempB[resultIndex] = 0;
                    }
                }
            }

            // Normalisasi nilai float ke rentang 0–255 dan tulis ke buffer byte.
            byte[] resultBuffer = new byte[resultStride * resultHeight];
            
            for (int y = 0; y < resultHeight; y++)
            {
                for (int x = 0; x < resultWidth; x++)
                {
                    int resultIndex = y * resultWidth + x;
                    int bufferIndex = y * resultStride + x * 4;

                    // Normalisasi per channel dari rentang [min,max] ke [0,255].
                    byte normalizedR = NormalizeValue(tempR[resultIndex], minR, maxR);
                    byte normalizedG = NormalizeValue(tempG[resultIndex], minG, maxG);
                    byte normalizedB = NormalizeValue(tempB[resultIndex], minB, maxB);

                    resultBuffer[bufferIndex] = normalizedB;      // B
                    resultBuffer[bufferIndex + 1] = normalizedG;  // G
                    resultBuffer[bufferIndex + 2] = normalizedR;  // R
                    resultBuffer[bufferIndex + 3] = 255;          // A
                }
            }

            // Kembalikan informasi rentang nilai sebelum normalisasi (berguna untuk debug / UI).
            normalizationInfo = $"R: [{minR:F2}, {maxR:F2}] | G: [{minG:F2}, {maxG:F2}] | B: [{minB:F2}, {maxB:F2}]";

            BitmapSource result = CreateBitmapFromBuffer(resultBuffer, resultWidth, resultHeight);
            string label = isMultiply ? "Hasil_Perkalian_Citra.png" : "Hasil_Pembagian_Citra.png";
            return ReplaceWorkspaceBitmap(result, label);
        }

        // Mengubah satu nilai float di rentang [min,max] ke byte 0–255.
        private byte NormalizeValue(float value, float min, float max)
        {
            if (max - min < 1e-6) // Avoid division by zero
            {
                return 0;
            }
            
            float normalized = ((value - min) / (max - min)) * 255f;
            return (byte)Math.Clamp((int)normalized, 0, 255);
        }

        // Implementasi inti operasi penjumlahan/pengurangan antar dua citra.
        // Hasilnya bisa lebih besar dari ukuran salah satu citra karena mempertimbangkan offset.
        private BitmapSource ApplyArithmeticOperation(BitmapSource overlay, int offsetX, int offsetY, ArithmeticMode mode)
        {
            if (overlay == null)
            {
                throw new ArgumentNullException(nameof(overlay));
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar utama untuk operasi aritmetika.");
            }

            BitmapSource? baseBitmap = _arithmeticOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk operasi aritmetika.");
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

            // Hitung dimensi kanvas hasil sehingga bisa memuat kedua citra
            // meskipun overlay bergeser ke kiri/atas (offset negatif).
            int minX = Math.Min(0, offsetX);
            int minY = Math.Min(0, offsetY);
            int baseOffsetX = -minX;
            int baseOffsetY = -minY;
            int overlayOffsetX = offsetX - minX;
            int overlayOffsetY = offsetY - minY;

            int resultWidth = Math.Max(baseOffsetX + baseWidth, overlayOffsetX + overlayWidth);
            int resultHeight = Math.Max(baseOffsetY + baseHeight, overlayOffsetY + overlayHeight);
            int resultStride = resultWidth * 4;
            byte[] resultBuffer = new byte[resultStride * resultHeight];

            for (int y = 0; y < resultHeight; y++)
            {
                int resultRow = y * resultStride;
                int baseY = y - baseOffsetY;
                int overlayY = y - overlayOffsetY;
                bool baseRowValid = baseY >= 0 && baseY < baseHeight;
                bool overlayRowValid = overlayY >= 0 && overlayY < overlayHeight;

                for (int x = 0; x < resultWidth; x++)
                {
                    int resultIndex = resultRow + (x * 4);
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    byte alpha = 0;
                    bool hasBase = false;
                    bool hasOverlay = false;

                    if (baseRowValid)
                    {
                        int baseX = x - baseOffsetX;
                        if (baseX >= 0 && baseX < baseWidth)
                        {
                            int baseIndex = baseY * baseStride + baseX * 4;
                            b = baseBuffer[baseIndex];
                            g = baseBuffer[baseIndex + 1];
                            r = baseBuffer[baseIndex + 2];
                            alpha = baseBuffer[baseIndex + 3];
                            hasBase = true;
                        }
                    }

                    if (overlayRowValid)
                    {
                        int overlayX = x - overlayOffsetX;
                        if (overlayX >= 0 && overlayX < overlayWidth)
                        {
                            int overlayIndex = overlayY * overlayStride + overlayX * 4;
                            int overlayB = overlayBuffer[overlayIndex];
                            int overlayG = overlayBuffer[overlayIndex + 1];
                            int overlayR = overlayBuffer[overlayIndex + 2];
                            byte overlayAlpha = overlayBuffer[overlayIndex + 3];

                            if (mode == ArithmeticMode.Add)
                            {
                                // Jika mode Add, jumlahkan channel base dan overlay.
                                b += overlayB;
                                g += overlayG;
                                r += overlayR;
                            }
                            else
                            {
                                // Jika mode Subtract, kurangi channel overlay dari base.
                                b -= overlayB;
                                g -= overlayG;
                                r -= overlayR;
                            }

                            alpha = hasBase ? (byte)Math.Max(alpha, overlayAlpha) : overlayAlpha;
                            hasOverlay = true;
                        }
                    }

                    if (!hasBase && !hasOverlay)
                    {
                        alpha = 255;
                    }

                    resultBuffer[resultIndex] = (byte)Math.Clamp(b, 0, 255);
                    resultBuffer[resultIndex + 1] = (byte)Math.Clamp(g, 0, 255);
                    resultBuffer[resultIndex + 2] = (byte)Math.Clamp(r, 0, 255);
                    resultBuffer[resultIndex + 3] = alpha;
                }
            }

            BitmapSource result = CreateBitmapFromBuffer(resultBuffer, resultWidth, resultHeight);
            string label = mode == ArithmeticMode.Add ? "Hasil_Penjumlahan.png" : "Hasil_Pengurangan.png";
            return ReplaceWorkspaceBitmap(result, label);
        }

        private BitmapSource ReplaceWorkspaceBitmap(BitmapSource bitmap, string label)
        {
            State.Reset();

            State.OriginalBitmap = bitmap;
            State.FilterCache[ImageFilterMode.Original] = bitmap;
            State.PixelCache = ExtractPixelCache(bitmap);
            State.CachedWidth = bitmap.PixelWidth;
            State.CachedHeight = bitmap.PixelHeight;
            State.CurrentFilePath = label;
            State.ActiveFilter = ImageFilterMode.Original;

            BuildPreviews();
            Build();

            return GetProcessedBitmap();
        }

        public BitmapSource RestoreArithmeticBase()
        {
            if (_arithmeticOriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar awal yang tersimpan untuk dipulihkan.");
            }

            BitmapSource original = _arithmeticOriginalBitmap;
            string label = _arithmeticOriginalLabel ?? "Gambar.png";

            BitmapSource result = ReplaceWorkspaceBitmap(original, label);
            _arithmeticOriginalBitmap = null;
            _arithmeticOriginalLabel = null;
            return result;
        }

        public void ClearArithmeticSnapshot()
        {
            _arithmeticOriginalBitmap = null;
            _arithmeticOriginalLabel = null;
        }

        private void CaptureArithmeticSnapshot()
        {
            if (_arithmeticOriginalBitmap != null)
            {
                return;
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar utama untuk operasi aritmetika.");
            }

            BitmapSource snapshot = State.OriginalBitmap.Clone();
            snapshot.Freeze();
            _arithmeticOriginalBitmap = snapshot;
            _arithmeticOriginalLabel = State.CurrentFilePath;
        }

        private enum ArithmeticMode
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }
    }
}
