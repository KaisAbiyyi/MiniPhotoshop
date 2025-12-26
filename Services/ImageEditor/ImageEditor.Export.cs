using System;
using System.Globalization;
using System.IO;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public void Export(string filePath)
        {
            if (State.PixelCache == null)
            {
                throw new InvalidOperationException("Tidak ada data pixel yang dapat diekspor.");
            }

            int width = State.CachedWidth;
            int height = State.CachedHeight;
            var cache = State.PixelCache;

            using var writer = new StreamWriter(filePath);
            writer.WriteLine($"# Data pixel untuk {Path.GetFileName(State.CurrentFilePath ?? "gambar")}");
            writer.WriteLine($"# Dimensi: [{width}][{height}][5]");
            writer.WriteLine("# Format: [x][y][channel] dimana channel: 0=Red, 1=Green, 2=Blue, 3=Grayscale, 4=Alpha");
            writer.WriteLine();
            writer.WriteLine("[");

            for (int x = 0; x < width; x++)
            {
                writer.WriteLine("  [");
                for (int y = 0; y < height; y++)
                {
                    byte r = cache[x, y, 0];
                    byte g = cache[x, y, 1];
                    byte b = cache[x, y, 2];
                    byte gray = cache[x, y, 3];
                    byte alpha = cache[x, y, 4];

                    string comma = y < height - 1 ? "," : string.Empty;
                    writer.WriteLine($"    [{r}, {g}, {b}, {gray}, {alpha}]{comma}");
                }

                string arrayComma = x < width - 1 ? "," : string.Empty;
                writer.WriteLine($"  ]{arrayComma}");
            }

            writer.WriteLine("]");
        }
    }
}

