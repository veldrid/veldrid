using System.Numerics;

namespace Veldrid
{
    public static class Utilities
    {
        public static Matrix4x4 CalculateInverseTranspose(Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Matrix4x4.Invert(m, out inverted);
            return Matrix4x4.Transpose(inverted);
        }
    }
}
