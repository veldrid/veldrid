namespace Veldrid.MTL
{
    internal class MTLFence : Fence
    {
        private bool _signaled;

        public MTLFence(bool signaled)
        {
            _signaled = signaled;
        }

        public override bool Signaled => _signaled;

        internal void SetSignaled(bool value)
        {
            _signaled = value;
        }

        public override string Name
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public override void Reset()
        {
            _signaled = false;
        }

        public override void Dispose()
        {
        }
    }
}