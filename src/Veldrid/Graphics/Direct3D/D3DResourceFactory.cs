using System;
using SharpDX.Direct3D11;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DResourceFactory : ResourceFactory
    {
        private static readonly string s_shaderFileExtension = "hlsl";

        private readonly Device _device;
        private List<ShaderLoader> _shaderLoaders = new List<ShaderLoader>();

        public D3DResourceFactory(Device device)
        {
            _device = device;
            AddShaderLoader(new FolderShaderLoader(Path.Combine(AppContext.BaseDirectory, "HLSL")));
        }

        public override ConstantBuffer CreateConstantBuffer(int sizeInBytes)
        {
            return new D3DConstantBuffer(_device, sizeInBytes);
        }

        public override Framebuffer CreateFramebuffer()
        {
            return new D3DFramebuffer(_device);
        }

        public override Framebuffer CreateFramebuffer(int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            D3DTexture2D colorTexture = new D3DTexture2D(_device, new Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            D3DTexture2D depthTexture = new D3DTexture2D(_device, new Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            return new D3DFramebuffer(_device, colorTexture, depthTexture);
        }

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new D3DIndexBuffer(_device, sizeInBytes, isDynamic);
        }
        public override IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic, IndexFormat format)
        {
            return new D3DIndexBuffer(_device, sizeInBytes, isDynamic, D3DFormats.ConvertIndexFormat(format));
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
            Stream vsStream = GetShaderStream(vertexShaderName);
            Stream psStream = GetShaderStream(pixelShaderName);

            return new D3DMaterial(
                _device,
                (D3DRenderContext)rc,
                vsStream,
                vertexShaderName,
                psStream,
                pixelShaderName,
                new[] { vertexInputs },
                globalInputs,
                perObjectInputs,
                textureInputs);
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
            Stream vsStream = GetShaderStream(vertexShaderName);
            Stream psStream = GetShaderStream(pixelShaderName);

            return new D3DMaterial(
                _device,
                (D3DRenderContext)rc,
                vsStream,
                vertexShaderName,
                psStream,
                pixelShaderName,
                new[] { vertexInputs0, vertexInputs1 },
                globalInputs,
                perObjectInputs,
                textureInputs);
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
            Stream vsStream = GetShaderStream(vertexShaderName);
            Stream psStream = GetShaderStream(pixelShaderName);

            return new D3DMaterial(
                _device,
                (D3DRenderContext)rc,
                vsStream,
                vertexShaderName,
                psStream,
                pixelShaderName,
                vertexInputs,
                globalInputs,
                perObjectInputs,
                textureInputs);
        }

        public override VertexBuffer CreateVertexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new D3DVertexBuffer(_device, sizeInBytes, isDynamic);
        }

        public override DeviceTexture2D CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            GCHandle handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            D3DTexture2D texture = new D3DTexture2D(
                _device,
                BindFlags.ShaderResource,
                ResourceUsage.Default,
                CpuAccessFlags.None,
                D3DFormats.ConvertPixelFormat(format),
                handle.AddrOfPinnedObject(),
                width,
                height,
                width * pixelSizeInBytes);
            handle.Free();
            return texture;
        }

        public override DeviceTexture2D CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            D3DTexture2D texture = new D3DTexture2D(
                _device,
                BindFlags.ShaderResource,
                ResourceUsage.Default,
                CpuAccessFlags.None,
                D3DFormats.ConvertPixelFormat(format),
                pixelData,
                width,
                height,
                width * pixelSizeInBytes);
            return texture;
        }

        public override DeviceTexture2D CreateDepthTexture(int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            if (format != PixelFormat.Alpha_UInt16)
            {
                throw new NotImplementedException("Alpha_UInt16 is the only supported depth texture format.");
            }

            return new D3DTexture2D(_device, new Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.R16_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
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
            return new D3DCubemapTexture(_device, pixelsFront, pixelsBack, pixelsLeft, pixelsRight, pixelsTop, pixelsBottom, width, height, pixelSizeinBytes, format);
        }

        public override ShaderTextureBinding CreateShaderTextureBinding(DeviceTexture texture)
        {
            D3DTexture d3dTexture = (D3DTexture)texture;
            ShaderResourceViewDescription srvd = d3dTexture.GetShaderResourceViewDescription();
            ShaderResourceView srv = new ShaderResourceView(_device, d3dTexture.DeviceTexture, srvd);
            return new D3DTextureBinding(srv, d3dTexture);
        }

        public override BlendState CreateCustomBlendState(bool isBlendEnabled, Blend srcBlend, Blend destBlend, BlendFunction blendFunc)
        {
            return new D3DBlendState(_device, isBlendEnabled, srcBlend, destBlend, blendFunc, srcBlend, destBlend, blendFunc);
        }

        public override BlendState CreateCustomBlendState(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc)
        {
            return new D3DBlendState(_device, isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc);
        }

        public override DepthStencilState CreateDepthStencilState(bool isDepthEnabled, DepthComparison comparison)
        {
            return new D3DDepthStencilState(_device, isDepthEnabled, comparison);
        }

        public override RasterizerState CreateRasterizerState(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool isDepthClipEnabled,
            bool isScissorTestEnabled)
        {
            return new D3DRasterizerState(_device, cullMode, fillMode, isDepthClipEnabled, isScissorTestEnabled);
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
