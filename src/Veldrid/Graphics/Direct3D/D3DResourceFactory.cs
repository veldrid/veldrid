using System;
using System.Collections.Generic;
using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DResourceFactory : ResourceFactory
    {
        private readonly Device _device;

        public D3DResourceFactory(Device device)
        {
            _device = device;
        }

        public override ConstantBuffer CreateConstantBuffer()
        {
            return new D3DConstantBuffer(_device);
        }

        public override IndexBuffer CreateIndexBuffer()
        {
            return new D3DIndexBuffer(_device);
        }

        public override Material CreateMaterial(string vertexShaderName, string pixelShaderName, MaterialVertexInput vertexInputs, MaterialGlobalInputs globalInputs, MaterialTextureInputs textureInputs)
        {
            return new D3DMaterial(
                _device,
                vertexShaderName,
                pixelShaderName,
                vertexInputs,
                globalInputs,
                textureInputs);
        }

        public override VertexBuffer CreateVertexBuffer()
        {
            return new D3DVertexBuffer(_device);
        }
    }
}
