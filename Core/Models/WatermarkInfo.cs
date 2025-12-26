using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Stores metadata for the last applied watermark.
    /// </summary>
    public sealed class WatermarkInfo
    {
        public WatermarkType Type { get; init; }

        public string? Text { get; init; }

        public BitmapSource? Image { get; init; }

        public double Opacity { get; init; }

        public double FontSize { get; init; }

        public double Scale { get; init; }

        public WatermarkPosition Position { get; init; }
    }
}
