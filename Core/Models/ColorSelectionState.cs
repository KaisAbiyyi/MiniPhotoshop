using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Holds runtime data for the color selection feature.
    /// </summary>
    public sealed class ColorSelectionState
    {
        public bool IsActive { get; set; }

        public byte TargetR { get; set; }

        public byte TargetG { get; set; }

        public byte TargetB { get; set; }

        public BitmapSource? OriginalBeforeSelection { get; set; }

        public bool HasTarget { get; set; }

        public void Reset()
        {
            IsActive = false;
            TargetR = TargetG = TargetB = 0;
            OriginalBeforeSelection = null;
            HasTarget = false;
        }
    }
}
