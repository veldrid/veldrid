using System.Runtime.InteropServices;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A data provider with dynamically and manually controllable data.
    /// </summary>
    /// <typeparam name="T">The type of data to provide.</typeparam>
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
}
