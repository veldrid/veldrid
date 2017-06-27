using System;

namespace Veldrid.Graphics
{
    public class Material : IDisposable
    {
        public Material(ShaderSet shaderSet, ShaderConstantBindings constantBindings, ShaderTextureBindingSlots textureBindingSlots)
            : this(shaderSet, constantBindings, textureBindingSlots, Array.Empty<DefaultTextureBindingInfo>(), Array.Empty<DefaultSamplerBindingInfo>())
        {
        }

        public Material(
            ShaderSet shaderSet,
            ShaderConstantBindings constantBindings,
            ShaderTextureBindingSlots textureBindingSlots,
            DefaultTextureBindingInfo[] defaultTextureBindings)
            : this(shaderSet, constantBindings, textureBindingSlots, defaultTextureBindings, Array.Empty<DefaultSamplerBindingInfo>())
        {
        }

        public Material(
            ShaderSet shaderSet,
            ShaderConstantBindings constantBindings,
            ShaderTextureBindingSlots textureBindingSlots,
            DefaultTextureBindingInfo[] defaultTextureBindings,
            DefaultSamplerBindingInfo[] defaultSamplerBindings)
        {
            ShaderSet = shaderSet;
            ConstantBindings = constantBindings;
            TextureBindingSlots = textureBindingSlots;
            DefaultTextureBindings = defaultTextureBindings;
            DefaultSamplerBindings = defaultSamplerBindings;
        }

        public ShaderSet ShaderSet { get; }
        public ShaderConstantBindings ConstantBindings { get; }
        public ShaderTextureBindingSlots TextureBindingSlots { get; }
        public DefaultTextureBindingInfo[] DefaultTextureBindings { get; }
        public DefaultSamplerBindingInfo[] DefaultSamplerBindings { get; }

        public void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider)
        {
            ConstantBindings.ApplyPerObjectInput(dataProvider);
        }

        public void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders)
        {
            ConstantBindings.ApplyPerObjectInputs(dataProviders);
        }

        public void Dispose()
        {
            ShaderSet.Dispose();
            ConstantBindings.Dispose();
            foreach (var binding in DefaultTextureBindings)
            {
                binding.TextureBinding.Dispose();
            }
        }
    }

    public struct DefaultTextureBindingInfo
    {
        public readonly int Slot;
        public readonly ShaderTextureBinding TextureBinding;
        public DefaultTextureBindingInfo(int slot, ShaderTextureBinding binding)
        {
            Slot = slot;
            TextureBinding = binding;
        }
    }

    public struct DefaultSamplerBindingInfo
    {
        public readonly int Slot;
        public readonly SamplerState SamplerState;
        public DefaultSamplerBindingInfo(int slot, SamplerState samplerState)
        {
            Slot = slot;
            SamplerState = samplerState;
        }
    }
}
