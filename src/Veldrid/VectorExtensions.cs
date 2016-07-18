using System.Numerics;

namespace Veldrid
{
    public static class VectorExtensions
    {
        public static Vector3 XYZ(this Vector4 v) => new Vector3(v.X, v.Y, v.Z);
        public static Vector2 XY(this Vector4 v) => new Vector2(v.X, v.Y);
        public static Vector2 XY(this Vector3 v) => new Vector2(v.X, v.Y);
    }
}
