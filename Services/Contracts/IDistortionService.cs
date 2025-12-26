using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IDistortionService
    {
        BitmapSource ApplyDistortion(int level);
        BitmapSource RestoreDistortion();
        void ClearDistortionSnapshot();
    }
}
