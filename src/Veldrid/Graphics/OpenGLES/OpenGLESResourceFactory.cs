using OpenTK.Graphics.ES30;
using System;
using System.Collections.Generic;
using System.IO;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESResourceFactory : ResourceFactory
    {
        private static readonly string s_shaderFileExtension = "glsl";

        private List<ShaderLoader> _shaderLoaders = new List<ShaderLoader>();

        public OpenGLESResourceFactory()
        {
            AddShaderLoader(new FolderShaderLoader(Path.Combine(AppContext.BaseDirectory, "GLSL")));
        }

        public override ConstantBuffer CreateConstantBuffer(int sizeInBytes)
        {
            return new OpenGLESConstantBuffer();
        }

        public override Framebuffer CreateFramebuffer()
        {
            return new OpenGLESFramebuffer();
        }

        public override Framebuffer CreateFramebuffer(int width, int height)
        {
            OpenGLESTexture2D colorTexture = new OpenGLESTexture2D(
                1,
                width, height,
                PixelFormat.R8_G8_B8_A8_UInt,
                OpenTK.Graphics.ES30.PixelFormat.Rgba,
                PixelType.UnsignedByte);
            OpenGLESTexture2D depthTexture = new OpenGLESTexture2D(
                1,
                width,
                height,
                PixelFormat.R16_UInt,
                OpenTK.Graphics.ES30.PixelFormat.DepthComponent,
                PixelType.UnsignedShort);

            return new OpenGLESFramebuffer(colorTexture, depthTexture);
        }

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic, IndexFormat format)
        {
            return new OpenGLESIndexBuffer(isDynamic, OpenGLESFormats.MapIndexFormat(format));
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
            return new OpenGLESShader(shaderCode, OpenGLESFormats.VeldridToGLShaderType(type));
        }

        public override ShaderSet CreateShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader fragmentShader)
        {
            return new OpenGLESShaderSet((OpenGLESVertexInputLayout)inputLayout, (OpenGLESShader)vertexShader, (OpenGLESShader)fragmentShader);
        }

        public override ShaderSet CreateShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader geometryShader, Shader fragmentShader)
        {
            throw new NotSupportedException();
        }

        public override ShaderConstantBindingSlots CreateShaderConstantBindingSlots(
            ShaderSet shaderSet,
            ShaderConstantDescription[] constants)
        {
            return new OpenGLESShaderConstantBindingSlots(shaderSet, constants);
        }

        public override VertexInputLayout CreateInputLayout(VertexInputDescription[] vertexInputs)
        {
            return new OpenGLESVertexInputLayout(vertexInputs);
        }

        public override ShaderTextureBindingSlots CreateShaderTextureBindingSlots(ShaderSet shaderSet, ShaderTextureInput[] textureInputs)
        {
            return new OpenGLESTextureBindingSlots(shaderSet, textureInputs);
        }

        public override ShaderTextureBinding CreateShaderTextureBinding(DeviceTexture texture)
        {
            if (texture is OpenGLESTexture2D)
            {
                return new OpenGLESTextureBinding((OpenGLESTexture2D)texture);
            }
            else
            {
                return new OpenGLESTextureBinding((OpenGLESCubemapTexture)texture);
            }
        }

        public override DeviceTexture2D CreateTexture(int mipLevels, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return new OpenGLESTexture2D(mipLevels, width, height, format, OpenGLESFormats.MapPixelFormat(format), OpenGLESFormats.MapPixelType(format));
        }

        protected override SamplerState CreateSamplerStateCore(
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
            return new OpenGLESSamplerState(addressU, addressV, addressW, filter, maxAnisotropy, borderColor, comparison, minimumLod, maximumLod, lodBias);
        }

        public override DeviceTexture2D CreateDepthTexture(int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            if (format != PixelFormat.R16_UInt)
            {
                throw new NotImplementedException("R16_UInt is the only supported depth texture format.");
            }

            return new OpenGLESTexture2D(
                1,
                width,
                height,
                PixelFormat.R16_UInt,
                OpenTK.Graphics.ES30.PixelFormat.DepthComponent,
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
            return new OpenGLESCubemapTexture(
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
            return new OpenGLESVertexBuffer(isDynamic);
        }

        protected override BlendState CreateCustomBlendStateCore(
            bool isBlendEnabled,
            Blend srcAlpha,
            Blend destAlpha,
            BlendFunction alphaBlendFunc,
            Blend srcColor,
            Blend destColor,
            BlendFunction colorBlendFunc,
            RgbaFloat blendFactor)
        {
            return new OpenGLESBlendState(isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc, blendFactor);
        }

        protected override DepthStencilState CreateDepthStencilStateCore(bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled)
        {
            return new OpenGLESDepthStencilState(isDepthEnabled, comparison, isDepthWriteEnabled);
        }

        protected override RasterizerState CreateRasterizerStateCore(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool isDepthClipEnabled,
            bool isScissorTestEnabled)
        {
            return new OpenGLESRasterizerState(cullMode, fillMode, isDepthClipEnabled, isScissorTestEnabled);
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
