using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.Contracts
{
    public interface IImageLoaderService
    {
        ImageLoadResult Load(string filePath);
    }
}

