using ImGuiNET;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo
{
    internal static class Util
    {
        internal static uint SizeInBytes<T>(this T[] array) where T : struct
        {
            return (uint)(array.Length * Unsafe.SizeOf<T>());
        }

        // Code adapted from https://bitbucket.org/sinbad/ogre/src/9db75e3ba05c/OgreMain/include/OgreVector3.h
        internal static Quaternion FromToRotation(Vector3 from, Vector3 to, Vector3 fallbackAxis = default(Vector3))
        {
            // Based on Stan Melax's article in Game Programming Gems
            Quaternion q;
            // Copy, since cannot modify local
            Vector3 v0 = from;
            Vector3 v1 = to;
            v0 = Vector3.Normalize(v0);
            v1 = Vector3.Normalize(v1);

            float d = Vector3.Dot(v0, v1);
            // If dot == 1, vectors are the same
            if (d >= 1.0f)
            {
                return Quaternion.Identity;
            }
            if (d < (1e-6f - 1.0f))
            {
                if (fallbackAxis != Vector3.Zero)
                {
                    // rotate 180 degrees about the fallback axis
                    q = Quaternion.CreateFromAxisAngle(fallbackAxis, (float)Math.PI);
                }
                else
                {
                    // Generate an axis
                    Vector3 axis = Vector3.Cross(Vector3.UnitX, from);
                    if (axis.LengthSquared() == 0) // pick another if colinear
                    {
                        axis = Vector3.Cross(Vector3.UnitY, from);
                    }

                    axis = Vector3.Normalize(axis);
                    q = Quaternion.CreateFromAxisAngle(axis, (float)Math.PI);
                }
            }
            else
            {
                float s = (float)Math.Sqrt((1 + d) * 2);
                float invs = 1.0f / s;

                Vector3 c = Vector3.Cross(v0, v1);

                q.X = c.X * invs;
                q.Y = c.Y * invs;
                q.Z = c.Z * invs;
                q.W = s * 0.5f;
                q = Quaternion.Normalize(q);
            }
            return q;
        }

        // modifies projection matrix in place
        // clipPlane is in camera space
        public static void CalculateObliqueMatrixPerspective(ref Matrix4x4 projection, Matrix4x4 view, Plane clipPlane)
        {
            Matrix4x4 invTransposeView = VdUtilities.CalculateInverseTranspose(view);
            Vector4 clipV4 = new Vector4(clipPlane.Normal, clipPlane.D);
            clipV4 = Vector4.Transform(clipV4, invTransposeView);

            Vector4 q = new Vector4(
                (Math.Sign(clipV4.X) + projection.M13) / projection.M11,
                (Math.Sign(clipV4.Y) + projection.M23) / projection.M22,
                -1f,
                (1 + projection.M33) / projection.M34);

            Vector4 c = clipV4 * (1f / Vector4.Dot(clipV4, q));

            projection.M31 = c.X;
            projection.M32 = c.Y;
            projection.M33 = c.Z;
            projection.M34 = c.W;
        }

        private static float sgn(float x)
        {
            if (x > 0) return 1;
            else if (x < 0) return -1;
            else return 0;
        }

        public static Matrix4x4 Inverse(this Matrix4x4 src)
        {
            Matrix4x4.Invert(src, out Matrix4x4 result);
            return result;
        }
    }
}
