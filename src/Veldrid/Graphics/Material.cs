using System;

namespace Veldrid.Graphics
{
    public class Material : IDisposable
    {
        private readonly RenderContext _rc; // TODO: Temporary, remove when Material.UseTexture is obsolete.

        public Material(RenderContext rc, ShaderSet shaderSet, ShaderConstantBindings constantBindings, ShaderTextureBindingSlots textureBindingSlots)
            : this(rc, shaderSet, constantBindings, textureBindingSlots, Array.Empty<DefaultTextureBindingInfo>(), Array.Empty<DefaultSamplerBindingInfo>())
        {
        }

        public Material(
            RenderContext rc,
            ShaderSet shaderSet,
            ShaderConstantBindings constantBindings,
            ShaderTextureBindingSlots textureBindingSlots,
            DefaultTextureBindingInfo[] defaultTextureBindings)
            : this(rc, shaderSet, constantBindings, textureBindingSlots, defaultTextureBindings, Array.Empty<DefaultSamplerBindingInfo>())
        {
        }

        public Material(
            RenderContext rc,
            ShaderSet shaderSet,
            ShaderConstantBindings constantBindings,
            ShaderTextureBindingSlots textureBindingSlots,
            DefaultTextureBindingInfo[] defaultTextureBindings,
            DefaultSamplerBindingInfo[] defaultSamplerBindings)
        {
            _rc = rc;
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

        public void UseDefaultTextures()
        {
            foreach (var defaultBinding in DefaultTextureBindings)
            {
                _rc.SetTexture(defaultBinding.Slot, defaultBinding.TextureBinding);
            }
            foreach (var samplerBinding in DefaultSamplerBindings)
            {
                _rc.SetSamplerState(samplerBinding.Slot, samplerBinding.SamplerState);
            }
        }

        public void UseTexture(int slot, ShaderTextureBinding binding)
        {
            // This method should go away.
            _rc.SetTexture(slot, binding);
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
