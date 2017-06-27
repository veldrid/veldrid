using System;

namespace Veldrid.Graphics
{
    public class ContextDeviceBinding<T>
    {
        private T _value;
        private bool _valid;

        public T Value
        {
            get
            {
                if (!_valid)
                {
                    throw new InvalidOperationException($"No value has been bound to context binding of type {typeof(T).FullName}");
                }

                return _value;
            }
            set
            {
                _value = value;
                _valid = true;
            }
        }

        public ContextDeviceBinding(T value)
        {
            _value = value;
            _valid = true;
        }

        public ContextDeviceBinding()
        {
        }
    }
}
