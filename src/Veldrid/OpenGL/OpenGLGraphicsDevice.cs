using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using System;
using Veldrid.OpenGLBinding;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLGraphicsDevice : GraphicsDevice
    {
        private readonly uint _vao;
        private readonly ConcurrentQueue<OpenGLDeferredResource> _resourcesToDispose
            = new ConcurrentQueue<OpenGLDeferredResource>();
        private readonly IntPtr _glContext;
        private readonly Action<IntPtr> _deleteContext;
        private readonly Action _swapBuffers;
        private readonly OpenGLSwapchainFramebuffer _swapchainFramebuffer;
        private readonly OpenGLCommandExecutor _commandExecutor;
        private DebugProc _debugMessageCallback;
        private readonly OpenGLExtensions _extensions;

        private readonly Action<IntPtr> _makeCurrent;
        private readonly TextureSampleCount _maxColorTextureSamples;

        private readonly StagingMemoryPool _stagingMemoryPool = new StagingMemoryPool();
        private readonly BlockingCollection<ExecutionThreadWorkItem> _workItems;
        private readonly ExecutionThread _executionThread;

        private readonly object _commandListDisposalLock = new object();
        private readonly HashSet<OpenGLCommandList> _submittedCommandLists = new HashSet<OpenGLCommandList>();
        private readonly HashSet<OpenGLCommandList> _commandListsToDispose = new HashSet<OpenGLCommandList>();

        public override GraphicsBackend BackendType => GraphicsBackend.OpenGL;

        public override ResourceFactory ResourceFactory { get; }

        public override Framebuffer SwapchainFramebuffer => _swapchainFramebuffer;

        public OpenGLExtensions Extensions => _extensions;

        public OpenGLGraphicsDevice(
            OpenGLPlatformInfo platformInfo,
            uint width,
            uint height,
            bool debugDevice)
        {
            _glContext = platformInfo.OpenGLContextHandle;
            _makeCurrent = platformInfo.MakeCurrent;
            _deleteContext = platformInfo.DeleteContext;
            _swapBuffers = platformInfo.SwapBuffers;
            LoadAllFunctions(_glContext, platformInfo.GetProcAddress);

            ResourceFactory = new OpenGLResourceFactory(this);

            glGenVertexArrays(1, out _vao);
            CheckLastError();

            glBindVertexArray(_vao);
            CheckLastError();

            _swapchainFramebuffer = new OpenGLSwapchainFramebuffer(width, height);

            if (debugDevice)
            {
                EnableDebugCallback();
            }

            // Set miscellaneous initial states.
            glEnable(EnableCap.TextureCubeMapSeamless);
            CheckLastError();

            int extensionCount;
            glGetIntegerv(GetPName.NumExtensions, &extensionCount);
            CheckLastError();

            HashSet<string> extensions = new HashSet<string>();
            for (uint i = 0; i < extensionCount; i++)
            {
                byte* extensionNamePtr = glGetStringi(StringNameIndexed.Extensions, i);
                CheckLastError();
                if (extensionNamePtr != null)
                {
                    string extensionName = Util.GetString(extensionNamePtr);
                    extensions.Add(extensionName);
                }
            }

            _extensions = new OpenGLExtensions(extensions);
            _commandExecutor = new OpenGLCommandExecutor(_extensions);

            int maxColorTextureSamples;
            glGetIntegerv(GetPName.MaxColorTextureSamples, &maxColorTextureSamples);
            CheckLastError();
            if (maxColorTextureSamples >= 32)
            {
                _maxColorTextureSamples = TextureSampleCount.Count32;
            }
            else if (maxColorTextureSamples >= 16)
            {
                _maxColorTextureSamples = TextureSampleCount.Count16;
            }
            else if (maxColorTextureSamples >= 8)
            {
                _maxColorTextureSamples = TextureSampleCount.Count8;
            }
            else if (maxColorTextureSamples >= 4)
            {
                _maxColorTextureSamples = TextureSampleCount.Count4;
            }
            else if (maxColorTextureSamples >= 2)
            {
                _maxColorTextureSamples = TextureSampleCount.Count2;
            }
            else
            {
                _maxColorTextureSamples = TextureSampleCount.Count1;
            }

            _workItems = new BlockingCollection<ExecutionThreadWorkItem>(new ConcurrentQueue<ExecutionThreadWorkItem>());
            platformInfo.ClearCurrentContext();
            _executionThread = new ExecutionThread(this, _workItems, _makeCurrent, _glContext);

            PostDeviceCreated();
        }

        public override void ExecuteCommands(CommandList cl)
        {
            OpenGLCommandList glCommandList = Util.AssertSubtype<CommandList, OpenGLCommandList>(cl);
            _submittedCommandLists.Add(glCommandList);
            _executionThread.ExecuteCommands(glCommandList);
        }

        public override void ResizeMainWindow(uint width, uint height)
        {
            _swapchainFramebuffer.Resize(width, height);
        }

        public override void SwapBuffers()
        {
            _executionThread.Run(() =>
            {
                _swapBuffers();
                FlushDisposables();
            });
        }

        public override void SetResourceName(DeviceResource resource, string name)
        {
            switch (resource)
            {
                case OpenGLBuffer buffer:
                    buffer.Name = name;
                    break;
                case OpenGLFramebuffer framebuffer:
                    framebuffer.Name = name;
                    break;
                case OpenGLSampler sampler:
                    sampler.Name = name;
                    break;
                case OpenGLShader shader:
                    shader.Name = name;
                    break;
                case OpenGLTexture tex:
                    tex.Name = name;
                    break;
            }
        }

        public override void WaitForIdle()
        {
            FlushDisposables();
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            return _maxColorTextureSamples;
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            if (resource is OpenGLBuffer buffer)
            {
                IntPtr mappedPtr = _executionThread.Map(resource, mode, subresource);
                return new MappedResource(buffer, mode, mappedPtr, buffer.SizeInBytes);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
            if (resource is OpenGLBuffer buffer)
            {
                _executionThread.Unmap(resource, subresource);
            }
            else
            {
            }
        }

        public override void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            StagingBlock sb = _stagingMemoryPool.Stage(source, sizeInBytes);
            _executionThread.UpdateBuffer(buffer, bufferOffsetInBytes, sb);
        }

        public override void UpdateTexture(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            StagingBlock sb = _stagingMemoryPool.Stage(source, sizeInBytes);
            _executionThread.Run(() =>
            {
                fixed (byte* dataPtr = &sb.Array[0])
                {
                    _commandExecutor.UpdateTexture(texture, (IntPtr)dataPtr, x, y, z, width, height, depth, mipLevel, arrayLayer);
                    sb.Free();
                }
            });
        }

        internal void EnqueueDisposal(OpenGLDeferredResource resource)
        {
            _resourcesToDispose.Enqueue(resource);
        }

        internal void EnqueueDisposal(OpenGLCommandList commandList)
        {
            lock (_commandListDisposalLock)
            {
                if (_submittedCommandLists.Contains(commandList))
                {
                    _commandListsToDispose.Add(commandList);
                }
                else
                {
                    commandList.DestroyResources();
                }
            }
        }

        internal bool CheckCommandListDisposal(OpenGLCommandList commandList)
        {
            lock (_commandListDisposalLock)
            {
                bool result = _submittedCommandLists.Remove(commandList);
                Debug.Assert(result);
                if (_commandListsToDispose.Remove(commandList))
                {
                    commandList.DestroyResources();
                    return true;
                }

                return false;
            }
        }

        private void FlushDisposables()
        {
            // Check if the OpenGL context has already been destroyed by the OS. If so, just exit out.
            uint error = glGetError();
            if (error == (uint)ErrorCode.InvalidOperation)
            {
                return;
            }

            while (_resourcesToDispose.TryDequeue(out OpenGLDeferredResource resource))
            {
                resource.DestroyGLResources();
            }
        }

        public void EnableDebugCallback() => EnableDebugCallback(DebugSeverity.DebugSeverityNotification);
        public void EnableDebugCallback(DebugSeverity minimumSeverity) => EnableDebugCallback(DefaultDebugCallback(minimumSeverity));
        public void EnableDebugCallback(DebugProc callback)
        {
            glEnable(EnableCap.DebugOutput);
            CheckLastError();
            // The debug callback delegate must be persisted, otherwise errors will occur
            // when the OpenGL drivers attempt to call it after it has been collected.
            _debugMessageCallback = callback;
            glDebugMessageCallback(_debugMessageCallback, null);
            CheckLastError();
        }

        private DebugProc DefaultDebugCallback(DebugSeverity minimumSeverity)
        {
            return (source, type, id, severity, length, message, userParam) =>
            {
                if (severity >= minimumSeverity)
                {
                    string messageString = Marshal.PtrToStringAnsi((IntPtr)message, (int)length);
                    Debug.WriteLine($"GL DEBUG MESSAGE: {source}, {type}, {id}. {severity}: {messageString}");
                }
            };
        }

        protected override void PlatformDispose()
        {
            _executionThread.Terminate(() =>
            {
                FlushDisposables();
                _deleteContext(_glContext);
            });
        }

        private class ExecutionThread
        {
            private readonly OpenGLGraphicsDevice _gd;
            private readonly BlockingCollection<ExecutionThreadWorkItem> _workItems;
            private readonly Action<IntPtr> _makeCurrent;
            private readonly IntPtr _context;
            private bool _terminated;

            public ExecutionThread(
                OpenGLGraphicsDevice gd,
                BlockingCollection<ExecutionThreadWorkItem> workItems,
                Action<IntPtr> makeCurrent,
                IntPtr context)
            {
                // TODO: Only need _gd._commandExecutor here.
                _gd = gd;
                _workItems = workItems;
                _makeCurrent = makeCurrent;
                _context = context;
                new Thread(Run).Start();
            }

            private void Run()
            {
                _makeCurrent(_context);
                while (!_terminated)
                {
                    ExecutionThreadWorkItem workItem = _workItems.Take();
                    ExecuteWorkItem(workItem);
                }
            }

            private void ExecuteWorkItem(ExecutionThreadWorkItem workItem)
            {
                _makeCurrent(_context);
                if (workItem.CommandListToExecute != null)
                {
                    workItem.CommandListToExecute.Commands.ExecuteAll(_gd._commandExecutor);
                    if (!_gd.CheckCommandListDisposal(workItem.CommandListToExecute))
                    {
                        workItem.CommandListToExecute.Reset();
                    }
                }
                else if (workItem.ResourceToMap != null)
                {
                    if (workItem.Map)
                    {
                        ExecuteMapResource(
                            workItem.ResourceToMap,
                            workItem.MapMode,
                            workItem.MapSubresource,
                            workItem.ValueDestination,
                            workItem.ResetEvent);
                    }
                    else
                    {
                        ExecuteUnmapResource(workItem.ResourceToMap, workItem.MapSubresource);
                    }
                }
                else if (workItem.UpdateBuffer != null)
                {
                    StagingBlock stagingBlock = workItem.UpdateBufferStagedSource;
                    fixed (byte* dataPtr = &stagingBlock.Array[0])
                    {
                        _gd._commandExecutor.UpdateBuffer(
                            workItem.UpdateBuffer,
                            workItem.UpdateBufferOffsetInBytes,
                            (IntPtr)dataPtr,
                            stagingBlock.SizeInBytes);
                    }
                    stagingBlock.Free();
                }
                else if (workItem.Delegate != null)
                {
                    workItem.Delegate();
                }
                else
                {
                    Debug.Assert(workItem.TerminateAction != null);
                    workItem.TerminateAction();
                    _terminated = true;
                }
            }

            private void ExecuteMapResource(
                MappableResource resource,
                MapMode mode,
                uint mapSubresource,
                IntPtr* result,
                ManualResetEventSlim mre)
            {
                if (resource is OpenGLBuffer buffer)
                {
                    buffer.EnsureResourcesCreated();
                    void* mappedPtr;
                    BufferAccessMask accessMask = OpenGLFormats.VdToGLMapMode(mode);
                    if (_gd.Extensions.ARB_DirectStateAccess)
                    {
                        mappedPtr = glMapNamedBufferRange(buffer.Buffer, IntPtr.Zero, buffer.SizeInBytes, accessMask);
                        CheckLastError();
                    }
                    else
                    {
                        glBindBuffer(BufferTarget.CopyWriteBuffer, buffer.Buffer);
                        CheckLastError();

                        mappedPtr = glMapBufferRange(BufferTarget.CopyWriteBuffer, IntPtr.Zero, (IntPtr)buffer.SizeInBytes, accessMask);
                        CheckLastError();
                    }

                    *result = (IntPtr)mappedPtr;
                    mre.Set();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            private void ExecuteUnmapResource(MappableResource resource, uint mapSubresource)
            {
                if (resource is OpenGLBuffer buffer)
                {
                    if (_gd.Extensions.ARB_DirectStateAccess)
                    {
                        glUnmapNamedBuffer(buffer.Buffer);
                        CheckLastError();
                    }
                    else
                    {
                        glBindBuffer(BufferTarget.CopyWriteBuffer, buffer.Buffer);
                        CheckLastError();

                        glUnmapBuffer(BufferTarget.CopyWriteBuffer);
                        CheckLastError();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            public IntPtr Map(MappableResource resource, MapMode mode, uint subresource)
            {
                IntPtr result;
                ManualResetEventSlim mre = new ManualResetEventSlim(false);
                _workItems.Add(new ExecutionThreadWorkItem(resource, mode, subresource, true, &result, mre));
                mre.Wait();
                return result;
            }

            public void ExecuteCommands(OpenGLCommandList commandList)
            {

                _workItems.Add(new ExecutionThreadWorkItem(commandList));
            }

            internal void Unmap(MappableResource resource, uint subresource)
            {
                _workItems.Add(new ExecutionThreadWorkItem(resource, 0, subresource, false, null, null));
            }

            internal void UpdateBuffer(Buffer buffer, uint offsetInBytes, StagingBlock stagingBlock)
            {
                _workItems.Add(new ExecutionThreadWorkItem(buffer, offsetInBytes, stagingBlock));
            }

            internal void Run(Action a)
            {
                _workItems.Add(new ExecutionThreadWorkItem(a));
            }

            internal void Terminate(Action a)
            {
                _workItems.Add(new ExecutionThreadWorkItem(a, isTermination: true));
            }
        }

        private unsafe struct ExecutionThreadWorkItem
        {
            public readonly MappableResource ResourceToMap;
            public readonly MapMode MapMode;
            public readonly uint MapSubresource;
            public readonly bool Map; // false == Unmap

            public readonly OpenGLCommandList CommandListToExecute;

            public readonly Buffer UpdateBuffer;
            public readonly uint UpdateBufferOffsetInBytes;
            public readonly StagingBlock UpdateBufferStagedSource;

            public readonly IntPtr* ValueDestination;
            public readonly ManualResetEventSlim ResetEvent;

            public readonly Action Delegate;
            public readonly Action TerminateAction;

            public ExecutionThreadWorkItem(
                MappableResource resource,
                MapMode mapMode,
                uint subresource,
                bool map,
                IntPtr* valueDestination,
                ManualResetEventSlim resetEvent)
            {
                ResourceToMap = resource;
                MapMode = mapMode;
                MapSubresource = subresource;
                Map = map;

                CommandListToExecute = null;

                UpdateBuffer = null;
                UpdateBufferOffsetInBytes = 0;
                UpdateBufferStagedSource = default(StagingBlock);

                ValueDestination = valueDestination;
                ResetEvent = resetEvent;

                Delegate = null;
                TerminateAction = null;
            }

            public ExecutionThreadWorkItem(OpenGLCommandList commandList)
            {
                CommandListToExecute = commandList;

                ResourceToMap = null;
                MapMode = 0;
                MapSubresource = 0;
                Map = false;

                UpdateBuffer = null;
                UpdateBufferOffsetInBytes = 0;
                UpdateBufferStagedSource = default(StagingBlock);

                ValueDestination = null;
                ResetEvent = null;

                Delegate = null;
                TerminateAction = null;
            }

            public ExecutionThreadWorkItem(Buffer updateBuffer, uint offsetInBytes, StagingBlock stagedSource)
            {
                UpdateBuffer = updateBuffer;
                UpdateBufferOffsetInBytes = offsetInBytes;
                UpdateBufferStagedSource = stagedSource;

                CommandListToExecute = null;

                ResourceToMap = null;
                MapMode = 0;
                MapSubresource = 0;
                Map = false;

                ValueDestination = null;
                ResetEvent = null;

                Delegate = null;
                TerminateAction = null;
            }

            public ExecutionThreadWorkItem(Action a, bool isTermination = false)
            {
                CommandListToExecute = null;

                ResourceToMap = null;
                MapMode = 0;
                MapSubresource = 0;
                Map = false;

                UpdateBuffer = null;
                UpdateBufferOffsetInBytes = 0;
                UpdateBufferStagedSource = default(StagingBlock);

                ValueDestination = null;
                ResetEvent = null;

                if (isTermination)
                {
                    TerminateAction = a;
                    Delegate = null;
                }
                else
                {
                    Delegate = a;
                    TerminateAction = null;
                }
            }
        }
    }
}
