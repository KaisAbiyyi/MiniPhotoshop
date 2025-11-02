using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface INegationService
    {
        BitmapSource SetNegationActive(bool isActive);
    }
}
