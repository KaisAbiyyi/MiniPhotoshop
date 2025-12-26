using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    /// <summary>
    /// Watermarking operations for ImageEditor.
    /// </summary>
    internal sealed partial class ImageEditor
    {
        /// <summary>
        /// Applies a text watermark on top of the current image.
        /// Input: watermark text, opacity (0-1), font size, and position.
        /// Output: new bitmap with the text overlay.
        /// Algorithm: render original + text via DrawingVisual.
        /// </summary>
        public BitmapSource ApplyTextWatermark(string text, double opacity, double fontSize, WatermarkPosition position)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Watermark text cannot be empty.", nameof(text));
            }

            if (double.IsNaN(opacity) || double.IsInfinity(opacity))
            {
                throw new ArgumentException("Opacity must be a valid number.", nameof(opacity));
            }

            if (double.IsNaN(fontSize) || double.IsInfinity(fontSize) || fontSize <= 0)
            {
                throw new ArgumentException("Font size must be a positive number.", nameof(fontSize));
            }

            opacity = Math.Clamp(opacity, 0.0, 1.0);

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            var visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(source, new Rect(0, 0, width, height));

                var brush = new SolidColorBrush(Color.FromArgb((byte)Math.Round(opacity * 255), 255, 255, 255));
                brush.Freeze();

                var dpi = VisualTreeHelper.GetDpi(visual).PixelsPerDip;
                var formatted = new FormattedText(
                    text,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    fontSize,
                    brush,
                    dpi);

                const double margin = 12;
                Point pos = GetWatermarkPosition(position, width, height, formatted.Width, formatted.Height, margin);
                dc.DrawText(formatted, pos);
            }

            State.LastWatermark = new WatermarkInfo
            {
                Type = WatermarkType.Text,
                Text = text,
                Opacity = opacity,
                FontSize = fontSize,
                Position = position
            };

            return RenderVisualToBitmap(visual, width, height);
        }

        /// <summary>
        /// Applies an image watermark on top of the current image.
        /// Input: watermark bitmap, opacity (0-1), scale (0-1 of base width), and position.
        /// Output: new bitmap with the image overlay.
        /// Algorithm: render original + scaled watermark via DrawingVisual.
        /// </summary>
        public BitmapSource ApplyImageWatermark(BitmapSource watermark, double opacity, double scale, WatermarkPosition position)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (watermark == null)
            {
                throw new ArgumentNullException(nameof(watermark));
            }

            if (double.IsNaN(opacity) || double.IsInfinity(opacity))
            {
                throw new ArgumentException("Opacity must be a valid number.", nameof(opacity));
            }

            if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
            {
                throw new ArgumentException("Scale must be a positive number.", nameof(scale));
            }

            opacity = Math.Clamp(opacity, 0.0, 1.0);
            scale = Math.Clamp(scale, 0.05, 1.0);

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            BitmapSource watermarkSource = EnsureBgra32(watermark);

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            const double margin = 12;
            double maxWidth = Math.Max(1, width - (margin * 2));
            double maxHeight = Math.Max(1, height - (margin * 2));

            double targetWidth = Math.Min(maxWidth, width * scale);
            double aspect = watermarkSource.PixelWidth / (double)watermarkSource.PixelHeight;
            double targetHeight = targetWidth / aspect;

            if (targetHeight > maxHeight)
            {
                targetHeight = maxHeight;
                targetWidth = targetHeight * aspect;
            }

            var visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(source, new Rect(0, 0, width, height));
                dc.PushOpacity(opacity);

                Point pos = GetWatermarkPosition(position, width, height, targetWidth, targetHeight, margin);
                dc.DrawImage(watermarkSource, new Rect(pos.X, pos.Y, targetWidth, targetHeight));
                dc.Pop();
            }

            State.LastWatermark = new WatermarkInfo
            {
                Type = WatermarkType.Image,
                Image = watermarkSource,
                Opacity = opacity,
                Scale = scale,
                Position = position
            };

            return RenderVisualToBitmap(visual, width, height);
        }

        public WatermarkInfo? GetLastWatermarkInfo()
        {
            return State.LastWatermark;
        }

        private static Point GetWatermarkPosition(
            WatermarkPosition position,
            double containerWidth,
            double containerHeight,
            double contentWidth,
            double contentHeight,
            double margin)
        {
            double x = margin;
            double y = margin;

            switch (position)
            {
                case WatermarkPosition.TopRight:
                    x = containerWidth - contentWidth - margin;
                    y = margin;
                    break;
                case WatermarkPosition.BottomLeft:
                    x = margin;
                    y = containerHeight - contentHeight - margin;
                    break;
                case WatermarkPosition.BottomRight:
                    x = containerWidth - contentWidth - margin;
                    y = containerHeight - contentHeight - margin;
                    break;
                case WatermarkPosition.Center:
                    x = (containerWidth - contentWidth) / 2.0;
                    y = (containerHeight - contentHeight) / 2.0;
                    break;
                default:
                    x = margin;
                    y = margin;
                    break;
            }

            return new Point(Math.Max(0, x), Math.Max(0, y));
        }

        private static BitmapSource RenderVisualToBitmap(DrawingVisual visual, int width, int height)
        {
            var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);
            target.Freeze();
            return target;
        }
    }
}
