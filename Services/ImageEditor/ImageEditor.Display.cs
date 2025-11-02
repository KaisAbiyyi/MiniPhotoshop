using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public BitmapSource GetCurrentImage()
        {
            return GetProcessedBitmap();
        }
    }
}

