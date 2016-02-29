using SharpDX.Direct3D11;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DMaterial : Material
    {
        private readonly Device _device;
        private readonly VertexShader _vertexShader;
        private readonly PixelShader _pixelShader;
        private readonly InputLayout _inputLayout;

        public D3DMaterial(
            Device device,
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput vertexInputs,
            MaterialGlobalInputs globalInputs,
            MaterialTextureInputs textureInputs)
        {
            _device = device;
        }

        public void Apply()
        {
            _device.ImmediateContext.InputAssembler.InputLayout = _inputLayout;
            _device.ImmediateContext.VertexShader.Set(_vertexShader);
            _device.ImmediateContext.PixelShader.Set(_pixelShader);
        }
    }
}