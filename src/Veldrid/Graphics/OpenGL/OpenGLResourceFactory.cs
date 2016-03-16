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

        public override Framebuffer CreateFramebuffer(int width, int height)
        {
            OpenGLTexture colorTexture = new OpenGLTexture(
                width, height,
                PixelInternalFormat.Rgba32f,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
                PixelType.Float);
            OpenGLTexture depthTexture = new OpenGLTexture(
                width,
                height,
                PixelInternalFormat.DepthComponent24,
                OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                PixelType.UnsignedInt);

            return new OpenGLFramebuffer(colorTexture, depthTexture);
        }

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new OpenGLIndexBuffer(isDynamic);
        }

        public override Material CreateMaterial(
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput inputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
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

            return new OpenGLMaterial(this, vertexShader, fragmentShader, inputs, globalInputs, perObjectInputs, textureInputs);
        }

        public override DeviceTexture CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return new OpenGLTexture(width, height, format, pixelData);
        }

        public override DeviceTexture CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return OpenGLTexture.Create(pixelData, width, height, pixelSizeInBytes, format);
        }

        public override VertexBuffer CreateVertexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new OpenGLVertexBuffer(isDynamic);
        }

        public override BlendState CreateCustomBlendState(bool isBlendEnabled, Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc, Blend srcColor, Blend destColor, BlendFunction colorBlendFunc)
        {
            return new OpenGLBlendState(isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc);
        }

        private string GetShaderPathFromName(string shaderName)
        {
            return Path.Combine(AppContext.BaseDirectory, s_shaderDirectory, shaderName + "." + s_shaderFileExtension);
        }
    }
}
