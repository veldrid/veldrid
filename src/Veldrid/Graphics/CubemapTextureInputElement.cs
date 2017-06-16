using ImageSharp;
using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A <see cref="MaterialTextureInputElement"/> describing a <see cref="CubemapTexture"/>.
    /// </summary>
    public class CubemapTextureInputElement : MaterialTextureInputElement
    {
        private readonly ImageSharpTexture _front;
        private readonly ImageSharpTexture _back;
        private readonly ImageSharpTexture _left;
        private readonly ImageSharpTexture _right;
        private readonly ImageSharpTexture _top;
        private readonly ImageSharpTexture _bottom;

        /// <summary>
        /// Constructs a <see cref="CubemapTextureInputElement"/> from six face textures.
        /// </summary>
        /// <param name="name">The name of the cubemap texture.</param>
        /// <param name="front">The front texture.</param>
        /// <param name="back">The back texture.</param>
        /// <param name="left">The left texture.</param>
        /// <param name="right">The right texture.</param>
        /// <param name="top">The top texture.</param>
        /// <param name="bottom">The bottom texture.</param>
        public CubemapTextureInputElement(
            string name,
            ImageSharpTexture front,
            ImageSharpTexture back,
            ImageSharpTexture left,
            ImageSharpTexture right,
            ImageSharpTexture top,
            ImageSharpTexture bottom)
            : base(name)
        {
            _front = front;
            _back = back;
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
        }

        public unsafe override DeviceTexture GetDeviceTexture(RenderContext rc)
        {
            fixed (Rgba32* frontPin = &_front.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* backPin = &_back.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* leftPin = &_left.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* rightPin = &_right.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* topPin = &_top.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* bottomPin = &_bottom.Pixels.DangerousGetPinnableReference())
            {
                return rc.ResourceFactory.CreateCubemapTexture(
                    (IntPtr)frontPin,
                    (IntPtr)backPin,
                    (IntPtr)leftPin,
                    (IntPtr)rightPin,
                    (IntPtr)topPin,
                    (IntPtr)bottomPin,
                    _front.Width,
                    _front.Height,
                    RgbaFloat.SizeInBytes,
                    PixelFormat.R32_G32_B32_A32_Float);
            }
        }
    }
}
