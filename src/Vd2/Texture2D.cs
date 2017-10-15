using System;

namespace Vd2
{
    public abstract class Texture2D : Texture
    {
        public abstract uint Width { get; }
        public abstract uint Height { get; }
    }
}
