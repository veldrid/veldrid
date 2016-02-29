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
        private T _data;

        public T Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }
        }

        public DynamicDataProvider(T data)
        {
            Data = data;
            _dataSizeInBytes = Marshal.SizeOf<T>();
        }

        public DynamicDataProvider()
        {
            _dataSizeInBytes = Marshal.SizeOf<T>();
        }

        public int DataSizeInBytes => _dataSizeInBytes;

        public void SetData(ConstantBuffer buffer)
        {
            buffer.SetData(ref _data, _dataSizeInBytes);
        }
    }

    public class ConstantDataProvider<T> : ConstantBufferDataProvider where T : struct
    {
        private T _data;
        private readonly int _dataSizeInBytes;

        public ConstantDataProvider(T data)
        {
            _data = data;
            _dataSizeInBytes = Marshal.SizeOf<T>();
        }

        public int DataSizeInBytes => _dataSizeInBytes;

        public void SetData(ConstantBuffer buffer)
        {
            buffer.SetData(ref _data, _dataSizeInBytes);
        }
    }
}