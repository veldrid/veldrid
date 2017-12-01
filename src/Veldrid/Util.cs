using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Veldrid.D3D11;

namespace Veldrid
{
    internal static class Util
    {
        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TDerived AssertSubtype<TBase, TDerived>(TBase value) where TDerived : class, TBase where TBase : class
        {
#if DEBUG
            if (value == null)
            {
                throw new VeldridException($"Expected object of type {typeof(TDerived).FullName} but received null instead.");
            }

            if (!(value is TDerived derived))
            {
                throw new VeldridException($"object {value} must be derived type {typeof(TDerived).FullName} to be used in this context.");
            }

            return derived;

#else
            return (TDerived)value;
#endif
        }

        internal static void EnsureArrayMinimumSize<T>(ref T[] array, uint size)
        {
            if (array == null)
            {
                array = new T[size];
            }
            else if (array.Length < size)
            {
                Array.Resize(ref array, (int)size);
            }
        }

        internal static uint USizeOf<T>() where T : struct
        {
            return (uint)Unsafe.SizeOf<T>();
        }

        internal static unsafe string GetString(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(stringStart, characters);
        }

        internal static bool ArrayEquals<T>(T[] left, T[] right) where T : class
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (!ReferenceEquals(left[i], right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool ArrayEqualsEquatable<T>(T[] left, T[] right) where T : struct, IEquatable<T>
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (!left[i].Equals(right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        internal static void ClearArray<T>(T[] array)
        {
            Array.Clear(array, 0, array.Length);
        }

        public static uint Clamp(uint value, uint min, uint max)
        {
            if (value <= min)
            {
                return min;
            }
            else if (value >= max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        // TODO: Move entirely into Texture.ComputeSubresource
        internal static uint GetSubresourceIndex(Texture tex, uint mipLevel, uint arrayLayer)
        {
            return arrayLayer * tex.MipLevels + mipLevel;
        }

        internal static void GetMipLevelAndArrayLayer(Texture tex, uint subresource, out uint mipLevel, out uint arrayLayer)
        {
            arrayLayer = subresource / tex.MipLevels;
            mipLevel = subresource - (arrayLayer * tex.MipLevels);
        }

        internal static void GetMipDimensions(Texture tex, uint mipLevel, out uint width, out uint height, out uint depth)
        {
            width = GetDimension(tex.Width, mipLevel);
            height = GetDimension(tex.Height, mipLevel);
            depth = GetDimension(tex.Depth, mipLevel);
        }

        internal static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            uint ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++)
            {
                ret /= 2;
            }

            return Math.Max(1, ret);
        }
    }
}
