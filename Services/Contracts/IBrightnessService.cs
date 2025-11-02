using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IBrightnessService
    {
        void Reset();

        BitmapSource Update(double newValue);
    }
}

