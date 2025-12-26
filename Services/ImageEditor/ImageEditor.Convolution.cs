using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Services.ImageEditor
{
    /// <summary>
    /// Convolution operations for ImageEditor.
    /// Implements local area operations using kernel matrices.
    /// Single Responsibility: Only handles convolution-related operations.
    /// </summary>
    internal sealed partial class ImageEditor
    {
        #region Core Convolution

        /// <summary>
        /// Applies convolution operation using a custom kernel matrix.
        /// Formula: G(x,y) = Round(C × ΣΣ K(i,j)·I(x+i, y+j))
        /// </summary>
        public BitmapSource ApplyConvolution(double[,] kernel, double multiplier = 1.0)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            // Get source pixels
            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            // Validate kernel (must be odd-sized square)
            int kernelHeight = kernel.GetLength(0);
            int kernelWidth = kernel.GetLength(1);

            if (kernelHeight != kernelWidth || kernelHeight % 2 == 0)
            {
                throw new ArgumentException("Kernel must be odd-sized square matrix.");
            }

            // Calculate padding: P = (K - 1) / 2
            int padding = (kernelHeight - 1) / 2;

            // Create padded image using copy-edge method
            byte[] paddedPixels = CreatePaddedImage(sourcePixels, width, height, padding);
            int paddedWidth = width + 2 * padding;
            int paddedStride = paddedWidth * 4;

            // Apply convolution for each channel (RGB)
            byte[] resultPixels = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Apply convolution for each color channel separately
                    double sumB = 0, sumG = 0, sumR = 0;

                    for (int ky = -padding; ky <= padding; ky++)
                    {
                        for (int kx = -padding; kx <= padding; kx++)
                        {
                            // Position in padded image (offset by padding)
                            int px = x + padding + kx;
                            int py = y + padding + ky;
                            int paddedIndex = py * paddedStride + px * 4;

                            // Kernel value (convert kernel coordinates to array indices)
                            double kValue = kernel[ky + padding, kx + padding];

                            // Accumulate weighted sum for each channel
                            sumB += kValue * paddedPixels[paddedIndex];
                            sumG += kValue * paddedPixels[paddedIndex + 1];
                            sumR += kValue * paddedPixels[paddedIndex + 2];
                        }
                    }

                    // Apply multiplier and round: Round(Value + 0.5)
                    int resultIndex = y * stride + x * 4;
                    resultPixels[resultIndex] = ClampToByte(RoundValue(sumB * multiplier));
                    resultPixels[resultIndex + 1] = ClampToByte(RoundValue(sumG * multiplier));
                    resultPixels[resultIndex + 2] = ClampToByte(RoundValue(sumR * multiplier));
                    resultPixels[resultIndex + 3] = sourcePixels[resultIndex + 3]; // Preserve alpha
                }
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        #endregion

        #region Image Padding

        /// <summary>
        /// Creates a padded image using copy-edge method.
        /// Formula: I_pad(x,y) = I_original(nearest_edge) for border pixels
        /// </summary>
        private byte[] CreatePaddedImage(byte[] source, int width, int height, int padding)
        {
            int paddedWidth = width + 2 * padding;
            int paddedHeight = height + 2 * padding;
            int sourceStride = width * 4;
            int paddedStride = paddedWidth * 4;

            byte[] padded = new byte[paddedStride * paddedHeight];

            for (int y = 0; y < paddedHeight; y++)
            {
                for (int x = 0; x < paddedWidth; x++)
                {
                    // Determine source coordinates using copy-edge logic
                    int srcX = ClampToRange(x - padding, 0, width - 1);
                    int srcY = ClampToRange(y - padding, 0, height - 1);

                    int srcIndex = srcY * sourceStride + srcX * 4;
                    int dstIndex = y * paddedStride + x * 4;

                    // Copy BGRA values
                    padded[dstIndex] = source[srcIndex];         // B
                    padded[dstIndex + 1] = source[srcIndex + 1]; // G
                    padded[dstIndex + 2] = source[srcIndex + 2]; // R
                    padded[dstIndex + 3] = source[srcIndex + 3]; // A
                }
            }

            return padded;
        }

        /// <summary>
        /// Clamps value to specified range.
        /// </summary>
        private int ClampToRange(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// Rounds value using floor(value + 0.5) method.
        /// </summary>
        private int RoundValue(double value)
        {
            return (int)Math.Floor(value + 0.5);
        }

        /// <summary>
        /// Clamps double value to byte range [0, 255].
        /// </summary>
        private byte ClampToByte(int value)
        {
            return (byte)Math.Max(0, Math.Min(255, value));
        }

        #endregion

        #region Predefined Filters

        /// <summary>
        /// Applies Low Pass Filter (averaging/blur).
        /// Reduces noise by averaging neighboring pixels.
        /// </summary>
        public BitmapSource ApplyLowPassFilter(int kernelSize = 3)
        {
            if (kernelSize % 2 == 0 || kernelSize < 3)
            {
                throw new ArgumentException("Kernel size must be odd and >= 3.");
            }

            // Create averaging kernel (all 1s)
            double[,] kernel = new double[kernelSize, kernelSize];
            for (int i = 0; i < kernelSize; i++)
            {
                for (int j = 0; j < kernelSize; j++)
                {
                    kernel[i, j] = 1.0;
                }
            }

            // Multiplier = 1 / (kernelSize^2) for averaging
            double multiplier = 1.0 / (kernelSize * kernelSize);

            return ApplyConvolution(kernel, multiplier);
        }

        /// <summary>
        /// Applies High Pass Filter (sharpening).
        /// Enhances edges and fine details.
        /// </summary>
        public BitmapSource ApplyHighPassFilter(int kernelSize = 3)
        {
            // Standard 3x3 high pass kernel
            double[,] kernel = new double[3, 3]
            {
                { -1, -1, -1 },
                { -1,  9, -1 },
                { -1, -1, -1 }
            };

            return ApplyConvolution(kernel, 1.0);
        }

        /// <summary>
        /// Applies Gaussian Blur filter.
        /// Uses weighted average based on Gaussian distribution.
        /// Sigma is calculated based on kernel size for optimal blur effect.
        /// </summary>
        public BitmapSource ApplyGaussianBlur(int kernelSize = 3, double sigma = 0)
        {
            if (kernelSize % 2 == 0 || kernelSize < 3)
            {
                throw new ArgumentException("Kernel size must be odd and >= 3.");
            }

            // Calculate optimal sigma based on kernel size if not provided
            // Using sigma = kernelSize / 2 for more pronounced blur effect
            // This ensures Gaussian extends to edge of kernel
            if (sigma <= 0)
            {
                sigma = kernelSize / 2.0;
            }

            // Generate Gaussian kernel
            double[,] kernel = GenerateGaussianKernel(kernelSize, sigma);

            // Calculate sum for normalization (multiplier)
            double sum = 0;
            for (int i = 0; i < kernelSize; i++)
            {
                for (int j = 0; j < kernelSize; j++)
                {
                    sum += kernel[i, j];
                }
            }

            return ApplyConvolution(kernel, 1.0 / sum);
        }

        /// <summary>
        /// Applies Median Filter (non-linear) for noise reduction.
        /// Input: kernel size (odd). Output: new bitmap with median per channel.
        /// Algorithm: collect neighborhood values, sort, and take median.
        /// </summary>
        public BitmapSource ApplyMedianFilter(int kernelSize = 3)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (kernelSize % 2 == 0 || kernelSize < 3)
            {
                throw new ArgumentException("Kernel size must be odd and >= 3.");
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            int padding = kernelSize / 2;
            byte[] paddedPixels = CreatePaddedImage(sourcePixels, width, height, padding);
            int paddedWidth = width + 2 * padding;
            int paddedStride = paddedWidth * 4;

            int windowSize = kernelSize * kernelSize;
            byte[] windowB = new byte[windowSize];
            byte[] windowG = new byte[windowSize];
            byte[] windowR = new byte[windowSize];

            byte[] resultPixels = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = 0;
                    for (int ky = -padding; ky <= padding; ky++)
                    {
                        for (int kx = -padding; kx <= padding; kx++)
                        {
                            int px = x + padding + kx;
                            int py = y + padding + ky;
                            int paddedIndex = py * paddedStride + px * 4;

                            windowB[idx] = paddedPixels[paddedIndex];
                            windowG[idx] = paddedPixels[paddedIndex + 1];
                            windowR[idx] = paddedPixels[paddedIndex + 2];
                            idx++;
                        }
                    }

                    Array.Sort(windowB);
                    Array.Sort(windowG);
                    Array.Sort(windowR);

                    int medianIndex = windowSize / 2;
                    int resultIndex = y * stride + x * 4;
                    resultPixels[resultIndex] = windowB[medianIndex];
                    resultPixels[resultIndex + 1] = windowG[medianIndex];
                    resultPixels[resultIndex + 2] = windowR[medianIndex];
                    resultPixels[resultIndex + 3] = sourcePixels[resultIndex + 3];
                }
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        /// <summary>
        /// Generates Gaussian kernel matrix.
        /// Formula: G(x,y) = (1/(2πσ²)) × e^(-(x²+y²)/(2σ²))
        /// </summary>
        private double[,] GenerateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            int center = size / 2;
            double sigma2 = 2 * sigma * sigma;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - center;
                    int dy = y - center;
                    kernel[y, x] = Math.Exp(-(dx * dx + dy * dy) / sigma2);
                }
            }

            return kernel;
        }

        /// <summary>
        /// Applies Sobel edge detection (horizontal edges).
        /// </summary>
        public BitmapSource ApplySobelHorizontal()
        {
            double[,] kernel = new double[3, 3]
            {
                { -1, -2, -1 },
                {  0,  0,  0 },
                {  1,  2,  1 }
            };

            return ApplyConvolution(kernel, 1.0);
        }

        /// <summary>
        /// Applies Sobel edge detection (vertical edges).
        /// </summary>
        public BitmapSource ApplySobelVertical()
        {
            double[,] kernel = new double[3, 3]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };

            return ApplyConvolution(kernel, 1.0);
        }

        /// <summary>
        /// Applies Laplacian edge detection.
        /// Second-order derivative for detecting edges in all directions.
        /// </summary>
        public BitmapSource ApplyLaplacian()
        {
            double[,] kernel = new double[3, 3]
            {
                {  0, -1,  0 },
                { -1,  4, -1 },
                {  0, -1,  0 }
            };

            return ApplyConvolution(kernel, 1.0);
        }

        /// <summary>
        /// Applies Laplacian sharpening by adding the Laplacian response to the original image.
        /// Input: strength multiplier. Output: sharpened bitmap.
        /// Algorithm: Laplacian convolution + original, with clamping.
        /// </summary>
        public BitmapSource ApplyLaplacianSharpen(double strength = 1.0)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (double.IsNaN(strength) || double.IsInfinity(strength) || strength <= 0)
            {
                throw new ArgumentException("Strength must be a positive number.", nameof(strength));
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            int padding = 1;
            byte[] paddedPixels = CreatePaddedImage(sourcePixels, width, height, padding);
            int paddedWidth = width + 2 * padding;
            int paddedStride = paddedWidth * 4;

            double[,] kernel = new double[3, 3]
            {
                {  0, -1,  0 },
                { -1,  4, -1 },
                {  0, -1,  0 }
            };

            byte[] resultPixels = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double sumB = 0, sumG = 0, sumR = 0;

                    for (int ky = -padding; ky <= padding; ky++)
                    {
                        for (int kx = -padding; kx <= padding; kx++)
                        {
                            int px = x + padding + kx;
                            int py = y + padding + ky;
                            int paddedIndex = py * paddedStride + px * 4;

                            double kValue = kernel[ky + padding, kx + padding];
                            sumB += kValue * paddedPixels[paddedIndex];
                            sumG += kValue * paddedPixels[paddedIndex + 1];
                            sumR += kValue * paddedPixels[paddedIndex + 2];
                        }
                    }

                    int resultIndex = y * stride + x * 4;
                    int outB = (int)Math.Round(sourcePixels[resultIndex] + (sumB * strength));
                    int outG = (int)Math.Round(sourcePixels[resultIndex + 1] + (sumG * strength));
                    int outR = (int)Math.Round(sourcePixels[resultIndex + 2] + (sumR * strength));

                    resultPixels[resultIndex] = ClampToByte(outB);
                    resultPixels[resultIndex + 1] = ClampToByte(outG);
                    resultPixels[resultIndex + 2] = ClampToByte(outR);
                    resultPixels[resultIndex + 3] = sourcePixels[resultIndex + 3];
                }
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        /// <summary>
        /// Applies emboss effect.
        /// Creates a 3D raised appearance.
        /// </summary>
        public BitmapSource ApplyEmboss()
        {
            double[,] kernel = new double[3, 3]
            {
                { -2, -1, 0 },
                { -1,  1, 1 },
                {  0,  1, 2 }
            };

            return ApplyConvolution(kernel, 1.0);
        }

        #endregion
    }
}
