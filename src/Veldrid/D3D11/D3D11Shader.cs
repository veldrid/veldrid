using System;
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
            : base(description.Stage, description.EntryPoint)
        {
            if (description.ShaderBytes.Length > 4
                && description.ShaderBytes[0] == 0x44
                && description.ShaderBytes[1] == 0x58
                && description.ShaderBytes[2] == 0x42
                && description.ShaderBytes[3] == 0x43)
            {
                Bytecode = Util.ShallowClone(description.ShaderBytes);
            }
            else
            {
                Bytecode = CompileCode(description);
            }

            switch (description.Stage)
            {
                case ShaderStages.Vertex:
                    DeviceShader = new VertexShader(device, Bytecode);
                    break;
                case ShaderStages.Geometry:
                    DeviceShader = new GeometryShader(device, Bytecode);
                    break;
                case ShaderStages.TessellationControl:
                    DeviceShader = new HullShader(device, Bytecode);
                    break;
                case ShaderStages.TessellationEvaluation:
                    DeviceShader = new DomainShader(device, Bytecode);
                    break;
                case ShaderStages.Fragment:
                    DeviceShader = new PixelShader(device, Bytecode);
                    break;
                case ShaderStages.Compute:
                    DeviceShader = new ComputeShader(device, Bytecode);
                    break;
                default:
                    throw Illegal.Value<ShaderStages>();
            }
        }

        private byte[] CompileCode(ShaderDescription description)
        {
            string profile;
            switch (description.Stage)
            {
                case ShaderStages.Vertex:
                    profile = "vs_5_0";
                    break;
                case ShaderStages.Geometry:
                    profile = "gs_5_0";
                    break;
                case ShaderStages.TessellationControl:
                    profile = "hs_5_0";
                    break;
                case ShaderStages.TessellationEvaluation:
                    profile = "ds_5_0";
                    break;
                case ShaderStages.Fragment:
                    profile = "ps_5_0";
                    break;
                case ShaderStages.Compute:
                    profile = "cs_5_0";
                    break;
                default:
                    throw Illegal.Value<ShaderStages>();
            }

            ShaderFlags flags = description.Debug ? ShaderFlags.Debug : ShaderFlags.OptimizationLevel3;
            CompilationResult result = ShaderBytecode.Compile(
                description.ShaderBytes,
                description.EntryPoint,
                profile,
                flags);

            if (result.ResultCode.Failure)
            {
                throw new VeldridException($"Failed to compile HLSL code: {result.Message}");
            }

            return result.Bytecode.Data;
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
