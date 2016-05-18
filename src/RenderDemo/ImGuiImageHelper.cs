using System.Collections.Generic;
using Veldrid.Graphics;
using System;

namespace Veldrid.RenderDemo
{
    public static class ImGuiImageHelper
    {
        private static readonly Dictionary<DeviceTexture, ShaderTextureBindingInfo> s_textureBindings = new Dictionary<DeviceTexture, ShaderTextureBindingInfo>();
        private static Dictionary<IntPtr, ShaderTextureBindingInfo> s_bindings = new Dictionary<IntPtr, ShaderTextureBindingInfo>();
        private static int s_lastAssigned = 100;

        private struct ShaderTextureBindingInfo
        {
            public readonly IntPtr ImGuiBinding;
            public readonly ShaderTextureBinding DeviceBinding;

            public ShaderTextureBindingInfo(IntPtr imGuiBinding, ShaderTextureBinding deviceBinding)
            {
                ImGuiBinding = imGuiBinding;
                DeviceBinding = deviceBinding;
            }
        }

        public static IntPtr GetOrCreateImGuiBinding(RenderContext rc, DeviceTexture texture)
        {
            var deviceBinding = rc.ResourceFactory.CreateShaderTextureBinding(texture);
            return GetOrCreateImGuiBinding(rc, deviceBinding);
        }

        public static IntPtr GetOrCreateImGuiBinding(RenderContext rc, ShaderTextureBinding stb)
        {
            ShaderTextureBindingInfo stbi;

            if (!s_textureBindings.TryGetValue(stb.BoundTexture, out stbi))
            {
                var imGuiBinding = new IntPtr(++s_lastAssigned);
                stbi = new ShaderTextureBindingInfo(imGuiBinding, stb);

                s_textureBindings.Add(stb.BoundTexture, stbi);
                s_bindings.Add(imGuiBinding, stbi);
            }

            return stbi.ImGuiBinding;
        }

        public static ShaderTextureBinding GetShaderTextureBinding(IntPtr imGuiBinding)
        {
            ShaderTextureBindingInfo stbi;
            if (!s_bindings.TryGetValue(imGuiBinding, out stbi))
            {
                throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding.ToString());
            }

            return stbi.DeviceBinding;
        }

        public static void InvalidateCache()
        {
            foreach (var kvp in s_textureBindings)
            {
                kvp.Value.DeviceBinding.Dispose();
            }
        }
    }
}
