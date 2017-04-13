using System;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DShaderTextureBindingSlots : ShaderTextureBindingSlots
    {
        public MaterialTextureInputs TextureInputs { get; }

        public D3DShaderTextureBindingSlots(D3DShaderSet shaderSet, MaterialTextureInputs textureInputs)
        {
            //TODO: arg 'shaderSet' is unused.
            TextureInputs = textureInputs;
        }

        public ShaderStageApplicabilityFlags GetApplicabilityForSlot(int slot)
        {
            // TODO: This should be able to return specific applicability based on the computed
            // stages which use the texture binding for the given slot.
            return ShaderStageApplicabilityFlags.Fragment;
        }
    }

    [Flags]
    public enum ShaderStageApplicabilityFlags : byte
    {
        Vertex = 1 << 0,
        Geometry = 1 << 1,
        Fragment = 1 << 2
    }
}