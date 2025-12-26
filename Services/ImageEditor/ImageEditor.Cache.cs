using System;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        /// <summary>
        /// Rebuilds workspace caches (filter cache, pixel cache, dimensions) for a new bitmap.
        /// Input: updated bitmap. Output: refreshed workspace state.
        /// Algorithm: ensure Bgra32 then rebuild pixel cache and filter cache.
        /// </summary>
        public void RefreshWorkspaceCaches(BitmapSource bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            BitmapSource normalized = EnsureBgra32(bitmap);

            State.OriginalBitmap = normalized;
            State.FilterCache.Clear();
            State.FilterCache[ImageFilterMode.Original] = normalized;
            State.PixelCache = ExtractPixelCache(normalized);
            State.CachedWidth = normalized.PixelWidth;
            State.CachedHeight = normalized.PixelHeight;
        }
    }
}
