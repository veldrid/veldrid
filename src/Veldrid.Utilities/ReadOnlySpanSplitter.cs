using System;

namespace Veldrid.Utilities
{
    internal ref struct ReadOnlySpanSplitter<T>
        where T : IEquatable<T>
    {
        private readonly ReadOnlySpan<T> _span;
        private readonly ReadOnlySpan<T> _separators;
        private readonly int _separatorLength;
        private readonly bool _initialized;
        private readonly StringSplitOptions _splitOptions;

        private int _startCurrent;
        private int _endCurrent;
        private int _startNext;

        public ReadOnlySpan<T> Current => _span.Slice(_startCurrent, _endCurrent - _startCurrent);

        public ReadOnlySpanSplitter(ReadOnlySpan<T> span, ReadOnlySpan<T> separators, StringSplitOptions splitOptions)
        {
            _initialized = true;
            _span = span;
            _separators = separators;
            _separatorLength = _separators.Length != 0 ? _separators.Length : 1;
            _splitOptions = splitOptions;

            _startCurrent = 0;
            _endCurrent = 0;
            _startNext = 0;
        }

        public ReadOnlySpanSplitter<T> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            TrySlice:
            if (!_initialized || _startNext > _span.Length)
            {
                return false;
            }

            ReadOnlySpan<T> slice = _span.Slice(_startNext);
            _startCurrent = _startNext;

            int separatorIndex = slice.IndexOfAny(_separators);
            int elementLength = separatorIndex != -1 ? separatorIndex : slice.Length;

            _endCurrent = _startCurrent + elementLength;
            _startNext = _endCurrent + _separatorLength;

            if ((_splitOptions & StringSplitOptions.RemoveEmptyEntries) != 0 &&
                _endCurrent - _startCurrent == 0)
            {
                goto TrySlice;
            }
            return true;
        }
    }
}
