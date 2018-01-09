using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLTexture : Texture
    {
        private bool _disposed;

        /// <summary>
        /// The native MTLTexture object. This property is only valid for non-staging Textures.
        /// </summary>
        public MetalBindings.MTLTexture DeviceTexture { get; }
        /// <summary>
        /// The staging MTLBuffer object. This property is only valid for staging Textures.
        /// </summary>
        public MetalBindings.MTLBuffer StagingBuffer { get; }

        public override PixelFormat Format { get; }

        public override uint Width { get; }

        public override uint Height { get; }

        public override uint Depth { get; }

        public override uint MipLevels { get; }

        public override uint ArrayLayers { get; }

        public override TextureUsage Usage { get; }

        public override TextureType Type { get; }

        public override TextureSampleCount SampleCount { get; }
        public override string Name { get; set; }
        public MTLPixelFormat MTLPixelFormat { get; }
        public MTLTextureType MTLTextureType { get; }

        public MTLTexture(ref TextureDescription description, MTLGraphicsDevice _gd)
        {
            Width = description.Width;
            Height = description.Height;
            Depth = description.Depth;
            ArrayLayers = description.ArrayLayers;
            MipLevels = description.MipLevels;
            Format = description.Format;
            Usage = description.Usage;
            Type = description.Type;
            SampleCount = description.SampleCount;
            bool isDepth = (Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil;

            MTLPixelFormat = MTLFormats.VdToMTLPixelFormat(Format, isDepth);
            MTLTextureType = MTLFormats.VdToMTLTextureType(
                    Type,
                    ArrayLayers,
                    SampleCount != TextureSampleCount.Count1,
                    (Usage & TextureUsage.Cubemap) != 0);
            if (Usage != TextureUsage.Staging)
            {
                MTLTextureDescriptor texDescriptor = MTLUtil.AllocInit<MTLTextureDescriptor>();
                texDescriptor.width = (UIntPtr)Width;
                texDescriptor.height = (UIntPtr)Height;
                texDescriptor.depth = (UIntPtr)Depth;
                texDescriptor.mipmapLevelCount = (UIntPtr)MipLevels;
                texDescriptor.arrayLength = (UIntPtr)ArrayLayers;
                texDescriptor.sampleCount = (UIntPtr)FormatHelpers.GetSampleCountUInt32(SampleCount);
                texDescriptor.textureType = MTLTextureType;
                texDescriptor.pixelFormat = MTLPixelFormat;
                texDescriptor.textureUsage = MTLFormats.VdToMTLTextureUsage(Usage);
                texDescriptor.storageMode = MTLStorageMode.Private;

                DeviceTexture = _gd.Device.newTextureWithDescriptor(texDescriptor);
                ObjectiveCRuntime.release(texDescriptor.NativePtr);
            }
            else
            {
                uint pixelSize = FormatHelpers.GetSizeInBytes(Format);
                uint totalStorageSize = 0;
                for (uint level = 0; level < MipLevels; level++)
                {
                    Util.GetMipDimensions(this, level, out uint levelWidth, out uint levelHeight, out uint levelDepth);
                    totalStorageSize += pixelSize * levelWidth * levelHeight * levelDepth * ArrayLayers;
                }

                StagingBuffer = _gd.Device.newBufferWithLengthOptions(
                    (UIntPtr)totalStorageSize,
                    MTLResourceOptions.StorageModeManaged);
            }
        }

        private static MTLTextureType GetTextureType(
            uint width,
            uint height,
            uint depth,
            uint arrayLayers,
            TextureUsage usage,
            TextureSampleCount sampleCount)
        {
            bool isCube = (usage & TextureUsage.Cubemap) != 0;
            if (depth == 1)
            {
                if (height == 1)
                {
                    // 1D
                    return arrayLayers == 1 ? MTLTextureType.Type1D : MTLTextureType.Type1DArray;
                }
                else
                {
                    // 2D
                    if (isCube)
                    {
                        return arrayLayers == 1 ? MTLTextureType.TypeCube : MTLTextureType.TypeCubeArray;
                    }
                    else
                    {
                        if (sampleCount == TextureSampleCount.Count1)
                        {

                            return arrayLayers == 1 ? MTLTextureType.Type2D : MTLTextureType.Type2DArray;
                        }
                        else
                        {
                            return MTLTextureType.Type2DMultisample;
                        }
                    }
                }
            }
            else
            {
                return MTLTextureType.Type3D;
            }
        }

        internal uint GetSubresourceSize(uint mipLevel, uint arrayLayer)
        {
            uint pixelSize = FormatHelpers.GetSizeInBytes(Format);
            Util.GetMipDimensions(this, mipLevel, out uint width, out uint height, out uint depth);
            return pixelSize * width * height * depth;
        }

        internal void GetSubresourceLayout(uint mipLevel, uint arrayLayer, out uint rowPitch, out uint depthPitch)
        {
            uint pixelSize = FormatHelpers.GetSizeInBytes(Format);
            Util.GetMipDimensions(this, mipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
            rowPitch = mipWidth * pixelSize;
            depthPitch = rowPitch * Height;
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (!StagingBuffer.IsNull)
                {
                    ObjectiveCRuntime.release(StagingBuffer.NativePtr);
                }
                else
                {
                    ObjectiveCRuntime.release(DeviceTexture.NativePtr);
                }
            }
        }
    }
}