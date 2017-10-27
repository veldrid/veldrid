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

        public bool Equals(Point other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object obj) => obj is Point p && Equals(p);
        public override int GetHashCode() => HashHelper.Combine(X.GetHashCode(), Y.GetHashCode());
        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(Point left, Point right) => left.Equals(right);
        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        private string DebuggerDisplayString => ToString();
    }
}
