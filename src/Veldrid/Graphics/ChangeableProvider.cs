using System;

namespace Veldrid.Graphics
{
    // TODO: Remove this
    public class ChangeableProvider : ConstantBufferDataProvider
    {
        private ConstantBufferDataProvider _dataProvider;

        public ConstantBufferDataProvider DataProvider
        {
            get { return _dataProvider; }
            set
            {
                _dataProvider.DataChanged -= OnParentDataChanged;
                _dataProvider = value;
                _dataProvider.DataChanged += OnParentDataChanged;
                OnParentDataChanged();
            }
        }

        public event Action DataChanged;

        public ChangeableProvider(ConstantBufferDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            DataSizeInBytes = dataProvider.DataSizeInBytes;
            dataProvider.DataChanged += OnParentDataChanged;
        }

        private void OnParentDataChanged()
        {
            DataChanged?.Invoke();
        }

        public int DataSizeInBytes { get; }

        public void SetData(ConstantBuffer buffer)
        {
            _dataProvider.SetData(buffer);
        }
    }
}
