using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public BitmapSource SetActiveFilter(ImageFilterMode mode)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar yang dimuat.");
            }

            State.ActiveFilter = mode;
            State.Brightness.Reset();
            return GetProcessedBitmap();
        }

        public BitmapSource GetProcessedBitmap()
        {
            var source = GetFilteredBitmap(State.ActiveFilter);
            source = ApplyNegationIfNeeded(source);
            source = ApplyBrightnessIfNeeded(source);

            if (State.IsBinaryThresholdActive)
            {
                source = ApplyBinaryThreshold(source, State.BinaryThresholdValue);
            }

            if (State.ColorSelection.IsActive)
            {
                source = ApplyColorSelection(source);
            }

            return source;
        }

        public IReadOnlyList<PreviewItem> BuildPreviews()
        {
            State.PreviewItems.Clear();
            if (State.OriginalBitmap == null)
            {
                return State.PreviewItems;
            }

            AddPreview(ImageFilterMode.Original, "Normal");
            AddPreview(ImageFilterMode.RedOnly, "Red Only");
            AddPreview(ImageFilterMode.GreenOnly, "Green Only");
            AddPreview(ImageFilterMode.BlueOnly, "Blue Only");
            AddPreview(ImageFilterMode.Grayscale, "Grayscale");
            SyncPreviewActivation();
            return State.PreviewItems;
        }

        public void SyncPreviewActivation()
        {
            foreach (var item in State.PreviewItems)
            {
                item.IsActive = item.Mode == State.ActiveFilter;
            }
        }

        private void AddPreview(ImageFilterMode mode, string title)
        {
            BitmapSource full = GetFilteredBitmap(mode);
            BitmapSource preview = CreateThumbnail(full, PreviewThumbnailSize);
            State.PreviewItems.Add(new PreviewItem(mode, title, preview, mode == State.ActiveFilter));
        }

        private BitmapSource GetFilteredBitmap(ImageFilterMode mode)
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("Tidak ada gambar yang dimuat.");
            }

            if (State.FilterCache.TryGetValue(mode, out var cached))
            {
                return cached;
            }

            BitmapSource result = mode switch
            {
                ImageFilterMode.RedOnly => CreateFilteredBitmap((r, _, _, _) => (r, (byte)0, (byte)0)),
                ImageFilterMode.GreenOnly => CreateFilteredBitmap((_, g, _, _) => ((byte)0, g, (byte)0)),
                ImageFilterMode.BlueOnly => CreateFilteredBitmap((_, _, b, _) => ((byte)0, (byte)0, b)),
                ImageFilterMode.Grayscale => CreateFilteredBitmap((r, g, b, gray) => (gray, gray, gray)),
                _ => State.OriginalBitmap
            };

            State.FilterCache[mode] = result;
            return result;
        }

        private BitmapSource CreateFilteredBitmap(Func<byte, byte, byte, byte, (byte R, byte G, byte B)> selector)
        {
            if (State.PixelCache == null)
            {
                throw new InvalidOperationException("Pixel cache belum diinisialisasi.");
            }

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            var cache = State.PixelCache;

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = rowOffset + (x * 4);
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];
                    byte gray = cache[x, y, 3];

                    (byte rr, byte gg, byte bb) = selector(r, g, b, gray);
                    buffer[offset] = bb;
                    buffer[offset + 1] = gg;
                    buffer[offset + 2] = rr;
                    buffer[offset + 3] = cache[x, y, 4];
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }
    }
}
