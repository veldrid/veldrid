using System;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A data provider with dynamically and manually controllable data.
    /// </summary>
    /// <typeparam name="T">The type of data to provide.</typeparam>
    public class DynamicDataProvider<T> : ConstantBufferDataProvider where T : struct, IEquatable<T>
    {
        private readonly int _dataSizeInBytes;
        private T _data;

        public event Action DataChanged;

        public T Data
        {
            get
            {
                return _data;
            }

            set
            {
                if (!_data.Equals(value))
                {
                    _data = value;
                    DataChanged?.Invoke();
                }
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
}
