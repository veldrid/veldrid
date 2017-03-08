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
                width, height,
                PixelFormat.R8_G8_B8_A8,
                OpenTK.Graphics.ES30.PixelFormat.Rgba,
                PixelType.UnsignedByte);
            OpenGLESTexture2D depthTexture = new OpenGLESTexture2D(
                width,
                height,
                PixelFormat.Alpha_UInt16,
                OpenTK.Graphics.ES30.PixelFormat.DepthComponent,
                PixelType.UnsignedShort);

            return new OpenGLESFramebuffer(colorTexture, depthTexture);
        }

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new OpenGLESIndexBuffer(isDynamic);
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

        public override ShaderConstantBindings CreateShaderConstantBindings(
            RenderContext rc,
            ShaderSet shaderSet,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs)
        {
            return new OpenGLESShaderConstantBindings(rc, shaderSet, globalInputs, perObjectInputs);
        }

        public override VertexInputLayout CreateInputLayout(Shader shader, MaterialVertexInput[] vertexInputs)
        {
            return new OpenGLESVertexInputLayout(vertexInputs);
        }

        public override ShaderTextureBindingSlots CreateShaderTextureBindingSlots(ShaderSet shaderSet, MaterialTextureInputs textureInputs)
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

        public override DeviceTexture2D CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return new OpenGLESTexture2D(width, height, format, pixelData);
        }

        public override DeviceTexture2D CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return OpenGLESTexture2D.Create(pixelData, width, height, pixelSizeInBytes, format);
        }

        public override DeviceTexture2D CreateDepthTexture(int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            if (format != PixelFormat.Alpha_UInt16)
            {
                throw new NotImplementedException("Alpha_UInt16 is the only supported depth texture format.");
            }

            return new OpenGLESTexture2D(
                width,
                height,
                PixelFormat.Alpha_UInt16,
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

        public override BlendState CreateCustomBlendState(bool isBlendEnabled, Blend srcBlend, Blend destBlend, BlendFunction blendFunc)
        {
            return new OpenGLESBlendState(isBlendEnabled, srcBlend, destBlend, blendFunc, srcBlend, destBlend, blendFunc);
        }

        public override BlendState CreateCustomBlendState(bool isBlendEnabled, Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc, Blend srcColor, Blend destColor, BlendFunction colorBlendFunc)
        {
            return new OpenGLESBlendState(isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc);
        }

        public override DepthStencilState CreateDepthStencilState(bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled)
        {
            return new OpenGLESDepthStencilState(isDepthEnabled, comparison, isDepthWriteEnabled);
        }

        public override RasterizerState CreateRasterizerState(
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
