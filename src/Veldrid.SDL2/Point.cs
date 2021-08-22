using System;
using System.Diagnostics;

namespace Veldrid
{
    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public struct Point : IEquatable<Point>
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public readonly bool Equals(Point other) => X.Equals(other.X) && Y.Equals(other.Y);
        public readonly override bool Equals(object? obj) => obj is Point p && Equals(p);
        public readonly override int GetHashCode() => HashCode.Combine(X, Y);
        public readonly override string ToString() => $"({X}, {Y})";

        public static bool operator ==(Point left, Point right) => left.Equals(right);
        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        private string DebuggerDisplayString => ToString();
    }
}
