using System;

namespace Veldrid.Graphics
{
    public static class MipmapHelper
    {
        public static int ComputeMipLevels(int width, int height)
        {
            return 1 + (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
        }
    }
}
