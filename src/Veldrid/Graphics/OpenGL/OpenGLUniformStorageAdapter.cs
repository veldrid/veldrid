using OpenTK.Graphics.OpenGL;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLUniformStorageAdapter
    {
        private readonly int _programID;
        private readonly int _uniformLocation;
        private readonly UniformSetter _setterFunction;
        private readonly ActiveUniformType _uniformType;

        public OpenGLUniformStorageAdapter(int programID, int uniformLocation)
        {
            _programID = programID;
            _uniformLocation = uniformLocation;

            int typeVal;
            GL.GetActiveUniforms(_programID, 1, ref uniformLocation, ActiveUniformParameter.UniformType, out typeVal);
            _uniformType = (ActiveUniformType)typeVal;
            _setterFunction = GetSetterFunction(_uniformType);
        }

        public void SetData(IntPtr data, int dataSizeInBytes)
            => SetData(data, dataSizeInBytes, 0);
        public unsafe void SetData(IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes)
        {
            if (dataSizeInBytes % sizeof(float) != 0)
            {
                throw new VeldridException($"{nameof(dataSizeInBytes)} must be a multiple of 4 bytes");
            }
            if (destinationOffsetInBytes % sizeof(float) != 0)
            {
                throw new VeldridException($"{nameof(destinationOffsetInBytes)} must be a multiple of 4 bytes");
            }

            _setterFunction(_uniformLocation, data, dataSizeInBytes, destinationOffsetInBytes);
        }

        public void SetData<T>(ref T data, int dataSizeInBytes) where T : struct
            => SetData(ref data, dataSizeInBytes, 0);
        public unsafe void SetData<T>(ref T data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            IntPtr dataPtr = new IntPtr(Unsafe.AsPointer(ref data));
            SetData(dataPtr, dataSizeInBytes, destinationOffsetInBytes);
        }

        public void SetData<T>(T[] data, int dataSizeInBytes) where T : struct
            => SetData(data, dataSizeInBytes, 0);
        public void SetData<T>(T[] data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            SetArrayDataCore(data, 0, dataSizeInBytes, destinationOffsetInBytes);
        }

        public void SetData<T>(ArraySegment<T> data, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            SetArrayDataCore(data.Array, data.Offset, dataSizeInBytes, destinationOffsetInBytes);
        }

        private unsafe void SetArrayDataCore<T>(T[] data, int startIndex, int dataSizeInBytes, int destinationOffsetInBytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr sourceAddress = new IntPtr((byte*)handle.AddrOfPinnedObject().ToPointer() + (startIndex * Unsafe.SizeOf<T>()));
            SetData(sourceAddress, dataSizeInBytes, destinationOffsetInBytes);
            handle.Free();
        }

        private delegate void UniformSetter(int uniformLocation, IntPtr data, int dataSizeInBytes, int destinationOffsetInBytes);

        private unsafe UniformSetter GetSetterFunction(ActiveUniformType uniformType)
        {
            switch (uniformType)
            {
                case ActiveUniformType.FloatMat4:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numMatrices = dataSizeInBytes / sizeof(Matrix4x4);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numMatrices, false, dataPtr);
                    };
                case ActiveUniformType.FloatVec2:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numFloat2s = dataSizeInBytes / sizeof(Vector2);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numFloat2s, false, dataPtr);
                    };
                case ActiveUniformType.FloatVec3:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numFloat3s = dataSizeInBytes / sizeof(Vector3);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numFloat3s, false, dataPtr);
                    };
                case ActiveUniformType.FloatVec4:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numFloat4s = dataSizeInBytes / sizeof(Vector4);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numFloat4s, false, dataPtr);
                    };
                default:
                    return (int location, IntPtr data, int dataSizeInBytes, int offsetInBytes) =>
                    {
                        float* dataPtr = (float*)data.ToPointer();
                        int numMatrices = dataSizeInBytes / sizeof(Matrix4x4);
                        int offsetElements = offsetInBytes / sizeof(float);
                        dataPtr += offsetElements;
                        GL.UniformMatrix4(location, numMatrices, false, dataPtr);
                    };
            }
        }

        public IntPtr MapBuffer(int numBytes)
        {
            throw new NotSupportedException();
        }

        public void UnmapBuffer()
        {
            throw new NotSupportedException();
        }

        public void Dispose() { }
    }
}
