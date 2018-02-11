using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using System;
using Veldrid.OpenGLBinding;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Collections.Generic;
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
        private readonly Dictionary<OpenGLCommandList, int> _submittedCommandListCounts
            = new Dictionary<OpenGLCommandList, int>();
        private readonly HashSet<OpenGLCommandList> _commandListsToDispose = new HashSet<OpenGLCommandList>();

        private readonly object _mappedResourceLock = new object();
        private readonly Dictionary<MappedResourceCacheKey, MappedResourceInfoWithStaging> _mappedResources
            = new Dictionary<MappedResourceCacheKey, MappedResourceInfoWithStaging>();
        private readonly MapResultHolder _mapResultHolder = new MapResultHolder();

        private bool _syncToVBlank;
        public int MajorVersion { get; }
        public int MinorVersion { get; }

        public override GraphicsBackend BackendType => GraphicsBackend.OpenGL;

        public override ResourceFactory ResourceFactory { get; }

        public OpenGLExtensions Extensions => _extensions;

        public override Swapchain MainSwapchain => _mainSwapchain;

        public override bool SyncToVerticalBlank
        {
            get => _syncToVBlank;
            set
            {
                if (_syncToVBlank != value)
                {
                    _syncToVBlank = value;
                    _executionThread.SetSyncToVerticalBlank(value);
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

            int majorVersion, minorVersion;
            glGetIntegerv(GetPName.MajorVersion, &majorVersion);
            CheckLastError();
            glGetIntegerv(GetPName.MinorVersion, &minorVersion);
            CheckLastError();

            MajorVersion = majorVersion;
            MinorVersion = majorVersion;

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

            ResourceFactory = new OpenGLResourceFactory(this);

            glGenVertexArrays(1, out _vao);
            CheckLastError();

            glBindVertexArray(_vao);
            CheckLastError();

            _swapchainFramebuffer = new OpenGLSwapchainFramebuffer(
                width,
                height,
                PixelFormat.B8_G8_R8_A8_UNorm,
                options.SwapchainDepthFormat);

            if (options.Debug && _extensions.ARB_DebugOutput)
            {
                EnableDebugCallback();
            }

            // Set miscellaneous initial states.
            glEnable(EnableCap.TextureCubeMapSeamless);
            CheckLastError();

            _commandExecutor = new OpenGLCommandExecutor(_extensions, _stagingMemoryPool);

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

            _mainSwapchain = new OpenGLSwapchain(this, width, height, options.SwapchainDepthFormat);

            _workItems = new BlockingCollection<ExecutionThreadWorkItem>(new ConcurrentQueue<ExecutionThreadWorkItem>());
            platformInfo.ClearCurrentContext();
            _executionThread = new ExecutionThread(this, _workItems, _makeCurrent, _glContext);

            PostDeviceCreated();
        }

        protected override void SubmitCommandsCore(
            CommandList cl,
            Fence fence)
        {
            lock (_commandListDisposalLock)
            {
                OpenGLCommandList glCommandList = Util.AssertSubtype<CommandList, OpenGLCommandList>(cl);
                OpenGLCommandEntryList entryList = glCommandList.CurrentCommands;
                IncrementCount(glCommandList);
                _executionThread.ExecuteCommands(entryList);
                if (fence is OpenGLFence glFence)
                {
                    glFence.Set();
                }
            }
        }

        private int IncrementCount(OpenGLCommandList glCommandList)
        {
            if (_submittedCommandListCounts.TryGetValue(glCommandList, out int count))
            {
                count += 1;
            }
            else
            {
                count = 1;
            }

            _submittedCommandListCounts[glCommandList] = count;
            return count;
        }

        private int DecrementCount(OpenGLCommandList glCommandList)
        {
            if (_submittedCommandListCounts.TryGetValue(glCommandList, out int count))
            {
                count -= 1;
            }
            else
            {
                count = -1;
            }

            _submittedCommandListCounts[glCommandList] = count;
            return count;
        }

        private int GetCount(OpenGLCommandList glCommandList)
        {
            return _submittedCommandListCounts.TryGetValue(glCommandList, out int count) ? count : 0;
        }

        protected override void SwapBuffersCore(Swapchain swapchain)
        {
            WaitForIdle();

            _executionThread.SwapBuffers();
        }

        protected override void WaitForIdleCore()
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

        protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
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

        protected override void UpdateTextureCore(
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

        public override bool WaitForFence(Fence fence, ulong nanosecondTimeout)
        {
            return Util.AssertSubtype<Fence, OpenGLFence>(fence).Wait(nanosecondTimeout);
        }

        public override bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout)
        {
            int msTimeout = (int)(nanosecondTimeout / 1_000_000);
            ManualResetEvent[] events = GetResetEventArray(fences.Length);
            for (int i = 0; i < fences.Length; i++)
            {
                events[i] = Util.AssertSubtype<Fence, OpenGLFence>(fences[i]).ResetEvent;
            }
            bool result;
            if (waitAll)
            {
                result = WaitHandle.WaitAll(events, msTimeout);
            }
            else
            {
                int index = WaitHandle.WaitAny(events, msTimeout);
                result = index != WaitHandle.WaitTimeout;
            }

            ReturnResetEventArray(events);

            return result;
        }

        private readonly object _resetEventsLock = new object();
        private readonly List<ManualResetEvent[]> _resetEvents = new List<ManualResetEvent[]>();
        private readonly Swapchain _mainSwapchain;

        private ManualResetEvent[] GetResetEventArray(int length)
        {
            lock (_resetEventsLock)
            {
                for (int i = _resetEvents.Count - 1; i > 0; i--)
                {
                    ManualResetEvent[] array = _resetEvents[i];
                    if (array.Length == length)
                    {
                        _resetEvents.RemoveAt(i);
                        return array;
                    }
                }
            }

            ManualResetEvent[] newArray = new ManualResetEvent[length];
            return newArray;
        }

        private void ReturnResetEventArray(ManualResetEvent[] array)
        {
            lock (_resetEventsLock)
            {
                _resetEvents.Add(array);
            }
        }

        public override void ResetFence(Fence fence)
        {
            Util.AssertSubtype<Fence, OpenGLFence>(fence).Reset();
        }

        internal void EnqueueDisposal(OpenGLDeferredResource resource)
        {
            _resourcesToDispose.Enqueue(resource);
        }

        internal void EnqueueDisposal(OpenGLCommandList commandList)
        {
            lock (_commandListDisposalLock)
            {
                if (GetCount(commandList) > 0)
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
                int count = DecrementCount(commandList);
                if (count == 0)
                {
                    if (_commandListsToDispose.Remove(commandList))
                    {
                        commandList.DestroyResources();
                        return true;
                    }
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
            _executionThread.Terminate();
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
                    switch (workItem.Type)
                    {
                        case WorkItemType.ExecuteList:
                            {
                                OpenGLCommandEntryList list = (OpenGLCommandEntryList)workItem.Object0;
                                try
                                {
                                    list.ExecuteAll(_gd._commandExecutor);
                                    list.Parent.OnCompleted(list);
                                }
                                finally
                                {
                                    if (!_gd.CheckCommandListDisposal(list.Parent))
                                    {
                                        list.Reset();
                                    }
                                }
                            }
                            break;
                        case WorkItemType.Map:
                            {
                                MappableResource resourceToMap = (MappableResource)workItem.Object0;
                                ManualResetEventSlim mre = (ManualResetEventSlim)workItem.Object1;
                                MapMode mode = (MapMode)workItem.UInt0;
                                uint subresource = workItem.UInt1;
                                bool map = workItem.UInt2 == 1 ? true : false;
                                if (map)
                                {
                                    ExecuteMapResource(
                                        resourceToMap,
                                        mode,
                                        subresource,
                                        mre);
                                }
                                else
                                {
                                    ExecuteUnmapResource(resourceToMap, subresource, mre);
                                }
                            }
                            break;
                        case WorkItemType.UpdateBuffer:
                            {
                                DeviceBuffer updateBuffer = (DeviceBuffer)workItem.Object0;
                                byte[] stagingArray = (byte[])workItem.Object1;
                                StagingMemoryPool pool = (StagingMemoryPool)workItem.Object2;
                                uint offsetInBytes = workItem.UInt0;
                                uint sizeInBytes = workItem.UInt1;

                                fixed (byte* dataPtr = &stagingArray[0])
                                {
                                    _gd._commandExecutor.UpdateBuffer(
                                        updateBuffer,
                                        offsetInBytes,
                                        (IntPtr)dataPtr,
                                        sizeInBytes);
                                }
                                pool.Free(stagingArray);
                            }
                            break;
                        case WorkItemType.GenericAction:
                            {
                                ((Action)workItem.Object0)();
                            }
                            break;
                        case WorkItemType.SignalResetEvent:
                            {
                                _gd.FlushDisposables();
                                ((ManualResetEventSlim)workItem.Object0).Set();
                            }
                            break;
                        case WorkItemType.TerminateAction:
                            {
                                // Check if the OpenGL context has already been destroyed by the OS. If so, just exit out.
                                uint error = glGetError();
                                if (error == (uint)ErrorCode.InvalidOperation)
                                {
                                    return;
                                }
                                _makeCurrent(_gd._glContext);

                                _gd.FlushDisposables();
                                _gd._deleteContext(_gd._glContext);
                                _terminated = true;
                            }
                            break;
                        case WorkItemType.SetSyncToVerticalBlank:
                            {
                                bool value = workItem.UInt0 == 1 ? true : false;
                                _gd._setSyncToVBlank(value);
                            }
                            break;
                        case WorkItemType.SwapBuffers:
                            {
                                _gd._swapBuffers();
                                _gd.FlushDisposables();
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Invalid command type: " + workItem.Type);
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
                            Util.GetMipDimensions(texture, mipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);

                            uint subresourceSize = FormatHelpers.GetDepthPitch(
                                FormatHelpers.GetRowPitch(mipWidth, texture.Format),
                                mipHeight,
                                texture.Format)
                                * mipDepth;

                            bool isCompressed = FormatHelpers.IsCompressedFormat(texture.Format);
                            if (isCompressed)
                            {
                                int compressedSize;
                                glGetTexLevelParameteriv(
                                    texture.TextureTarget,
                                    (int)mipLevel,
                                    GetTextureParameter.TextureCompressedImageSize,
                                    &compressedSize);
                                CheckLastError();
                                subresourceSize = (uint)compressedSize;
                            }

                            FixedStagingBlock block = _gd._stagingMemoryPool.GetFixedStagingBlock(subresourceSize);

                            uint packAlignment = 4;
                            if (!isCompressed)
                            {
                                packAlignment = FormatHelpers.GetSizeInBytes(texture.Format);
                            }

                            if (packAlignment < 4)
                            {
                                glPixelStorei(PixelStoreParameter.PackAlignment, (int)packAlignment);
                                CheckLastError();
                            }

                            if (mode == MapMode.Read || mode == MapMode.ReadWrite)
                            {
                                if (!isCompressed)
                                {
                                    // Read data into buffer.
                                    if (_gd.Extensions.ARB_DirectStateAccess)
                                    {
                                        int zoffset = texture.ArrayLayers > 1 ? (int)arrayLayer : 0;
                                        glGetTextureSubImage(
                                            texture.Texture,
                                            (int)mipLevel,
                                            0, 0, zoffset,
                                            mipWidth, mipHeight, mipDepth,
                                            texture.GLPixelFormat,
                                            texture.GLPixelType,
                                            subresourceSize,
                                            block.Data);
                                        CheckLastError();
                                    }
                                    else
                                    {
                                        glBindTexture(texture.TextureTarget, texture.Texture);
                                        CheckLastError();

                                        if (texture.TextureTarget == TextureTarget.Texture2DArray
                                            || texture.TextureTarget == TextureTarget.Texture2DMultisampleArray
                                            || texture.TextureTarget == TextureTarget.TextureCubeMapArray)
                                        {
                                            // We only want a single subresource (array slice), so we need to copy
                                            // a subsection of the downloaded data into our staging block.

                                            uint fullDataSize = subresourceSize * texture.ArrayLayers;
                                            FixedStagingBlock fullBlock
                                                = _gd._stagingMemoryPool.GetFixedStagingBlock(fullDataSize);

                                            glGetTexImage(
                                                texture.TextureTarget,
                                                (int)mipLevel,
                                                texture.GLPixelFormat,
                                                texture.GLPixelType,
                                                fullBlock.Data);
                                            CheckLastError();
                                            byte* sliceStart = (byte*)fullBlock.Data + (arrayLayer * subresourceSize);
                                            Buffer.MemoryCopy(sliceStart, block.Data, subresourceSize, subresourceSize);

                                            fullBlock.Free();
                                        }
                                        else
                                        {
                                            glGetTexImage(
                                                texture.TextureTarget,
                                                (int)mipLevel,
                                                texture.GLPixelFormat,
                                                texture.GLPixelType,
                                                block.Data);
                                            CheckLastError();
                                        }
                                    }
                                }
                                else // isCompressed
                                {
                                    if (_gd.Extensions.ARB_DirectStateAccess)
                                    {
                                        glGetCompressedTextureImage(
                                            texture.Texture,
                                            (int)mipLevel,
                                            block.SizeInBytes,
                                            block.Data);
                                        CheckLastError();
                                    }
                                    else
                                    {
                                        if (texture.TextureTarget == TextureTarget.Texture2DArray
                                            || texture.TextureTarget == TextureTarget.Texture2DMultisampleArray
                                            || texture.TextureTarget == TextureTarget.TextureCubeMapArray)
                                        {
                                            throw new VeldridException(
                                                $"Mapping an OpenGL compressed array Texture requires ARB_DirectStateAccess.");
                                        }

                                        glBindTexture(texture.TextureTarget, texture.Texture);
                                        CheckLastError();

                                        glGetCompressedTexImage(texture.TextureTarget, (int)mipLevel, block.Data);
                                        CheckLastError();
                                    }
                                }
                            }

                            if (packAlignment < 4)
                            {
                                glPixelStorei(PixelStoreParameter.PackAlignment, 4);
                                CheckLastError();
                            }

                            uint rowPitch = FormatHelpers.GetRowPitch(mipWidth, texture.Format);
                            uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, mipHeight, texture.Format);
                            MappedResourceInfoWithStaging info = new MappedResourceInfoWithStaging();
                            info.MappedResource = new MappedResource(
                                resource,
                                mode,
                                (IntPtr)block.Data,
                                subresourceSize,
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
                    throw;
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

                mre.Dispose();
            }

            internal void Unmap(MappableResource resource, uint subresource)
            {
                CheckExceptions();

                ManualResetEventSlim mre = new ManualResetEventSlim(false);
                _workItems.Add(new ExecutionThreadWorkItem(resource, 0, subresource, false, mre));
                mre.Wait();
                mre.Dispose();
            }

            public void ExecuteCommands(OpenGLCommandEntryList entryList)
            {
                CheckExceptions();
                entryList.Parent.OnSubmitted(entryList);
                _workItems.Add(new ExecutionThreadWorkItem(entryList));
            }

            internal void UpdateBuffer(DeviceBuffer buffer, uint offsetInBytes, StagingBlock stagingBlock)
            {
                CheckExceptions();

                _workItems.Add(new ExecutionThreadWorkItem(buffer, offsetInBytes, stagingBlock));
            }

            internal void Run(Action a)
            {
                CheckExceptions();

                _workItems.Add(new ExecutionThreadWorkItem(a));
            }

            internal void Terminate()
            {
                CheckExceptions();

                _workItems.Add(new ExecutionThreadWorkItem(WorkItemType.TerminateAction));
            }

            internal void WaitForIdle()
            {
                ManualResetEventSlim mre = new ManualResetEventSlim();
                _workItems.Add(new ExecutionThreadWorkItem(mre));
                mre.Wait();
                mre.Dispose();

                CheckExceptions();
            }

            internal void SetSyncToVerticalBlank(bool value)
            {
                _workItems.Add(new ExecutionThreadWorkItem(value));
            }

            internal void SwapBuffers()
            {
                _workItems.Add(new ExecutionThreadWorkItem(WorkItemType.SwapBuffers));
            }
        }

        public enum WorkItemType : byte
        {
            Map,
            Unmap,
            ExecuteList,
            UpdateBuffer,
            UpdateTexture,
            GenericAction,
            TerminateAction,
            SignalResetEvent,
            SetSyncToVerticalBlank,
            SwapBuffers,
        }

        private unsafe struct ExecutionThreadWorkItem
        {
            public readonly WorkItemType Type;
            public readonly object Object0;
            public readonly object Object1;
            public readonly object Object2;
            public readonly uint UInt0;
            public readonly uint UInt1;
            public readonly uint UInt2; // TODO: Technically, our max data size could fit into just two UInt32's.

            public ExecutionThreadWorkItem(
                MappableResource resource,
                MapMode mapMode,
                uint subresource,
                bool map,
                ManualResetEventSlim resetEvent)
            {
                Type = WorkItemType.Map;
                Object0 = resource;
                Object1 = resetEvent;
                Object2 = null;

                UInt0 = (uint)mapMode;
                UInt1 = subresource;
                UInt2 = map ? 1u : 0u;
            }

            public ExecutionThreadWorkItem(OpenGLCommandEntryList commandList)
            {
                Type = WorkItemType.ExecuteList;
                Object0 = commandList;
                Object1 = null;
                Object2 = null;

                UInt0 = 0;
                UInt1 = 0;
                UInt2 = 0;
            }

            public ExecutionThreadWorkItem(DeviceBuffer updateBuffer, uint offsetInBytes, StagingBlock stagedSource)
            {
                Type = WorkItemType.UpdateBuffer;
                Object0 = updateBuffer;
                Object1 = stagedSource.Array;
                Object2 = stagedSource.Pool;

                UInt0 = offsetInBytes;
                UInt1 = stagedSource.SizeInBytes;
                UInt2 = 0;
            }

            public ExecutionThreadWorkItem(Action a, bool isTermination = false)
            {
                Type = isTermination ? WorkItemType.TerminateAction : WorkItemType.GenericAction;
                Object0 = a;
                Object1 = null;
                Object2 = null;

                UInt0 = 0;
                UInt1 = 0;
                UInt2 = 0;
            }

            public ExecutionThreadWorkItem(ManualResetEventSlim mre)
            {
                Type = WorkItemType.SignalResetEvent;
                Object0 = mre;
                Object1 = null;
                Object2 = null;

                UInt0 = 0;
                UInt1 = 0;
                UInt2 = 0;
            }

            public ExecutionThreadWorkItem(bool value)
            {
                Type = WorkItemType.SetSyncToVerticalBlank;
                Object0 = null;
                Object1 = null;
                Object2 = null;

                UInt0 = value ? 1u : 0u;
                UInt1 = 0;
                UInt2 = 0;
            }

            public ExecutionThreadWorkItem(WorkItemType type)
            {
                Type = type;
                Object0 = null;
                Object1 = null;
                Object2 = null;

                UInt0 = 0;
                UInt1 = 0;
                UInt2 = 0;

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
