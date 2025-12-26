using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.Contracts
{
    /// <summary>
    /// Provides watermarking operations for images.
    /// </summary>
    public interface IWatermarkService
    {
        /// <summary>
        /// Applies a text watermark over the current image.
        /// </summary>
        BitmapSource ApplyTextWatermark(string text, double opacity, double fontSize, WatermarkPosition position);

        /// <summary>
        /// Applies an image watermark over the current image.
        /// </summary>
        BitmapSource ApplyImageWatermark(BitmapSource watermark, double opacity, double scale, WatermarkPosition position);

        WatermarkInfo? GetLastWatermarkInfo();
    }
}
