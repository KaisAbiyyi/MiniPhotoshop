using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IBinaryThresholdService
    {
        BitmapSource SetActive(bool isActive);

        BitmapSource UpdateThreshold(int thresholdValue);
    }
}

