using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IColorSelectionService
    {
        BitmapSource SetActive(bool isActive);

        BitmapSource ApplySelection(int pixelX, int pixelY);
    }
}

