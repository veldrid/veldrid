using System;
using System.Numerics;

namespace Veldrid
{
    /// <summary>
    /// A color stored in four 32-bit floating-point values, in RGBA component order.
    /// </summary>
    public struct RgbaFloat : IEquatable<RgbaFloat>
    {
        private readonly Vector4 _channels;

        /// <summary>
        /// The red component.
        /// </summary>
        public float R => _channels.X;
        /// <summary>
        /// The green component.
        /// </summary>
        public float G => _channels.Y;
        /// <summary>
        /// The blue component.
        /// </summary>
        public float B => _channels.Z;
        /// <summary>
        /// The alpha component.
        /// </summary>
        public float A => _channels.W;

        /// <summary>
        /// Constructs a new RgbaFloat from the given components.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        public RgbaFloat(float r, float g, float b, float a)
        {
            _channels = new Vector4(r, g, b, a);
        }

        /// <summary>
        /// The total size, in bytes, of an RgbaFloat value.
        /// </summary>
        public static readonly int SizeInBytes = 16;

        /// <summary>
        /// Red (1, 0, 0, 1)
        /// </summary>
        public static readonly RgbaFloat Red = new RgbaFloat(1, 0, 0, 1);
        /// <summary>
        /// Dark Red (0.6f, 0, 0, 1)
        /// </summary>
        public static readonly RgbaFloat DarkRed = new RgbaFloat(0.6f, 0, 0, 1);
        /// <summary>
        /// Green (0, 1, 0, 1)
        /// </summary>
        public static readonly RgbaFloat Green = new RgbaFloat(0, 1, 0, 1);
        /// <summary>
        /// Blue (0, 0, 1, 1)
        /// </summary>
        public static readonly RgbaFloat Blue = new RgbaFloat(0, 0, 1, 1);
        /// <summary>
        /// Yellow (1, 1, 0, 0)
        /// </summary>
        public static readonly RgbaFloat Yellow = new RgbaFloat(1, 1, 0, 1);
        /// <summary>
        /// Grey (0.25f, 0.25f, 0.25f, 1)
        /// </summary>
        public static readonly RgbaFloat Grey = new RgbaFloat(.25f, .25f, .25f, 1);
        /// <summary>
        /// Light Grey (0.65f, 0.65f, 0.65f, 1)
        /// </summary>
        public static readonly RgbaFloat LightGrey = new RgbaFloat(.65f, .65f, .65f, 1);
        /// <summary>
        /// Cyan (0, 1, 1, 1)
        /// </summary>
        public static readonly RgbaFloat Cyan = new RgbaFloat(0, 1, 1, 1);
        /// <summary>
        /// White (1, 1, 1, 1)
        /// </summary>
        public static readonly RgbaFloat White = new RgbaFloat(1, 1, 1, 1);
        /// <summary>
        /// Cornflower Blue (0.3921f, 0.5843f, 0.9294f, 1)
        /// </summary>
        public static readonly RgbaFloat CornflowerBlue = new RgbaFloat(0.3921f, 0.5843f, 0.9294f, 1);
        /// <summary>
        /// Clear (0, 0, 0, 0)
        /// </summary>
        public static readonly RgbaFloat Clear = new RgbaFloat(0, 0, 0, 0);
        /// <summary>
        /// Black (0, 0, 0, 1)
        /// </summary>
        public static readonly RgbaFloat Black = new RgbaFloat(0, 0, 0, 1);
        /// <summary>
        /// Pink (1, 0.45f, 0.75f, 1)
        /// </summary>
        public static readonly RgbaFloat Pink = new RgbaFloat(1f, 0.45f, 0.75f, 1);
        /// <summary>
        /// Orange (1, 0.36f, 0, 1)
        /// </summary>
        public static readonly RgbaFloat Orange = new RgbaFloat(1f, 0.36f, 0f, 1);

        /// <summary>
        /// Converts this RgbaFloat into a Vector4.
        /// </summary>
        /// <returns></returns>
        public Vector4 ToVector4()
        {
            return _channels;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(RgbaFloat other)
        {
            return _channels.Equals(other._channels);
        }

        public override string ToString()
        {
            return string.Format("R:{0}, G:{1}, B:{2}, A:{3}", R, G, B, A);
        }
    }
}
