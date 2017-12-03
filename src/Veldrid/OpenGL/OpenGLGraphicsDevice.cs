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
        private readonly Action<IntPtr> _makeCurrent;
        private readonly Func<IntPtr> _getCurrentContext;
        private readonly Action<IntPtr> _deleteContext;
        private readonly Action _swapBuffers;
        private readonly Action<bool> _setSyncToVBlank;
        private readonly OpenGLSwapchainFramebuffer _swapchainFramebuffer;
        private readonly OpenGLCommandExecutor _commandExecutor;
        private DebugProc _debugMessageCallback;
        private readonly OpenGLExtensions _extensions;

        private readonly TextureSampleCount _maxColorTextureSamples;

        private readonly StagingMemoryPool _stagingMemoryPool = new StagingMemoryPool();
        private readonly BlockingCollection<ExecutionThreadWorkItem> _workItems;
        private readonly ExecutionThread _executionThread;

        private readonly object _commandListDisposalLock = new object();
        private readonly HashSet<OpenGLCommandList> _submittedCommandLists = new HashSet<OpenGLCommandList>();
        private readonly HashSet<OpenGLCommandList> _commandListsToDispose = new HashSet<OpenGLCommandList>();

        private readonly object _mappedResourceLock = new object();
        private readonly Dictionary<MappedResourceCacheKey, MappedResourceInfoWithStaging> _mappedResources
            = new Dictionary<MappedResourceCacheKey, MappedResourceInfoWithStaging>();
        private readonly MapResultHolder _mapResultHolder = new MapResultHolder();

        private bool _syncToVBlank;

        public override GraphicsBackend BackendType => GraphicsBackend.OpenGL;

        public override ResourceFactory ResourceFactory { get; }

        public override Framebuffer SwapchainFramebuffer => _swapchainFramebuffer;

        public OpenGLExtensions Extensions => _extensions;

        public override bool SyncToVerticalBlank
        {
            get => _syncToVBlank;
            set
            {
                if (_syncToVBlank != value)
                {
                    _syncToVBlank = value;
                    _executionThread.Run(() => _setSyncToVBlank(value));
                }
            }
        }

        public OpenGLGraphicsDevice(
            GraphicsDeviceOptions options,
            OpenGLPlatformInfo platformInfo,
            uint width,
            uint height)
        {
            _syncToVBlank = options.SyncToVerticalBlank;
            _glContext = platformInfo.OpenGLContextHandle;
            _makeCurrent = platformInfo.MakeCurrent;
            _getCurrentContext = platformInfo.GetCurrentContext;
            _deleteContext = platformInfo.DeleteContext;
            _swapBuffers = platformInfo.SwapBuffers;
            _setSyncToVBlank = platformInfo.SetSyncToVerticalBlank;
            LoadAllFunctions(_glContext, platformInfo.GetProcAddress);

            ResourceFactory = new OpenGLResourceFactory(this);

            glGenVertexArrays(1, out _vao);
            CheckLastError();

            glBindVertexArray(_vao);
            CheckLastError();

            _swapchainFramebuffer = new OpenGLSwapchainFramebuffer(width, height, options.SwapchainDepthFormat);

            if (options.Debug)
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
            _executionThread.WaitForIdle();
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            return _maxColorTextureSamples;
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            MappedResourceCacheKey key = new MappedResourceCacheKey(resource, subresource);
            lock (_mappedResourceLock)
            {
                if (_mappedResources.TryGetValue(key, out MappedResourceInfoWithStaging info))
                {
                    if (info.Mode != mode)
                    {
                        throw new VeldridException("The given resource was already mapped with a different MapMode.");
                    }

                    info.RefCount += 1;
                    _mappedResources[key] = info;
                    return info.MappedResource;
                }
            }

            _executionThread.Map(resource, mode, subresource);
            return _mapResultHolder.Resource;
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
            _executionThread.Unmap(resource, subresource);
        }

        public override void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            lock (_mappedResourceLock)
            {
                if (_mappedResources.ContainsKey(new MappedResourceCacheKey(buffer, 0)))
                {
                    throw new VeldridException("Cannot call UpdateBuffer on a currently-mapped Buffer.");
                }
            }
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
                // Check if the OpenGL context has already been destroyed by the OS. If so, just exit out.
                uint error = glGetError();
                if (error == (uint)ErrorCode.InvalidOperation)
                {
                    return;
                }
                _makeCurrent(_glContext);

                //FlushDisposables();
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
            private readonly List<Exception> _exceptions = new List<Exception>();
            private readonly object _exceptionsLock = new object();

            public ExecutionThread(
                OpenGLGraphicsDevice gd,
                BlockingCollection<ExecutionThreadWorkItem> workItems,
                Action<IntPtr> makeCurrent,
                IntPtr context)
            {
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
                try
                {
                    if (workItem.CommandListToExecute != null)
                    {
                        try
                        {
                            workItem.CommandListToExecute.Commands.ExecuteAll(_gd._commandExecutor);
                        }
                        finally
                        {
                            if (!_gd.CheckCommandListDisposal(workItem.CommandListToExecute))
                            {
                                workItem.CommandListToExecute.Reset();
                            }
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
                                workItem.ResetEvent);
                        }
                        else
                        {
                            ExecuteUnmapResource(workItem.ResourceToMap, workItem.MapSubresource, workItem.ResetEvent);
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
                    else if (workItem.ResetEvent != null)
                    {
                        // Wait for idle.
                        _gd.FlushDisposables(); // TODO: This should be in this class.
                        workItem.ResetEvent.Set();
                    }
                    else
                    {
                        Debug.Assert(workItem.TerminateAction != null);
                        workItem.TerminateAction();
                        _terminated = true;
                    }
                }
                catch (Exception e)
                {
                    lock (_exceptionsLock)
                    {
                        _exceptions.Add(e);
                    }
                }
            }

            private void ExecuteMapResource(
                MappableResource resource,
                MapMode mode,
                uint subresource,
                ManualResetEventSlim mre)
            {
                MappedResourceCacheKey key = new MappedResourceCacheKey(resource, subresource);
                try
                {
                    lock (_gd._mappedResourceLock)
                    {
                        Debug.Assert(!_gd._mappedResources.ContainsKey(key));
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

                            MappedResourceInfoWithStaging info = new MappedResourceInfoWithStaging();
                            info.MappedResource = new MappedResource(
                                resource,
                                mode,
                                (IntPtr)mappedPtr,
                                buffer.SizeInBytes);
                            info.RefCount = 1;
                            info.Mode = mode;
                            _gd._mappedResources.Add(key, info);
                            _gd._mapResultHolder.Resource = info.MappedResource;
                            _gd._mapResultHolder.Succeeded = true;
                        }
                        else
                        {
                            OpenGLTexture texture = Util.AssertSubtype<MappableResource, OpenGLTexture>(resource);
                            texture.EnsureResourcesCreated();

                            Util.GetMipLevelAndArrayLayer(texture, subresource, out uint mipLevel, out uint arrayLayer);
                            Util.GetMipDimensions(texture, mipLevel, out uint width, out uint height, out uint depth);

                            uint pixelSize = FormatHelpers.GetSizeInBytes(texture.Format);
                            uint sizeInBytes = texture.Width * texture.Height * pixelSize;

                            FixedStagingBlock block = _gd._stagingMemoryPool.GetFixedStagingBlock(sizeInBytes);

                            if (mode == MapMode.Read || mode == MapMode.ReadWrite)
                            {
                                // Read data into buffer.
                                if (_gd.Extensions.ARB_DirectStateAccess)
                                {
                                    int zoffset = texture.ArrayLayers > 1 ? (int)arrayLayer : 0;
                                    glGetTextureSubImage(
                                        texture.Texture,
                                        (int)mipLevel,
                                        0, 0, zoffset,
                                        width, height, depth,
                                        texture.GLPixelFormat,
                                        texture.GLPixelType,
                                        sizeInBytes,
                                        block.Data);
                                    CheckLastError();
                                }
                                else
                                {
                                    if (texture.TextureTarget == TextureTarget.Texture2DArray
                                        || texture.TextureTarget == TextureTarget.Texture2DMultisampleArray
                                        || texture.TextureTarget == TextureTarget.TextureCubeMapArray)
                                    {
                                        throw new NotImplementedException();
                                    }

                                    glBindTexture(texture.TextureTarget, texture.Texture);
                                    CheckLastError();

                                    glGetTexImage(
                                        texture.TextureTarget,
                                        (int)mipLevel,
                                        texture.GLPixelFormat,
                                        texture.GLPixelType,
                                        block.Data);
                                    CheckLastError();
                                }
                            }

                            uint rowPitch = texture.Width * pixelSize;
                            uint depthPitch = pixelSize * texture.Width * texture.Height;
                            MappedResourceInfoWithStaging info = new MappedResourceInfoWithStaging();
                            info.MappedResource = new MappedResource(
                                resource,
                                mode,
                                (IntPtr)block.Data,
                                sizeInBytes,
                                subresource,
                                rowPitch,
                                depthPitch);
                            info.RefCount = 1;
                            info.Mode = mode;
                            info.StagingBlock = block;
                            _gd._mappedResources.Add(key, info);
                            _gd._mapResultHolder.Resource = info.MappedResource;
                            _gd._mapResultHolder.Succeeded = true;
                        }
                    }
                }
                catch
                {
                    _gd._mapResultHolder.Succeeded = false;
                }
                finally
                {
                    mre.Set();
                }
            }

            private void ExecuteUnmapResource(MappableResource resource, uint subresource, ManualResetEventSlim mre)
            {
                MappedResourceCacheKey key = new MappedResourceCacheKey(resource, subresource);
                lock (_gd._mappedResourceLock)
                {
                    MappedResourceInfoWithStaging info = _gd._mappedResources[key];
                    if (info.RefCount == 1)
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
                            OpenGLTexture texture = Util.AssertSubtype<MappableResource, OpenGLTexture>(resource);

                            if (info.Mode == MapMode.Write || info.Mode == MapMode.ReadWrite)
                            {
                                Util.GetMipLevelAndArrayLayer(texture, subresource, out uint mipLevel, out uint arrayLayer);
                                Util.GetMipDimensions(texture, mipLevel, out uint width, out uint height, out uint depth);

                                IntPtr data = (IntPtr)info.StagingBlock.Data;

                                _gd._commandExecutor.UpdateTexture(
                                    texture,
                                    data,
                                    0, 0, 0,
                                    width, height, depth,
                                    mipLevel,
                                    arrayLayer);
                            }

                            info.StagingBlock.Free();
                        }

                        _gd._mappedResources.Remove(key);
                    }
                }

                mre.Set();
            }

            private void CheckExceptions()
            {
                lock (_exceptionsLock)
                {
                    if (_exceptions.Count > 0)
                    {
                        Exception innerException = _exceptions.Count == 1
                            ? _exceptions[0]
                            : new AggregateException(_exceptions.ToArray());
                        _exceptions.Clear();
                        throw new VeldridException(
                            "Error(s) were encountered during the execution of OpenGL commands. See InnerException for more information.",
                            innerException);

                    }
                }
            }

            public void Map(MappableResource resource, MapMode mode, uint subresource)
            {
                CheckExceptions();

                ManualResetEventSlim mre = new ManualResetEventSlim(false);
                _workItems.Add(new ExecutionThreadWorkItem(resource, mode, subresource, true, mre));
                mre.Wait();
                if (!_gd._mapResultHolder.Succeeded)
                {
                    throw new VeldridException("Failed to map OpenGL resource.");
                }
            }

            internal void Unmap(MappableResource resource, uint subresource)
            {
                CheckExceptions();

                ManualResetEventSlim mre = new ManualResetEventSlim(false);
                _workItems.Add(new ExecutionThreadWorkItem(resource, 0, subresource, false, mre));
                mre.Wait();
            }

            public void ExecuteCommands(OpenGLCommandList commandList)
            {
                CheckExceptions();

                _workItems.Add(new ExecutionThreadWorkItem(commandList));
            }

            internal void UpdateBuffer(Buffer buffer, uint offsetInBytes, StagingBlock stagingBlock)
            {
                CheckExceptions();

                _workItems.Add(new ExecutionThreadWorkItem(buffer, offsetInBytes, stagingBlock));
            }

            internal void Run(Action a)
            {
                CheckExceptions();

                _workItems.Add(new ExecutionThreadWorkItem(a));
            }

            internal void Terminate(Action a)
            {
                CheckExceptions();

                _workItems.Add(new ExecutionThreadWorkItem(a, isTermination: true));
            }

            internal void WaitForIdle()
            {
                ManualResetEventSlim mre = new ManualResetEventSlim();
                _workItems.Add(new ExecutionThreadWorkItem(mre));
                mre.Wait();

                CheckExceptions();
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

            public readonly ManualResetEventSlim ResetEvent;

            public readonly Action Delegate;
            public readonly Action TerminateAction;

            public ExecutionThreadWorkItem(
                MappableResource resource,
                MapMode mapMode,
                uint subresource,
                bool map,
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

            public ExecutionThreadWorkItem(ManualResetEventSlim mre)
            {
                ResetEvent = mre;

                CommandListToExecute = null;

                ResourceToMap = null;
                MapMode = 0;
                MapSubresource = 0;
                Map = false;

                UpdateBuffer = null;
                UpdateBufferOffsetInBytes = 0;
                UpdateBufferStagedSource = default(StagingBlock);

                Delegate = null;

                TerminateAction = null;
            }
        }

        private class MapResultHolder
        {
            public bool Succeeded;
            public MappedResource Resource;
        }

        internal struct MappedResourceInfoWithStaging
        {
            public int RefCount;
            public MapMode Mode;
            public MappedResource MappedResource;
            public FixedStagingBlock StagingBlock;
        }
    }
}
