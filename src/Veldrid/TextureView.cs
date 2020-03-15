using System;

namespace Veldrid
{
    /// <summary>
    /// A bindable device resource which provides a shader with access to a sampled <see cref="Texture"/> object.
    /// See <see cref="TextureViewDescription"/>.
    /// </summary>
    public abstract class TextureView : BindableResource, DeviceResource, IDisposable
    {
        /// <summary>
        /// The target <see cref="Texture"/> object to be sampled via this instance.
        /// </summary>
        public Texture Target { get; }
        /// <summary>
        /// The base mip level visible in the view.
        /// </summary>
        public uint BaseMipLevel { get; }
        /// <summary>
        /// The number of mip levels visible in the view.
        /// </summary>
        public uint MipLevels { get; }
        /// <summary>
        /// The base array layer visible in the view.
        /// </summary>
        public uint BaseArrayLayer { get; }
        /// <summary>
        /// The number of array layers visible in the view.
        /// </summary>
        public uint ArrayLayers { get; }
        /// <summary>
        /// The format used to interpret the contents of the target Texture. This may be different from the target Texture's
        /// true storage format, but it will be the same size.
        /// </summary>
        public PixelFormat Format { get; }
        /// <summary>
        /// Miscellaneous option flags.
        /// </summary>
        public TextureViewOptions Options { get; }

        internal TextureView(ref TextureViewDescription description)
        {
            Target = description.Target;
            BaseMipLevel = description.BaseMipLevel;
            MipLevels = description.MipLevels;
            BaseArrayLayer = description.BaseArrayLayer;
            ArrayLayers = description.ArrayLayers;
            if (description.Format.HasValue)
            {
                Format = description.Format.Value;
            }
            else
            {
                if ((description.Options & TextureViewOptions.Stencil) != 0)
                {
                    if (!FormatHelpers.IsStencilFormat(description.Target.Format))
                    {
                        throw new VeldridException(
                            $"{nameof(TextureViewOptions)} can only be used with a depth-stencil texture.");
                    }
                    Format = PixelFormat.R8_UInt;
                }
                else
                {
                    Format = description.Target.Format;
                }
            }
            Options = description.Options;
        }

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
