using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    /// <summary>
    /// Contrast enhancement operations for ImageEditor.
    /// </summary>
    internal sealed partial class ImageEditor
    {
        /// <summary>
        /// Applies linear contrast transform per channel using s = a * r + b.
        /// Input: slope (a) and intercept (b). Output: new bitmap with clamped intensities.
        /// Algorithm: LUT-based linear mapping for each RGB channel.
        /// </summary>
        public BitmapSource ApplyLinearContrast(double slope, double intercept)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (double.IsNaN(slope) || double.IsInfinity(slope))
            {
                throw new ArgumentException("Slope must be a valid number.", nameof(slope));
            }

            if (double.IsNaN(intercept) || double.IsInfinity(intercept))
            {
                throw new ArgumentException("Intercept must be a valid number.", nameof(intercept));
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            source.CopyPixels(buffer, stride, 0);

            byte[] map = BuildLinearContrastMap(slope, intercept);

            for (int i = 0; i < buffer.Length; i += 4)
            {
                buffer[i] = map[buffer[i]];         // B
                buffer[i + 1] = map[buffer[i + 1]]; // G
                buffer[i + 2] = map[buffer[i + 2]]; // R
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }

        /// <summary>
        /// Applies gamma correction per channel using s = c * r^gamma.
        /// Input: gamma and gain (c). Output: new bitmap with clamped intensities.
        /// Algorithm: LUT-based gamma mapping on normalized 0-1 intensities.
        /// </summary>
        public BitmapSource ApplyGammaCorrection(double gamma, double gain = 1.0)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (double.IsNaN(gamma) || double.IsInfinity(gamma) || gamma <= 0)
            {
                throw new ArgumentException("Gamma must be a positive number.", nameof(gamma));
            }

            if (double.IsNaN(gain) || double.IsInfinity(gain) || gain <= 0)
            {
                throw new ArgumentException("Gain must be a positive number.", nameof(gain));
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            source.CopyPixels(buffer, stride, 0);

            byte[] map = BuildGammaMap(gamma, gain);

            for (int i = 0; i < buffer.Length; i += 4)
            {
                buffer[i] = map[buffer[i]];         // B
                buffer[i + 1] = map[buffer[i + 1]]; // G
                buffer[i + 2] = map[buffer[i + 2]]; // R
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }

        /// <summary>
        /// Applies adaptive local contrast enhancement on a local window.
        /// Input: windowSize (odd) and gain. Output: new bitmap with locally adjusted contrast.
        /// Algorithm: compute local mean/std-dev and scale contrast based on global vs local variance.
        /// </summary>
        public BitmapSource ApplyAdaptiveContrast(int windowSize = 3, double gain = 1.0)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (windowSize < 3 || windowSize % 2 == 0)
            {
                throw new ArgumentException("Window size must be odd and >= 3.", nameof(windowSize));
            }

            if (double.IsNaN(gain) || double.IsInfinity(gain) || gain <= 0)
            {
                throw new ArgumentException("Gain must be a positive number.", nameof(gain));
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            int totalPixels = width * height;
            double sumB = 0, sumG = 0, sumR = 0;
            double sumSqB = 0, sumSqG = 0, sumSqR = 0;

            for (int i = 0; i < sourcePixels.Length; i += 4)
            {
                double b = sourcePixels[i];
                double g = sourcePixels[i + 1];
                double r = sourcePixels[i + 2];

                sumB += b;
                sumG += g;
                sumR += r;
                sumSqB += b * b;
                sumSqG += g * g;
                sumSqR += r * r;
            }

            double meanGlobalB = sumB / totalPixels;
            double meanGlobalG = sumG / totalPixels;
            double meanGlobalR = sumR / totalPixels;

            double sigmaGlobalB = Math.Sqrt(Math.Max(0, (sumSqB / totalPixels) - (meanGlobalB * meanGlobalB)));
            double sigmaGlobalG = Math.Sqrt(Math.Max(0, (sumSqG / totalPixels) - (meanGlobalG * meanGlobalG)));
            double sigmaGlobalR = Math.Sqrt(Math.Max(0, (sumSqR / totalPixels) - (meanGlobalR * meanGlobalR)));

            const double epsilon = 1e-3;
            const double maxScale = 3.0;

            int padding = windowSize / 2;
            byte[] paddedPixels = CreatePaddedImage(sourcePixels, width, height, padding);
            int paddedWidth = width + 2 * padding;
            int paddedStride = paddedWidth * 4;

            int windowArea = windowSize * windowSize;
            byte[] resultPixels = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    double localSumB = 0, localSumG = 0, localSumR = 0;
                    double localSumSqB = 0, localSumSqG = 0, localSumSqR = 0;

                    for (int ky = -padding; ky <= padding; ky++)
                    {
                        for (int kx = -padding; kx <= padding; kx++)
                        {
                            int px = x + padding + kx;
                            int py = y + padding + ky;
                            int paddedIndex = py * paddedStride + px * 4;

                            double b = paddedPixels[paddedIndex];
                            double g = paddedPixels[paddedIndex + 1];
                            double r = paddedPixels[paddedIndex + 2];

                            localSumB += b;
                            localSumG += g;
                            localSumR += r;
                            localSumSqB += b * b;
                            localSumSqG += g * g;
                            localSumSqR += r * r;
                        }
                    }

                    double meanB = localSumB / windowArea;
                    double meanG = localSumG / windowArea;
                    double meanR = localSumR / windowArea;
                    double sigmaB = Math.Sqrt(Math.Max(0, (localSumSqB / windowArea) - (meanB * meanB)));
                    double sigmaG = Math.Sqrt(Math.Max(0, (localSumSqG / windowArea) - (meanG * meanG)));
                    double sigmaR = Math.Sqrt(Math.Max(0, (localSumSqR / windowArea) - (meanR * meanR)));

                    int srcIndex = rowOffset + (x * 4);
                    double srcB = sourcePixels[srcIndex];
                    double srcG = sourcePixels[srcIndex + 1];
                    double srcR = sourcePixels[srcIndex + 2];

                    double scaleB = gain * (sigmaGlobalB / (sigmaB + epsilon));
                    double scaleG = gain * (sigmaGlobalG / (sigmaG + epsilon));
                    double scaleR = gain * (sigmaGlobalR / (sigmaR + epsilon));

                    scaleB = Math.Clamp(scaleB, 0.0, maxScale);
                    scaleG = Math.Clamp(scaleG, 0.0, maxScale);
                    scaleR = Math.Clamp(scaleR, 0.0, maxScale);

                    int outB = (int)Math.Round(meanGlobalB + (srcB - meanB) * scaleB);
                    int outG = (int)Math.Round(meanGlobalG + (srcG - meanG) * scaleG);
                    int outR = (int)Math.Round(meanGlobalR + (srcR - meanR) * scaleR);

                    resultPixels[srcIndex] = (byte)Math.Clamp(outB, 0, 255);
                    resultPixels[srcIndex + 1] = (byte)Math.Clamp(outG, 0, 255);
                    resultPixels[srcIndex + 2] = (byte)Math.Clamp(outR, 0, 255);
                    resultPixels[srcIndex + 3] = sourcePixels[srcIndex + 3];
                }
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        /// <summary>
        /// Applies global contrast stretching (normalization) based on per-channel min/max intensities.
        /// Input: current image pixels. Output: new bitmap with stretched histogram.
        /// Algorithm: compute min/max per channel, then map each channel to [0,255].
        /// </summary>
        public BitmapSource ApplyGlobalContrastStretching()
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            source.CopyPixels(buffer, stride, 0);

            byte minR = 255, minG = 255, minB = 255;
            byte maxR = 0, maxG = 0, maxB = 0;

            for (int i = 0; i < buffer.Length; i += 4)
            {
                byte b = buffer[i];
                byte g = buffer[i + 1];
                byte r = buffer[i + 2];

                if (r < minR) minR = r;
                if (g < minG) minG = g;
                if (b < minB) minB = b;
                if (r > maxR) maxR = r;
                if (g > maxG) maxG = g;
                if (b > maxB) maxB = b;
            }

            byte[] mapR = BuildLinearMap(minR, maxR);
            byte[] mapG = BuildLinearMap(minG, maxG);
            byte[] mapB = BuildLinearMap(minB, maxB);

            for (int i = 0; i < buffer.Length; i += 4)
            {
                buffer[i] = mapB[buffer[i]];
                buffer[i + 1] = mapG[buffer[i + 1]];
                buffer[i + 2] = mapR[buffer[i + 2]];
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }

        private static byte[] BuildLinearContrastMap(double slope, double intercept)
        {
            var map = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                double value = (slope * i) + intercept;
                map[i] = (byte)Math.Clamp((int)Math.Round(value), 0, 255);
            }
            return map;
        }

        private static byte[] BuildGammaMap(double gamma, double gain)
        {
            var map = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                double normalized = i / 255.0;
                double corrected = gain * Math.Pow(normalized, gamma) * 255.0;
                map[i] = (byte)Math.Clamp((int)Math.Round(corrected), 0, 255);
            }
            return map;
        }
    }
}
