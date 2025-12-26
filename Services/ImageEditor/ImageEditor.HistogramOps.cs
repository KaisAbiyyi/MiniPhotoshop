using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        /// <summary>
        /// Applies global histogram equalization per-channel (R,G,B) and returns new bitmap.
        /// </summary>
        public BitmapSource ApplyHistogramEqualization()
        {
            if (State.PixelCache == null)
                throw new InvalidOperationException("Tidak ada data pixel untuk operasi histogram.");

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            int total = width * height;

            var cache = State.PixelCache;
            var histR = new int[256];
            var histG = new int[256];
            var histB = new int[256];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                histR[cache[x, y, 0]]++;
                histG[cache[x, y, 1]]++;
                histB[cache[x, y, 2]]++;
            }

            byte[] mapR = BuildEqualizationMap(histR, total);
            byte[] mapG = BuildEqualizationMap(histG, total);
            byte[] mapB = BuildEqualizationMap(histB, total);

            int stride = width * 4;
            byte[] buffer = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                int row = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int off = row + x * 4;
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];

                    buffer[off]     = mapB[b];
                    buffer[off + 1] = mapG[g];
                    buffer[off + 2] = mapR[r];
                    buffer[off + 3] = cache[x, y, 4]; // alpha
                }
            }

            var bmp = CreateBitmapFromBuffer(buffer, width, height);
            State.OriginalBitmap = bmp;
            State.FilterCache[MiniPhotoshop.Core.Enums.ImageFilterMode.Original] = bmp;
            State.PixelCache = ExtractPixelCache(bmp);
            State.CachedWidth = bmp.PixelWidth;
            State.CachedHeight = bmp.PixelHeight;
            return bmp;
        }

        /// <summary>
        /// Applies linear contrast stretching per-channel.
        /// </summary>
        public BitmapSource ApplyLinearStretchEqualization()
        {
            if (State.PixelCache == null)
                throw new InvalidOperationException("Tidak ada data pixel untuk operasi histogram.");

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            var cache = State.PixelCache;

            byte minR = 255, minG = 255, minB = 255;
            byte maxR = 0,   maxG = 0,   maxB = 0;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                byte r = cache[x, y, 0];
                byte g = cache[x, y, 1];
                byte b = cache[x, y, 2];

                if (r < minR) minR = r;
                if (g < minG) minG = g;
                if (b < minB) minB = b;

                if (r > maxR) maxR = r;
                if (g > maxG) maxG = g;
                if (b > maxB) maxB = b;
            }

            byte[] mapR = BuildLinearMap(minR, maxR);
            byte[] mapG = BuildLinearMap(minG, maxG);
            byte[] mapB = BuildLinearMap(minB, maxB);

            int stride = width * 4;
            byte[] buffer = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                int row = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int off = row + x * 4;
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];

                    buffer[off]     = mapB[b];
                    buffer[off + 1] = mapG[g];
                    buffer[off + 2] = mapR[r];
                    buffer[off + 3] = cache[x, y, 4]; // alpha
                }
            }

            var bmp = CreateBitmapFromBuffer(buffer, width, height);
            State.OriginalBitmap = bmp;
            State.FilterCache[MiniPhotoshop.Core.Enums.ImageFilterMode.Original] = bmp;
            State.PixelCache = ExtractPixelCache(bmp);
            State.CachedWidth = bmp.PixelWidth;
            State.CachedHeight = bmp.PixelHeight;
            return bmp;
        }

        /// <summary>
        /// AHE murni (per-pixel local window).
        /// windowSize harus ganjil (contoh: 31, 51, 63). Semakin besar -> makin mirip global HE.
        /// </summary>
        public BitmapSource ApplyAdaptiveHistogramEqualization(int windowSize = 63)
        {
            // Refactored to use the split methods to avoid code duplication
            var (buffer, width, height) = ComputeAdaptiveHistogramBuffer(windowSize);
            return ApplyAdaptiveHistogramFromBuffer(buffer, width, height);
        }

        /// <summary>
        /// Computes the pixel buffer for AHE/CLAHE. Designed to be run on a background thread.
        /// Uses tile-based interpolation so each pixel only looks up a handful of LUTs instead of rebuilding a
        /// histogram per-pixel, reducing the work for larger windows.
        /// </summary>
        public (byte[] buffer, int width, int height) ComputeAdaptiveHistogramBuffer(int windowSize)
        {
            if (State.PixelCache == null)
                throw new InvalidOperationException("Tidak ada data pixel untuk operasi histogram.");

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            var cache = State.PixelCache;

            int minTile = 16;
            int maxTile = Math.Max(minTile, Math.Max(width, height));
            windowSize = Math.Clamp(windowSize, minTile, maxTile);
            int tileSize = Math.Max(1, windowSize);
            int tileCols = Math.Max(1, (width + tileSize - 1) / tileSize);
            int tileRows = Math.Max(1, (height + tileSize - 1) / tileSize);

            int[] columnWidths = ComputeTileSizes(width, tileSize, tileCols);
            int[] rowHeights = ComputeTileSizes(height, tileSize, tileRows);

            byte[][] mapR = BuildTileMaps(cache, width, height, tileSize, tileCols, tileRows, channelIndex: 0);
            byte[][] mapG = BuildTileMaps(cache, width, height, tileSize, tileCols, tileRows, channelIndex: 1);
            byte[][] mapB = BuildTileMaps(cache, width, height, tileSize, tileCols, tileRows, channelIndex: 2);

            int stride = width * 4;
            byte[] buffer = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                int row = y * stride;
                int ty = Math.Min(tileRows - 1, y / tileSize);
                int ty2 = Math.Min(tileRows - 1, ty + 1);
                int yOffset = ty * tileSize;
                double yWeight = rowHeights[ty] > 0
                    ? Math.Clamp((double)(y - yOffset) / rowHeights[ty], 0.0, 1.0)
                    : 0.0;

                for (int x = 0; x < width; x++)
                {
                    int tx = Math.Min(tileCols - 1, x / tileSize);
                    int tx2 = Math.Min(tileCols - 1, tx + 1);
                    int xOffset = tx * tileSize;
                    double xWeight = columnWidths[tx] > 0
                        ? Math.Clamp((double)(x - xOffset) / columnWidths[tx], 0.0, 1.0)
                        : 0.0;

                    int off = row + x * 4;
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];

                    buffer[off] = InterpolateHistogramValue(mapB, tileCols, tx, ty, tx2, ty2, xWeight, yWeight, b);
                    buffer[off + 1] = InterpolateHistogramValue(mapG, tileCols, tx, ty, tx2, ty2, xWeight, yWeight, g);
                    buffer[off + 2] = InterpolateHistogramValue(mapR, tileCols, tx, ty, tx2, ty2, xWeight, yWeight, r);
                    buffer[off + 3] = cache[x, y, 4];
                }
            }

            return (buffer, width, height);
        }

        /// <summary>
        /// Applies the computed buffer to the application state and returns the bitmap. Must run on UI thread.
        /// </summary>
        public BitmapSource ApplyAdaptiveHistogramFromBuffer(byte[] buffer, int width, int height)
        {
            var bmp = CreateBitmapFromBuffer(buffer, width, height);
            State.OriginalBitmap = bmp;
            State.FilterCache[MiniPhotoshop.Core.Enums.ImageFilterMode.Original] = bmp;
            State.PixelCache = ExtractPixelCache(bmp);
            State.CachedWidth = bmp.PixelWidth;
            State.CachedHeight = bmp.PixelHeight;
            return bmp;
        }

        // =========================
        // Helpers
        // =========================

        private static byte[][] BuildTileMaps(
            byte[,,] cache,
            int width,
            int height,
            int tileSize,
            int tileCols,
            int tileRows,
            int channelIndex)
        {
            int tileCount = tileCols * tileRows;
            var maps = new byte[tileCount][];

            for (int ty = 0; ty < tileRows; ty++)
            {
                int yStart = ty * tileSize;
                int tileHeight = Math.Min(tileSize, Math.Max(0, height - yStart));

                for (int tx = 0; tx < tileCols; tx++)
                {
                    int xStart = tx * tileSize;
                    int tileWidth = Math.Min(tileSize, Math.Max(0, width - xStart));
                    var hist = new int[256];
                    int total = 0;

                    for (int y = yStart; y < yStart + tileHeight; y++)
                    for (int x = xStart; x < xStart + tileWidth; x++)
                    {
                        hist[cache[x, y, channelIndex]]++;
                        total++;
                    }

                    maps[ty * tileCols + tx] = BuildEqualizationMap(hist, total);
                }
            }

            return maps;
        }

        private static int[] ComputeTileSizes(int totalSize, int tileSize, int tileCount)
        {
            var sizes = new int[tileCount];
            for (int i = 0; i < tileCount; i++)
            {
                int start = i * tileSize;
                int size = Math.Min(tileSize, Math.Max(0, totalSize - start));
                sizes[i] = size > 0 ? size : tileSize;
            }
            return sizes;
        }

        private static byte InterpolateHistogramValue(
            byte[][] maps,
            int tileCols,
            int tx,
            int ty,
            int tx2,
            int ty2,
            double xWeight,
            double yWeight,
            byte value)
        {
            int tlIndex = ty * tileCols + tx;
            int trIndex = ty * tileCols + tx2;
            int blIndex = ty2 * tileCols + tx;
            int brIndex = ty2 * tileCols + tx2;

            double tl = maps[tlIndex][value];
            double tr = maps[trIndex][value];
            double bl = maps[blIndex][value];
            double br = maps[brIndex][value];

            double wTL = (1.0 - xWeight) * (1.0 - yWeight);
            double wTR = xWeight * (1.0 - yWeight);
            double wBL = (1.0 - xWeight) * yWeight;
            double wBR = xWeight * yWeight;

            int interpolated = (int)Math.Round(tl * wTL + tr * wTR + bl * wBL + br * wBR);
            return (byte)Math.Clamp(interpolated, 0, 255);
        }

        private static byte[] BuildEqualizationMap(int[] hist, int total)
        {
            var map = new byte[256];
            if (total <= 0)
            {
                for (int i = 0; i < 256; i++) map[i] = (byte)i;
                return map;
            }

            double[] cdf = new double[256];
            double cum = 0;
            for (int i = 0; i < 256; i++)
            {
                cum += (double)hist[i] / total;
                cdf[i] = cum;
            }

            for (int i = 0; i < 256; i++)
                map[i] = (byte)Math.Clamp((int)Math.Round(cdf[i] * 255.0), 0, 255);

            return map;
        }

        private static byte[] BuildLinearMap(byte min, byte max)
        {
            var map = new byte[256];
            if (max <= min)
            {
                for (int i = 0; i < 256; i++) map[i] = (byte)i;
                return map;
            }

            double denom = max - min;
            for (int i = 0; i < 256; i++)
            {
                double val = (i - min) / denom * 255.0;
                map[i] = (byte)Math.Clamp((int)Math.Round(val), 0, 255);
            }
            return map;
        }
    }
}