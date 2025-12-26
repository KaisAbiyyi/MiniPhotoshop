using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    /// <summary>
    /// Interface for convolution operations on images.
    /// Handles local area operations using kernel matrices.
    /// </summary>
    public interface IConvolutionService
    {
        /// <summary>
        /// Applies convolution operation using a custom kernel matrix.
        /// Formula: G(x,y) = Round(C × ΣΣ K(i,j)·I(x+i, y+j))
        /// </summary>
        /// <param name="kernel">The kernel/filter matrix (must be odd-sized square matrix)</param>
        /// <param name="multiplier">Constant multiplier C (e.g., 1/9 for averaging)</param>
        /// <returns>Convolved image</returns>
        BitmapSource ApplyConvolution(double[,] kernel, double multiplier = 1.0);

        /// <summary>
        /// Applies Low Pass Filter (smoothing/blur).
        /// Uses averaging kernel for noise reduction.
        /// </summary>
        BitmapSource ApplyLowPassFilter(int kernelSize = 3);

        /// <summary>
        /// Applies High Pass Filter (sharpening/edge detection).
        /// Enhances edges and fine details.
        /// </summary>
        BitmapSource ApplyHighPassFilter(int kernelSize = 3);

        /// <summary>
        /// Applies Gaussian Blur filter.
        /// Uses weighted average based on Gaussian distribution.
        /// </summary>
        BitmapSource ApplyGaussianBlur(int kernelSize = 3, double sigma = 1.0);

        /// <summary>
        /// Applies Sobel edge detection (horizontal).
        /// </summary>
        BitmapSource ApplySobelHorizontal();

        /// <summary>
        /// Applies Sobel edge detection (vertical).
        /// </summary>
        BitmapSource ApplySobelVertical();

        /// <summary>
        /// Applies Laplacian edge detection.
        /// </summary>
        BitmapSource ApplyLaplacian();

        /// <summary>
        /// Applies emboss effect.
        /// </summary>
        BitmapSource ApplyEmboss();
    }
}
