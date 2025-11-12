using System;
using System.IO;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public ImageLoadResult Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path must be provided.", nameof(filePath));
            }

            ClearArithmeticSnapshot();
            State.Reset();

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            State.OriginalBitmap = bitmap;
            State.FilterCache[ImageFilterMode.Original] = bitmap;
            State.PixelCache = ExtractPixelCache(bitmap);
            State.CachedWidth = bitmap.PixelWidth;
            State.CachedHeight = bitmap.PixelHeight;
            State.CurrentFilePath = filePath;
            State.ActiveFilter = ImageFilterMode.Original;

            BuildPreviews();
            Build();

            return new ImageLoadResult(
                bitmap,
                filePath,
                bitmap.PixelWidth,
                bitmap.PixelHeight,
                bitmap.Format.ToString()
            );
        }
    }
}

