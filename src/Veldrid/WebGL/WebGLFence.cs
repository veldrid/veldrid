namespace Veldrid.WebGL
{
    internal class WebGLFence : Fence
    {
        private bool _signaled;
        private bool _disposed;

        public WebGLFence(WebGLGraphicsDevice gd, bool signaled)
            : base(gd)
        {
            _signaled = signaled;
        }

        public override bool Signaled => _signaled;

        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            _disposed = true;
        }

        public override void Reset()
        {
            _signaled = false;
        }

        public void Set()
        {
            _signaled = true;
        }
    }
}
