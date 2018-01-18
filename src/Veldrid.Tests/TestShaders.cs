using System;
using System.IO;

namespace Veldrid.Tests
{
    internal static class TestShaders
    {
        public static Shader Load(ResourceFactory factory, string setName, ShaderStages stage, string entryPoint)
        {
            string path = Path.Combine(
                AppContext.BaseDirectory,
                "Shaders",
                $"{setName}-{stage.ToString().ToLowerInvariant()}.{GetShaderExtension(factory.BackendType)}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return factory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
        }

        private static string GetShaderExtension(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11: return "hlsl.bytes";
                case GraphicsBackend.Vulkan: return "450.glsl.spv";
                case GraphicsBackend.OpenGL: return "330.glsl";
                case GraphicsBackend.Metal: return "metallib";
                default: throw new InvalidOperationException();
            }
        }
    }
}
