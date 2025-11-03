using MiniPhotoshop.Core.Models;
using MiniPhotoshop.Services.Contracts;

namespace MiniPhotoshop.Services.ImageEditor
{
    /// <summary>
    /// Coordinates image editing operations over a shared <see cref="ImageWorkspaceState"/>.
    /// Each functional slice is implemented in a dedicated partial file to keep responsibilities isolated.
    /// </summary>
    internal sealed partial class ImageEditor :
        IImageLoaderService,
        IPixelExportService,
        IFilterService,
        INegationService,
        IBrightnessService,
        IBinaryThresholdService,
        IColorSelectionService,
        IHistogramService,
        IProcessedImageProvider,
        IWorkspaceResetService,
        IGrayscaleComparisonService,
        IArithmeticService,
        IBinaryImageService
    {
        private const int PreviewThumbnailSize = 160;

        public ImageEditor(ImageWorkspaceState state)
        {
            State = state;
        }

        public ImageWorkspaceState State { get; }
    }
}
