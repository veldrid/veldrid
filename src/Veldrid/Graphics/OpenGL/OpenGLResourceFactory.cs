using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLResourceFactory : ResourceFactory
    {
        private static readonly string s_shaderDirectory = "Graphics/OpenGL/Shaders";
        private static readonly string s_shaderFileExtension = "glsl";

        public override ConstantBuffer CreateConstantBuffer(int sizeInBytes)
        {
            return new OpenGLConstantBuffer();
        }

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes)
        {
            return new OpenGLIndexBuffer();
        }

        public override Material CreateMaterial(
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput inputs,
            MaterialGlobalInputs globalInputs,
            MaterialTextureInputs textureInputs)
        {
            string vertexShaderPath = GetShaderPathFromName(vertexShaderName);
            string pixelShaderPath = GetShaderPathFromName(pixelShaderName);

            if (!File.Exists(vertexShaderPath))
            {
                throw new FileNotFoundException($"The shader file '{vertexShaderName}' was not found at {vertexShaderPath}.");
            }
            string vsSource = File.ReadAllText(vertexShaderPath);

            if (!File.Exists(pixelShaderPath))
            {
                throw new FileNotFoundException($"The shader file '{pixelShaderPath}' was not found at {pixelShaderPath}.");
            }
            string psSource = File.ReadAllText(pixelShaderPath);

            OpenGLShader vertexShader = new OpenGLShader(vsSource, ShaderType.VertexShader);
            OpenGLShader fragmentShader = new OpenGLShader(psSource, ShaderType.FragmentShader);

            return new OpenGLMaterial(vertexShader, fragmentShader, inputs, globalInputs, textureInputs);
        }

        public override VertexBuffer CreateVertexBuffer(int sizeInBytes)
        {
            return new OpenGLVertexBuffer();
        }

        private string GetShaderPathFromName(string shaderName)
        {
            return Path.Combine(s_shaderDirectory, shaderName + "." + s_shaderFileExtension);
        }
    }
}
