using System;

namespace Veldrid.Graphics
{
    public struct Rectangle : IEquatable<Rectangle>
    {
        public readonly int Left;
        public readonly int Right;
        public readonly int Top;
        public readonly int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public Rectangle(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public bool Equals(Rectangle other)
        {
            return other.Left == Left && other.Right == Right
                && other.Top == Top && other.Bottom == Bottom;
        }

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle && ((Rectangle)obj).Equals(this);
        }

        public override int GetHashCode()
        {
            return (int)(
                ((long)Left.GetHashCode() << 32 + Right.GetHashCode())
                ^ (((long)Top.GetHashCode() << 32 + Bottom.GetHashCode()) >> 5));
        }
    }
}