using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal sealed class MTLTextureView : TextureView
    {
        private readonly bool _hasTextureView;
        private bool _disposed;

        public MetalBindings.MTLTexture TargetDeviceTexture { get; }

        public override string? Name { get; set; }

        public override bool IsDisposed => _disposed;

        public MTLTextureView(in TextureViewDescription description, MTLGraphicsDevice gd)
            : base(description)
        {
            MTLTexture targetMTLTexture = Util.AssertSubtype<Texture, MTLTexture>(description.Target);
            if (BaseMipLevel != 0 || MipLevels != Target.MipLevels
                || BaseArrayLayer != 0 || ArrayLayers != Target.ArrayLayers
                || Format != Target.Format)
            {
                _hasTextureView = true;
                uint effectiveArrayLayers = (Target.Usage & TextureUsage.Cubemap) != 0 ? ArrayLayers * 6 : ArrayLayers;
                TargetDeviceTexture = targetMTLTexture.DeviceTexture.newTextureView(
                    MTLFormats.VdToMTLPixelFormat(Format, (description.Target.Usage & TextureUsage.DepthStencil) != 0),
                    targetMTLTexture.MTLTextureType,
                    new NSRange(BaseMipLevel, MipLevels),
                    new NSRange(BaseArrayLayer, effectiveArrayLayers));
            }
            else
            {
                TargetDeviceTexture = targetMTLTexture.DeviceTexture;
            }
        }

        public override void Dispose()
        {
            if (_hasTextureView && !_disposed)
            {
                _disposed = true;
                ObjectiveCRuntime.release(TargetDeviceTexture.NativePtr);
            }
        }
    }
}
