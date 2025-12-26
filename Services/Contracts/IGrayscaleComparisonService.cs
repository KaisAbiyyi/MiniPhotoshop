using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IGrayscaleComparisonService
    {
        BitmapSource CreateAverageGrayscale();

        BitmapSource CreateLuminanceGrayscale();
    }
}

