using System;
using System.IO;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class ShaderHelper
    {

        public static CompiledShaderCode LoadShaderCode(string shaderName, ShaderType type, ResourceFactory factory)
        {
            GraphicsBackend backend = factory.BackendType;
            string searchPath = Path.Combine(AppContext.BaseDirectory, backend == GraphicsBackend.Direct3D11 ? "HLSL" : "GLSL");

            if (backend == GraphicsBackend.Direct3D11)
            {
                // Try to load bytecode first.
                string bytecodePath = Path.Combine(searchPath, $"{shaderName}.hlsl.bytes");
                if (File.Exists(bytecodePath))
                {
                    return factory.LoadProcessedShader(File.ReadAllBytes(bytecodePath));
                }
                else
                {
                    string hlslPath = Path.Combine(searchPath, $"{shaderName}.hlsl");
                    if (File.Exists(hlslPath))
                    {
                        return factory.ProcessShaderCode(type, File.ReadAllText(hlslPath));
                    }
                }
            }
            else
            {
                string glslPath = Path.Combine(searchPath, $"{shaderName}.glsl");
                if (File.Exists(glslPath))
                {
                    return factory.ProcessShaderCode(type, File.ReadAllText(glslPath));
                }
            }

            throw new InvalidOperationException($"Couldn't open a shader stream for shader with name \"{shaderName}\" for backend \"{backend}\"");
        }
    }
}
