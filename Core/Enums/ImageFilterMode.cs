using System;

namespace MiniPhotoshop.Core.Enums
{
    /// <summary>
    /// Represents the supported base filter modes that can be composed with additional image processing steps.
    /// </summary>
    public enum ImageFilterMode
    {
        Original,
        RedOnly,
        GreenOnly,
        BlueOnly,
        Grayscale
    }
}

