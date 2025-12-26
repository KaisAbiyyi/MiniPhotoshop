using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    /// <summary>
    /// Provides contrast enhancement operations on images.
    /// </summary>
    public interface IContrastEnhancementService
    {
        /// <summary>
        /// Applies linear contrast transformation: s = a * r + b.
        /// </summary>
        BitmapSource ApplyLinearContrast(double slope, double intercept);

        /// <summary>
        /// Applies gamma correction: s = c * r^gamma.
        /// </summary>
        BitmapSource ApplyGammaCorrection(double gamma, double gain = 1.0);

        /// <summary>
        /// Applies adaptive local contrast enhancement using a local window.
        /// </summary>
        BitmapSource ApplyAdaptiveContrast(int windowSize = 3, double gain = 1.0);

        /// <summary>
        /// Applies global contrast stretching based on min/max intensity.
        /// </summary>
        BitmapSource ApplyGlobalContrastStretching();
    }
}
