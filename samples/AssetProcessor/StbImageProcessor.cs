using AssetPrimitives;
using System;
using System.IO;
using System.Threading.Tasks;
using StbSharp;
using Veldrid;

namespace AssetProcessor
{
    public class StbImageProcessor : BinaryAssetProcessor<ProcessedTexture>
    {
        public override async Task<ProcessedTexture> ProcessT(Stream stream, string extension)
        {
            byte[] streamBytes = new byte[stream.Length];
            await stream.ReadAsync(streamBytes, 0, streamBytes.Length);
            Image image = StbImage.LoadFromMemory(streamBytes, 4);

            return new ProcessedTexture(
                PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                TextureType.Texture2D,
                (uint)image.Width, (uint)image.Height, 1,
                1, 1,
                image.Data);
        }
    }
}
