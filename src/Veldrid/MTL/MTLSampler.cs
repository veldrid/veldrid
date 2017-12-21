using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLSampler : Sampler
    {
        public MTLSamplerState DeviceSampler { get; }

        public MTLSampler(ref SamplerDescription description, MTLGraphicsDevice gd)
        {
            MTLFormats.GetMinMagMipFilter(
                description.Filter,
                out MTLSamplerMinMagFilter min,
                out MTLSamplerMinMagFilter mag,
                out MTLSamplerMipFilter mip);

            MTLSamplerDescriptor mtlDesc = MTLUtil.AllocInit<MTLSamplerDescriptor>();
            mtlDesc.rAddressMode = MTLFormats.VdToMTLAddressMode(description.AddressModeU);
            mtlDesc.sAddressMode = MTLFormats.VdToMTLAddressMode(description.AddressModeV);
            mtlDesc.tAddressMode = MTLFormats.VdToMTLAddressMode(description.AddressModeW);
            mtlDesc.minFilter = min;
            mtlDesc.magFilter = mag;
            mtlDesc.mipFilter = mip;
            mtlDesc.borderColor = MTLFormats.VdToMTLBorderColor(description.BorderColor);
            if (description.ComparisonKind != null)
            {
                mtlDesc.compareFunction = MTLFormats.VdToMTLCompareFunction(description.ComparisonKind.Value);
            }
            mtlDesc.lodMinClamp = description.MinimumLod;
            mtlDesc.lodMaxClamp = description.MaximumLod;
            mtlDesc.maxAnisotropy = (UIntPtr)(Math.Max(1, description.MaximumAnisotropy));
            DeviceSampler = gd.Device.newSamplerStateWithDescriptor(mtlDesc);
            ObjectiveCRuntime.release(mtlDesc.NativePtr);
        }

        public override string Name
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            ObjectiveCRuntime.release(DeviceSampler.NativePtr);
        }
    }
}