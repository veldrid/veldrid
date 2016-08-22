using System;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A type which only provides a pre-set data value.
    /// </summary>
    /// <typeparam name="T">The type of data to provide.</typeparam>
    public class ConstantDataProvider<T> : ConstantBufferDataProvider<T> where T : struct
    {
        private T _data;
        private readonly int _dataSizeInBytes;

#pragma warning disable 0067 // Never called; data is constant.
        public event Action DataChanged;
#pragma warning restore

        /// <summary>
        /// Returns the data provided by this ConstantDataProvider.
        /// </summary>
        public T Data => _data;

        /// <summary>
        /// Constructs a new ConstantDataProvider which will provide the given data value.
        /// </summary>
        /// <param name="data"></param>
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
