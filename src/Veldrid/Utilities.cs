using System;
using System.Diagnostics;
using System.Numerics;
using System.Text;

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
        
        public static Matrix4x4 ConvertToMatrix3x3(Matrix4x4 m)
        {
            return new Matrix4x4(
                m.M11, m.M12, m.M13, 0,
                m.M21, m.M22, m.M23, 0,
                m.M31, m.M32, m.M33, 0,
                0, 0, 0, 1);
        }

        [Conditional("DEBUG")]
        public static void CheckLastGLES3Error()
        {
            var ec = OpenTK.Graphics.ES30.GL.GetError();
            if (ec != OpenTK.Graphics.ES30.ErrorCode.NoError)
            {
                throw new InvalidOperationException("OpenGLES Error: " + ec);
            }
        }

        [Conditional("DEBUG")]
        public static void CheckLastGLError()
        {
            var ec = OpenTK.Graphics.OpenGL.GL.GetError();
            if (ec != OpenTK.Graphics.OpenGL.ErrorCode.NoError)
            {
                throw new InvalidOperationException("OpenGL Error: " + ec);
            }
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

    }
}
