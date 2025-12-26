using System;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Stores histogram counts for the RGB and grayscale channels.
    /// </summary>
    public sealed class HistogramData
    {
        public int[] Red { get; private set; } = Array.Empty<int>();

        public int[] Green { get; private set; } = Array.Empty<int>();

        public int[] Blue { get; private set; } = Array.Empty<int>();

        public int[] Gray { get; private set; } = Array.Empty<int>();

        public void SetChannels(int[] red, int[] green, int[] blue, int[] gray)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Gray = gray;
        }

        public void Reset()
        {
            Red = Array.Empty<int>();
            Green = Array.Empty<int>();
            Blue = Array.Empty<int>();
            Gray = Array.Empty<int>();
        }
    }
}

