using System;
using System.Collections.Generic;

namespace Veldrid.Utilities
{
    public class DisposeCollector
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void Add(IDisposable first, IDisposable second)
        {
            _disposables.Add(first);
            _disposables.Add(second);
        }

        public void Add(IDisposable first, IDisposable second, IDisposable third)
        {
            _disposables.Add(first);
            _disposables.Add(second);
            _disposables.Add(third);
        }

        public void Add(IDisposable first, IDisposable second, IDisposable third, IDisposable fourth)
        {
            _disposables.Add(first);
            _disposables.Add(second);
            _disposables.Add(third);
            _disposables.Add(fourth);
        }

        public void Add(
            IDisposable first,
            IDisposable second,
            IDisposable third,
            IDisposable fourth,
            IDisposable fifth)
        {
            _disposables.Add(first);
            _disposables.Add(second);
            _disposables.Add(third);
            _disposables.Add(fourth);
            _disposables.Add(fifth);
        }


        public void Add(
            IDisposable first,
            IDisposable second,
            IDisposable third,
            IDisposable fourth,
            IDisposable fifth,
            IDisposable sixth)
        {
            _disposables.Add(first);
            _disposables.Add(second);
            _disposables.Add(third);
            _disposables.Add(fourth);
            _disposables.Add(fifth);
            _disposables.Add(sixth);
        }


        public void Add(
            IDisposable first,
            IDisposable second,
            IDisposable third,
            IDisposable fourth,
            IDisposable fifth,
            IDisposable sixth,
            IDisposable seventh)
        {
            _disposables.Add(first);
            _disposables.Add(second);
            _disposables.Add(third);
            _disposables.Add(fourth);
            _disposables.Add(fifth);
            _disposables.Add(sixth);
            _disposables.Add(seventh);
        }


        public void Add(
            IDisposable first,
            IDisposable second,
            IDisposable third,
            IDisposable fourth,
            IDisposable fifth,
            IDisposable sixth,
            IDisposable seventh,
            IDisposable eighth)
        {
            _disposables.Add(first);
            _disposables.Add(second);
            _disposables.Add(third);
            _disposables.Add(fourth);
            _disposables.Add(fifth);
            _disposables.Add(sixth);
            _disposables.Add(seventh);
            _disposables.Add(eighth);
        }

        public void Add(
            IDisposable first,
            IDisposable second,
            IDisposable third,
            IDisposable fourth,
            IDisposable fifth,
            IDisposable sixth,
            IDisposable seventh,
            IDisposable eighth,
            IDisposable ninth)
        {
            _disposables.Add(first);
            _disposables.Add(second);
            _disposables.Add(third);
            _disposables.Add(fourth);
            _disposables.Add(fifth);
            _disposables.Add(sixth);
            _disposables.Add(seventh);
            _disposables.Add(eighth);
            _disposables.Add(ninth);
        }

        public void Add(
            IDisposable first,
            IDisposable second,
            IDisposable third,
            IDisposable fourth,
            IDisposable fifth,
            IDisposable sixth,
            IDisposable seventh,
            IDisposable eighth,
            IDisposable ninth,
            IDisposable tenth)
        {
            _disposables.Add(first);
            _disposables.Add(second);
            _disposables.Add(third);
            _disposables.Add(fourth);
            _disposables.Add(fifth);
            _disposables.Add(sixth);
            _disposables.Add(seventh);
            _disposables.Add(eighth);
            _disposables.Add(ninth);
            _disposables.Add(tenth);
        }

        public void Add<T>(T[] array) where T : IDisposable
        {
            foreach (T item in array)
            {
                _disposables.Add(item);
            }
        }

        public void Remove(IDisposable disposable)
        {
            if (!_disposables.Remove(disposable))
            {
                throw new InvalidOperationException("Unable to untrack " + disposable + ". It was not previously tracked.");
            }
        }

        public void DisposeAll()
        {
            foreach (IDisposable disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }
    }
}
