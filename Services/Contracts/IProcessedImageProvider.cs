using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IProcessedImageProvider
    {
        BitmapSource GetCurrentImage();
    }
}

