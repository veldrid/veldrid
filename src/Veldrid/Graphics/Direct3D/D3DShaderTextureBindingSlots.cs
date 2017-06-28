using SharpDX.D3DCompiler;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DShaderTextureBindingSlots : ShaderTextureBindingSlots
    {
        private readonly ShaderStageApplicabilityFlags[] _applicabilities;

        public ShaderTextureInput[] TextureInputs { get; }

        public D3DShaderTextureBindingSlots(D3DShaderSet shaderSet, ShaderTextureInput[] textureInputs)
        {
            TextureInputs = textureInputs;
            _applicabilities = ComputeStageApplicabilities(shaderSet, textureInputs);
        }

        public ShaderStageApplicabilityFlags GetApplicabilityForSlot(int slot)
        {
            return _applicabilities[slot];
        }

        private ShaderStageApplicabilityFlags[] ComputeStageApplicabilities(D3DShaderSet shaderSet, ShaderTextureInput[] textureInputs)
        {
            ShaderStageApplicabilityFlags[] stageFlagsBySlot = new ShaderStageApplicabilityFlags[textureInputs.Length];
            for (int i = 0; i < stageFlagsBySlot.Length; i++)
            {
                ShaderTextureInput element = textureInputs[i];
                ShaderStageApplicabilityFlags flags = ShaderStageApplicabilityFlags.None;

                if (IsTextureSlotUsedInShader(shaderSet.VertexShader, i
#if DEBUG
                    , element.Name
#endif
                ))
                {
                    flags |= ShaderStageApplicabilityFlags.Vertex;
                }

                if (IsTextureSlotUsedInShader(shaderSet.FragmentShader, i
#if DEBUG
                    , element.Name
#endif
                ))
                {
                    flags |= ShaderStageApplicabilityFlags.Fragment;
                }


                if (shaderSet.GeometryShader != null && IsTextureSlotUsedInShader(shaderSet.GeometryShader, i
#if DEBUG
                    , element.Name
#endif
                ))
                {
                    flags |= ShaderStageApplicabilityFlags.Geometry;
                }

                stageFlagsBySlot[i] = flags;
            }

            return stageFlagsBySlot;
        }

        private bool IsTextureSlotUsedInShader<TShader>(D3DShader<TShader> shader, int slot
#if DEBUG
            , string name)
#else
            )
#endif
            where TShader : IDisposable
        {
            ShaderReflection reflection = shader.Reflection;
            int numResources = reflection.Description.BoundResources;
            for (int i = 0; i < numResources; i++)
            {
                InputBindingDescription desc = reflection.GetResourceBindingDescription(i);
                if (desc.Type == ShaderInputType.Texture && desc.BindPoint == slot)
                {
#if DEBUG
                    if (desc.Name != name)
                    {
                        Debug.WriteLine($"The texture resource in slot {slot} had an unexpected name. Expected: {name} Actual: {desc.Name}");
                    }
#endif
                    return true;
                }
            }

            return false;
        }
    }

    [Flags]
    public enum ShaderStageApplicabilityFlags : byte
    {
        None = 0,
        Vertex = 1 << 0,
        Geometry = 1 << 1,
        Fragment = 1 << 2
    }
}