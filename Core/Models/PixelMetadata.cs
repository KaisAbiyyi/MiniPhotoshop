using System;

namespace MiniPhotoshop.Core.Models
{
    public struct PixelMetadata
    {
        public Guid? ImageObjectId { get; set; }
        public int LocalX { get; set; }
        public int LocalY { get; set; }
    }
}
