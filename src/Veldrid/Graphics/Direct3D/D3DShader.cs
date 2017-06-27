using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public abstract class D3DShader<TShader> : Shader where TShader : IDisposable
    {
        private const ShaderFlags DefaultShaderFlags
#if DEBUG
            = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
            = ShaderFlags.OptimizationLevel3;
#endif

        private ShaderReflection _reflection;

        public ShaderType Type { get; }
        public ShaderBytecode Bytecode { get; }
        public TShader DeviceShader { get; }
        public ShaderReflection Reflection => _reflection ?? (_reflection = new ShaderReflection(Bytecode.Data));

        public D3DShader(Device device, ShaderType type, string shaderCode, string name)
        {
            Type = type;
            CompilationResult compilation = ShaderBytecode.Compile(shaderCode, GetEntryPoint(type), GetProfile(type), DefaultShaderFlags, sourceFileName: name);
            if (compilation.HasErrors || compilation.Message != null)
            {
                throw new InvalidOperationException("Error compiling shader: " + compilation.Message);
            }

            Bytecode = compilation.Bytecode;
            DeviceShader = CreateDeviceShader(device, compilation.Bytecode);
        }

        private string GetEntryPoint(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.Vertex:
                    return "VS";
                case ShaderType.Geometry:
                    return "GS";
                case ShaderType.Fragment:
                    return "PS";
                default:
                    throw Illegal.Value<ShaderType>();
            }
        }

        private string GetProfile(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.Vertex:
                    return "vs_5_0";
                case ShaderType.Geometry:
                    return "gs_5_0";
                case ShaderType.Fragment:
                    return "ps_5_0";
                default:
                    throw Illegal.Value<ShaderType>();
            }
        }

        protected abstract TShader CreateDeviceShader(Device device, ShaderBytecode bytecode);

        public void Dispose()
        {
            DeviceShader.Dispose();
        }

    }

    public class D3DVertexShader : D3DShader<VertexShader>
    {
        public D3DVertexShader(Device device, string shaderCode, string name)
            : base(device, ShaderType.Vertex, shaderCode, name) { }

        protected override VertexShader CreateDeviceShader(Device device, ShaderBytecode bytecode)
        {
            return new VertexShader(device, bytecode);
        }
    }

    public class D3DGeometryShader : D3DShader<GeometryShader>
    {
        public D3DGeometryShader(Device device, string shaderCode, string name)
            : base(device, ShaderType.Geometry, shaderCode, name) { }

        protected override GeometryShader CreateDeviceShader(Device device, ShaderBytecode bytecode)
        {
            return new GeometryShader(device, bytecode);
        }
    }

    public class D3DFragmentShader : D3DShader<PixelShader>
    {
        public D3DFragmentShader(Device device, string shaderCode, string name)
            : base(device, ShaderType.Fragment, shaderCode, name) { }

        protected override PixelShader CreateDeviceShader(Device device, ShaderBytecode bytecode)
        {
            return new PixelShader(device, bytecode);
        }
    }
}
