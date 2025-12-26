using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IImageObjectManager
    {
        void AddImage(BitmapSource bitmap, string name);
        void RemoveImage(Guid id);
        void SelectImage(Guid id);
        void DeselectAll();
        ImageObject? GetSelectedImage();
        IEnumerable<ImageObject> GetImages();
        void MoveSelectedImage(int deltaX, int deltaY);
        void SetSelectedImagePosition(int x, int y);
        void BringToFront(Guid id);
        void SendToBack(Guid id);
    }
}
