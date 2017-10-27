using System;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Veldrid.Utilities
{
    public static class VdUtilities
    {
        public static Matrix4x4 CalculateInverseTranspose(Matrix4x4 m) => CalculateInverseTranspose(ref m);
        public static Matrix4x4 CalculateInverseTranspose(ref Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Matrix4x4.Invert(m, out inverted);
            return Matrix4x4.Transpose(inverted);
        }


        public static unsafe string GetString(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(stringStart, characters);
        }

        // Code adapted from https://bitbucket.org/sinbad/ogre/src/9db75e3ba05c/OgreMain/include/OgreVector3.h
        public static Quaternion FromToRotation(Vector3 from, Vector3 to, Vector3 fallbackAxis = default(Vector3))
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
    }
}
