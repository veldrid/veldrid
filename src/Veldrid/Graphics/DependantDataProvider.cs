using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A type providing data which is calculated from another DynamicDataProvider.
    /// </summary>
    /// <typeparam name="T">The type of data to provide.</typeparam>
    public class DependantDataProvider<T> : ConstantBufferDataProvider where T : struct
    {
        private readonly DynamicDataProvider<T> _dataProvider;
        private readonly Func<T, T> _derivedFunc;

        public DependantDataProvider(DynamicDataProvider<T> dataProvider, Func<T, T> derivedFunc)
        {
            _dataProvider = dataProvider;
            DataSizeInBytes = dataProvider.DataSizeInBytes;
            _derivedFunc = derivedFunc;
        }

        public int DataSizeInBytes { get; }

        public void SetData(ConstantBuffer buffer)
        {
            T value = _derivedFunc(_dataProvider.Data);
            buffer.SetData(ref value, DataSizeInBytes);
        }
    }
}
