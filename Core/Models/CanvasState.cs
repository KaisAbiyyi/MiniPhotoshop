using System.Windows.Media;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Represents the canvas configuration state.
    /// Single Responsibility: Only holds canvas-related state data.
    /// </summary>
    public sealed class CanvasState
    {
        public const int DefaultWidth = 800;
        public const int DefaultHeight = 600;
        public const int DefaultImageOffsetX = 0;
        public const int DefaultImageOffsetY = 0;
        public static readonly Color DefaultBackgroundColor = Colors.White;

        public int Width { get; set; } = DefaultWidth;
        public int Height { get; set; } = DefaultHeight;
        public Color BackgroundColor { get; set; } = DefaultBackgroundColor;
        public int ImageOffsetX { get; set; } = DefaultImageOffsetX;
        public int ImageOffsetY { get; set; } = DefaultImageOffsetY;
        public bool IsInitialized { get; set; }

        public void Reset()
        {
            Width = DefaultWidth;
            Height = DefaultHeight;
            BackgroundColor = DefaultBackgroundColor;
            ImageOffsetX = DefaultImageOffsetX;
            ImageOffsetY = DefaultImageOffsetY;
            IsInitialized = false;
        }
    }
}
