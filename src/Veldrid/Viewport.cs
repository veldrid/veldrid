using System;

namespace Veldrid
{
    public struct Viewport : IEquatable<Viewport>
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public float MinDepth;
        public float MaxDepth;

        public Viewport(float x, float y, float width, float height, float minDepth, float maxDepth)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }

        public bool Equals(Viewport other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y)
                && Width.Equals(other.Width) && Height.Equals(other.Height)
                && MinDepth.Equals(other.MinDepth) && MaxDepth.Equals(other.MaxDepth);
        }
    }
}
