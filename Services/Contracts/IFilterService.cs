using System.Collections.Generic;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IFilterService
    {
        BitmapSource SetActiveFilter(ImageFilterMode mode);

        BitmapSource GetProcessedBitmap();

        IReadOnlyList<PreviewItem> BuildPreviews();

        void SyncPreviewActivation();
    }
}

