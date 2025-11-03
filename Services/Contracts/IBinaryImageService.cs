using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IBinaryImageService
    {
        BitmapSource ToBinary(int threshold);

        BitmapSource AndImage(BitmapSource overlay, int offsetX, int offsetY);

        BitmapSource OrImage(BitmapSource overlay, int offsetX, int offsetY);

        BitmapSource NotImage();

        BitmapSource XorImage(BitmapSource overlay, int offsetX, int offsetY);

        BitmapSource RestoreBinaryBase();

        void ClearBinarySnapshot();
    }
}
