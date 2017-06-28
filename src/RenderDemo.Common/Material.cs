using System;

namespace Veldrid.Graphics
{
    // TODO REMOVE EVERYTHING HERE.
    public class Material : IDisposable
    {
        public Material(
            ShaderSet shaderSet,
            ShaderConstantBindingSlots constantBindings,
            ShaderTextureBindingSlots textureBindingSlots)
        {
            ShaderSet = shaderSet;
            ConstantBindings = constantBindings;
            TextureBindingSlots = textureBindingSlots;
        }

        public ShaderSet ShaderSet { get; }
        public ShaderConstantBindingSlots ConstantBindings { get; }
        public ShaderTextureBindingSlots TextureBindingSlots { get; }

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
        }

        internal void Apply(RenderContext rc)
        {
            rc.ShaderSet = ShaderSet;
            rc.ShaderConstantBindings = ConstantBindings;
            rc.ShaderTextureBindingSlots = TextureBindingSlots;
        }
    }
}
