using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vd2
{
    internal static class Util
    {
        [DebuggerNonUserCode]
        internal static TDerived AssertSubtype<TBase, TDerived>(TBase value) where TDerived : TBase
        {
            if (!(value is TDerived derived))
            {
                throw new VdException($"object {value} must be derived type {typeof(TDerived).FullName} to be used in this context.");
            }

            return derived;
        }

        internal static void EnsureArraySize<T>(ref T[] array, uint size)
        {
            if (array.Length < size)
            {
                Array.Resize(ref array, (int)size);
            }
        }

        internal static uint USizeOf<T>() where T : struct
        {
            return (uint)Unsafe.SizeOf<T>();
        }
    }
}
