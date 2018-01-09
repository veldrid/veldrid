using System;
using System.Runtime.InteropServices;

namespace Veldrid.MetalBindings
{
    public static unsafe class Dispatch
    {
        private const string LibdispatchLocation = @"/usr/lib/system/libdispatch.dylib";

        [DllImport(LibdispatchLocation)]
        public static extern DispatchQueue dispatch_get_global_queue(QualityOfServiceLevel identifier, ulong flags);

        [DllImport(LibdispatchLocation)]
        public static extern DispatchData dispatch_data_create(
            void* buffer,
            UIntPtr size,
            DispatchQueue queue,
            IntPtr destructorBlock);

        [DllImport(LibdispatchLocation)]
        public static extern void dispatch_release(IntPtr nativePtr);
    }

    public enum QualityOfServiceLevel : long
    {
        QOS_CLASS_USER_INTERACTIVE = 0x21,
        QOS_CLASS_USER_INITIATED = 0x19,
        QOS_CLASS_DEFAULT = 0x15,
        QOS_CLASS_UTILITY = 0x11,
        QOS_CLASS_BACKGROUND = 0x9,
        QOS_CLASS_UNSPECIFIED = 0,
    }

    public struct DispatchQueue
    {
        public readonly IntPtr NativePtr;
    }

    public struct DispatchData
    {
        public readonly IntPtr NativePtr;
    }
}