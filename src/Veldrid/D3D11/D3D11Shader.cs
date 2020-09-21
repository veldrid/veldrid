using System;
using System.Text;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Veldrid.D3D11
{
    internal class D3D11Shader : Shader
    {
        private string _name;

        public ID3D11DeviceChild DeviceShader { get; }
        public byte[] Bytecode { get; internal set; }

        public D3D11Shader(ID3D11Device device, ShaderDescription description)
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
                    DeviceShader = device.CreateVertexShader(Bytecode);
                    break;
                case ShaderStages.Geometry:
                    DeviceShader = device.CreateGeometryShader(Bytecode);
                    break;
                case ShaderStages.TessellationControl:
                    DeviceShader = device.CreateHullShader(Bytecode);
                    break;
                case ShaderStages.TessellationEvaluation:
                    DeviceShader = device.CreateDomainShader(Bytecode);
                    break;
                case ShaderStages.Fragment:
                    DeviceShader = device.CreatePixelShader(Bytecode);
                    break;
                case ShaderStages.Compute:
                    DeviceShader = device.CreateComputeShader(Bytecode);
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
            Compiler.Compile(description.ShaderBytes,
                             description.EntryPoint, null,
                             profile, out Blob result, out Blob error);

            if (result == null)
            {
                throw new VeldridException($"Failed to compile HLSL code: {Encoding.ASCII.GetString(error.GetBytes())}");
            }

            return result.GetBytes();
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

        public override bool IsDisposed => DeviceShader.IsDisposed;

        public override void Dispose()
        {
            DeviceShader.Dispose();
        }
    }
}
