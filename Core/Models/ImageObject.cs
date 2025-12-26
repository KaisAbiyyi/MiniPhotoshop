using System;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Represents a single image object on the canvas.
    /// </summary>
    public class ImageObject
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public BitmapSource Bitmap { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int Width => Bitmap?.PixelWidth ?? 0;
        public int Height => Bitmap?.PixelHeight ?? 0;
        public bool IsSelected { get; set; }
        public int ZIndex { get; set; }
        public bool IsVisible { get; set; } = true;
        public double Opacity { get; set; } = 1.0;

        public ImageObject(BitmapSource bitmap, string name = "Layer")
        {
            Bitmap = bitmap;
            Name = name;
        }
    }
}
