using System;
using WebAssembly;
using WebGLDotNET;

namespace Veldrid.WebGL
{
    internal class WebGLGraphicsDevice : GraphicsDevice
    {
        private readonly object _canvas;

        public WebGLRenderingContext Ctx { get; }

        public WebGLGraphicsDevice(GraphicsDeviceOptions options, object canvas)
        {
            _canvas = canvas;
            ResourceFactory = new WebGLResourceFactory(this);
            Features = new GraphicsDeviceFeatures(
                computeShader: false,
                geometryShader: false,
                tessellationShaders: false,
                multipleViewports: false,
                samplerLodBias: true,
                drawBaseVertex: true,
                drawBaseInstance: true,
                drawIndirect: true,
                drawIndirectBaseInstance: false,
                fillModeWireframe: false,
                samplerAnisotropy: false,
                depthClipDisable: false,
                texture1D: true,
                independentBlend: false,
                structuredBuffer: true,
                subsetTextureView: true,
                commandListDebugMarkers: false,
                bufferRangeBinding: true);
            Ctx = new WebGLRenderingContext((JSObject)_canvas);
            MainSwapchain = new WebGLSwapchain(this);
        }

        public override GraphicsBackend BackendType => GraphicsBackend.WebGL;

        public override bool IsUvOriginTopLeft => throw new NotImplementedException();

        public override bool IsDepthRangeZeroToOne => throw new NotImplementedException();

        public override bool IsClipSpaceYInverted => throw new NotImplementedException();

        public override ResourceFactory ResourceFactory { get; }

        public override Swapchain MainSwapchain { get; }

        public override GraphicsDeviceFeatures Features { get; }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            throw new NotImplementedException();
        }

        public override void ResetFence(Fence fence)
        {
            Util.AssertSubtype<Fence, WebGLFence>(fence).Reset();
        }

        public override bool WaitForFence(Fence fence, ulong nanosecondTimeout)
        {
            return true;
        }

        public override bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout)
        {
            return true;
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            throw new NotImplementedException();
        }

        protected override void PlatformDispose()
        {
            throw new NotImplementedException();
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
            throw new NotImplementedException();
        }

        internal override uint GetStructuredBufferMinOffsetAlignmentCore()
        {
            throw new NotImplementedException();
        }

        internal override uint GetUniformBufferMinOffsetAlignmentCore()
        {
            throw new NotImplementedException();
        }

        private protected override uint AcquireNextImageCore(Swapchain swapchain, Semaphore semaphore, Fence fence)
        {
            throw new NotImplementedException();
        }

        private protected override bool GetPixelFormatSupportCore(PixelFormat format, TextureType type, TextureUsage usage, out PixelFormatProperties properties)
        {
            throw new NotImplementedException();
        }

        private protected override void PresentCore(Swapchain swapchain, Semaphore waitSemaphore, uint index)
        {
            throw new NotImplementedException();
        }

        private protected override void SubmitCommandsCore(CommandList commandList, Fence fence)
        {
            throw new NotImplementedException();
        }

        private protected override void SubmitCommandsCore(CommandBuffer commandBuffer, Semaphore wait, Semaphore signal, Fence fence)
        {
            Util.AssertSubtype<CommandBuffer, WebGLCommandBuffer>(commandBuffer).ExecuteAll();
            Util.AssertSubtype<Fence, WebGLFence>(fence).Set();
        }

        private protected override void SwapBuffersCore(Swapchain swapchain)
        {
            throw new NotImplementedException();
        }

        private protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            throw new NotImplementedException();
        }

        private protected override void UpdateTextureCore(Texture texture, IntPtr source, uint sizeInBytes, uint x, uint y, uint z, uint width, uint height, uint depth, uint mipLevel, uint arrayLayer)
        {
            throw new NotImplementedException();
        }

        private protected override void WaitForIdleCore()
        {
            throw new NotImplementedException();
        }
    }
}
