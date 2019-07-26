using System;
using System.Diagnostics;
using WebAssembly;
using WebGLDotNET;

namespace Veldrid.WebGL
{
    internal class WebGLGraphicsDevice : GraphicsDevice
    {
        private readonly WebGLCommandExecutor _executor;

        public WebGL2RenderingContext Ctx { get; }
        public JSObject Canvas { get; }
        public WebGLTextureSamplerManager TextureSamplerManager { get; }
        public override GraphicsBackend BackendType => GraphicsBackend.WebGL;
        public override bool IsUvOriginTopLeft => false;
        public override bool IsDepthRangeZeroToOne => false;
        public override bool IsClipSpaceYInverted => false;
        public override ResourceFactory ResourceFactory { get; }
        public override Swapchain MainSwapchain { get; }
        public override GraphicsDeviceFeatures Features { get; }

        public WebGLGraphicsDevice(GraphicsDeviceOptions options, object canvas)
            : base(ref options)
        {
            Features = new GraphicsDeviceFeatures(
                computeShader: false,
                geometryShader: false,
                tessellationShaders: false,
                multipleViewports: false,
                samplerLodBias: true,
                drawBaseVertex: false,
                drawBaseInstance: false,
                drawIndirect: false,
                drawIndirectBaseInstance: false,
                fillModeWireframe: false,
                samplerAnisotropy: false,
                depthClipDisable: false,
                texture1D: false,
                independentBlend: false,
                structuredBuffer: false,
                subsetTextureView: false,
                commandListDebugMarkers: false,
                bufferRangeBinding: false,
                commandBuffers: options.EnableCommandBuffers);
            Canvas = (JSObject)canvas;
            ResourceFactory = new WebGLResourceFactory(this);
            Ctx = new WebGL2RenderingContext(Canvas);
            MainSwapchain = new WebGLSwapchain(this);

            TextureSamplerManager = new WebGLTextureSamplerManager(this);
            _executor = new WebGLCommandExecutor(this);

            PostDeviceCreated();
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            throw new NotImplementedException();
        }

        public override void ResetFence(Fence fence)
        {
            Util.AssertSubtype<Fence, WebGLFence>(fence).Reset();
        }

        public override bool WaitForFence(Fence fence, ulong nanosecondTimeout) => true;

        public override bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout) => true;

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            throw new NotImplementedException();
        }

        protected override void PlatformDispose()
        {
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

        private protected override AcquireResult AcquireNextImageCore(
            Swapchain swapchain,
            Semaphore semaphore,
            Fence fence,
            out uint imageIndex)
        {
            (fence as WebGLFence)?.Set();
            imageIndex = 0;
            return AcquireResult.Success;
        }

        private protected override bool GetPixelFormatSupportCore(
            PixelFormat format,
            TextureType type,
            TextureUsage usage,
            out PixelFormatProperties properties)
        {
            throw new NotImplementedException();
        }

        private protected override void PresentCore(Swapchain swapchain, Semaphore waitSemaphore, uint index)
        {
        }

        private protected override void SubmitCommandsCore(CommandList commandList, Fence fence)
        {
            throw new NotImplementedException();
        }

        private protected override void SubmitCommandsCore(CommandBuffer commandBuffer, Semaphore wait, Semaphore signal, Fence fence)
        {
            WebGLCommandBuffer wglCB = Util.AssertSubtype<CommandBuffer, WebGLCommandBuffer>(commandBuffer);
            wglCB.BeginExecuting();
            wglCB.GetEntryList().ExecuteAll(_executor);
            wglCB.EndExecuting();
            Util.AssertSubtype<Fence, WebGLFence>(fence).Set();
        }

        private protected override void SubmitCommandsCore(CommandBuffer[] commandBuffers, Semaphore[] waits, Semaphore[] signals, Fence fence)
        {
            foreach (CommandBuffer commandBuffer in commandBuffers)
            {
                WebGLCommandBuffer wglCB = Util.AssertSubtype<CommandBuffer, WebGLCommandBuffer>(commandBuffer);
                wglCB.BeginExecuting();
                wglCB.GetEntryList().ExecuteAll(_executor);
                wglCB.EndExecuting();
            }
            Util.AssertSubtype<Fence, WebGLFence>(fence).Set();
        }

        private protected override void SubmitCommandsCore(CommandBuffer[] commandBuffers, Semaphore wait, Semaphore signal, Fence fence)
        {
            foreach (CommandBuffer commandBuffer in commandBuffers)
            {
                WebGLCommandBuffer wglCB = Util.AssertSubtype<CommandBuffer, WebGLCommandBuffer>(commandBuffer);
                wglCB.BeginExecuting();
                wglCB.GetEntryList().ExecuteAll(_executor);
                wglCB.EndExecuting();
            }
            Util.AssertSubtype<Fence, WebGLFence>(fence).Set();
        }

        private protected override void SwapBuffersCore(Swapchain swapchain)
        {
        }

        private protected override void UpdateBufferCore(
            DeviceBuffer buffer, uint bufferOffsetInBytes,
            IntPtr source, uint sizeInBytes)
        {
            _executor.UpdateBuffer(buffer, bufferOffsetInBytes, source, sizeInBytes);
        }

        private protected override void UpdateTextureCore(
            Texture texture,
            IntPtr source, uint sizeInBytes,
            uint x, uint y, uint z,
            uint width, uint height, uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            _executor.UpdateTexture(texture, source, sizeInBytes, x, y, z, width, height, depth, mipLevel, arrayLayer);
        }

        private protected override void WaitForIdleCore()
        {
        }

        [Conditional("WEBGL_CHECK_ERRORS")]
        [DebuggerNonUserCode]
        public void CheckError()
        {
            uint error = Ctx.GetError();
            if (error != 0)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw new VeldridException("WebGL getError indicated an error: " + error);
            }
        }
    }
}
