using System;
using System.Numerics;

namespace Veldrid
{
    public struct Rectangle : IEquatable<Rectangle>
    {
        public int X;

        public int Y;

        public int Width;

        public int Height;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Point topLeft, Point size)
        {
            X = topLeft.X;
            Y = topLeft.Y;
            Width = size.X;
            Height = size.Y;
        }

        public int Left => X;
        public int Right => X + Width;
        public int Top => Y;
        public int Bottom => Y + Height;

        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);

        public bool Contains(Point p) => Contains(p.X, p.Y);
        public bool Contains(int x, int y)
        {
            return (X <= x && (X + Width) > x)
                && (Y <= y && (Y + Height) > y);
        }

        public bool Equals(Rectangle other) => X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);

        public override bool Equals(object obj) => obj is Rectangle r && Equals(r);

        public override int GetHashCode()
        {
            return HashHelper.Combine(X.GetHashCode(), HashHelper.Combine(Y.GetHashCode(), HashHelper.Combine(Width.GetHashCode(), Height.GetHashCode())));
        }

        public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);
        public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);
    }
}
