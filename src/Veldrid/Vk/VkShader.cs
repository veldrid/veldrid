using System;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static Veldrid.Vulkan.VulkanUtil;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkShader : Shader, IResourceRefCountTarget
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkShaderModule _shaderModule;
        private string? _name;

        public ResourceRefCount RefCount { get; }

        public VkShaderModule ShaderModule => _shaderModule;

        public override bool IsDisposed => RefCount.IsDisposed;

        public VkShader(VkGraphicsDevice gd, in ShaderDescription description)
            : base(description.Stage, description.EntryPoint)
        {
            _gd = gd;

            fixed (byte* codePtr = description.ShaderBytes)
            {
                VkShaderModuleCreateInfo shaderModuleCI = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO,
                    codeSize = (UIntPtr)description.ShaderBytes.Length,
                    pCode = (uint*)codePtr
                };

                VkShaderModule shaderModule;
                VkResult result = vkCreateShaderModule(gd.Device, &shaderModuleCI, null, &shaderModule);
                CheckResult(result);
                _shaderModule = shaderModule;
            }

            RefCount = new ResourceRefCount(this);
        }

        public override string? Name
        {
            get => _name;
            set
            {
                _name = value;
                _gd.SetResourceName(this, value);
            }
        }

        public override void Dispose()
        {
            RefCount.DecrementDispose();
        }

        public void RefZeroed()
        {
            vkDestroyShaderModule(_gd.Device, ShaderModule, null);
        }
    }
}
