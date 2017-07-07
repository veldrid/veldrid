using SharpDX.D3DCompiler;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DShaderBytecode : CompiledShaderCode
    {
        public ShaderBytecode Bytecode { get; }

        public D3DShaderBytecode(string shaderCode, string entryPoint, string profile, ShaderFlags flags)
        {
            CompilationResult compilationResult = ShaderBytecode.Compile(shaderCode, entryPoint, profile, flags);
            if (compilationResult.HasErrors)
            {
                throw new InvalidOperationException($"Error compiling shader code: {compilationResult.Message}");
            }

            Bytecode = compilationResult.Bytecode;
        }

        public D3DShaderBytecode(byte[] bytes)
        {
            Bytecode = new ShaderBytecode(bytes);
        }
    }
}
