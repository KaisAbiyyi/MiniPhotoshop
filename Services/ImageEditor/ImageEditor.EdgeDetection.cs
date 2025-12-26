using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    /// <summary>
    /// Edge Detection operations for ImageEditor.
    /// Implements Sobel, Prewitt, Robert, and Canny edge detection algorithms.
    /// Single Responsibility: Only handles edge detection operations.
    /// </summary>
    internal sealed partial class ImageEditor
    {
        #region Sobel Edge Detection

        /// <summary>
        /// Applies Sobel edge detection for horizontal edges (Gy).
        /// Uses Sobel Y kernel: [1,2,1; 0,0,0; -1,-2,-1]
        /// Detects horizontal edges (changes in vertical direction).
        /// </summary>
        public BitmapSource ApplySobelHorizontalEdge()
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            // Sobel Y kernel for horizontal edge detection
            double[,] kernelY = new double[3, 3]
            {
                {  1,  2,  1 },
                {  0,  0,  0 },
                { -1, -2, -1 }
            };

            return ApplyEdgeKernel(kernelY);
        }

        /// <summary>
        /// Applies Sobel edge detection for vertical edges (Gx).
        /// Uses Sobel X kernel: [-1,0,1; -2,0,2; -1,0,1]
        /// Detects vertical edges (changes in horizontal direction).
        /// </summary>
        public BitmapSource ApplySobelVerticalEdge()
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            // Sobel X kernel for vertical edge detection
            double[,] kernelX = new double[3, 3]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };

            return ApplyEdgeKernel(kernelX);
        }

        /// <summary>
        /// Applies combined Sobel edge detection (Magnitude).
        /// Calculates G = sqrt(Gx² + Gy²) for each pixel.
        /// This produces a complete edge map showing all edges.
        /// </summary>
        public BitmapSource ApplySobelMagnitude()
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            // Create padded image for border handling
            int padding = 1;
            byte[] paddedPixels = CreatePaddedImage(sourcePixels, width, height, padding);
            int paddedWidth = width + 2 * padding;
            int paddedStride = paddedWidth * 4;

            // Sobel kernels
            int[,] kernelX = new int[,]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };

            int[,] kernelY = new int[,]
            {
                {  1,  2,  1 },
                {  0,  0,  0 },
                { -1, -2, -1 }
            };

            byte[] resultPixels = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate gradients for each channel
                    double gxB = 0, gyB = 0;
                    double gxG = 0, gyG = 0;
                    double gxR = 0, gyR = 0;

                    // Apply 3x3 convolution
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int px = x + padding + kx;
                            int py = y + padding + ky;
                            int paddedIndex = py * paddedStride + px * 4;

                            int kxVal = kernelX[ky + 1, kx + 1];
                            int kyVal = kernelY[ky + 1, kx + 1];

                            // Blue channel
                            gxB += kxVal * paddedPixels[paddedIndex];
                            gyB += kyVal * paddedPixels[paddedIndex];

                            // Green channel
                            gxG += kxVal * paddedPixels[paddedIndex + 1];
                            gyG += kyVal * paddedPixels[paddedIndex + 1];

                            // Red channel
                            gxR += kxVal * paddedPixels[paddedIndex + 2];
                            gyR += kyVal * paddedPixels[paddedIndex + 2];
                        }
                    }

                    // Calculate magnitude: G = sqrt(Gx² + Gy²)
                    int magB = (int)Math.Sqrt(gxB * gxB + gyB * gyB);
                    int magG = (int)Math.Sqrt(gxG * gxG + gyG * gyG);
                    int magR = (int)Math.Sqrt(gxR * gxR + gyR * gyR);

                    // Clamp to [0, 255]
                    int resultIndex = y * stride + x * 4;
                    resultPixels[resultIndex] = ClampToByte(magB);
                    resultPixels[resultIndex + 1] = ClampToByte(magG);
                    resultPixels[resultIndex + 2] = ClampToByte(magR);
                    resultPixels[resultIndex + 3] = 255; // Full opacity
                }
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        #endregion

        #region Prewitt Edge Detection

        /// <summary>
        /// Applies Prewitt edge detection.
        /// Px = [-1 0 1; -1 0 1; -1 0 1]
        /// Py = [-1 -1 -1; 0 0 0; 1 1 1]
        /// Jedge = sqrt(Jx² + Jy²)
        /// </summary>
        public BitmapSource ApplyPrewitt()
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            // Create padded image for border handling
            int padding = 1;
            byte[] paddedPixels = CreatePaddedImage(sourcePixels, width, height, padding);
            int paddedWidth = width + 2 * padding;
            int paddedStride = paddedWidth * 4;

            // Prewitt kernels
            // Px = [-1 0 1; -1 0 1; -1 0 1]
            int[,] kernelX = new int[,]
            {
                { -1, 0, 1 },
                { -1, 0, 1 },
                { -1, 0, 1 }
            };

            // Py = [-1 -1 -1; 0 0 0; 1 1 1]
            int[,] kernelY = new int[,]
            {
                { -1, -1, -1 },
                {  0,  0,  0 },
                {  1,  1,  1 }
            };

            byte[] resultPixels = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate gradients for each channel
                    double gxB = 0, gyB = 0;
                    double gxG = 0, gyG = 0;
                    double gxR = 0, gyR = 0;

                    // Apply 3x3 convolution
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int px = x + padding + kx;
                            int py = y + padding + ky;
                            int paddedIndex = py * paddedStride + px * 4;

                            int kxVal = kernelX[ky + 1, kx + 1];
                            int kyVal = kernelY[ky + 1, kx + 1];

                            // Blue channel
                            gxB += kxVal * paddedPixels[paddedIndex];
                            gyB += kyVal * paddedPixels[paddedIndex];

                            // Green channel
                            gxG += kxVal * paddedPixels[paddedIndex + 1];
                            gyG += kyVal * paddedPixels[paddedIndex + 1];

                            // Red channel
                            gxR += kxVal * paddedPixels[paddedIndex + 2];
                            gyR += kyVal * paddedPixels[paddedIndex + 2];
                        }
                    }

                    // Calculate magnitude: G = sqrt(Gx² + Gy²)
                    int magB = (int)Math.Sqrt(gxB * gxB + gyB * gyB);
                    int magG = (int)Math.Sqrt(gxG * gxG + gyG * gyG);
                    int magR = (int)Math.Sqrt(gxR * gxR + gyR * gyR);

                    // Clamp to [0, 255]
                    int resultIndex = y * stride + x * 4;
                    resultPixels[resultIndex] = ClampToByte(magB);
                    resultPixels[resultIndex + 1] = ClampToByte(magG);
                    resultPixels[resultIndex + 2] = ClampToByte(magR);
                    resultPixels[resultIndex + 3] = 255; // Full opacity
                }
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        #endregion

        #region Robert Cross Edge Detection

        /// <summary>
        /// Applies Robert Cross edge detection using 2x2 diagonal kernels.
        /// Rx = [1, 0; 0, -1] and Ry = [0, 1; -1, 0]
        /// Gx = I(x,y) - I(x+1,y+1)
        /// Gy = I(x+1,y) - I(x,y+1)
        /// Jedge = sqrt(Gx² + Gy²)
        /// </summary>
        public BitmapSource ApplyRobert()
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            byte[] resultPixels = new byte[stride * height];

            // Robert Cross uses 2x2 kernel, loop stops 1 pixel before edge
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int idx00 = y * stride + x * 4;           // I(x, y)
                    int idx10 = y * stride + (x + 1) * 4;     // I(x+1, y)
                    int idx01 = (y + 1) * stride + x * 4;     // I(x, y+1)
                    int idx11 = (y + 1) * stride + (x + 1) * 4; // I(x+1, y+1)

                    // Blue channel
                    // Gx = I(x,y) - I(x+1,y+1)
                    // Gy = I(x+1,y) - I(x,y+1)
                    double gxB = sourcePixels[idx00] - sourcePixels[idx11];
                    double gyB = sourcePixels[idx10] - sourcePixels[idx01];

                    // Green channel
                    double gxG = sourcePixels[idx00 + 1] - sourcePixels[idx11 + 1];
                    double gyG = sourcePixels[idx10 + 1] - sourcePixels[idx01 + 1];

                    // Red channel
                    double gxR = sourcePixels[idx00 + 2] - sourcePixels[idx11 + 2];
                    double gyR = sourcePixels[idx10 + 2] - sourcePixels[idx01 + 2];

                    // Calculate magnitude: G = sqrt(Gx² + Gy²)
                    int magB = (int)Math.Sqrt(gxB * gxB + gyB * gyB);
                    int magG = (int)Math.Sqrt(gxG * gxG + gyG * gyG);
                    int magR = (int)Math.Sqrt(gxR * gxR + gyR * gyR);

                    // Clamp to [0, 255]
                    int resultIndex = y * stride + x * 4;
                    resultPixels[resultIndex] = ClampToByte(magB);
                    resultPixels[resultIndex + 1] = ClampToByte(magG);
                    resultPixels[resultIndex + 2] = ClampToByte(magR);
                    resultPixels[resultIndex + 3] = 255; // Full opacity
                }
            }

            // Handle last row and column (set to black or copy edge values)
            for (int x = 0; x < width; x++)
            {
                int idx = (height - 1) * stride + x * 4;
                resultPixels[idx] = 0;
                resultPixels[idx + 1] = 0;
                resultPixels[idx + 2] = 0;
                resultPixels[idx + 3] = 255;
            }
            for (int y = 0; y < height; y++)
            {
                int idx = y * stride + (width - 1) * 4;
                resultPixels[idx] = 0;
                resultPixels[idx + 1] = 0;
                resultPixels[idx + 2] = 0;
                resultPixels[idx + 3] = 255;
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        #endregion

        #region Canny Edge Detection

        /// <summary>
        /// Applies Canny edge detection - the optimal edge detector.
        /// 5-stage algorithm:
        /// 1. Gaussian Blur (noise reduction)
        /// 2. Gradient Calculation (Sobel)
        /// 3. Non-Maximum Suppression (thin edges)
        /// 4. Double Thresholding (strong/weak edges)
        /// 5. Edge Tracking by Hysteresis
        /// </summary>
        public BitmapSource ApplyCanny(double lowThreshold = 50, double highThreshold = 150, int gaussianKernelSize = 5, double sigma = 1.4)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            // Convert to grayscale for Canny processing
            double[,] grayscale = new double[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * stride + x * 4;
                    // Standard grayscale conversion
                    grayscale[y, x] = 0.299 * sourcePixels[idx + 2] + 
                                      0.587 * sourcePixels[idx + 1] + 
                                      0.114 * sourcePixels[idx];
                }
            }

            // Stage 1: Gaussian Blur with configurable kernel size and sigma
            double[,] blurred = ApplyGaussianBlurWithSigma(grayscale, width, height, gaussianKernelSize, sigma);

            // Stage 2: Gradient Calculation using Sobel
            double[,] magnitude = new double[height, width];
            double[,] direction = new double[height, width];
            CalculateSobelGradients(blurred, width, height, magnitude, direction);

            // Stage 3: Non-Maximum Suppression
            double[,] suppressed = ApplyNonMaxSuppression(magnitude, direction, width, height);

            // Stage 4: Double Thresholding
            int[,] edges = ApplyDoubleThreshold(suppressed, width, height, lowThreshold, highThreshold);

            // Stage 5: Edge Tracking by Hysteresis
            int[,] finalEdges = ApplyHysteresis(edges, width, height);

            // Convert back to image
            byte[] resultPixels = new byte[stride * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * stride + x * 4;
                    byte edgeValue = (byte)(finalEdges[y, x] == 2 ? 255 : 0);
                    resultPixels[idx] = edgeValue;     // B
                    resultPixels[idx + 1] = edgeValue; // G
                    resultPixels[idx + 2] = edgeValue; // R
                    resultPixels[idx + 3] = 255;       // A
                }
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        /// <summary>
        /// Stage 1: Apply Gaussian Blur with configurable kernel size and sigma.
        /// </summary>
        private double[,] ApplyGaussianBlurWithSigma(double[,] image, int width, int height, int kernelSize, double sigma)
        {
            // Ensure kernel size is odd
            if (kernelSize % 2 == 0) kernelSize++;
            
            int halfSize = kernelSize / 2;
            
            // Generate Gaussian kernel
            double[,] kernel = new double[kernelSize, kernelSize];
            double kernelSum = 0;
            
            for (int y = -halfSize; y <= halfSize; y++)
            {
                for (int x = -halfSize; x <= halfSize; x++)
                {
                    // Gaussian formula: G(x,y) = (1/(2*pi*sigma^2)) * e^(-(x^2+y^2)/(2*sigma^2))
                    double value = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    kernel[y + halfSize, x + halfSize] = value;
                    kernelSum += value;
                }
            }
            
            // Normalize kernel
            for (int y = 0; y < kernelSize; y++)
            {
                for (int x = 0; x < kernelSize; x++)
                {
                    kernel[y, x] /= kernelSum;
                }
            }

            double[,] result = new double[height, width];

            for (int y = halfSize; y < height - halfSize; y++)
            {
                for (int x = halfSize; x < width - halfSize; x++)
                {
                    double sum = 0;
                    for (int ky = -halfSize; ky <= halfSize; ky++)
                    {
                        for (int kx = -halfSize; kx <= halfSize; kx++)
                        {
                            sum += image[y + ky, x + kx] * kernel[ky + halfSize, kx + halfSize];
                        }
                    }
                    result[y, x] = sum;
                }
            }

            // Copy border pixels
            for (int y = 0; y < halfSize; y++)
                for (int x = 0; x < width; x++)
                    result[y, x] = image[y, x];
            for (int y = height - halfSize; y < height; y++)
                for (int x = 0; x < width; x++)
                    result[y, x] = image[y, x];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < halfSize; x++)
                    result[y, x] = image[y, x];
                for (int x = width - halfSize; x < width; x++)
                    result[y, x] = image[y, x];
            }

            return result;
        }

        /// <summary>
        /// Stage 1: Apply Gaussian Blur with 5x5 kernel (legacy method).
        /// </summary>
        private double[,] ApplyGaussianBlur(double[,] image, int width, int height)
        {
            return ApplyGaussianBlurWithSigma(image, width, height, 5, 1.4);
        }

        /// <summary>
        /// Stage 2: Calculate gradients using Sobel operators.
        /// </summary>
        private void CalculateSobelGradients(double[,] image, int width, int height, 
            double[,] magnitude, double[,] direction)
        {
            int[,] sobelX = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] sobelY = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double gx = 0, gy = 0;
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            gx += image[y + ky, x + kx] * sobelX[ky + 1, kx + 1];
                            gy += image[y + ky, x + kx] * sobelY[ky + 1, kx + 1];
                        }
                    }
                    magnitude[y, x] = Math.Sqrt(gx * gx + gy * gy);
                    direction[y, x] = Math.Atan2(gy, gx) * 180 / Math.PI;
                }
            }
        }

        /// <summary>
        /// Stage 3: Non-Maximum Suppression - thin edges to 1 pixel width.
        /// </summary>
        private double[,] ApplyNonMaxSuppression(double[,] magnitude, double[,] direction, 
            int width, int height)
        {
            double[,] result = new double[height, width];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double angle = direction[y, x];
                    // Normalize angle to 0-180
                    if (angle < 0) angle += 180;

                    double neighbor1, neighbor2;

                    // Determine neighbors based on gradient direction
                    if ((angle >= 0 && angle < 22.5) || (angle >= 157.5 && angle <= 180))
                    {
                        // Horizontal: compare left and right
                        neighbor1 = magnitude[y, x - 1];
                        neighbor2 = magnitude[y, x + 1];
                    }
                    else if (angle >= 22.5 && angle < 67.5)
                    {
                        // Diagonal: top-right and bottom-left
                        neighbor1 = magnitude[y - 1, x + 1];
                        neighbor2 = magnitude[y + 1, x - 1];
                    }
                    else if (angle >= 67.5 && angle < 112.5)
                    {
                        // Vertical: compare top and bottom
                        neighbor1 = magnitude[y - 1, x];
                        neighbor2 = magnitude[y + 1, x];
                    }
                    else // angle >= 112.5 && angle < 157.5
                    {
                        // Diagonal: top-left and bottom-right
                        neighbor1 = magnitude[y - 1, x - 1];
                        neighbor2 = magnitude[y + 1, x + 1];
                    }

                    // Keep pixel only if it's local maximum
                    if (magnitude[y, x] >= neighbor1 && magnitude[y, x] >= neighbor2)
                    {
                        result[y, x] = magnitude[y, x];
                    }
                    else
                    {
                        result[y, x] = 0;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Stage 4: Double Thresholding - classify edges as strong (2), weak (1), or none (0).
        /// </summary>
        private int[,] ApplyDoubleThreshold(double[,] image, int width, int height,
            double lowThreshold, double highThreshold)
        {
            int[,] result = new int[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (image[y, x] >= highThreshold)
                    {
                        result[y, x] = 2; // Strong edge
                    }
                    else if (image[y, x] >= lowThreshold)
                    {
                        result[y, x] = 1; // Weak edge
                    }
                    else
                    {
                        result[y, x] = 0; // No edge
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Stage 5: Edge Tracking by Hysteresis - keep weak edges only if connected to strong edges.
        /// </summary>
        private int[,] ApplyHysteresis(int[,] edges, int width, int height)
        {
            int[,] result = new int[height, width];
            Array.Copy(edges, result, edges.Length);

            // Keep iterating until no changes
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        if (result[y, x] == 1) // Weak edge
                        {
                            // Check 8-connected neighbors for strong edge
                            bool hasStrongNeighbor = false;
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    if (result[y + dy, x + dx] == 2)
                                    {
                                        hasStrongNeighbor = true;
                                        break;
                                    }
                                }
                                if (hasStrongNeighbor) break;
                            }

                            if (hasStrongNeighbor)
                            {
                                result[y, x] = 2; // Promote to strong
                                changed = true;
                            }
                        }
                    }
                }
            }

            // Remove remaining weak edges
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (result[y, x] == 1)
                    {
                        result[y, x] = 0;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Edge Detection Helpers

        /// <summary>
        /// Applies a single edge detection kernel to the image.
        /// Used for directional edge detection (horizontal or vertical only).
        /// </summary>
        private BitmapSource ApplyEdgeKernel(double[,] kernel)
        {
            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[stride * height];
            source.CopyPixels(sourcePixels, stride, 0);

            // Create padded image
            int padding = 1;
            byte[] paddedPixels = CreatePaddedImage(sourcePixels, width, height, padding);
            int paddedWidth = width + 2 * padding;
            int paddedStride = paddedWidth * 4;

            byte[] resultPixels = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double sumB = 0, sumG = 0, sumR = 0;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int px = x + padding + kx;
                            int py = y + padding + ky;
                            int paddedIndex = py * paddedStride + px * 4;

                            double kValue = kernel[ky + 1, kx + 1];

                            sumB += kValue * paddedPixels[paddedIndex];
                            sumG += kValue * paddedPixels[paddedIndex + 1];
                            sumR += kValue * paddedPixels[paddedIndex + 2];
                        }
                    }

                    // Take absolute value for edge visualization
                    int resultIndex = y * stride + x * 4;
                    resultPixels[resultIndex] = ClampToByte((int)Math.Abs(sumB));
                    resultPixels[resultIndex + 1] = ClampToByte((int)Math.Abs(sumG));
                    resultPixels[resultIndex + 2] = ClampToByte((int)Math.Abs(sumR));
                    resultPixels[resultIndex + 3] = 255; // Full opacity
                }
            }

            return CreateBitmapFromBuffer(resultPixels, width, height);
        }

        #endregion
    }
}
