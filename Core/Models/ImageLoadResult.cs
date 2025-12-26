using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Holds metadata and bitmap information returned after loading an image from disk.
    /// </summary>
    public sealed record ImageLoadResult(
        BitmapSource Bitmap,
        string FilePath,
        int Width,
        int Height,
        string PixelFormatDescription
    );
}

