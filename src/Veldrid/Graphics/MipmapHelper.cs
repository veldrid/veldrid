using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// Contains helper methods for dealing with mipmaps.
    /// </summary>
    public static class MipmapHelper
    {
        /// <summary>
        /// Computes the number of mipmap levels in a texture.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The number of mipmap levels needed for a texture of the given dimensions.</returns>
        public static int ComputeMipLevels(int width, int height)
        {
            return 1 + (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
        }

        public static int GetDimension(int largestLevelDimension, int mipLevel)
        {
            int ret = largestLevelDimension;
            for (int i = 0; i < mipLevel; i++)
            {
                ret /= 2;
            }

            return Math.Max(1, ret);
        }
    }
}
