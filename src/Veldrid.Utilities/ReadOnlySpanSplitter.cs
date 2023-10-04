using System;

namespace Veldrid.Utilities
{
    internal ref struct ReadOnlySpanSplitter<T>
        where T : IEquatable<T>
    {
        private int _offset;
        private bool _isLastSeparator;

        public ReadOnlySpan<T> Value { get; }
        public ReadOnlySpan<T> Separator { get; }
        public StringSplitOptions SplitOptions { get; }
        public int? MaxCount { get; }

        public ReadOnlySpan<T> Current { get; private set; }

        public ReadOnlySpanSplitter(
            ReadOnlySpan<T> value,
            ReadOnlySpan<T> separator,
            StringSplitOptions splitOptions,
            int? maxCount = null)
            : this()
        {
            if (separator.IsEmpty)
                splitOptions &= ~StringSplitOptions.TrimEntries;

            Value = value;
            Separator = separator;
            SplitOptions = splitOptions;
            MaxCount = maxCount;
        }

        public bool MoveNext()
        {
            if (_isLastSeparator)
            {
                _isLastSeparator = false;
                Current = ReadOnlySpan<T>.Empty;
                return true;
            }

            ReadOnlySpan<T> value = Value;
            ReadOnlySpan<T> separator = Separator;
            int start = _offset;

            for (; _offset < value.Length; _offset++)
            {
                if (separator.IsEmpty)
                {
                    Current = value[start.._offset++];
                    return true;
                }

                if (value[_offset].Equals(separator[0]) &&
                    separator.Length <= value.Length - _offset)
                {
                    if (separator.Length == 1 ||
                        value.Slice(_offset, separator.Length).SequenceEqual(separator))
                    {
                        Current = value[start.._offset];
                        _offset += separator.Length;

                        if ((SplitOptions & StringSplitOptions.RemoveEmptyEntries) != 0)
                        {
                            if (_offset - start == 1)
                            {
                                start = _offset;
                                continue;
                            }
                        }
                        else
                        {
                            if (_offset == value.Length)
                                _isLastSeparator = true;
                        }
                        return true;
                    }
                }
            }

            if (start != _offset)
            {
                Current = value[start.._offset];
                return Current.Length > 0;
            }

            Current = default;
            return false;
        }

        public ReadOnlySpanSplitter<T> GetEnumerator()
        {
            return this;
        }
    }
}
