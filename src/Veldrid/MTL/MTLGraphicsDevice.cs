using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal unsafe class MTLGraphicsDevice : GraphicsDevice
    {
        private static readonly Lazy<bool> s_isSupported = new Lazy<bool>(GetIsSupported);

        private readonly MTLDevice _device;
        private readonly MTLCommandQueue _commandQueue;
        private readonly MTLSwapchain _mainSwapchain;
        private readonly TextureSampleCount _maxSampleCount;
        private readonly bool _multiThreaded;
        private readonly MTLCommandList _immediateCL;
        private readonly ConditionalLock _submittedCommandsLock = new ConditionalLock();
        private readonly List<(MTLCommandBuffer, MTLFence)> _submittedCBs = new List<(MTLCommandBuffer, MTLFence)>();

        private readonly ConditionalLock _resetEventsLock = new ConditionalLock();
        private readonly List<ManualResetEvent[]> _resetEvents = new List<ManualResetEvent[]>();

        private const string UnalignedBufferCopyPipelineMacOSName = "MTL_UnalignedBufferCopy_macOS";
        private const string UnalignedBufferCopyPipelineiOSName = "MTL_UnalignedBufferCopy_iOS";
        private readonly ConditionalLock _unalignedBufferCopyPipelineLock = new ConditionalLock();
        private MTLShader _unalignedBufferCopyShader;
        private MTLComputePipelineState _unalignedBufferCopyPipeline;

        public MTLDevice Device => _device;
        public MTLCommandQueue CommandQueue => _commandQueue;
        public MTLFeatureSupport MetalFeatures { get; }
        public ResourceBindingModel ResourceBindingModel { get; }

        public MTLGraphicsDevice(
            GraphicsDeviceOptions options,
            SwapchainDescription? swapchainDesc) : base(options)
        {
            _device = MTLDevice.MTLCreateSystemDefaultDevice();
            MetalFeatures = new MTLFeatureSupport(_device);
            Features = new GraphicsDeviceFeatures(
                computeShader: true,
                geometryShader: false,
                tessellationShaders: false,
                multipleViewports: MetalFeatures.IsSupported(MTLFeatureSet.macOS_GPUFamily1_v3),
                samplerLodBias: false,
                drawBaseVertex: true,
                drawBaseInstance: true,
                drawIndirect: true,
                drawIndirectBaseInstance: true,
                fillModeWireframe: true,
                samplerAnisotropy: true,
                depthClipDisable: true,
                texture1D: true, // TODO: Should be macOS 10.11+ and iOS 11.0+.
                independentBlend: true);
            ResourceBindingModel = options.ResourceBindingModel;

            ResourceFactory = new MTLResourceFactory(this);
            _commandQueue = _device.newCommandQueue();

            foreach (TextureSampleCount count in (TextureSampleCount[])Enum.GetValues(typeof(TextureSampleCount)))
            {
                uint uintValue = FormatHelpers.GetSampleCountUInt32(count);
                if (_device.supportsTextureSampleCount((UIntPtr)uintValue))
                {
                    _maxSampleCount = count;
                }
            }

            if (swapchainDesc != null)
            {
                SwapchainDescription desc = swapchainDesc.Value;
                _mainSwapchain = new MTLSwapchain(this, ref desc);
            }

            _multiThreaded = !options.SingleThreaded;
            if (!_multiThreaded)
            {
                _immediateCL = Util.AssertSubtype<CommandList, MTLCommandList>(ResourceFactory.CreateCommandList());
            }

            PostDeviceCreated();
        }

        public override GraphicsBackend BackendType => GraphicsBackend.Metal;

        public override bool IsUvOriginTopLeft => true;

        public override ResourceFactory ResourceFactory { get; }

        public override Swapchain MainSwapchain => _mainSwapchain;

        public override GraphicsDeviceFeatures Features { get; }

        protected override CommandList GetImmediateCommandListCore() { return _immediateCL; }

        protected override void SubmitCommandsCore(CommandList commandList, Fence fence)
        {
            MTLCommandList mtlCL = Util.AssertSubtype<CommandList, MTLCommandList>(commandList);
            MTLCommandBuffer cb = mtlCL.Commit();
            using (_submittedCommandsLock.Lock(_multiThreaded))
            {
                CheckSubmittedCommands(assumeCompletion: false);

                MTLFence mtlFence = fence as MTLFence;
                _submittedCBs.Add((cb, mtlFence));
            }

            if (commandList != _immediateCL)
            {
                _immediateCL.Reset();
            }
        }

        private void CheckSubmittedCommands(bool assumeCompletion)
        {
            for (int i = 0; i < _submittedCBs.Count; i++)
            {
                (MTLCommandBuffer, MTLFence) pair = _submittedCBs[i];
                if (pair.Item1.status == MTLCommandBufferStatus.Completed)
                {
                    if (pair.Item2 != null)
                    {
                        pair.Item2.Set();
                    }

                    ObjectiveCRuntime.release(pair.Item1.NativePtr);
                    _submittedCBs.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            return _maxSampleCount;
        }

        protected override bool GetPixelFormatSupportCore(
            PixelFormat format,
            TextureType type,
            TextureUsage usage,
            out PixelFormatProperties properties)
        {
            if (!MTLFormats.IsFormatSupported(format, usage, MetalFeatures))
            {
                properties = default(PixelFormatProperties);
                return false;
            }

            uint sampleCounts = 0;
            int max = (int)_maxSampleCount + 1;
            for (int i = 0; i < max; i++)
            {
                sampleCounts |= (uint)(1 << i);
            }

            MTLFeatureSet maxFeatureSet = MetalFeatures.MaxFeatureSet;
            uint maxArrayLayer = MTLFormats.GetMaxTextureVolume(maxFeatureSet);
            uint maxWidth;
            uint maxHeight;
            uint maxDepth;
            if (type == TextureType.Texture1D)
            {
                maxWidth = MTLFormats.GetMaxTexture1DWidth(maxFeatureSet);
                maxHeight = 1;
                maxDepth = 1;
            }
            else if (type == TextureType.Texture2D)
            {
                uint maxDimensions;
                if ((usage & TextureUsage.Cubemap) != 0)
                {
                    maxDimensions = MTLFormats.GetMaxTextureCubeDimensions(maxFeatureSet);
                }
                else
                {
                    maxDimensions = MTLFormats.GetMaxTexture2DDimensions(maxFeatureSet);
                }

                maxWidth = maxDimensions;
                maxHeight = maxDimensions;
                maxDepth = 1;
            }
            else if (type == TextureType.Texture3D)
            {
                maxWidth = maxArrayLayer;
                maxHeight = maxArrayLayer;
                maxDepth = maxArrayLayer;
                maxArrayLayer = 1;
            }
            else
            {
                throw Illegal.Value<TextureType>();
            }

            properties = new PixelFormatProperties(
                maxWidth,
                maxHeight,
                maxDepth,
                uint.MaxValue,
                maxArrayLayer,
                sampleCounts);
            return true;
        }

        protected override void SwapBuffersCore(Swapchain swapchain)
        {
            MTLSwapchain mtlSC = Util.AssertSubtype<Swapchain, MTLSwapchain>(swapchain);
            IntPtr currentDrawablePtr = mtlSC.CurrentDrawable.NativePtr;
            if (currentDrawablePtr != IntPtr.Zero)
            {
                using (NSAutoreleasePool.Begin())
                {
                    MTLCommandBuffer submitCB = _commandQueue.commandBuffer();
                    submitCB.presentDrawable(currentDrawablePtr);
                    submitCB.commit();
                }
            }

            mtlSC.GetNextDrawable();
        }

        protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            var mtlBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(buffer);
            void* destPtr = mtlBuffer.DeviceBuffer.contents();
            byte* destOffsetPtr = (byte*)destPtr + bufferOffsetInBytes;
            Unsafe.CopyBlock(destOffsetPtr, source.ToPointer(), sizeInBytes);
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
            MTLTexture mtlTex = Util.AssertSubtype<Texture, MTLTexture>(texture);
            if (mtlTex.StagingBuffer.IsNull)
            {
                Texture stagingTex = ResourceFactory.CreateTexture(new TextureDescription(
                    width, height, depth, 1, 1, texture.Format, TextureUsage.Staging, texture.Type));
                UpdateTexture(stagingTex, source, sizeInBytes, 0, 0, 0, width, height, depth, 0, 0);
                CommandList cl = ResourceFactory.CreateCommandList();
                cl.Begin();
                cl.CopyTexture(
                    stagingTex, 0, 0, 0, 0, 0,
                    texture, x, y, z, mipLevel, arrayLayer,
                    width, height, depth, 1);
                cl.End();
                SubmitCommands(cl);

                cl.Dispose();
                stagingTex.Dispose();
            }
            else
            {
                mtlTex.GetSubresourceLayout(mipLevel, arrayLayer, out uint dstRowPitch, out uint dstDepthPitch);
                ulong dstOffset = Util.ComputeSubresourceOffset(mtlTex, mipLevel, arrayLayer);
                uint srcRowPitch = FormatHelpers.GetRowPitch(width, texture.Format);
                uint srcDepthPitch = FormatHelpers.GetDepthPitch(srcRowPitch, height, texture.Format);
                Util.CopyTextureRegion(
                    source.ToPointer(),
                    0, 0, 0,
                    srcRowPitch, srcDepthPitch,
                    (byte*)mtlTex.StagingBuffer.contents() + dstOffset,
                    x, y, z,
                    dstRowPitch, dstDepthPitch,
                    width, height, depth,
                    texture.Format);
            }
        }

        protected override void WaitForIdleCore()
        {
            using (_submittedCommandsLock.Lock(_multiThreaded))
            {
                int lastIndex = _submittedCBs.Count - 1;
                if (lastIndex >= 0)
                {
                    (MTLCommandBuffer, MTLFence) lastPair = _submittedCBs[lastIndex];
                    lastPair.Item1.waitUntilCompleted();
                    CheckSubmittedCommands(assumeCompletion: true);
                }
            }
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            if (resource is MTLBuffer buffer)
            {
                return MapBuffer(buffer, mode);
            }
            else
            {
                MTLTexture texture = Util.AssertSubtype<MappableResource, MTLTexture>(resource);
                return MapTexture(texture, mode, subresource);
            }
        }

        private MappedResource MapBuffer(MTLBuffer buffer, MapMode mode)
        {
            void* data = buffer.DeviceBuffer.contents();
            return new MappedResource(
                buffer,
                mode,
                (IntPtr)data,
                buffer.SizeInBytes,
                0,
                buffer.SizeInBytes,
                buffer.SizeInBytes);
        }

        private MappedResource MapTexture(MTLTexture texture, MapMode mode, uint subresource)
        {
            Debug.Assert(!texture.StagingBuffer.IsNull);
            void* data = texture.StagingBuffer.contents();
            Util.GetMipLevelAndArrayLayer(texture, subresource, out uint mipLevel, out uint arrayLayer);
            Util.GetMipDimensions(texture, mipLevel, out uint width, out uint height, out uint depth);
            uint subresourceSize = texture.GetSubresourceSize(mipLevel, arrayLayer);
            texture.GetSubresourceLayout(mipLevel, arrayLayer, out uint rowPitch, out uint depthPitch);
            ulong offset = Util.ComputeSubresourceOffset(texture, mipLevel, arrayLayer);
            byte* offsetPtr = (byte*)data + offset;
            return new MappedResource(texture, mode, (IntPtr)offsetPtr, subresourceSize, subresource, rowPitch, depthPitch);
        }

        protected override void PlatformDispose()
        {
            WaitForIdle();
            if (!_unalignedBufferCopyPipeline.IsNull)
            {
                _unalignedBufferCopyShader.Dispose();
                ObjectiveCRuntime.release(_unalignedBufferCopyPipeline.NativePtr);
            }
            _mainSwapchain?.Dispose();
            _immediateCL?.Dispose();
            ObjectiveCRuntime.release(_commandQueue.NativePtr);
            ObjectiveCRuntime.release(_device.NativePtr);
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
        }

        public override bool WaitForFence(Fence fence, ulong nanosecondTimeout)
        {
            return Util.AssertSubtype<Fence, MTLFence>(fence).Wait(nanosecondTimeout);
        }

        public override bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout)
        {
            int msTimeout = (int)(nanosecondTimeout / 1_000_000);
            ManualResetEvent[] events = GetResetEventArray(fences.Length);
            for (int i = 0; i < fences.Length; i++)
            {
                events[i] = Util.AssertSubtype<Fence, MTLFence>(fences[i]).ResetEvent;
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
            Util.AssertSubtype<Fence, MTLFence>(fence).Reset();
        }

        internal static bool IsSupported() => s_isSupported.Value;

        private static bool GetIsSupported()
        {
            bool result = false;
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (RuntimeInformation.OSDescription.Contains("Darwin"))
                    {
                        NSArray allDevices = MTLDevice.MTLCopyAllDevices();
                        result |= (ulong)allDevices.count > 0;
                        ObjectiveCRuntime.release(allDevices.NativePtr);
                    }
                    else
                    {
                        MTLDevice defaultDevice = MTLDevice.MTLCreateSystemDefaultDevice();
                        if (defaultDevice.NativePtr != IntPtr.Zero)
                        {
                            result = true;
                            ObjectiveCRuntime.release(defaultDevice.NativePtr);
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        internal MTLComputePipelineState GetUnalignedBufferCopyPipeline()
        {
            using (_unalignedBufferCopyPipelineLock.Lock(_multiThreaded))
            {
                if (_unalignedBufferCopyPipeline.IsNull)
                {
                    MTLComputePipelineDescriptor descriptor = MTLUtil.AllocInit<MTLComputePipelineDescriptor>(
                       nameof(MTLComputePipelineDescriptor));
                    MTLPipelineBufferDescriptor buffer0 = descriptor.buffers[0];
                    buffer0.mutability = MTLMutability.Mutable;
                    MTLPipelineBufferDescriptor buffer1 = descriptor.buffers[1];
                    buffer0.mutability = MTLMutability.Mutable;

                    Debug.Assert(_unalignedBufferCopyShader == null);
                    string name = MetalFeatures.IsMacOS ? UnalignedBufferCopyPipelineMacOSName : UnalignedBufferCopyPipelineiOSName;
                    using (Stream resourceStream = typeof(MTLGraphicsDevice).Assembly.GetManifestResourceStream(name))
                    {
                        byte[] data = new byte[resourceStream.Length];
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            resourceStream.CopyTo(ms);
                            ShaderDescription shaderDesc = new ShaderDescription(ShaderStages.Compute, data, "copy_bytes");
                            _unalignedBufferCopyShader = new MTLShader(ref shaderDesc, this);
                        }
                    }

                    descriptor.computeFunction = _unalignedBufferCopyShader.Function;
                    _unalignedBufferCopyPipeline = _device.newComputePipelineStateWithDescriptor(descriptor);
                    ObjectiveCRuntime.release(descriptor.NativePtr);
                }

                return _unalignedBufferCopyPipeline;
            }
        }
    }
}
