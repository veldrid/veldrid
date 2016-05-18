using System;
using System.Runtime.InteropServices;

namespace Veldrid.RenderDemo
{
    public class TextInputBuffer : IDisposable
    {
        public readonly IntPtr Data;
        public readonly uint CapacityInBytes;

        public int Length { get; private set; }

        public TextInputBuffer(uint byteCount)
        {
            CapacityInBytes = byteCount;
            Data = Marshal.AllocHGlobal((int)byteCount);
            SharpDX.Utilities.ClearMemory(Data, 0, (int)CapacityInBytes);
            Length = 0;
        }

        public unsafe string StringValue
        {
            get
            {
                string ret = Marshal.PtrToStringAnsi(Data);
                return ret;
            }
            set
            {
                IntPtr copy = Marshal.StringToHGlobalAnsi(value);
                uint bytesToCopy = Math.Min(CapacityInBytes, (uint)value.Length);
                SharpDX.Utilities.CopyMemory(Data, copy, (int)bytesToCopy);
                Length = (int)bytesToCopy;
                SharpDX.Utilities.ClearMemory(new IntPtr((byte*)(Data.ToPointer()) + Length), 0, (int)(CapacityInBytes - Length));
                Marshal.FreeHGlobal(copy);
            }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Data);
        }
    }
}
