using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A type providing data which is calculated from another DynamicDataProvider.
    /// </summary>
    /// <typeparam name="T">The type of data to provide.</typeparam>
    public class DependantDataProvider<T> : ConstantBufferDataProvider<T> where T : struct, IEquatable<T>
    {
        private readonly ConstantBufferDataProvider<T> _dataProvider;
        private readonly Func<T, T> _derivedFunc;

        public event Action DataChanged;

        public DependantDataProvider(ConstantBufferDataProvider<T> dataProvider, Func<T, T> derivedFunc)
        {
            _dataProvider = dataProvider;
            DataSizeInBytes = dataProvider.DataSizeInBytes;
            _derivedFunc = derivedFunc;
            dataProvider.DataChanged += OnParentDataChanged;
        }

        private void OnParentDataChanged()
        {
            DataChanged?.Invoke();
        }

        public T Data { get { return _derivedFunc(_dataProvider.Data); } }

        public int DataSizeInBytes { get; }

        public void SetData(ConstantBuffer buffer)
        {
            T value = _derivedFunc(_dataProvider.Data);
            buffer.SetData(ref value, DataSizeInBytes);
        }
    }
}
