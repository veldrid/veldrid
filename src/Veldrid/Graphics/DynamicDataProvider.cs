using System;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A data provider with dynamically and manually controllable data.
    /// </summary>
    /// <typeparam name="T">The type of data to provide.</typeparam>
    public class DynamicDataProvider<T> : ConstantBufferDataProvider<T> where T : struct, IEquatable<T>
    {
        private readonly int _dataSizeInBytes;
        private T _data;

        /// <summary>
        /// Provides a notification when this provider's data changes.
        /// </summary>
        public event Action DataChanged;

        /// <summary>
        /// Gets or sets the data provided by this <see cref="DynamicDataProvider{T}"/>.
        /// </summary>
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

        /// <summary>
        /// Constructs a new <see cref="DynamicDataProvider{T}"/> with the given initial data.
        /// </summary>
        /// <param name="data">The initial data to provide.</param>
        public DynamicDataProvider(T data)
        {
            Data = data;
            _dataSizeInBytes = Marshal.SizeOf<T>();
        }

        /// <summary>
        /// Constructs a new <see cref="DynamicDataProvider{T}"/> with default data.
        /// </summary>
        public DynamicDataProvider()
        {
            _dataSizeInBytes = Marshal.SizeOf<T>();
        }

        /// <summary>
        /// Gets the size of data contained in this <see cref="DynamicDataProvider{T}"/>.
        /// </summary>
        public int DataSizeInBytes => _dataSizeInBytes;

        /// <summary>
        /// Propogates data from this provider into the given GPU buffer.
        /// </summary>
        /// <param name="buffer">The GPU buffer to transfer data to.</param>
        public void SetData(ConstantBuffer buffer)
        {
            buffer.SetData(ref _data, _dataSizeInBytes);
        }
    }
}
