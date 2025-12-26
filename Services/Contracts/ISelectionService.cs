using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.Contracts
{
    /// <summary>
    /// Interface for selection operations.
    /// Interface Segregation: Only selection-specific operations.
    /// </summary>
    public interface ISelectionService
    {
        /// <summary>
        /// Gets the current selection state.
        /// </summary>
        SelectionState SelectionState { get; }

        /// <summary>
        /// Creates a rectangular selection.
        /// Validates boundary: x1,y1 >= 0 and x2,y2 <= image dimensions.
        /// </summary>
        void CreateRectangleSelection(int x1, int y1, int x2, int y2);

        /// <summary>
        /// Creates a lasso (freeform) selection from points.
        /// Uses Ray Casting algorithm for point-in-polygon test.
        /// </summary>
        void CreateLassoSelection(List<Point> points);

        /// <summary>
        /// Checks if a point is inside the current selection.
        /// For lasso: uses Ray Casting algorithm.
        /// </summary>
        bool IsPointInSelection(int x, int y);

        /// <summary>
        /// Moves the current selection by delta.
        /// Formula: Pos_new = Pos_old + Δ
        /// </summary>
        void MoveSelection(int deltaX, int deltaY);

        /// <summary>
        /// Applies the selection move and renders result.
        /// Fills original area with background color.
        /// </summary>
        BitmapSource ApplySelectionMove();

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        void ClearSelection();

        /// <summary>
        /// Renders the selection overlay (marching ants or highlight).
        /// </summary>
        BitmapSource RenderSelectionOverlay(BitmapSource source);

        /// <summary>
        /// Copies selected pixels to buffer.
        /// </summary>
        void CopySelectedPixels();
    }
}
