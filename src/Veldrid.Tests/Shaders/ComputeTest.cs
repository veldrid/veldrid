using ShaderGen;
using System.Runtime.InteropServices;

[assembly: ComputeShaderSet("BasicComputeTest", "Veldrid.Tests.Shaders.ComputeTest.CS")]

namespace Veldrid.Tests.Shaders
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ComputeTestParams
    {
        public uint Width;
        public uint Height;
        private uint _padding1;
        private uint _padding2;
    }

    public class ComputeTest
    {
        public ComputeTestParams Params;

        public RWStructuredBuffer<float> Source; // Count: Width
        public RWStructuredBuffer<float> Destination; // Count: Width

        [ComputeShader(16, 16, 1)]
        public void CS()
        {
            UInt3 id = ShaderBuiltins.DispatchThreadID;
            uint index = id.Y * Params.Width + id.X;
            Destination[index] = Source[index];
            Source[index] += Source[index];
        }
    }
}
