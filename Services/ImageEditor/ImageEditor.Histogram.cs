using System;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public HistogramData Build()
        {
            if (State.PixelCache == null)
            {
                throw new InvalidOperationException("Tidak ada data pixel untuk membuat histogram.");
            }

            var red = new int[256];
            var green = new int[256];
            var blue = new int[256];
            var gray = new int[256];
            var cache = State.PixelCache;

            int width = State.CachedWidth;
            int height = State.CachedHeight;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    red[cache[x, y, 0]]++;
                    green[cache[x, y, 1]]++;
                    blue[cache[x, y, 2]]++;
                    gray[cache[x, y, 3]]++;
                }
            }

            State.Histogram.SetChannels(red, green, blue, gray);
            return State.Histogram;
        }
    }
}

