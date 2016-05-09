using System;
using System.Runtime.InteropServices;

namespace Veldrid.RenderDemo
{
    public class TextInputBuffer : IDisposable
    {
        public readonly IntPtr Data;
        public readonly uint ByteCount;

        public TextInputBuffer(uint byteCount)
        {
            ByteCount = byteCount;
            Data = Marshal.AllocHGlobal((int)byteCount);
            SharpDX.Utilities.ClearMemory(Data, 0, (int)ByteCount);
        }

        public string StringValue
        {
            get
            {
                string ret = Marshal.PtrToStringAnsi(Data);
                return ret;
            }
            set
            {
                IntPtr copy = Marshal.StringToHGlobalAnsi(value);
                uint bytesToCopy = Math.Min(ByteCount, (uint)value.Length);
                SharpDX.Utilities.CopyMemory(Data, copy, (int)bytesToCopy);
                Marshal.FreeHGlobal(copy);
            }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Data);
        }
    }
}
