using System;
using System.Collections.Generic;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Represents a canvas size preset for quick selection.
    /// </summary>
    public class CanvasPreset
    {
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public string Category { get; set; } = "Custom";
        public string Description { get; set; } = string.Empty;

        public CanvasPreset() { }

        public CanvasPreset(string name, int width, int height, string category = "Custom", string description = "")
        {
            Name = name;
            Width = width;
            Height = height;
            Category = category;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Name} ({Width}×{Height})";
        }

        /// <summary>
        /// Common canvas presets organized by category.
        /// </summary>
        public static class Presets
        {
            // Social Media
            public static CanvasPreset InstagramPost => new("Instagram Post", 1080, 1080, "Social Media", "Square post");
            public static CanvasPreset InstagramStory => new("Instagram Story", 1080, 1920, "Social Media", "Vertical story");
            public static CanvasPreset FacebookCover => new("Facebook Cover", 820, 312, "Social Media", "Profile cover");
            public static CanvasPreset TwitterHeader => new("Twitter Header", 1500, 500, "Social Media", "Profile header");
            public static CanvasPreset YouTubeThumbnail => new("YouTube Thumbnail", 1280, 720, "Social Media", "Video thumbnail");

            // Print - Paper Sizes (at 300 DPI)
            public static CanvasPreset A4Portrait => new("A4 Portrait", 2480, 3508, "Print", "210×297mm @ 300 DPI");
            public static CanvasPreset A4Landscape => new("A4 Landscape", 3508, 2480, "Print", "297×210mm @ 300 DPI");
            public static CanvasPreset LetterPortrait => new("Letter Portrait", 2550, 3300, "Print", "8.5×11\" @ 300 DPI");
            public static CanvasPreset LetterLandscape => new("Letter Landscape", 3300, 2550, "Print", "11×8.5\" @ 300 DPI");

            // Screen Sizes
            public static CanvasPreset HD => new("HD 720p", 1280, 720, "Screen", "Standard HD");
            public static CanvasPreset FullHD => new("Full HD 1080p", 1920, 1080, "Screen", "Full HD");
            public static CanvasPreset QuadHD => new("Quad HD 1440p", 2560, 1440, "Screen", "2K resolution");
            public static CanvasPreset UHD4K => new("4K UHD", 3840, 2160, "Screen", "Ultra HD");

            // Legacy
            public static CanvasPreset VGA => new("VGA", 640, 480, "Legacy", "Standard VGA");
            public static CanvasPreset SVGA => new("SVGA", 800, 600, "Legacy", "Super VGA");
            public static CanvasPreset XGA => new("XGA", 1024, 768, "Legacy", "Extended VGA");

            /// <summary>
            /// Gets all available presets organized by category.
            /// </summary>
            public static Dictionary<string, List<CanvasPreset>> GetAllPresets()
            {
                return new Dictionary<string, List<CanvasPreset>>
                {
                    ["Social Media"] = new List<CanvasPreset>
                    {
                        InstagramPost,
                        InstagramStory,
                        FacebookCover,
                        TwitterHeader,
                        YouTubeThumbnail
                    },
                    ["Print"] = new List<CanvasPreset>
                    {
                        A4Portrait,
                        A4Landscape,
                        LetterPortrait,
                        LetterLandscape
                    },
                    ["Screen"] = new List<CanvasPreset>
                    {
                        HD,
                        FullHD,
                        QuadHD,
                        UHD4K
                    },
                    ["Legacy"] = new List<CanvasPreset>
                    {
                        VGA,
                        SVGA,
                        XGA
                    }
                };
            }

            /// <summary>
            /// Gets a flat list of all presets.
            /// </summary>
            public static List<CanvasPreset> GetAllPresetsList()
            {
                var result = new List<CanvasPreset>();
                foreach (var category in GetAllPresets().Values)
                {
                    result.AddRange(category);
                }
                return result;
            }
        }
    }
}
