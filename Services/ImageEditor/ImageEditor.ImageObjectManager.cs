using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Models;
using MiniPhotoshop.Services.Contracts;

namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor : IImageObjectManager
    {
        public void AddImage(BitmapSource bitmap, string name)
        {
            var image = new ImageObject(bitmap, name)
            {
                OffsetX = 0,
                OffsetY = 0,
                ZIndex = State.ImageObjects.Count > 0 ? State.ImageObjects.Max(x => x.ZIndex) + 1 : 0,
                IsSelected = true,
                IsVisible = true,
                Opacity = 1.0
            };

            // Deselect others
            foreach (var img in State.ImageObjects)
            {
                img.IsSelected = false;
            }

            State.ImageObjects.Add(image);
            State.SelectedImageId = image.Id;
        }

        public void RemoveImage(Guid id)
        {
            var image = State.ImageObjects.FirstOrDefault(x => x.Id == id);
            if (image != null)
            {
                State.ImageObjects.Remove(image);
                if (State.SelectedImageId == id)
                {
                    State.SelectedImageId = null;
                }
            }
        }

        public void SelectImage(Guid id)
        {
            var image = State.ImageObjects.FirstOrDefault(x => x.Id == id);
            if (image != null)
            {
                DeselectAll();
                image.IsSelected = true;
                State.SelectedImageId = id;
            }
        }

        public void DeselectAll()
        {
            foreach (var img in State.ImageObjects)
            {
                img.IsSelected = false;
            }
            State.SelectedImageId = null;
        }

        public ImageObject? GetSelectedImage()
        {
            if (State.SelectedImageId == null) return null;
            return State.ImageObjects.FirstOrDefault(x => x.Id == State.SelectedImageId);
        }

        public IEnumerable<ImageObject> GetImages()
        {
            return State.ImageObjects;
        }

        public void MoveSelectedImage(int deltaX, int deltaY)
        {
            var selected = GetSelectedImage();
            if (selected != null)
            {
                selected.OffsetX += deltaX;
                selected.OffsetY += deltaY;
            }
        }

        public void SetSelectedImagePosition(int x, int y)
        {
            var selected = GetSelectedImage();
            if (selected != null)
            {
                selected.OffsetX = x;
                selected.OffsetY = y;
            }
        }

        public void BringToFront(Guid id)
        {
            var image = State.ImageObjects.FirstOrDefault(x => x.Id == id);
            if (image != null && State.ImageObjects.Count > 0)
            {
                int maxZ = State.ImageObjects.Max(x => x.ZIndex);
                if (image.ZIndex < maxZ)
                {
                    image.ZIndex = maxZ + 1;
                }
            }
        }

        public void SendToBack(Guid id)
        {
            var image = State.ImageObjects.FirstOrDefault(x => x.Id == id);
            if (image != null && State.ImageObjects.Count > 0)
            {
                int minZ = State.ImageObjects.Min(x => x.ZIndex);
                if (image.ZIndex > minZ)
                {
                    image.ZIndex = minZ - 1;
                }
            }
        }
    }
}
