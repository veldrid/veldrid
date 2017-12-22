using System.Threading;

namespace Veldrid.OpenGL
{
    internal class OpenGLFence : Fence
    {
        private readonly ManualResetEvent _mre;

        public OpenGLFence(bool signaled)
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
            return _mre.WaitOne((int)(nanosecondTimeout / 1_000_000));
        }
    }
}