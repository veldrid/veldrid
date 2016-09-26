using System;

namespace Veldrid.Graphics
{
    public class Material : IDisposable
    {
        private readonly RenderContext _rc; // TODO: Temporary, remove when Material.UseTexture is obsolete.
        private readonly DefaultTextureBindingInfo[] _defaultTextureBindings;

        public Material(
            RenderContext rc,
            ShaderSet shaderSet,
            ShaderConstantBindings constantBindings,
            ShaderTextureBindingSlots textureBindingSlots,
            DefaultTextureBindingInfo[] defaultTextureBindings)
        {
            _rc = rc;
            ShaderSet = shaderSet;
            ConstantBindings = constantBindings;
            TextureBindingSlots = textureBindingSlots;
            _defaultTextureBindings = defaultTextureBindings;
        }

        public ShaderSet ShaderSet { get; }
        public ShaderConstantBindings ConstantBindings { get; }
        public ShaderTextureBindingSlots TextureBindingSlots { get; }

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
            foreach (var defaultBinding in _defaultTextureBindings)
            {
                _rc.SetTexture(defaultBinding.Slot, defaultBinding.TextureBinding);
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
            foreach (var binding in _defaultTextureBindings)
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
}
