using System.Collections.Generic;
using System.Windows;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Represents the selection state on canvas.
    /// Single Responsibility: Only holds selection-related state data.
    /// </summary>
    public sealed class SelectionState
    {
        /// <summary>
        /// Whether there is an active selection.
        /// </summary>
        public bool HasSelection { get; set; }

        /// <summary>
        /// Type of selection (None, Rectangle, Lasso).
        /// </summary>
        public SelectionType Type { get; set; } = SelectionType.None;

        /// <summary>
        /// Bounding rectangle of the selection.
        /// </summary>
        public Rect Bounds { get; set; }

        /// <summary>
        /// Points for lasso selection polygon.
        /// </summary>
        public List<Point> LassoPoints { get; set; } = new();

        /// <summary>
        /// Selection mask buffer (true = selected, false = not selected).
        /// Array size matches image dimensions.
        /// </summary>
        public bool[,] Mask { get; set; } = new bool[0, 0];

        /// <summary>
        /// Pixel buffer for selected area (for move operation).
        /// Stores BGRA values.
        /// </summary>
        public byte[] SelectedPixels { get; set; } = System.Array.Empty<byte>();

        /// <summary>
        /// Width of selected area.
        /// </summary>
        public int SelectedWidth { get; set; }

        /// <summary>
        /// Height of selected area.
        /// </summary>
        public int SelectedHeight { get; set; }

        /// <summary>
        /// Offset X of selection relative to canvas.
        /// </summary>
        public int OffsetX { get; set; }

        /// <summary>
        /// Offset Y of selection relative to canvas.
        /// </summary>
        public int OffsetY { get; set; }

        public void Reset()
        {
            HasSelection = false;
            Type = SelectionType.None;
            Bounds = Rect.Empty;
            LassoPoints.Clear();
            Mask = new bool[0, 0];
            SelectedPixels = System.Array.Empty<byte>();
            SelectedWidth = 0;
            SelectedHeight = 0;
            OffsetX = 0;
            OffsetY = 0;
        }
    }

    /// <summary>
    /// Types of selection tools.
    /// </summary>
    public enum SelectionType
    {
        None,
        Rectangle,
        Lasso
    }
}
