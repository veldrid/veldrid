using System;
using System.Threading;

namespace Veldrid.D3D11
{
    internal class D3D11Fence : Fence
    {
        private readonly ManualResetEvent _mre;

        public D3D11Fence(bool signaled)
        {
            _mre = new ManualResetEvent(signaled);
        }

        public override string Name { get; set; }
        public ManualResetEvent ResetEvent => _mre;

        public void Set() => _mre.Set();
        public override void Reset() => _mre.Reset();
        public override bool Signaled => _mre.WaitOne(0);

        public override void Dispose()
        {
            _mre.Dispose();
        }

        internal bool Wait(ulong nanosecondTimeout)
        {
            ulong timeout = Math.Min(int.MaxValue, nanosecondTimeout / 1_000_000);
            return _mre.WaitOne((int)timeout);
        }
    }
}