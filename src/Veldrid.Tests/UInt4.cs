using System.Runtime.InteropServices;

namespace Veldrid.Tests
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UInt4
    {
        public uint X, Y, Z, W;
    }
}
