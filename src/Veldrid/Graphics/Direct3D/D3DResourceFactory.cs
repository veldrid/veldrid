using System;
using SharpDX.Direct3D11;
using System.IO;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DResourceFactory : ResourceFactory
    {
        private static readonly string s_shaderDirectory = "Graphics/Direct3D/Shaders";
        private static readonly string s_shaderFileExtension = "hlsl";

        private readonly Device _device;

        public D3DResourceFactory(Device device)
        {
            _device = device;
        }

        public override ConstantBuffer CreateConstantBuffer(int sizeInBytes)
        {
            return new D3DConstantBuffer(_device, sizeInBytes);
        }

        public override Framebuffer CreateFramebuffer(int width, int height)
        {
            D3DTexture colorTexture = new D3DTexture(_device, new Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            D3DTexture depthTexture = new D3DTexture(_device, new Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = Math.Max(1, width),
                Height = Math.Max(1, height),
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            return new D3DFramebuffer(_device, colorTexture, depthTexture);
        }

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes)
        {
            return new D3DIndexBuffer(_device, sizeInBytes);
        }

        public override Material CreateMaterial(
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs)
        {
            string vertexShaderPath = GetShaderPathFromName(vertexShaderName);
            string pixelShaderPath = GetShaderPathFromName(pixelShaderName);

            return new D3DMaterial(
                _device,
                this,
                vertexShaderPath,
                pixelShaderPath,
                vertexInputs,
                globalInputs,
                perObjectInputs,
                textureInputs);
        }

        public override VertexBuffer CreateVertexBuffer(int sizeInBytes)
        {
            return new D3DVertexBuffer(_device, sizeInBytes);
        }

        public override DeviceTexture CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            GCHandle handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            D3DTexture texture = new D3DTexture(
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

        public override DeviceTexture CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format)
        {
            D3DTexture texture = new D3DTexture(
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

        private string GetShaderPathFromName(string shaderName)
        {
            return Path.Combine(AppContext.BaseDirectory, s_shaderDirectory, shaderName + "." + s_shaderFileExtension);
        }
    }
}
