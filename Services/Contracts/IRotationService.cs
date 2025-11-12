using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IRotationService
    {
        BitmapSource Rotate45();
        BitmapSource Rotate90();
        BitmapSource Rotate180();
        BitmapSource Rotate270();
        BitmapSource RotateCustom(double degrees);
        BitmapSource RestoreOriginal();
        void ClearRotationSnapshot();
    }
}
