using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IArithmeticService
    {
        BitmapSource AddImage(BitmapSource overlay, int offsetX, int offsetY);

        BitmapSource SubtractImage(BitmapSource overlay, int offsetX, int offsetY);

        BitmapSource MultiplyImage(BitmapSource overlay, int offsetX, int offsetY, out string normalizationInfo);

        BitmapSource DivideImage(BitmapSource overlay, int offsetX, int offsetY, out string normalizationInfo);

        BitmapSource MultiplyByScalar(double scalar);

        BitmapSource DivideByScalar(double scalar);

        BitmapSource RestoreArithmeticBase();

        void ClearArithmeticSnapshot();
    }
}
