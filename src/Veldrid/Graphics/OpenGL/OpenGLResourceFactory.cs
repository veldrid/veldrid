﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLResourceFactory : ResourceFactory
    {
        private static readonly string s_shaderDirectory = "GLSL";
        private static readonly string s_shaderFileExtension = "glsl";

        public override ConstantBuffer CreateConstantBuffer(int sizeInBytes)
        {
            return new OpenGLConstantBuffer();
        }

        public override Framebuffer CreateFramebuffer()
        {
            return new OpenGLFramebuffer();
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
                PixelInternalFormat.DepthComponent16,
                OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                PixelType.UnsignedShort);

            return new OpenGLFramebuffer(colorTexture, depthTexture);
        }

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new OpenGLIndexBuffer(isDynamic);
        }
        public override IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic, IndexFormat format)
        {
            return new OpenGLIndexBuffer(isDynamic, OpenGLFormats.MapIndexFormat(format));
        }

        public override Material CreateMaterial(
            RenderContext rc,
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

            return new OpenGLMaterial((OpenGLRenderContext)rc, vertexShader, fragmentShader, inputs, globalInputs, perObjectInputs, textureInputs);
        }

        public override DeviceTexture CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return new OpenGLTexture(width, height, format, pixelData);
        }

        public override DeviceTexture CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return OpenGLTexture.Create(pixelData, width, height, pixelSizeInBytes, format);
        }

        public override DeviceTexture CreateDepthTexture(int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            if (format != PixelFormat.Alpha_UInt16)
            {
                throw new NotImplementedException("Alpha_UInt16 is the only supported depth texture format.");
            }

            return new OpenGLTexture(
                width,
                height,
                PixelInternalFormat.DepthComponent16,
                OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                PixelType.UnsignedShort);
        }

        public override VertexBuffer CreateVertexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new OpenGLVertexBuffer(isDynamic);
        }

        public override BlendState CreateCustomBlendState(bool isBlendEnabled, Blend srcBlend, Blend destBlend, BlendFunction blendFunc)
        {
            return new OpenGLBlendState(isBlendEnabled, srcBlend, destBlend, blendFunc, srcBlend, destBlend, blendFunc);
        }

        public override BlendState CreateCustomBlendState(bool isBlendEnabled, Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc, Blend srcColor, Blend destColor, BlendFunction colorBlendFunc)
        {
            return new OpenGLBlendState(isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc);
        }

        private string GetShaderPathFromName(string shaderName)
        {
            return Path.Combine(AppContext.BaseDirectory, s_shaderDirectory, shaderName + "." + s_shaderFileExtension);
        }

        public override DepthStencilState CreateDepthStencilState(bool isDepthEnabled, DepthComparison comparison)
        {
            return new OpenGLDepthStencilState(isDepthEnabled, comparison);
        }

        public override RasterizerState CreateRasterizerState(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool isDepthClipEnabled,
            bool isScissorTestEnabled)
        {
            return new OpenGLRasterizerState(cullMode, fillMode, isDepthClipEnabled, isScissorTestEnabled);
        }
    }
}
