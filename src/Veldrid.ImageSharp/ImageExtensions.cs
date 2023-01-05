using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Veldrid.ImageSharp
{
    public static class ImageExtensions
    {
        public static Span<Rgba32> GetPixelSpan(this Image<Rgba32> image)
        {
            if (image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixelMemory))
            {
                return pixelMemory.Span;
            }

            var bufferSize = image.Width * image.Height;
            var buffer = new Rgba32[bufferSize];
            var span = new Span<Rgba32>(buffer);
            image.CopyPixelDataTo(span);
            return span;
        }
    }
}
