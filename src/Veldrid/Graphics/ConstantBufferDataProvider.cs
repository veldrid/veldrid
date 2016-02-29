using System;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics
{
    public interface ConstantBufferDataProvider
    {
        int DataSizeInBytes { get; }
        void SetData(ConstantBuffer buffer);
    }

    public class DynamicDataProvider<T> : ConstantBufferDataProvider where T : struct
    {
        private readonly int _dataSizeInBytes;

        public T Data { get; set; }

        public int DataSizeInBytes => _dataSizeInBytes;

        public DynamicDataProvider(T data)
        {
            Data = data;
            _dataSizeInBytes = Marshal.SizeOf<T>();
        }

        public DynamicDataProvider()
        {
            _dataSizeInBytes = Marshal.SizeOf<T>();
        }

        public void SetData(ConstantBuffer buffer)
        {
            buffer.SetData(Data, _dataSizeInBytes);
        }
    }

    public class ConstantDataProvider<T> : ConstantBufferDataProvider where T : struct
    {
        private readonly T _data;
        private readonly int _dataSizeInBytes;

        public ConstantDataProvider(T data)
        {
            _data = data;
            _dataSizeInBytes = Marshal.SizeOf<T>();
        }

        public int DataSizeInBytes => _dataSizeInBytes;

        public void SetData(ConstantBuffer buffer)
        {
            buffer.SetData(_data, _dataSizeInBytes);
        }
    }
}