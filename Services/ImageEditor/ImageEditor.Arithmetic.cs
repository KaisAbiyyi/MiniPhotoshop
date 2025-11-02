using System;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public BitmapSource AddImage(BitmapSource overlay, int offsetX, int offsetY)
        {
            return ApplyArithmeticOperation(overlay, offsetX, offsetY, ArithmeticMode.Add);
        }

        public BitmapSource SubtractImage(BitmapSource overlay, int offsetX, int offsetY)
        {
            return ApplyArithmeticOperation(overlay, offsetX, offsetY, ArithmeticMode.Subtract);
        }

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

            BitmapSource baseSource = EnsureBgra32(State.OriginalBitmap);
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
                                b += overlayB;
                                g += overlayG;
                                r += overlayR;
                            }
                            else
                            {
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

        private enum ArithmeticMode
        {
            Add,
            Subtract
        }
    }
}
