using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLResourceFactory : ResourceFactory
    {
        private static readonly string s_shaderFileExtension = "glsl";

        private List<ShaderLoader> _shaderLoaders = new List<ShaderLoader>();

        public OpenGLResourceFactory()
        {
            AddShaderLoader(new FolderShaderLoader(Path.Combine(AppContext.BaseDirectory, "GLSL")));
        }

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
            OpenGLTexture2D colorTexture = new OpenGLTexture2D(
                1,
                width, height,
                PixelFormat.R32_G32_B32_A32_Float,
                PixelInternalFormat.Rgba32f,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
                PixelType.Float);
            OpenGLTexture2D depthTexture = new OpenGLTexture2D(
                1,
                width,
                height,
                PixelFormat.R16_UInt,
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

        public override Shader CreateShader(ShaderType type, string name)
        {
            using (Stream stream = GetShaderStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                return CreateShader(type, reader.ReadToEnd(), name);
            }
        }

        public override Shader CreateShader(ShaderType type, string shaderCode, string name)
        {
            return new OpenGLShader(shaderCode, OpenGLFormats.VeldridToGLShaderType(type));
        }

        public override ShaderSet CreateShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader fragmentShader)
        {
            return new OpenGLShaderSet((OpenGLVertexInputLayout)inputLayout, (OpenGLShader)vertexShader, null, (OpenGLShader)fragmentShader);
        }

        public override ShaderSet CreateShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader geometryShader, Shader fragmentShader)
        {
            return new OpenGLShaderSet((OpenGLVertexInputLayout)inputLayout, (OpenGLShader)vertexShader, (OpenGLShader)geometryShader, (OpenGLShader)fragmentShader);
        }

        public override ShaderConstantBindings CreateShaderConstantBindings(
            RenderContext rc,
            ShaderSet shaderSet,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs)
        {
            return new OpenGLShaderConstantBindings(rc, shaderSet, globalInputs, perObjectInputs);
        }

        public override VertexInputLayout CreateInputLayout(Shader shader, MaterialVertexInput[] vertexInputs)
        {
            return new OpenGLVertexInputLayout(vertexInputs);
        }

        public override ShaderTextureBindingSlots CreateShaderTextureBindingSlots(ShaderSet shaderSet, MaterialTextureInputs textureInputs)
        {
            return new OpenGLTextureBindingSlots(shaderSet, textureInputs);
        }

        public override ShaderTextureBinding CreateShaderTextureBinding(DeviceTexture texture)
        {
            if (texture is OpenGLTexture2D)
            {
                return new OpenGLTextureBinding((OpenGLTexture2D)texture);
            }
            else
            {
                return new OpenGLTextureBinding((OpenGLCubemapTexture)texture);
            }
        }

        public override DeviceTexture2D CreateTexture(int mipLevels, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return new OpenGLTexture2D(
                mipLevels,
                width,
                height,
                format,
                OpenGLFormats.MapPixelInternalFormat(format),
                OpenGLFormats.MapPixelFormat(format),
                OpenGLFormats.MapPixelType(format));
        }

        public override SamplerState CreateSamplerStateCore(
            SamplerAddressMode addressU,
            SamplerAddressMode addressV,
            SamplerAddressMode addressW,
            SamplerFilter filter,
            int maxAnisotropy,
            RgbaFloat borderColor,
            DepthComparison comparison,
            int minimumLod,
            int maximumLod,
            int lodBias)
        {
            return new OpenGLSamplerState(addressU, addressV, addressW, filter, maxAnisotropy, borderColor, comparison, minimumLod, maximumLod, lodBias);
        }

        public override DeviceTexture2D CreateDepthTexture(int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            if (format != PixelFormat.R16_UInt)
            {
                throw new NotImplementedException("R16_UInt is the only supported depth texture format.");
            }

            return new OpenGLTexture2D(
                1,
                width,
                height,
                PixelFormat.R16_UInt,
                PixelInternalFormat.DepthComponent16,
                OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                PixelType.UnsignedShort);
        }

        public override CubemapTexture CreateCubemapTexture(
            IntPtr pixelsFront,
            IntPtr pixelsBack,
            IntPtr pixelsLeft,
            IntPtr pixelsRight,
            IntPtr pixelsTop,
            IntPtr pixelsBottom,
            int width,
            int height,
            int pixelSizeinBytes,
            PixelFormat format)
        {
            return new OpenGLCubemapTexture(
                pixelsFront,
                pixelsBack,
                pixelsLeft,
                pixelsRight,
                pixelsTop,
                pixelsBottom,
                width,
                height,
                format);
        }

        public override VertexBuffer CreateVertexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new OpenGLVertexBuffer(isDynamic);
        }

        public override BlendState CreateCustomBlendStateCore(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc,
            RgbaFloat blendFactor)
        {
            return new OpenGLBlendState(isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc, blendFactor);
        }

        public override DepthStencilState CreateDepthStencilState(bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled)
        {
            return new OpenGLDepthStencilState(isDepthEnabled, comparison, isDepthWriteEnabled);
        }

        public override RasterizerState CreateRasterizerState(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool isDepthClipEnabled,
            bool isScissorTestEnabled)
        {
            return new OpenGLRasterizerState(cullMode, fillMode, isDepthClipEnabled, isScissorTestEnabled);
        }

        public override void AddShaderLoader(ShaderLoader loader)
        {
            _shaderLoaders.Add(loader);
        }

        private Stream GetShaderStream(string name)
        {
            foreach (var loader in _shaderLoaders)
            {
                Stream s;
                if (loader.TryOpenShader(name, s_shaderFileExtension, out s))
                {
                    return s;
                }
            }

            throw new InvalidOperationException("No registered loader was able to find shader: " + name);
        }
    }
}
