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
                width, height,
                PixelInternalFormat.Rgba32f,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
                PixelType.Float);
            OpenGLTexture2D depthTexture = new OpenGLTexture2D(
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
            MaterialVertexInput vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs)
        {
            OpenGLShader vertexShader, fragmentShader;
            using (Stream vsStream = GetShaderStream(vertexShaderName))
            {
                vertexShader = new OpenGLShader(vsStream, OpenTK.Graphics.OpenGL.ShaderType.VertexShader);
            }
            using (Stream psStream = GetShaderStream(pixelShaderName))
            {
                fragmentShader = new OpenGLShader(psStream, OpenTK.Graphics.OpenGL.ShaderType.FragmentShader);
            }

            VertexInputLayout inputLayout = CreateInputLayout(vertexShader, new[] { vertexInputs });
            ShaderSet shaderSet = CreateShaderSet(inputLayout, vertexShader, fragmentShader);
            ShaderConstantBindings constantBindings = CreateShaderConstantBindings(rc, shaderSet, globalInputs, perObjectInputs);
            return new OpenGLMaterial((OpenGLRenderContext)rc, shaderSet, inputLayout, constantBindings, textureInputs);
        }

        public override Material CreateMaterial(
            RenderContext rc,
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput vertexInputs0,
            MaterialVertexInput vertexInputs1,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs)
        {
            OpenGLShader vertexShader, fragmentShader;
            using (Stream vsStream = GetShaderStream(vertexShaderName))
            {
                vertexShader = new OpenGLShader(vsStream, OpenTK.Graphics.OpenGL.ShaderType.VertexShader);
            }
            using (Stream psStream = GetShaderStream(pixelShaderName))
            {
                fragmentShader = new OpenGLShader(psStream, OpenTK.Graphics.OpenGL.ShaderType.FragmentShader);
            }

            VertexInputLayout inputLayout = CreateInputLayout(vertexShader, new[] { vertexInputs0, vertexInputs1 });
            ShaderSet shaderSet = CreateShaderSet(inputLayout, vertexShader, fragmentShader);
            ShaderConstantBindings constantBindings = CreateShaderConstantBindings(rc, shaderSet, globalInputs, perObjectInputs);
            return new OpenGLMaterial((OpenGLRenderContext)rc, shaderSet, inputLayout, constantBindings, textureInputs);
        }

        public override Material CreateMaterial(
            RenderContext rc,
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput[] vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs)
        {
            OpenGLShader vertexShader, fragmentShader;
            using (Stream vsStream = GetShaderStream(vertexShaderName))
            {
                vertexShader = new OpenGLShader(vsStream, OpenTK.Graphics.OpenGL.ShaderType.VertexShader);
            }
            using (Stream psStream = GetShaderStream(pixelShaderName))
            {
                fragmentShader = new OpenGLShader(psStream, OpenTK.Graphics.OpenGL.ShaderType.FragmentShader);
            }

            VertexInputLayout inputLayout = CreateInputLayout(vertexShader, vertexInputs);
            ShaderSet shaderSet = CreateShaderSet(inputLayout, vertexShader, fragmentShader);
            ShaderConstantBindings constantBindings = CreateShaderConstantBindings(rc, shaderSet, globalInputs, perObjectInputs);
            return new OpenGLMaterial((OpenGLRenderContext)rc, shaderSet, inputLayout, constantBindings, textureInputs);
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

        public override DeviceTexture2D CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return new OpenGLTexture2D(width, height, format, pixelData);
        }

        public override DeviceTexture2D CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            return OpenGLTexture2D.Create(pixelData, width, height, pixelSizeInBytes, format);
        }

        public override DeviceTexture2D CreateDepthTexture(int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            if (format != PixelFormat.Alpha_UInt16)
            {
                throw new NotImplementedException("Alpha_UInt16 is the only supported depth texture format.");
            }

            return new OpenGLTexture2D(
                width,
                height,
                PixelInternalFormat.DepthComponent16,
                OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                PixelType.UnsignedShort);
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

        public override BlendState CreateCustomBlendState(bool isBlendEnabled, Blend srcBlend, Blend destBlend, BlendFunction blendFunc)
        {
            return new OpenGLBlendState(isBlendEnabled, srcBlend, destBlend, blendFunc, srcBlend, destBlend, blendFunc);
        }

        public override BlendState CreateCustomBlendState(bool isBlendEnabled, Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc, Blend srcColor, Blend destColor, BlendFunction colorBlendFunc)
        {
            return new OpenGLBlendState(isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc);
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
