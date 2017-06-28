using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DShaderConstantBindingSlots : ShaderConstantBindingSlots
    {
        private readonly Device _device;
        private readonly ShaderStageApplicabilityFlags[] _applicabilityFlagsBySlot;
        
        public ShaderConstantDescription[] Constants { get; }

        public D3DShaderConstantBindingSlots(
            RenderContext rc,
            Device device,
            ShaderSet shaderSet,
            ShaderConstantDescription[] constants)
        {
            _device = device;
            Constants = constants;

            D3DShaderSet d3dShaderSet = (D3DShaderSet)shaderSet;

            ShaderReflection vsReflection = d3dShaderSet.VertexShader.Reflection;
            ShaderReflection psReflection = d3dShaderSet.FragmentShader.Reflection;
            ShaderReflection gsReflection = null;
            if (shaderSet.GeometryShader != null)
            {
                gsReflection = d3dShaderSet.GeometryShader.Reflection;
            }

            int numConstants = constants.Length;
            _applicabilityFlagsBySlot = new ShaderStageApplicabilityFlags[numConstants];
            for (int i = 0; i < numConstants; i++)
            {
                var genericElement = constants[i];
                bool isVsBuffer = DoesConstantBufferExist(vsReflection, i, genericElement.Name);
                bool isPsBuffer = DoesConstantBufferExist(psReflection, i, genericElement.Name);
                bool isGsBuffer = false;
                if (gsReflection != null)
                {
                    isGsBuffer = DoesConstantBufferExist(gsReflection, i, genericElement.Name);
                }

                ShaderStageApplicabilityFlags applicabilityFlags = ShaderStageApplicabilityFlags.None;
                if (isVsBuffer)
                {
                    applicabilityFlags |= ShaderStageApplicabilityFlags.Vertex;
                }
                if (isPsBuffer)
                {
                    applicabilityFlags |= ShaderStageApplicabilityFlags.Fragment;
                }
                if (isGsBuffer)
                {
                    applicabilityFlags |= ShaderStageApplicabilityFlags.Geometry;
                }

                _applicabilityFlagsBySlot[i] = applicabilityFlags;
            }
        }

        private static bool DoesConstantBufferExist(ShaderReflection reflection, int slot, string name)
        {
            InputBindingDescription bindingDesc;
            try
            {
                bindingDesc = reflection.GetResourceBindingDescription(name);

                if (bindingDesc.BindPoint != slot)
                {
                    throw new InvalidOperationException($"Mismatched binding slot for {name}. Expected: {slot}, Actual: {bindingDesc.BindPoint}");
                }

                return true;
            }
            catch (SharpDX.SharpDXException)
            {
                for (int i = 0; i < reflection.Description.BoundResources; i++)
                {
                    var desc = reflection.GetResourceBindingDescription(i);
                    if (desc.Type == ShaderInputType.ConstantBuffer && desc.BindPoint == slot)
                    {
                        System.Diagnostics.Debug.WriteLine("Buffer in slot " + slot + " has wrong name. Expected: " + name + ", Actual: " + desc.Name);
                        bindingDesc = desc;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}