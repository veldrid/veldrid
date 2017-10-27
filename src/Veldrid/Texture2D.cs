using System;

namespace Veldrid
{
    public abstract class Texture2D : Texture
    {
        public abstract uint Width { get; }
        public abstract uint Height { get; }
    }
}
