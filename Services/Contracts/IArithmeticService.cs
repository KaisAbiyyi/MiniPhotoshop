using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IArithmeticService
    {
        BitmapSource AddImage(BitmapSource overlay, int offsetX, int offsetY);

        BitmapSource SubtractImage(BitmapSource overlay, int offsetX, int offsetY);

        BitmapSource MultiplyByScalar(double scalar);

        BitmapSource DivideByScalar(double scalar);

        BitmapSource RestoreArithmeticBase();

        void ClearArithmeticSnapshot();
    }
}
