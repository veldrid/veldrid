using System;
using System.Collections.Generic;
using SharpDX.Direct3D11;
using System.IO;

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

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes)
        {
            return new D3DIndexBuffer(_device, sizeInBytes);
        }

        public override Material CreateMaterial(string vertexShaderName, string pixelShaderName, MaterialVertexInput vertexInputs, MaterialGlobalInputs globalInputs, MaterialTextureInputs textureInputs)
        {
            string vertexShaderPath = GetShaderPathFromName(vertexShaderName);
            string pixelShaderPath = GetShaderPathFromName(pixelShaderName);

            return new D3DMaterial(
                _device,
                vertexShaderPath,
                pixelShaderPath,
                vertexInputs,
                globalInputs,
                textureInputs);
        }

        public override VertexBuffer CreateVertexBuffer(int sizeInBytes)
        {
            return new D3DVertexBuffer(_device, sizeInBytes);
        }

        private string GetShaderPathFromName(string shaderName)
        {
            return Path.Combine(AppContext.BaseDirectory, s_shaderDirectory, shaderName + "." + s_shaderFileExtension);
        }
    }
}
