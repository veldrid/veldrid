using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetPrimitives
{
    public static class BinaryExtensions
    {
        public static unsafe T ReadEnum<T>(this BinaryReader reader)
        {
            int i32 = reader.ReadInt32();
            return Unsafe.Read<T>(&i32);
        }

        public static void WriteEnum<T>(this BinaryWriter writer, T value)
        {
            int i32 = Convert.ToInt32(value);
            writer.Write(i32);
        }

        public static byte[] ReadByteArray(this BinaryReader reader)
        {
            int byteCount = reader.ReadInt32();
            return reader.ReadBytes(byteCount);
        }

        public static void WriteByteArray(this BinaryWriter writer, byte[] array)
        {
            writer.Write(array.Length);
            writer.Write(array);
        }

        public static void WriteObjectArray<T>(this BinaryWriter writer, T[] array, Action<BinaryWriter, T> writeFunc)
        {
            writer.Write(array.Length);
            foreach (T item in array)
            {
                writeFunc(writer, item);
            }
        }

        public static T[] ReadObjectArray<T>(this BinaryReader reader, Func<BinaryReader, T> readFunc)
        {
            int length = reader.ReadInt32();
            T[] ret = new T[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = readFunc(reader);
            }

            return ret;
        }

        public static unsafe void WriteBlittableArray<T>(this BinaryWriter writer, T[] array)
        {
            int sizeofT = Unsafe.SizeOf<T>();
            int totalBytes = array.Length * sizeofT;

            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            byte* ptr = (byte*)handle.AddrOfPinnedObject();

            writer.Write(array.Length);
            for (int i = 0; i < totalBytes; i++)
            {
                writer.Write(ptr[i]);
            }

            handle.Free();
        }

        public static unsafe T[] ReadBlittableArray<T>(this BinaryReader reader)
        {
            int sizeofT = Unsafe.SizeOf<T>();
            int length = reader.ReadInt32();
            T[] ret = new T[length];
            GCHandle handle = GCHandle.Alloc(ret, GCHandleType.Pinned);

            int totalBytes = length * sizeofT;
            byte* ptr = (byte*)handle.AddrOfPinnedObject();
            for (int i = 0; i < totalBytes; i++)
            {
                ptr[i] = reader.ReadByte();
            }

            handle.Free();

            return ret;
        }
    }
}
