using System;

namespace Veldrid.Graphics
{
    // TODO: Try to remove this.
    public class BufferProviderPair : IDisposable
    {
        public readonly ConstantBuffer ConstantBuffer;
        public readonly ConstantBufferDataProvider DataProvider;

        private bool _dirty;

        public BufferProviderPair(ConstantBuffer buffer, ConstantBufferDataProvider provider)
        {
            ConstantBuffer = buffer;
            DataProvider = provider;
            provider.DataChanged += OnDataChanged;
            _dirty = true;
            UpdateData();
        }

        private void OnDataChanged()
        {
            _dirty = true;
        }

        public void UpdateData()
        {
            if (_dirty)
            {
                DataProvider.SetData(ConstantBuffer);
                _dirty = false;
            }
        }

        public void Dispose()
        {
            DataProvider.DataChanged -= OnDataChanged;
        }
    }
}
