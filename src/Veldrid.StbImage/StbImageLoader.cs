using StbSharp;
using System;
using System.IO;

namespace Veldrid.StbImage
{
    public static class StbTextureLoader
    {
        public static Texture Load(GraphicsDevice gd, ResourceFactory factory, string path, bool mipmap, bool srgb)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                return Load(gd, factory, fs, mipmap, srgb);
            }
        }

        public static Texture Load(GraphicsDevice gd, ResourceFactory factory, Stream stream, bool mipmap, bool srgb)
        {
            if (mipmap)
            {
                throw new NotImplementedException();
            }

            Image image = LoadImage(stream);

            PixelFormat format = srgb ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb : PixelFormat.R8_G8_B8_A8_UNorm;

            Texture t = factory.CreateTexture(TextureDescription.Texture2D(
                (uint)image.Width, (uint)image.Height,
                1, 1,
                format,
                TextureUsage.Sampled));
            gd.UpdateTexture(t, image.Data, 0, 0, 0, t.Width, t.Height, t.Depth, 0, 0);
            return t;
        }

        public static Texture LoadCube(GraphicsDevice gd, ResourceFactory factory, Stream[] streams, bool mipmap, bool srgb)
        {
            if (mipmap)
            {
                throw new NotImplementedException();
            }

            if (streams.Length != 6)
            {
                throw new VeldridException($"{nameof(streams)} must be length 6.");
            }

            PixelFormat format = srgb ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb : PixelFormat.R8_G8_B8_A8_UNorm;
            Image face0 = LoadImage(streams[0]);
            Texture t = factory.CreateTexture(TextureDescription.Texture2D(
                (uint)face0.Width, (uint)face0.Height,
                1, 1,
                format,
                TextureUsage.Cubemap | TextureUsage.Sampled));
            gd.UpdateTexture(t, face0.Data, 0, 0, 0, t.Width, t.Height, 1, 0, 0);
            for (uint i = 1; i < 6; i++)
            {
                Image face = LoadImage(streams[i]);
                gd.UpdateTexture(t, face.Data, 0, 0, 0, t.Width, t.Height, 1, 0, i);
            }

            return t;
        }

        private static Image LoadImage(Stream stream)
        {
            byte[] streamBytes = new byte[stream.Length];
            stream.Read(streamBytes, 0, streamBytes.Length);
            return StbSharp.StbImage.LoadFromMemory(streamBytes, 4);
        }
    }
}
