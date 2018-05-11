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
        private readonly D3D11Swapchain _mainSwapchain;
        private readonly bool _supportsConcurrentResources;
        private readonly bool _supportsCommandLists;
        private readonly ConditionalLock _immediateContextLock = new ConditionalLock();
        private readonly bool _multiThreaded;

        private readonly ConditionalLock _mappedResourceLock = new ConditionalLock();
        private readonly Dictionary<MappedResourceCacheKey, MappedResourceInfo> _mappedResources
            = new Dictionary<MappedResourceCacheKey, MappedResourceInfo>();

        private readonly ConditionalLock _stagingResourcesLock = new ConditionalLock();
        private readonly List<D3D11Buffer> _availableStagingBuffers = new List<D3D11Buffer>();

        private readonly ConditionalLock _resetEventsLock = new ConditionalLock();
        private readonly List<ManualResetEvent[]> _resetEvents = new List<ManualResetEvent[]>();
        private readonly D3D11CommandList _immediateCL;

        public override GraphicsBackend BackendType => GraphicsBackend.Direct3D11;

        public override bool IsUvOriginTopLeft => true;

        public override ResourceFactory ResourceFactory => _d3d11ResourceFactory;

        public SharpDX.Direct3D11.Device Device => _device;

        public bool SupportsConcurrentResources => _supportsConcurrentResources;

        public bool SupportsCommandLists => _supportsCommandLists;

        public override Swapchain MainSwapchain => _mainSwapchain;

        public override GraphicsDeviceFeatures Features { get; }

        protected override CommandList GetImmediateCommandListCore() => _immediateCL;

        public D3D11GraphicsDevice(GraphicsDeviceOptions options, SwapchainDescription? swapchainDesc)
            : base(options)
        {
            _multiThreaded = !options.SingleThreaded;
#if DEBUG
            DeviceCreationFlags creationFlags = DeviceCreationFlags.Debug;
#else
            DeviceCreationFlags creationFlags = options.Debug ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;
#endif 
            _device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, creationFlags);
            if (swapchainDesc != null)
            {
                SwapchainDescription desc = swapchainDesc.Value;
                _mainSwapchain = new D3D11Swapchain(_device, ref desc);
            }
            _immediateContext = _device.ImmediateContext;
            _device.CheckThreadingSupport(out _supportsConcurrentResources, out _supportsCommandLists);

            Features = new GraphicsDeviceFeatures(
                computeShader: true,
                geometryShader: true,
                tessellationShaders: true,
                multipleViewports: true,
                samplerLodBias: true,
                drawBaseVertex: true,
                drawBaseInstance: true,
                drawIndirect: true,
                drawIndirectBaseInstance: true,
                fillModeWireframe: true,
                samplerAnisotropy: true,
                depthClipDisable: true,
                texture1D: true,
                independentBlend: true);

            _d3d11ResourceFactory = new D3D11ResourceFactory(this);

            if (!_multiThreaded)
            {
                CommandListDescription clDesc;
                _immediateCL = new D3D11CommandList(this, ref clDesc, _immediateContext);
            }

            PostDeviceCreated();
        }

        protected override void SubmitCommandsCore(CommandList cl, Fence fence)
        {
            D3D11CommandList d3d11CL = Util.AssertSubtype<CommandList, D3D11CommandList>(cl);

            if (!d3d11CL.IsImmediate)
            {
                using (_immediateContextLock.Lock(_multiThreaded))
                {
                    if (d3d11CL.DeviceCommandList != null) // CommandList may have been reset in the meantime (resized swapchain).
                    {
                        _immediateContext.ExecuteCommandList(d3d11CL.DeviceCommandList, false);
                        d3d11CL.OnCompleted();
                    }
                }

                _immediateCL.Reset();
            }

            if (fence is D3D11Fence d3d11Fence)
            {
                d3d11Fence.Set();
            }
        }

        protected override void SwapBuffersCore(Swapchain swapchain)
        {
            using (_immediateContextLock.Lock(_multiThreaded))
            {
                D3D11Swapchain d3d11SC = Util.AssertSubtype<Swapchain, D3D11Swapchain>(swapchain);
                d3d11SC.DxgiSwapChain.Present(d3d11SC.SyncInterval, PresentFlags.None);
            }
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            Format dxgiFormat = D3D11Formats.ToDxgiFormat(format, depthFormat);
            if (CheckFormatMultisample(dxgiFormat, 32))
            {
                return TextureSampleCount.Count32;
            }
            else if (CheckFormatMultisample(dxgiFormat, 16))
            {
                return TextureSampleCount.Count16;
            }
            else if (CheckFormatMultisample(dxgiFormat, 8))
            {
                return TextureSampleCount.Count8;
            }
            else if (CheckFormatMultisample(dxgiFormat, 4))
            {
                return TextureSampleCount.Count4;
            }
            else if (CheckFormatMultisample(dxgiFormat, 2))
            {
                return TextureSampleCount.Count2;
            }

            return TextureSampleCount.Count1;
        }

        private bool CheckFormatMultisample(Format format, int sampleCount)
        {
            return _device.CheckMultisampleQualityLevels(format, sampleCount) != 0;
        }

        protected override bool GetPixelFormatSupportCore(
            PixelFormat format,
            TextureType type,
            TextureUsage usage,
            out PixelFormatProperties properties)
        {
            if (D3D11Formats.IsUnsupportedFormat(format))
            {
                properties = default(PixelFormatProperties);
                return false;
            }

            Format dxgiFormat = D3D11Formats.ToDxgiFormat(format, (usage & TextureUsage.DepthStencil) != 0);
            FormatSupport fs = _device.CheckFormatSupport(dxgiFormat);

            if ((usage & TextureUsage.RenderTarget) != 0 && (fs & FormatSupport.RenderTarget) == 0
                || (usage & TextureUsage.DepthStencil) != 0 && (fs & FormatSupport.DepthStencil) == 0
                || (usage & TextureUsage.Sampled) != 0 && (fs & FormatSupport.ShaderSample) == 0
                || (usage & TextureUsage.Cubemap) != 0 && (fs & FormatSupport.TextureCube) == 0
                || (usage & TextureUsage.Storage) != 0 && (fs & FormatSupport.TypedUnorderedAccessView) == 0)
            {
                properties = default(PixelFormatProperties);
                return false;
            }

            const uint MaxTextureDimension = 16384;
            const uint MaxVolumeExtent = 2048;

            uint sampleCounts = 0;
            if (CheckFormatMultisample(dxgiFormat, 1)) { sampleCounts |= (1 << 0); }
            if (CheckFormatMultisample(dxgiFormat, 2)) { sampleCounts |= (1 << 1); }
            if (CheckFormatMultisample(dxgiFormat, 4)) { sampleCounts |= (1 << 2); }
            if (CheckFormatMultisample(dxgiFormat, 8)) { sampleCounts |= (1 << 3); }
            if (CheckFormatMultisample(dxgiFormat, 16)) { sampleCounts |= (1 << 4); }
            if (CheckFormatMultisample(dxgiFormat, 32)) { sampleCounts |= (1 << 5); }

            properties = new PixelFormatProperties(
                MaxTextureDimension,
                type == TextureType.Texture1D ? 1 : MaxTextureDimension,
                type != TextureType.Texture3D ? 1 : MaxVolumeExtent,
                uint.MaxValue,
                type == TextureType.Texture3D ? 1 : MaxVolumeExtent,
                sampleCounts);
            return true;
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            MappedResourceCacheKey key = new MappedResourceCacheKey(resource, subresource);
            using (_mappedResourceLock.Lock(_multiThreaded))
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
                        using (_immediateContextLock.Lock(_multiThreaded))
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
                        using (_immediateContextLock.Lock(_multiThreaded))
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

            using (_mappedResourceLock.Lock(_multiThreaded))
            {
                if (!_mappedResources.TryGetValue(key, out MappedResourceInfo info))
                {
                    throw new VeldridException($"The given resource ({resource}) is not mapped.");
                }

                info.RefCount -= 1;
                commitUnmap = info.RefCount == 0;
                if (commitUnmap)
                {
                    using (_immediateContextLock.Lock(_multiThreaded))
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

            bool isDynamic = (buffer.Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic;
            bool isStaging = (buffer.Usage & BufferUsage.Staging) == BufferUsage.Staging;
            bool isUniformBuffer = (buffer.Usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer;
            bool updateFullBuffer = bufferOffsetInBytes == 0 && sizeInBytes == buffer.SizeInBytes;
            bool useUpdateSubresource = (!isDynamic && !isStaging) && (!isUniformBuffer || updateFullBuffer);
            bool useMap = (isDynamic && updateFullBuffer) || isStaging;

            if (useUpdateSubresource)
            {
                ResourceRegion? subregion = new ResourceRegion()
                {
                    Left = (int)bufferOffsetInBytes,
                    Right = (int)(sizeInBytes + bufferOffsetInBytes),
                    Bottom = 1,
                    Back = 1
                };

                if (isUniformBuffer)
                {
                    subregion = null;
                }

                using (_immediateContextLock.Lock(_multiThreaded))
                {
                    _immediateContext.UpdateSubresource(d3dBuffer.Buffer, 0, subregion, source, 0, 0);
                }
            }
            else if (useMap)
            {
                MappedResource mr = MapCore(buffer, MapMode.Write, 0);
                if (sizeInBytes < 1024)
                {
                    Unsafe.CopyBlock((byte*)mr.Data + bufferOffsetInBytes, source.ToPointer(), sizeInBytes);
                }
                else
                {
                    System.Buffer.MemoryCopy(
                        source.ToPointer(),
                        (byte*)mr.Data + bufferOffsetInBytes,
                        buffer.SizeInBytes,
                        sizeInBytes);
                }
                UnmapCore(buffer, 0);
            }
            else
            {
                D3D11Buffer staging = GetFreeStagingBuffer(sizeInBytes);
                UpdateBuffer(staging, 0, source, sizeInBytes);
                ResourceRegion sourceRegion = new ResourceRegion(0, 0, 0, (int)sizeInBytes, 1, 1);
                using (_immediateContextLock.Lock(_multiThreaded))
                {
                    _immediateContext.CopySubresourceRegion(
                        staging.Buffer, 0, sourceRegion,
                        d3dBuffer.Buffer, 0,
                        (int)bufferOffsetInBytes, 0, 0);
                }

                using (_stagingResourcesLock.Lock(_multiThreaded))
                {
                    _availableStagingBuffers.Add(staging);
                }
            }
        }

        private D3D11Buffer GetFreeStagingBuffer(uint sizeInBytes)
        {
            using (_stagingResourcesLock.Lock(_multiThreaded))
            {
                foreach (D3D11Buffer buffer in _availableStagingBuffers)
                {
                    if (buffer.SizeInBytes >= sizeInBytes)
                    {
                        _availableStagingBuffers.Remove(buffer);
                        return buffer;
                    }
                }
            }

            DeviceBuffer staging = ResourceFactory.CreateBuffer(
                new BufferDescription(sizeInBytes, BufferUsage.Staging));

            return Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(staging);
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
                using (_immediateContextLock.Lock(_multiThreaded))
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

        private ManualResetEvent[] GetResetEventArray(int length)
        {
            using (_resetEventsLock.Lock(_multiThreaded))
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
            using (_resetEventsLock.Lock(_multiThreaded))
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
            _immediateCL?.Dispose();
            _d3d11ResourceFactory.Dispose();
            _mainSwapchain?.Dispose();
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
