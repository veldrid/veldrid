using System;
using System.Threading;

namespace Veldrid.OpenGL
{
    internal class OpenGLFence : Fence
    {
        private readonly ManualResetEvent _mre;
        private bool _disposed;

        public OpenGLFence(bool signaled)
        {
            _mre = new ManualResetEvent(signaled);
        }

        public override string Name { get; set; }
        public ManualResetEvent ResetEvent => _mre;

        public void Set() => _mre.Set();
        public override void Reset() => _mre.Reset();
        public override bool Signaled => _mre.WaitOne(0);
        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            if (!_disposed)
            {
                _mre.Dispose();
                _disposed = true;
            }
        }

        internal bool Wait(ulong nanosecondTimeout)
        {
            ulong timeout = Math.Min(int.MaxValue, nanosecondTimeout / 1_000_000);
            return _mre.WaitOne((int)timeout);
        }
    }
}
