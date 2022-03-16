using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLTextureView : TextureView
    {
        private readonly bool _hasTextureView;
        private bool _disposed;

        public MetalBindings.MTLTexture TargetDeviceTexture { get; }

        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public MTLTextureView(ref TextureViewDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            MTLTexture targetMTLTexture = Util.AssertSubtype<Texture, MTLTexture>(description.Target);
            if (ViewType != TextureViewDescription.GetFromTexture(Target)
                || BaseMipLevel != 0
                || MipLevels != Target.MipLevels
                || BaseArrayLayer != 0
                || ArrayLayers != Target.ArrayLayers
                || Format != Target.Format)
            {
                _hasTextureView = true;

                MTLTextureType textureViewType = MTLFormats.VdToMTLTextureViewType(
                    description.ViewType,
                    targetMTLTexture.SampleCount != TextureSampleCount.Count1
                    );

                TargetDeviceTexture = targetMTLTexture.DeviceTexture.newTextureView(
                    MTLFormats.VdToMTLPixelFormat(Format, (description.Target.Usage & TextureUsage.DepthStencil) != 0),
                    textureViewType,
                    new NSRange(BaseMipLevel, MipLevels),
                    new NSRange(BaseArrayLayer, ArrayLayers));
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
