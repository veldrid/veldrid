using System;
using System.Text;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Veldrid.D3D11
{
    internal sealed class D3D11Shader : Shader
    {
        private string? _name;

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

            DeviceShader = description.Stage switch
            {
                ShaderStages.Vertex => device.CreateVertexShader(Bytecode),
                ShaderStages.Geometry => device.CreateGeometryShader(Bytecode),
                ShaderStages.TessellationControl => device.CreateHullShader(Bytecode),
                ShaderStages.TessellationEvaluation => device.CreateDomainShader(Bytecode),
                ShaderStages.Fragment => device.CreatePixelShader(Bytecode),
                ShaderStages.Compute => device.CreateComputeShader(Bytecode),
                _ => throw Illegal.Value<ShaderStages>(),
            };
        }

        private byte[] CompileCode(ShaderDescription description)
        {
            string profile = description.Stage switch
            {
                ShaderStages.Vertex => "vs_5_0",
                ShaderStages.Geometry => "gs_5_0",
                ShaderStages.TessellationControl => "hs_5_0",
                ShaderStages.TessellationEvaluation => "ds_5_0",
                ShaderStages.Fragment => "ps_5_0",
                ShaderStages.Compute => "cs_5_0",
                _ => throw Illegal.Value<ShaderStages>(),
            };

            ShaderFlags flags = description.Debug ? ShaderFlags.Debug : ShaderFlags.OptimizationLevel3;
            Compiler.Compile(
                description.ShaderBytes,
                null!,
                null!,
                description.EntryPoint,
                null!,
                profile,
                flags,
                out Blob result,
                out Blob error);

            if (result == null)
            {
                throw new VeldridException($"Failed to compile HLSL code: {Encoding.ASCII.GetString(error.AsBytes())}");
            }

            return result.AsBytes();
        }

        public override string? Name
        {
            get => _name;
            set
            {
                _name = value;
                DeviceShader.DebugName = value!;
            }
        }

        public override bool IsDisposed => DeviceShader.NativePointer == IntPtr.Zero;

        public override void Dispose()
        {
            DeviceShader.Dispose();
        }
    }
}
