using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    /// <summary>
    /// Interface for canvas management operations.
    /// Interface Segregation: Only canvas-specific operations.
    /// </summary>
    public interface ICanvasService
    {
        /// <summary>
        /// Initializes a new canvas with specified dimensions and background color.
        /// </summary>
        void InitializeCanvas(int width, int height, Color backgroundColor);

        /// <summary>
        /// Updates the canvas size while preserving existing content.
        /// </summary>
        void UpdateCanvasSize(int newWidth, int newHeight);

        /// <summary>
        /// Updates the canvas background color.
        /// </summary>
        void UpdateCanvasBackground(Color newColor);

        /// <summary>
        /// Renders the current canvas with any placed images.
        /// </summary>
        BitmapSource? RenderCanvas();

        /// <summary>
        /// Places an image onto the canvas at position (0,0).
        /// </summary>
        BitmapSource PlaceImageOnCanvas(BitmapSource image);

        /// <summary>
        /// Gets the current canvas dimensions.
        /// </summary>
        (int Width, int Height) GetCanvasDimensions();
    }
}
