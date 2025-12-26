namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Tracks transient data for brightness adjustments.
    /// </summary>
    public sealed class BrightnessState
    {
        public int[,,]? Buffer { get; set; }

        public double PreviousValue { get; set; }

        public void Reset()
        {
            Buffer = null;
            PreviousValue = 0;
        }
    }
}

