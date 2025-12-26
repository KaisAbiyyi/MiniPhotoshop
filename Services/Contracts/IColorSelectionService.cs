using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IColorSelectionService
    {
        BitmapSource SetColorSelectionActive(bool isActive);

        BitmapSource ApplySelection(int pixelX, int pixelY);

        BitmapSource UpdateTolerance(double tolerancePercent);
    }
}

