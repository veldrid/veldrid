using System;
using System.Collections.Generic;

namespace Veldrid.Utilities
{
    public class DisposeCollector : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private bool disposed = false;

        public void Add(params IDisposable[] disposable)
        {
            for(int i = 0;i<disposable.Length;i++)
            _disposables.Add(disposable[i]);
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
        public void Dispose(){
            if(disposed) return;
            DisposeAll();
            disposed = true;
        }
        ~DisposeCollector(){
            Dispose();
        }
    }
}
