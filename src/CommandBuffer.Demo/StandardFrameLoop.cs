namespace Veldrid
{
    public class StandardFrameLoop
    {
        private readonly GraphicsDevice _gd;
        private readonly Swapchain _sc;

        private uint _maxFramesInFlight;
        private FrameSet _currentSet;
        private FrameSet? _oldSet;
        private uint _oldSetExpiration = uint.MaxValue;

        private struct FrameSet
        {
            public Fence[] _fences;
            public Semaphore[] _imageAcquiredSems;
            public Semaphore[] _renderCompleteSems;
            public CommandBuffer[] _cbs;
        }

        public uint FrameIndex { get; private set; }

        public StandardFrameLoop(GraphicsDevice gd, Swapchain sc)
        {
            _gd = gd;
            _sc = sc;
            _maxFramesInFlight = sc.BufferCount;

            _currentSet = new FrameSet();

            _currentSet._fences = new Fence[_maxFramesInFlight];
            _currentSet._imageAcquiredSems = new Semaphore[_maxFramesInFlight];
            _currentSet._renderCompleteSems = new Semaphore[_maxFramesInFlight];
            _currentSet._cbs = new CommandBuffer[_maxFramesInFlight];
            for (uint i = 0; i < _maxFramesInFlight; i++)
            {
                _currentSet._fences[i] = gd.ResourceFactory.CreateFence(signaled: true);
                _currentSet._imageAcquiredSems[i] = gd.ResourceFactory.CreateSemaphore();
                _currentSet._renderCompleteSems[i] = gd.ResourceFactory.CreateSemaphore();
                _currentSet._cbs[i] = gd.ResourceFactory.CreateCommandBuffer();
            }

            FrameIndex = gd.AcquireNextImage(sc, _currentSet._imageAcquiredSems[0], null);
        }

        public void RunFrame(StandardFrameLoopHandler handler)
        {
            _currentSet._fences[FrameIndex].Wait();
            _currentSet._fences[FrameIndex].Reset();
            handler(_currentSet._cbs[FrameIndex], FrameIndex, _sc.Framebuffers[FrameIndex]);
            _gd.SubmitCommands(
                _currentSet._cbs[FrameIndex],
                _currentSet._imageAcquiredSems[FrameIndex],
                _currentSet._renderCompleteSems[FrameIndex],
                _currentSet._fences[FrameIndex]);
            _gd.Present(_sc, _currentSet._renderCompleteSems[FrameIndex], FrameIndex);
            uint nextFrame = (FrameIndex + 1) % _maxFramesInFlight;
            FrameIndex = _gd.AcquireNextImage(_sc, _currentSet._imageAcquiredSems[nextFrame], null);

            if (FrameIndex == _oldSetExpiration)
            {
                DestroySet(_oldSet.Value);
                _oldSetExpiration = uint.MaxValue;
            }
        }

        private void DestroySet(FrameSet set)
        {
            for (uint i = 0; i < _maxFramesInFlight; i++)
            {
                set._fences[i].Dispose();
                set._cbs[i].Dispose();
                set._imageAcquiredSems[i].Dispose();
                set._renderCompleteSems[i].Dispose();
            }
        }

        public void ResizeSwapchain(uint width, uint height) 
        {
            if (_oldSet == null)
            {
                // DestroySet(_currentSet);
                _oldSet = _currentSet;
                _oldSetExpiration = _maxFramesInFlight - 1;
            }
            else
            {
                // TODO: Keep a track of all old sets and track/destroy them in a loop above.
                _gd.WaitForIdle();
                DestroySet(_currentSet);
            }

            _currentSet = new FrameSet();
            _currentSet._fences = new Fence[_maxFramesInFlight];
            _currentSet._imageAcquiredSems = new Semaphore[_maxFramesInFlight];
            _currentSet._renderCompleteSems = new Semaphore[_maxFramesInFlight];
            _currentSet._cbs = new CommandBuffer[_maxFramesInFlight];
            for (uint i = 0; i < _maxFramesInFlight; i++)
            {
                _currentSet._fences[i] = _gd.ResourceFactory.CreateFence(signaled: true);
                _currentSet._imageAcquiredSems[i] = _gd.ResourceFactory.CreateSemaphore();
                _currentSet._renderCompleteSems[i] = _gd.ResourceFactory.CreateSemaphore();
                _currentSet._cbs[i] = _gd.ResourceFactory.CreateCommandBuffer();
            }

            _sc.Resize(width, height);

            FrameIndex = _gd.AcquireNextImage(_sc, _currentSet._imageAcquiredSems[0], null);
        }
    }

    public delegate void StandardFrameLoopHandler(CommandBuffer cb, uint frameIndex, Framebuffer fb);
}
