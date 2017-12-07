using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11Shader : Shader
    {
        private string _name;

        public DeviceChild DeviceShader { get; }
        public byte[] Bytecode { get; internal set; }

        public D3D11Shader(Device device, ShaderDescription description)
            : base(description.Stage)
        {
            switch (description.Stage)
            {
                case ShaderStages.Vertex:
                    DeviceShader = new VertexShader(device, description.ShaderBytes);
                    break;
                case ShaderStages.Geometry:
                    DeviceShader = new GeometryShader(device, description.ShaderBytes);
                    break;
                case ShaderStages.TessellationControl:
                    DeviceShader = new HullShader(device, description.ShaderBytes);
                    break;
                case ShaderStages.TessellationEvaluation:
                    DeviceShader = new DomainShader(device, description.ShaderBytes);
                    break;
                case ShaderStages.Fragment:
                    DeviceShader = new PixelShader(device, description.ShaderBytes);
                    break;
                case ShaderStages.Compute:
                    DeviceShader = new ComputeShader(device, description.ShaderBytes);
                    break;
                default:
                    throw Illegal.Value<ShaderStages>();
            }

            Bytecode = description.ShaderBytes;
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                DeviceShader.DebugName = value;
            }
        }

        public override void Dispose()
        {
            DeviceShader.Dispose();
        }
    }
}