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

        public void Dispose()
        {
            ShaderSet.Dispose();
        }

        internal void Apply(RenderContext rc)
        {
            rc.ShaderSet = ShaderSet;
            rc.ShaderConstantBindingSlots = ConstantBindings;
            rc.ShaderTextureBindingSlots = TextureBindingSlots;
        }
    }
}
