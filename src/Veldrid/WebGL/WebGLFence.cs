namespace Veldrid.WebGL
{
    internal class WebGLFence : Fence
    {
        private bool _signaled;

        public WebGLFence(WebGLGraphicsDevice gd, bool signaled)
            : base(gd)
        {
            _signaled = signaled;
        }

        public override bool Signaled => _signaled;

        public override string Name { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override void Dispose()
        {
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
