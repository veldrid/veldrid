using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Veldrid.D3D11
{
    internal class D3D11GraphicsDevice : GraphicsDevice
    {
        private readonly SharpDX.Direct3D11.Device _device;
        private readonly DeviceContext _immediateContext;
        private readonly D3D11ResourceFactory _d3d11ResourceFactory;
        private readonly SwapChain _swapChain;
        private D3D11Framebuffer _swapChainFramebuffer;
        private readonly bool _supportsConcurrentResources;
        private readonly bool _supportsCommandLists;
        private readonly object _immediateContextLock = new object();

        private readonly object _mappedResourceLock = new object();
        private readonly Dictionary<MappedResourceCacheKey, MappedResourceInfo> _mappedResources
            = new Dictionary<MappedResourceCacheKey, MappedResourceInfo>();

        private bool _syncToVBlank;
        private int _syncInterval;
        private readonly Format? _depthFormat;
        private readonly float _pixelScale = 1f;

        public override GraphicsBackend BackendType => GraphicsBackend.Direct3D11;

        public override ResourceFactory ResourceFactory => _d3d11ResourceFactory;

        public override Framebuffer SwapchainFramebuffer => _swapChainFramebuffer;

        public SharpDX.Direct3D11.Device Device => _device;

        public bool SupportsConcurrentResources => _supportsConcurrentResources;

        public bool SupportsCommandLists => _supportsCommandLists;

        public List<D3D11CommandList> CommandListsReferencingSwapchain { get; internal set; } = new List<D3D11CommandList>();

        public override bool SyncToVerticalBlank
        {
            get => _syncToVBlank;
            set
            {
                _syncToVBlank = value;
                _syncInterval = GetSyncInterval(_syncToVBlank);
            }
        }

        public D3D11GraphicsDevice(GraphicsDeviceOptions options, IntPtr hwnd, int width, int height)
        {
            SyncToVerticalBlank = options.SyncToVerticalBlank;
            _depthFormat = options.SwapchainDepthFormat.HasValue
                ? D3D11Formats.GetDepthFormat(options.SwapchainDepthFormat.Value)
                : (Format?)null;
            SwapChainDescription swapChainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                IsWindowed = true,
                ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = hwnd,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
#if DEBUG
            DeviceCreationFlags creationFlags = DeviceCreationFlags.Debug;
#else
            DeviceCreationFlags creationFlags = options.Debug ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;
#endif 
            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                creationFlags,
                swapChainDescription,
                out _device,
                out _swapChain);
            _immediateContext = _device.ImmediateContext;
            _device.CheckThreadingSupport(out _supportsConcurrentResources, out _supportsCommandLists);

            Factory factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAll);
            factory.Dispose();

            _d3d11ResourceFactory = new D3D11ResourceFactory(this);
            RecreateSwapchainFramebuffer(width, height);

            PostDeviceCreated();
        }

        public D3D11GraphicsDevice(
            GraphicsDeviceOptions options,
            object swapChainPanel,
            double renderWidth,
            double renderHeight,
            float logicalDpi)
        {
            SyncToVerticalBlank = options.SyncToVerticalBlank;
            _depthFormat = options.SwapchainDepthFormat.HasValue
                ? D3D11Formats.GetDepthFormat(options.SwapchainDepthFormat.Value)
                : (Format?)null;

#if DEBUG
            DeviceCreationFlags creationFlags = DeviceCreationFlags.Debug;
#else
            DeviceCreationFlags creationFlags = options.Debug ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;
#endif 

            using (SharpDX.Direct3D11.Device defaultDevice = new SharpDX.Direct3D11.Device(
                SharpDX.Direct3D.DriverType.Hardware,
                creationFlags))
            {
                _device = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device2>();
            }

            _pixelScale = logicalDpi / 96.0f;

            int width = (int)(renderWidth * _pixelScale);
            int height = (int)(renderHeight * _pixelScale);

            // Properties of the swap chain
            SwapChainDescription1 swapChainDescription = new SwapChainDescription1()
            {
                AlphaMode = AlphaMode.Ignore,
                BufferCount = 2,
                Format = Format.B8G8R8A8_UNorm,
                Height = width,
                Width = height,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.FlipSequential,
                Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
            };

            // Retrive the SharpDX.DXGI device associated to the Direct3D device.
            using (SharpDX.DXGI.Device3 dxgiDevice = _device.QueryInterface<SharpDX.DXGI.Device3>())
            {
                // Get the SharpDX.DXGI factory automatically created when initializing the Direct3D device.
                using (Factory2 dxgiFactory = dxgiDevice.Adapter.GetParent<Factory2>())
                {
                    // Create the swap chain and get the highest version available.
                    using (SwapChain1 swapChain1 = new SwapChain1(dxgiFactory, _device, ref swapChainDescription))
                    {
                        _swapChain = swapChain1.QueryInterface<SwapChain2>();
                    }
                }
            }

            ComObject co = new ComObject(swapChainPanel);

            ISwapChainPanelNative swapchainPanelNative = co.QueryInterfaceOrNull<ISwapChainPanelNative>();
            if (swapchainPanelNative != null)
            {
                swapchainPanelNative.SwapChain = _swapChain;
            }
            else
            {
                ISwapChainBackgroundPanelNative bgPanelNative = co.QueryInterfaceOrNull<ISwapChainBackgroundPanelNative>();
                if (bgPanelNative != null)
                {
                    bgPanelNative.SwapChain = _swapChain;
                }
            }

            _immediateContext = _device.ImmediateContext;
            _device.CheckThreadingSupport(out _supportsConcurrentResources, out _supportsCommandLists);

            _d3d11ResourceFactory = new D3D11ResourceFactory(this);
            RecreateSwapchainFramebuffer(width, height);

            PostDeviceCreated();
        }

        public override void ResizeMainWindow(uint width, uint height)
        {
            RecreateSwapchainFramebuffer((int)width, (int)height);
        }

        private void RecreateSwapchainFramebuffer(int width, int height)
        {
            // NOTE: Perhaps this should be deferred until all CommandLists naturally remove their references to the swapchain.
            // The actual resize could be done in ExecuteCommands() when it is found that this list is empty.
            foreach (D3D11CommandList d3dCL in CommandListsReferencingSwapchain)
            {
                d3dCL.Reset();
            }

            if (_swapChainFramebuffer != null)
            {
                if (_swapChainFramebuffer.DepthTarget.HasValue)
                {
                    _swapChainFramebuffer.DepthTarget.Value.Target.Dispose();
                }
                _swapChainFramebuffer.Dispose();
            }


            int actualWidth = (int)(width * _pixelScale);
            int actualHeight = (int)(height * _pixelScale);
            _swapChain.ResizeBuffers(2, actualWidth, actualHeight, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            using (Texture2D backBufferTexture = _swapChain.GetBackBuffer<Texture2D>(0))
            {
                Texture2D depthBufferTexture = null;
                if (_depthFormat != null)
                {
                    depthBufferTexture = new Texture2D(
                        _device,
                        new Texture2DDescription()
                        {
                            Format = _depthFormat.Value,
                            ArraySize = 1,
                            MipLevels = 1,
                            Width = backBufferTexture.Description.Width,
                            Height = backBufferTexture.Description.Height,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = ResourceUsage.Default,
                            BindFlags = BindFlags.DepthStencil,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None
                        });
                }

                D3D11Texture backBufferVdTexture = new D3D11Texture(backBufferTexture);
                D3D11Texture depthVdTexture = depthBufferTexture != null
                    ? new D3D11Texture(depthBufferTexture)
                    : null;
                FramebufferDescription desc = new FramebufferDescription(depthVdTexture, backBufferVdTexture);
                _swapChainFramebuffer = new D3D11Framebuffer(_device, ref desc);
                _swapChainFramebuffer.IsSwapchainFramebuffer = true;
            }
        }

        protected override void SubmitCommandsCore(CommandList cl, Fence fence)
        {
            D3D11CommandList d3d11CL = Util.AssertSubtype<CommandList, D3D11CommandList>(cl);
            lock (_immediateContextLock)
            {
                _immediateContext.ExecuteCommandList(d3d11CL.DeviceCommandList, false);
            }
            d3d11CL.DeviceCommandList.Dispose();
            d3d11CL.DeviceCommandList = null;
            CommandListsReferencingSwapchain.Remove(d3d11CL);

            if (fence is D3D11Fence d3d11Fence)
            {
                d3d11Fence.Set();
            }
        }

        protected override void SwapBuffersCore()
        {
            _swapChain.Present(_syncInterval, PresentFlags.None);
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            Format dxgiFormat = D3D11Formats.ToDxgiFormat(format, depthFormat);
            if (CheckFormat(dxgiFormat, 32))
            {
                return TextureSampleCount.Count32;
            }
            else if (CheckFormat(dxgiFormat, 16))
            {
                return TextureSampleCount.Count16;
            }
            else if (CheckFormat(dxgiFormat, 8))
            {
                return TextureSampleCount.Count8;
            }
            else if (CheckFormat(dxgiFormat, 4))
            {
                return TextureSampleCount.Count4;
            }
            else if (CheckFormat(dxgiFormat, 2))
            {
                return TextureSampleCount.Count2;
            }

            return TextureSampleCount.Count1;
        }

        private bool CheckFormat(Format format, int sampleCount)
        {
            return _device.CheckMultisampleQualityLevels(format, sampleCount) != 0;
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            MappedResourceCacheKey key = new MappedResourceCacheKey(resource, subresource);
            lock (_mappedResourceLock)
            {
                if (_mappedResources.TryGetValue(key, out MappedResourceInfo info))
                {
                    if (info.Mode != mode)
                    {
                        throw new VeldridException("The given resource was already mapped with a different MapMode.");
                    }

                    info.RefCount += 1;
                    _mappedResources[key] = info;
                }
                else
                {
                    // No current mapping exists -- create one.

                    if (resource is D3D11Buffer buffer)
                    {
                        lock (_immediateContextLock)
                        {
                            DataBox db = _immediateContext.MapSubresource(
                                buffer.Buffer,
                                0,
                                D3D11Formats.VdToD3D11MapMode((buffer.Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic, mode),
                                SharpDX.Direct3D11.MapFlags.None);

                            info.MappedResource = new MappedResource(resource, mode, db.DataPointer, buffer.SizeInBytes);
                            info.RefCount = 1;
                            info.Mode = mode;
                            _mappedResources.Add(key, info);
                        }
                    }
                    else
                    {
                        D3D11Texture texture = Util.AssertSubtype<MappableResource, D3D11Texture>(resource);
                        lock (_immediateContextLock)
                        {
                            DataBox db = _immediateContext.MapSubresource(
                                texture.DeviceTexture,
                                (int)subresource,
                                D3D11Formats.VdToD3D11MapMode(false, mode),
                                SharpDX.Direct3D11.MapFlags.None,
                                out DataStream ds);

                            info.MappedResource = new MappedResource(
                                resource,
                                mode,
                                db.DataPointer,
                                (uint)ds.Length,
                                subresource,
                                (uint)db.RowPitch,
                                (uint)db.SlicePitch);
                            info.RefCount = 1;
                            info.Mode = mode;
                            _mappedResources.Add(key, info);
                        }
                    }
                }

                return info.MappedResource;
            }
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
            MappedResourceCacheKey key = new MappedResourceCacheKey(resource, subresource);
            bool commitUnmap;

            lock (_mappedResourceLock)
            {
                if (!_mappedResources.TryGetValue(key, out MappedResourceInfo info))
                {
                    throw new VeldridException($"The given resource ({resource}) is not mapped.");
                }

                info.RefCount -= 1;
                commitUnmap = info.RefCount == 0;
                if (commitUnmap)
                {
                    lock (_immediateContextLock)
                    {
                        if (resource is D3D11Buffer buffer)
                        {
                            _immediateContext.UnmapSubresource(buffer.Buffer, 0);
                        }
                        else
                        {
                            D3D11Texture texture = Util.AssertSubtype<MappableResource, D3D11Texture>(resource);
                            _immediateContext.UnmapSubresource(texture.DeviceTexture, (int)subresource);
                        }

                        bool result = _mappedResources.Remove(key);
                        Debug.Assert(result);
                    }
                }
            }
        }

        protected unsafe override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            D3D11Buffer d3dBuffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(buffer);
            if (sizeInBytes == 0)
            {
                return;
            }

            bool useMap = (buffer.Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic
                || (buffer.Usage & BufferUsage.Staging) == BufferUsage.Staging;

            if (useMap)
            {
                if (bufferOffsetInBytes != 0)
                {
                    throw new NotImplementedException("bufferOffsetInBytes must be 0 for Dynamic Buffers.");
                }

                MappedResource mr = MapCore(buffer, MapMode.Write, 0);
                if (sizeInBytes < 1024)
                {
                    Unsafe.CopyBlock(mr.Data.ToPointer(), source.ToPointer(), sizeInBytes);
                }
                else
                {
                    System.Buffer.MemoryCopy(source.ToPointer(), mr.Data.ToPointer(), buffer.SizeInBytes, sizeInBytes);
                }
                UnmapCore(buffer, 0);
            }
            else
            {
                ResourceRegion? subregion = null;
                if ((d3dBuffer.Buffer.Description.BindFlags & BindFlags.ConstantBuffer) != BindFlags.ConstantBuffer)
                {
                    // For a shader-constant buffer; set pDstBox to null. It is not possible to use
                    // this method to partially update a shader-constant buffer

                    subregion = new ResourceRegion()
                    {
                        Left = (int)bufferOffsetInBytes,
                        Right = (int)(sizeInBytes + bufferOffsetInBytes),
                        Bottom = 1,
                        Back = 1
                    };
                }
                lock (_immediateContextLock)
                {
                    _immediateContext.UpdateSubresource(d3dBuffer.Buffer, 0, subregion, source, 0, 0);
                }
            }
        }

        protected unsafe override void UpdateTextureCore(
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
            D3D11Texture d3dTex = Util.AssertSubtype<Texture, D3D11Texture>(texture);
            bool useMap = (texture.Usage & TextureUsage.Staging) == TextureUsage.Staging;
            if (useMap)
            {
                uint subresource = texture.CalculateSubresource(mipLevel, arrayLayer);
                MappedResourceCacheKey key = new MappedResourceCacheKey(texture, subresource);
                MappedResource map = MapCore(texture, MapMode.Write, subresource);

                uint denseRowSize = FormatHelpers.GetRowPitch(width, texture.Format);
                uint denseSliceSize = FormatHelpers.GetDepthPitch(denseRowSize, height, texture.Format);

                Util.CopyTextureRegion(
                    source.ToPointer(),
                    0, 0, 0,
                    denseRowSize, denseSliceSize,
                    map.Data.ToPointer(),
                    x, y, z,
                    map.RowPitch, map.DepthPitch,
                    width, height, depth,
                    texture.Format);

                UnmapCore(texture, subresource);
            }
            else
            {
                int subresource = D3D11Util.ComputeSubresource(mipLevel, texture.MipLevels, arrayLayer);
                ResourceRegion resourceRegion = new ResourceRegion(
                    left: (int)x,
                    right: (int)(x + width),
                    top: (int)y,
                    front: (int)z,
                    bottom: (int)(y + height),
                    back: (int)(z + depth));
                uint srcRowPitch = FormatHelpers.GetSizeInBytes(texture.Format) * width;
                uint srcDepthPitch = srcRowPitch * depth;
                lock (_immediateContextLock)
                {
                    _immediateContext.UpdateSubresource(
                        d3dTex.DeviceTexture,
                        subresource,
                        resourceRegion,
                        source,
                        (int)srcRowPitch,
                        (int)srcDepthPitch);
                }
            }
        }

        public override bool WaitForFence(Fence fence, ulong nanosecondTimeout)
        {
            return Util.AssertSubtype<Fence, D3D11Fence>(fence).Wait(nanosecondTimeout);
        }

        public override bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout)
        {
            int msTimeout = (int)(nanosecondTimeout / 1_000_000);
            ManualResetEvent[] events = GetResetEventArray(fences.Length);
            for (int i = 0; i < fences.Length; i++)
            {
                events[i] = Util.AssertSubtype<Fence, D3D11Fence>(fences[i]).ResetEvent;
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
            Util.AssertSubtype<Fence, D3D11Fence>(fence).Reset();
        }

        private static int GetSyncInterval(bool syncToVBlank)
        {
            return syncToVBlank ? 1 : 0;
        }

        protected override void PlatformDispose()
        {
            _d3d11ResourceFactory.Dispose();
            if (_swapChainFramebuffer.DepthTarget.HasValue)
            {
                _swapChainFramebuffer.DepthTarget.Value.Target.Dispose();
            }
            _swapChainFramebuffer.Dispose();
            _swapChain.Dispose();
            _immediateContext.Dispose();

            DeviceDebug deviceDebug = _device.QueryInterfaceOrNull<DeviceDebug>();

            _device.Dispose();

            if (deviceDebug != null)
            {
                deviceDebug.ReportLiveDeviceObjects(ReportingLevel.Summary);
                deviceDebug.ReportLiveDeviceObjects(ReportingLevel.Detail);
                deviceDebug.Dispose();
            }
        }

        protected override void WaitForIdleCore()
        {
        }
    }
}
