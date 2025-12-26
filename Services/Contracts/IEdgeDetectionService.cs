using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    /// <summary>
    /// Interface for Edge Detection operations on images.
    /// Implements various edge detection algorithms: Sobel, Prewitt, Robert, Canny.
    /// </summary>
    public interface IEdgeDetectionService
    {
        /// <summary>
        /// Applies Sobel edge detection (horizontal edges - Gy).
        /// Uses Sobel Y kernel to detect horizontal edges.
        /// Kernel: [1,2,1; 0,0,0; -1,-2,-1]
        /// </summary>
        BitmapSource ApplySobelHorizontalEdge();

        /// <summary>
        /// Applies Sobel edge detection (vertical edges - Gx).
        /// Uses Sobel X kernel to detect vertical edges.
        /// Kernel: [-1,0,1; -2,0,2; -1,0,1]
        /// </summary>
        BitmapSource ApplySobelVerticalEdge();

        /// <summary>
        /// Applies combined Sobel edge detection.
        /// Calculates magnitude G = sqrt(Gx² + Gy²) from both horizontal and vertical gradients.
        /// </summary>
        BitmapSource ApplySobelMagnitude();

        /// <summary>
        /// Applies Prewitt edge detection.
        /// Uses Px = [-1,0,1; -1,0,1; -1,0,1] and Py = [-1,-1,-1; 0,0,0; 1,1,1]
        /// Calculates magnitude G = sqrt(Gx² + Gy²)
        /// </summary>
        BitmapSource ApplyPrewitt();

        /// <summary>
        /// Applies Robert Cross edge detection.
        /// Uses 2x2 diagonal kernels: Rx = [1,0; 0,-1] and Ry = [0,1; -1,0]
        /// Gx = I(x,y) - I(x+1,y+1), Gy = I(x+1,y) - I(x,y+1)
        /// Calculates magnitude G = sqrt(Gx² + Gy²)
        /// </summary>
        BitmapSource ApplyRobert();

        /// <summary>
        /// Applies Canny edge detection - the optimal edge detector.
        /// 5-stage algorithm:
        /// 1. Gaussian Blur (noise reduction)
        /// 2. Gradient Calculation (Sobel)
        /// 3. Non-Maximum Suppression (thin edges)
        /// 4. Double Thresholding (strong/weak edges)
        /// 5. Edge Tracking by Hysteresis
        /// </summary>
        /// <param name="lowThreshold">Low threshold for weak edges (default: 50)</param>
        /// <param name="highThreshold">High threshold for strong edges (default: 150)</param>
        /// <param name="gaussianKernelSize">Gaussian kernel size - must be odd (default: 5)</param>
        /// <param name="sigma">Gaussian sigma value (default: 1.4)</param>
        BitmapSource ApplyCanny(double lowThreshold = 50, double highThreshold = 150, int gaussianKernelSize = 5, double sigma = 1.4);
    }
}
