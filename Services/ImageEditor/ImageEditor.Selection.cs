using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    /// <summary>
    /// Selection operations for ImageEditor.
    /// Implements Rectangle and Lasso selection tools.
    /// Single Responsibility: Only handles selection-related operations.
    /// </summary>
    internal sealed partial class ImageEditor
    {
        private SelectionState _selectionState = new SelectionState();

        /// <summary>
        /// Gets the current selection state.
        /// </summary>
        public SelectionState SelectionState => _selectionState;

        #region Rectangle Selection

        /// <summary>
        /// Creates a rectangular selection with boundary validation.
        /// Boundary check: 0 <= x < width and 0 <= y < height
        /// </summary>
        public void CreateRectangleSelection(int x1, int y1, int x2, int y2)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            int imageWidth = State.OriginalBitmap.PixelWidth;
            int imageHeight = State.OriginalBitmap.PixelHeight;

            // Normalize coordinates (ensure x1 <= x2 and y1 <= y2)
            int left = Math.Min(x1, x2);
            int top = Math.Min(y1, y2);
            int right = Math.Max(x1, x2);
            int bottom = Math.Max(y1, y2);

            // Boundary validation: clamp to image bounds
            left = Math.Max(0, Math.Min(left, imageWidth - 1));
            top = Math.Max(0, Math.Min(top, imageHeight - 1));
            right = Math.Max(0, Math.Min(right, imageWidth - 1));
            bottom = Math.Max(0, Math.Min(bottom, imageHeight - 1));

            // Ensure valid selection (at least 1x1)
            if (right <= left || bottom <= top)
            {
                _selectionState.Reset();
                return;
            }

            _selectionState.Type = SelectionType.Rectangle;
            _selectionState.Bounds = new Rect(left, top, right - left, bottom - top);
            _selectionState.HasSelection = true;
            _selectionState.OffsetX = left;
            _selectionState.OffsetY = top;
            _selectionState.SelectedWidth = right - left;
            _selectionState.SelectedHeight = bottom - top;

            // Create selection mask
            CreateRectangleMask(left, top, right, bottom);
        }

        /// <summary>
        /// Creates mask for rectangle selection.
        /// </summary>
        private void CreateRectangleMask(int left, int top, int right, int bottom)
        {
            if (State.OriginalBitmap == null) return;

            int imageWidth = State.OriginalBitmap.PixelWidth;
            int imageHeight = State.OriginalBitmap.PixelHeight;

            _selectionState.Mask = new bool[imageHeight, imageWidth];

            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    _selectionState.Mask[y, x] = true;
                }
            }
        }

        #endregion

        #region Lasso Selection

        /// <summary>
        /// Creates a lasso (freeform) selection from polygon points.
        /// Uses Ray Casting algorithm to determine which pixels are inside.
        /// </summary>
        public void CreateLassoSelection(List<Point> points)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (points == null || points.Count < 3)
            {
                _selectionState.Reset();
                return;
            }

            int imageWidth = State.OriginalBitmap.PixelWidth;
            int imageHeight = State.OriginalBitmap.PixelHeight;

            _selectionState.Type = SelectionType.Lasso;
            _selectionState.LassoPoints = new List<Point>(points);
            _selectionState.HasSelection = true;

            // Calculate bounding box
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            foreach (var point in points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            // Clamp to image bounds
            int left = Math.Max(0, (int)Math.Floor(minX));
            int top = Math.Max(0, (int)Math.Floor(minY));
            int right = Math.Min(imageWidth - 1, (int)Math.Ceiling(maxX));
            int bottom = Math.Min(imageHeight - 1, (int)Math.Ceiling(maxY));

            _selectionState.Bounds = new Rect(left, top, right - left, bottom - top);
            _selectionState.OffsetX = left;
            _selectionState.OffsetY = top;
            _selectionState.SelectedWidth = right - left;
            _selectionState.SelectedHeight = bottom - top;

            // Create selection mask using Ray Casting
            CreateLassoMask(points, imageWidth, imageHeight);
        }

        /// <summary>
        /// Creates mask for lasso selection using Ray Casting algorithm.
        /// Ray Casting: cast horizontal ray from point, count intersections with polygon edges.
        /// If count is odd, point is inside polygon.
        /// </summary>
        private void CreateLassoMask(List<Point> points, int width, int height)
        {
            _selectionState.Mask = new bool[height, width];

            // Get bounding box for optimization
            int left = (int)_selectionState.Bounds.Left;
            int top = (int)_selectionState.Bounds.Top;
            int right = (int)(_selectionState.Bounds.Left + _selectionState.Bounds.Width);
            int bottom = (int)(_selectionState.Bounds.Top + _selectionState.Bounds.Height);

            // Test each pixel in bounding box
            for (int y = top; y <= bottom && y < height; y++)
            {
                for (int x = left; x <= right && x < width; x++)
                {
                    if (IsPointInPolygon(x, y, points))
                    {
                        _selectionState.Mask[y, x] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Ray Casting algorithm to test if point is inside polygon.
        /// Cast ray from (x, y) to the right (+X direction).
        /// Count how many polygon edges the ray crosses.
        /// Odd count = inside, Even count = outside.
        /// </summary>
        private bool IsPointInPolygon(double testX, double testY, List<Point> polygon)
        {
            if (polygon.Count < 3) return false;

            int intersections = 0;
            int n = polygon.Count;

            for (int i = 0; i < n; i++)
            {
                Point p1 = polygon[i];
                Point p2 = polygon[(i + 1) % n];

                // Check if ray can potentially intersect this edge
                // Edge must cross the horizontal line y = testY
                if ((p1.Y <= testY && p2.Y > testY) || (p2.Y <= testY && p1.Y > testY))
                {
                    // Calculate x coordinate of intersection
                    // Using linear interpolation: x = x1 + (y - y1) * (x2 - x1) / (y2 - y1)
                    double intersectX = p1.X + (testY - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X);

                    // If intersection is to the right of test point, count it
                    if (testX < intersectX)
                    {
                        intersections++;
                    }
                }
            }

            // Odd number of intersections = inside
            return (intersections % 2) == 1;
        }

        #endregion

        #region Selection Operations

        /// <summary>
        /// Checks if a point is inside the current selection.
        /// </summary>
        public bool IsPointInSelection(int x, int y)
        {
            if (!_selectionState.HasSelection)
                return false;

            if (x < 0 || y < 0)
                return false;

            if (y >= _selectionState.Mask.GetLength(0) || x >= _selectionState.Mask.GetLength(1))
                return false;

            return _selectionState.Mask[y, x];
        }

        /// <summary>
        /// Moves the current selection by delta.
        /// Formula: Pos_new = Pos_old + Δ
        /// </summary>
        public void MoveSelection(int deltaX, int deltaY)
        {
            if (!_selectionState.HasSelection)
                return;

            _selectionState.OffsetX += deltaX;
            _selectionState.OffsetY += deltaY;

            // Update bounds
            _selectionState.Bounds = new Rect(
                _selectionState.OffsetX,
                _selectionState.OffsetY,
                _selectionState.SelectedWidth,
                _selectionState.SelectedHeight);
        }

        /// <summary>
        /// Copies selected pixels to buffer.
        /// </summary>
        public void CopySelectedPixels()
        {
            if (!_selectionState.HasSelection || State.OriginalBitmap == null)
                return;

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int imageWidth = source.PixelWidth;
            int imageHeight = source.PixelHeight;
            int stride = imageWidth * 4;

            byte[] sourcePixels = new byte[stride * imageHeight];
            source.CopyPixels(sourcePixels, stride, 0);

            // Get selection bounds
            int selWidth = _selectionState.SelectedWidth;
            int selHeight = _selectionState.SelectedHeight;
            int selLeft = (int)_selectionState.Bounds.Left;
            int selTop = (int)_selectionState.Bounds.Top;

            // Create buffer for selected pixels (with alpha for transparency)
            int selStride = selWidth * 4;
            _selectionState.SelectedPixels = new byte[selStride * selHeight];

            for (int y = 0; y < selHeight; y++)
            {
                int srcY = selTop + y;
                if (srcY < 0 || srcY >= imageHeight) continue;

                for (int x = 0; x < selWidth; x++)
                {
                    int srcX = selLeft + x;
                    if (srcX < 0 || srcX >= imageWidth) continue;

                    // Check if pixel is in selection mask
                    bool inSelection = false;
                    if (srcY < _selectionState.Mask.GetLength(0) && 
                        srcX < _selectionState.Mask.GetLength(1))
                    {
                        inSelection = _selectionState.Mask[srcY, srcX];
                    }

                    int srcIndex = srcY * stride + srcX * 4;
                    int dstIndex = y * selStride + x * 4;

                    if (inSelection)
                    {
                        // Copy pixel from source
                        _selectionState.SelectedPixels[dstIndex] = sourcePixels[srcIndex];         // B
                        _selectionState.SelectedPixels[dstIndex + 1] = sourcePixels[srcIndex + 1]; // G
                        _selectionState.SelectedPixels[dstIndex + 2] = sourcePixels[srcIndex + 2]; // R
                        _selectionState.SelectedPixels[dstIndex + 3] = 255; // Fully opaque
                    }
                    else
                    {
                        // Transparent pixel
                        _selectionState.SelectedPixels[dstIndex] = 0;
                        _selectionState.SelectedPixels[dstIndex + 1] = 0;
                        _selectionState.SelectedPixels[dstIndex + 2] = 0;
                        _selectionState.SelectedPixels[dstIndex + 3] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Applies the selection move and renders result.
        /// Fills original area with canvas background color.
        /// </summary>
        public BitmapSource ApplySelectionMove()
        {
            if (!_selectionState.HasSelection || State.OriginalBitmap == null)
            {
                return State.OriginalBitmap!;
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] pixels = new byte[stride * height];
            source.CopyPixels(pixels, stride, 0);

            // Get background color from canvas state
            Color bgColor = _canvasState.BackgroundColor;

            // Original selection bounds (before move)
            int origLeft = (int)_selectionState.Bounds.Left - (_selectionState.OffsetX - (int)_selectionState.Bounds.Left);
            int origTop = (int)_selectionState.Bounds.Top - (_selectionState.OffsetY - (int)_selectionState.Bounds.Top);

            // Fill original area with background color
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y < _selectionState.Mask.GetLength(0) && 
                        x < _selectionState.Mask.GetLength(1) &&
                        _selectionState.Mask[y, x])
                    {
                        int index = y * stride + x * 4;
                        pixels[index] = bgColor.B;
                        pixels[index + 1] = bgColor.G;
                        pixels[index + 2] = bgColor.R;
                        pixels[index + 3] = bgColor.A;
                    }
                }
            }

            // Draw selected pixels at new position
            int selWidth = _selectionState.SelectedWidth;
            int selHeight = _selectionState.SelectedHeight;
            int selStride = selWidth * 4;
            int newLeft = _selectionState.OffsetX;
            int newTop = _selectionState.OffsetY;

            for (int y = 0; y < selHeight; y++)
            {
                int dstY = newTop + y;
                if (dstY < 0 || dstY >= height) continue;

                for (int x = 0; x < selWidth; x++)
                {
                    int dstX = newLeft + x;
                    if (dstX < 0 || dstX >= width) continue;

                    int srcIndex = y * selStride + x * 4;
                    int dstIndex = dstY * stride + dstX * 4;

                    // Only copy if pixel is opaque (part of selection)
                    if (_selectionState.SelectedPixels[srcIndex + 3] > 0)
                    {
                        pixels[dstIndex] = _selectionState.SelectedPixels[srcIndex];         // B
                        pixels[dstIndex + 1] = _selectionState.SelectedPixels[srcIndex + 1]; // G
                        pixels[dstIndex + 2] = _selectionState.SelectedPixels[srcIndex + 2]; // R
                        pixels[dstIndex + 3] = _selectionState.SelectedPixels[srcIndex + 3]; // A
                    }
                }
            }

            return CreateBitmapFromBuffer(pixels, width, height);
        }

        /// <summary>
        /// Renders selection overlay with dashed border (marching ants effect).
        /// </summary>
        public BitmapSource RenderSelectionOverlay(BitmapSource source)
        {
            if (!_selectionState.HasSelection || source == null)
            {
                return source;
            }

            BitmapSource bgra = EnsureBgra32(source);
            int width = bgra.PixelWidth;
            int height = bgra.PixelHeight;
            int stride = width * 4;

            byte[] pixels = new byte[stride * height];
            bgra.CopyPixels(pixels, stride, 0);

            // Draw selection border (dotted pattern)
            int dashLength = 4;
            Color borderColor1 = Colors.Black;
            Color borderColor2 = Colors.White;

            // For rectangle selection, draw border
            if (_selectionState.Type == SelectionType.Rectangle || _selectionState.Type == SelectionType.Lasso)
            {
                int left = Math.Max(0, _selectionState.OffsetX);
                int top = Math.Max(0, _selectionState.OffsetY);
                int right = Math.Min(width - 1, _selectionState.OffsetX + _selectionState.SelectedWidth);
                int bottom = Math.Min(height - 1, _selectionState.OffsetY + _selectionState.SelectedHeight);

                // Draw top and bottom edges
                for (int x = left; x <= right; x++)
                {
                    bool usePrimary = ((x / dashLength) % 2) == 0;
                    Color c = usePrimary ? borderColor1 : borderColor2;

                    // Top edge
                    if (top >= 0 && top < height)
                    {
                        int idx = top * stride + x * 4;
                        pixels[idx] = c.B;
                        pixels[idx + 1] = c.G;
                        pixels[idx + 2] = c.R;
                        pixels[idx + 3] = 255;
                    }

                    // Bottom edge
                    if (bottom >= 0 && bottom < height)
                    {
                        int idx = bottom * stride + x * 4;
                        pixels[idx] = c.B;
                        pixels[idx + 1] = c.G;
                        pixels[idx + 2] = c.R;
                        pixels[idx + 3] = 255;
                    }
                }

                // Draw left and right edges
                for (int y = top; y <= bottom; y++)
                {
                    bool usePrimary = ((y / dashLength) % 2) == 0;
                    Color c = usePrimary ? borderColor1 : borderColor2;

                    // Left edge
                    if (left >= 0 && left < width)
                    {
                        int idx = y * stride + left * 4;
                        pixels[idx] = c.B;
                        pixels[idx + 1] = c.G;
                        pixels[idx + 2] = c.R;
                        pixels[idx + 3] = 255;
                    }

                    // Right edge
                    if (right >= 0 && right < width)
                    {
                        int idx = y * stride + right * 4;
                        pixels[idx] = c.B;
                        pixels[idx + 1] = c.G;
                        pixels[idx + 2] = c.R;
                        pixels[idx + 3] = 255;
                    }
                }
            }

            return CreateBitmapFromBuffer(pixels, width, height);
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            _selectionState.Reset();
        }

        #endregion
    }
}
