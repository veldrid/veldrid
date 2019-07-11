using System;
using System.Diagnostics;

namespace Veldrid
{
    public class StandardFrameLoop : IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly Swapchain _sc;

        private uint _maxFramesInFlight;
        public Fence[] _fences;
        public Semaphore[] _imageAcquiredSems;
        public Semaphore[] _renderCompleteSems;
        public CommandBuffer[] _cbs;
        private uint _frameIndex;
        private bool _disposed;

        public uint FrameIndex => _frameIndex;

        public StandardFrameLoop(GraphicsDevice gd, Swapchain sc)
        {
            _gd = gd;
            _sc = sc;
            _maxFramesInFlight = sc.BufferCount;
            _frameIndex = _maxFramesInFlight - 1;

            _fences = new Fence[_maxFramesInFlight];
            _imageAcquiredSems = new Semaphore[_maxFramesInFlight];
            _renderCompleteSems = new Semaphore[_maxFramesInFlight];
            _cbs = new CommandBuffer[_maxFramesInFlight];
            for (uint i = 0; i < _maxFramesInFlight; i++)
            {
                _fences[i] = gd.ResourceFactory.CreateFence(signaled: true);
                _imageAcquiredSems[i] = gd.ResourceFactory.CreateSemaphore();
                _renderCompleteSems[i] = gd.ResourceFactory.CreateSemaphore();
                _cbs[i] = gd.ResourceFactory.CreateCommandBuffer();
            }
        }

        public void RunFrame(StandardFrameLoopHandler handler)
        {
            uint nextFrame = (_sc.LastAcquiredImage + 1) % _maxFramesInFlight;
            _fences[nextFrame].Wait();
            _fences[nextFrame].Reset();

            AcquireResult acquireResult = _gd.AcquireNextImage(
                _sc,
                _imageAcquiredSems[nextFrame],
                null,
                out _frameIndex);
            if (acquireResult == AcquireResult.OutOfDate)
            {
                _sc.Resize(_sc.Width, _sc.Height);

                nextFrame = 0;
                acquireResult = _gd.AcquireNextImage(
                    _sc,
                    _imageAcquiredSems[nextFrame],
                    null,
                    out _frameIndex);
                if (acquireResult != AcquireResult.Success)
                {
                    throw new VeldridException($"Failed to acquire Swapchain image.");
                }
            }

            handler(_cbs[FrameIndex], FrameIndex, _sc.Framebuffers[FrameIndex]);
            _gd.SubmitCommands(
                _cbs[FrameIndex],
                _imageAcquiredSems[FrameIndex],
                _renderCompleteSems[FrameIndex],
                _fences[FrameIndex]);
            _gd.Present(_sc, _renderCompleteSems[FrameIndex], FrameIndex);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                for (uint i = 0; i < _maxFramesInFlight; i++)
                {
                    _fences[i].Dispose();
                    _cbs[i].Dispose();
                    _imageAcquiredSems[i].Dispose();
                    _renderCompleteSems[i].Dispose();
                }
            }
        }
    }

    public delegate void StandardFrameLoopHandler(CommandBuffer cb, uint frameIndex, Framebuffer fb);

    public class AdvancedFrameLoop : IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly Swapchain _sc;

        private uint _maxFramesInFlight;
        public Fence[] _fences;
        public Semaphore[] _imageAcquiredSems;
        public Semaphore[] _renderCompleteSems;
        private uint _frameIndex;
        private bool _disposed;

        public uint FrameIndex => _frameIndex;

        public AdvancedFrameLoop(GraphicsDevice gd, Swapchain sc)
        {
            _gd = gd;
            _sc = sc;
            _maxFramesInFlight = sc.BufferCount;
            _frameIndex = _maxFramesInFlight - 1;

            _fences = new Fence[_maxFramesInFlight];
            _imageAcquiredSems = new Semaphore[_maxFramesInFlight];
            _renderCompleteSems = new Semaphore[_maxFramesInFlight];
            for (uint i = 0; i < _maxFramesInFlight; i++)
            {
                _fences[i] = gd.ResourceFactory.CreateFence(signaled: true);
                _fences[i].Name = $"Frame Loop Fence {i}";
                _imageAcquiredSems[i] = gd.ResourceFactory.CreateSemaphore();
                _renderCompleteSems[i] = gd.ResourceFactory.CreateSemaphore();
            }
        }

        public void RunFrame(AdvancedFrameLoopHandler handler)
        {
            uint nextFrame = (_sc.LastAcquiredImage + 1) % _maxFramesInFlight;
            _fences[nextFrame].Wait();
            _fences[nextFrame].Reset();

            AcquireResult acquireResult = _gd.AcquireNextImage(
                _sc,
                _imageAcquiredSems[nextFrame],
                null,
                out _frameIndex);
            if (acquireResult == AcquireResult.OutOfDate)
            {
                _sc.Resize(_sc.Width, _sc.Height);

                nextFrame = 0;
                acquireResult = _gd.AcquireNextImage(
                    _sc,
                    _imageAcquiredSems[nextFrame],
                    null,
                    out _frameIndex);
                if (acquireResult != AcquireResult.Success)
                {
                    throw new VeldridException($"Failed to acquire Swapchain image.");
                }
            }

            CommandBuffer[] cbs = handler(FrameIndex, _sc.Framebuffers[FrameIndex]);
            _gd.SubmitCommands(
                cbs,
                _imageAcquiredSems[FrameIndex],
                _renderCompleteSems[FrameIndex],
                _fences[FrameIndex]);
            _gd.Present(_sc, _renderCompleteSems[FrameIndex], FrameIndex);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                for (uint i = 0; i < _maxFramesInFlight; i++)
                {
                    _fences[i].Dispose();
                    _imageAcquiredSems[i].Dispose();
                    _renderCompleteSems[i].Dispose();
                }
            }
        }
    }

    public delegate CommandBuffer[] AdvancedFrameLoopHandler(uint frameIndex, Framebuffer fb);
}
