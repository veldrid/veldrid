using Vulkan;
using static Vulkan.VulkanNative;
using static Vd2.Vk.VulkanUtil;
using System;

namespace Vd2.Vk
{
    internal unsafe class VkShader : Shader
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkShaderModule _shaderModule;

        public VkShaderModule ShaderModule => _shaderModule;

        public VkShader(VkGraphicsDevice gd, ref ShaderDescription description)
        {
            _gd = gd;

            VkShaderModuleCreateInfo shaderModuleCI = VkShaderModuleCreateInfo.New();
            fixed (byte* codePtr = description.ShaderBytes)
            {
                shaderModuleCI.codeSize = (UIntPtr)description.ShaderBytes.Length;
                shaderModuleCI.pCode = (uint*)codePtr;
                VkResult result = vkCreateShaderModule(gd.Device, ref shaderModuleCI, null, out _shaderModule);
                CheckResult(result);
            }
        }
    }
}
