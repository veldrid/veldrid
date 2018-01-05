using System.Runtime.InteropServices;

namespace Veldrid.MetalBindings
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MTLCommandBufferHandler(MTLCommandBuffer buffer);
}