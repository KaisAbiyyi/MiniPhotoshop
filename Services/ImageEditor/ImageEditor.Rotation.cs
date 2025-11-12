using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        private BitmapSource? _rotationOriginalBitmap;
        private string? _rotationOriginalLabel;

        public BitmapSource Rotate45()
        {
            CaptureRotationSnapshot();
            return ApplyRotation(45);
        }

        public BitmapSource Rotate90()
        {
            CaptureRotationSnapshot();
            return ApplyRotation(90);
        }

        public BitmapSource Rotate180()
        {
            CaptureRotationSnapshot();
            return ApplyRotation(180);
        }

        public BitmapSource Rotate270()
        {
            CaptureRotationSnapshot();
            return ApplyRotation(270);
        }

        public BitmapSource RotateCustom(double degrees)
        {
            CaptureRotationSnapshot();
            return ApplyRotation(degrees);
        }

        private BitmapSource ApplyRotation(double degrees)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk dirotasi.");
            }

            BitmapSource? baseBitmap = _rotationOriginalBitmap ?? State.OriginalBitmap;
            if (baseBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar dasar untuk rotasi.");
            }

            BitmapSource source = EnsureBgra32(baseBitmap);
            
            // Normalize angle to 0-360
            double normalizedAngle = degrees % 360;
            if (normalizedAngle < 0) normalizedAngle += 360;

            // For 90, 180, 270 degrees, use optimized rotation
            if (Math.Abs(normalizedAngle - 90) < 0.01)
            {
                return RotateOptimized(source, 90);
            }
            else if (Math.Abs(normalizedAngle - 180) < 0.01)
            {
                return RotateOptimized(source, 180);
            }
            else if (Math.Abs(normalizedAngle - 270) < 0.01)
            {
                return RotateOptimized(source, 270);
            }
            else
            {
                // For other angles, use general rotation with bilinear interpolation
                return RotateGeneral(source, degrees);
            }
        }

        private BitmapSource RotateOptimized(BitmapSource source, int degrees)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] sourceBuffer = new byte[stride * height];
            source.CopyPixels(sourceBuffer, stride, 0);

            byte[] resultBuffer;
            int resultWidth, resultHeight, resultStride;

            if (degrees == 90)
            {
                resultWidth = height;
                resultHeight = width;
                resultStride = resultWidth * 4;
                resultBuffer = new byte[resultStride * resultHeight];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex = y * stride + x * 4;
                        int dstX = height - 1 - y;
                        int dstY = x;
                        int dstIndex = dstY * resultStride + dstX * 4;

                        resultBuffer[dstIndex] = sourceBuffer[srcIndex];
                        resultBuffer[dstIndex + 1] = sourceBuffer[srcIndex + 1];
                        resultBuffer[dstIndex + 2] = sourceBuffer[srcIndex + 2];
                        resultBuffer[dstIndex + 3] = sourceBuffer[srcIndex + 3];
                    }
                }
            }
            else if (degrees == 180)
            {
                resultWidth = width;
                resultHeight = height;
                resultStride = resultWidth * 4;
                resultBuffer = new byte[resultStride * resultHeight];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex = y * stride + x * 4;
                        int dstX = width - 1 - x;
                        int dstY = height - 1 - y;
                        int dstIndex = dstY * resultStride + dstX * 4;

                        resultBuffer[dstIndex] = sourceBuffer[srcIndex];
                        resultBuffer[dstIndex + 1] = sourceBuffer[srcIndex + 1];
                        resultBuffer[dstIndex + 2] = sourceBuffer[srcIndex + 2];
                        resultBuffer[dstIndex + 3] = sourceBuffer[srcIndex + 3];
                    }
                }
            }
            else // 270
            {
                resultWidth = height;
                resultHeight = width;
                resultStride = resultWidth * 4;
                resultBuffer = new byte[resultStride * resultHeight];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex = y * stride + x * 4;
                        int dstX = y;
                        int dstY = width - 1 - x;
                        int dstIndex = dstY * resultStride + dstX * 4;

                        resultBuffer[dstIndex] = sourceBuffer[srcIndex];
                        resultBuffer[dstIndex + 1] = sourceBuffer[srcIndex + 1];
                        resultBuffer[dstIndex + 2] = sourceBuffer[srcIndex + 2];
                        resultBuffer[dstIndex + 3] = sourceBuffer[srcIndex + 3];
                    }
                }
            }

            BitmapSource result = CreateBitmapFromBuffer(resultBuffer, resultWidth, resultHeight);
            string label = $"Rotasi_{degrees}.png";
            return ReplaceWorkspaceBitmapForRotation(result, label);
        }

        private BitmapSource RotateGeneral(BitmapSource source, double degrees)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] sourceBuffer = new byte[stride * height];
            source.CopyPixels(sourceBuffer, stride, 0);

            // Convert degrees to radians
            double radians = degrees * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            // Calculate bounding box for rotated image
            Point[] corners = new Point[]
            {
                new Point(0, 0),
                new Point(width, 0),
                new Point(width, height),
                new Point(0, height)
            };

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var corner in corners)
            {
                double rotX = corner.X * cos - corner.Y * sin;
                double rotY = corner.X * sin + corner.Y * cos;
                minX = Math.Min(minX, rotX);
                maxX = Math.Max(maxX, rotX);
                minY = Math.Min(minY, rotY);
                maxY = Math.Max(maxY, rotY);
            }

            int resultWidth = (int)Math.Ceiling(maxX - minX);
            int resultHeight = (int)Math.Ceiling(maxY - minY);
            int resultStride = resultWidth * 4;
            byte[] resultBuffer = new byte[resultStride * resultHeight];

            // Center of original image
            double centerX = width / 2.0;
            double centerY = height / 2.0;

            // Center of result image
            double resultCenterX = resultWidth / 2.0;
            double resultCenterY = resultHeight / 2.0;

            // Fill result buffer with transparent pixels
            for (int i = 0; i < resultBuffer.Length; i += 4)
            {
                resultBuffer[i + 3] = 0; // Transparent
            }

            // Backward mapping with bilinear interpolation
            for (int dstY = 0; dstY < resultHeight; dstY++)
            {
                for (int dstX = 0; dstX < resultWidth; dstX++)
                {
                    // Transform to centered coordinates
                    double x = dstX - resultCenterX;
                    double y = dstY - resultCenterY;

                    // Inverse rotation
                    double srcX = x * cos + y * sin + centerX;
                    double srcY = -x * sin + y * cos + centerY;

                    // Check if source coordinates are within bounds
                    if (srcX >= 0 && srcX < width - 1 && srcY >= 0 && srcY < height - 1)
                    {
                        // Bilinear interpolation
                        int x0 = (int)Math.Floor(srcX);
                        int y0 = (int)Math.Floor(srcY);
                        int x1 = x0 + 1;
                        int y1 = y0 + 1;

                        double fx = srcX - x0;
                        double fy = srcY - y0;

                        int dstIndex = dstY * resultStride + dstX * 4;

                        for (int c = 0; c < 4; c++)
                        {
                            int idx00 = y0 * stride + x0 * 4 + c;
                            int idx10 = y0 * stride + x1 * 4 + c;
                            int idx01 = y1 * stride + x0 * 4 + c;
                            int idx11 = y1 * stride + x1 * 4 + c;

                            double val = (1 - fx) * (1 - fy) * sourceBuffer[idx00] +
                                       fx * (1 - fy) * sourceBuffer[idx10] +
                                       (1 - fx) * fy * sourceBuffer[idx01] +
                                       fx * fy * sourceBuffer[idx11];

                            resultBuffer[dstIndex + c] = (byte)Math.Clamp(val, 0, 255);
                        }
                    }
                }
            }

            BitmapSource result = CreateBitmapFromBuffer(resultBuffer, resultWidth, resultHeight);
            string label = $"Rotasi_{degrees:F1}.png";
            return ReplaceWorkspaceBitmapForRotation(result, label);
        }

        private BitmapSource ReplaceWorkspaceBitmapForRotation(BitmapSource bitmap, string label)
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

        public BitmapSource RestoreOriginal()
        {
            if (_rotationOriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar awal yang tersimpan untuk dipulihkan.");
            }

            BitmapSource original = _rotationOriginalBitmap;
            string label = _rotationOriginalLabel ?? "Gambar.png";

            BitmapSource result = ReplaceWorkspaceBitmapForRotation(original, label);
            _rotationOriginalBitmap = null;
            _rotationOriginalLabel = null;
            return result;
        }

        public void ClearRotationSnapshot()
        {
            _rotationOriginalBitmap = null;
            _rotationOriginalLabel = null;
        }

        private void CaptureRotationSnapshot()
        {
            if (_rotationOriginalBitmap != null)
            {
                return;
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar untuk operasi rotasi.");
            }

            BitmapSource snapshot = State.OriginalBitmap.Clone();
            snapshot.Freeze();
            _rotationOriginalBitmap = snapshot;
            _rotationOriginalLabel = State.CurrentFilePath;
        }
    }
}
