using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLTextureView : TextureView
    {
        private readonly bool _hasTextureView;

        public MTLTexture TargetMTLTexture { get; }

        public override string Name
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public MTLTextureView(ref TextureViewDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            if (BaseMipLevel != 0 || MipLevels != Target.MipLevels
                || BaseArrayLayer != 0 || ArrayLayers != Target.ArrayLayers)
            {
                _hasTextureView = true;
                throw new NotImplementedException();
            }
            else
            {
                TargetMTLTexture = Util.AssertSubtype<Texture, MTLTexture>(description.Target);
            }
        }

        public override void Dispose()
        {
            if (_hasTextureView)
            {
                ObjectiveCRuntime.release(TargetMTLTexture.DeviceTexture.NativePtr);
            }
        }
    }
}