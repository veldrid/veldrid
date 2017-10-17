using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;

namespace Vd2.D3D11
{
    internal class D3D11GraphicsDevice : GraphicsDevice
    {
        private readonly SharpDX.Direct3D11.Device _device;
        private readonly DeviceContext _immediateContext;
        private readonly SwapChain _swapChain;
        private Framebuffer _swapChainFramebuffer;

        public override GraphicsBackend BackendType => GraphicsBackend.Direct3D11;

        public D3D11GraphicsDevice(IntPtr hwnd, int width, int height)
        {
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

            DeviceCreationFlags creationFlags = DeviceCreationFlags.Debug;
            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                creationFlags,
                swapChainDescription,
                out _device,
                out _swapChain);
            _immediateContext = _device.ImmediateContext;

            Factory factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAll);

            ResourceFactory = new D3D11ResourceFactory(_device);
            RecreateSwapchainFramebuffer(width, height);

            PostContextCreated();
        }

        public override void ResizeMainWindow(uint width, uint height)
        {
            RecreateSwapchainFramebuffer((int)width, (int)height);
        }

        private void RecreateSwapchainFramebuffer(int width, int height)
        {
            _swapChainFramebuffer?.Dispose();

            _swapChain.ResizeBuffers(2, width, height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            using (SharpDX.Direct3D11.Texture2D backBufferTexture = _swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            using (SharpDX.Direct3D11.Texture2D depthBufferTexture = new SharpDX.Direct3D11.Texture2D(
                _device,
                new Texture2DDescription()
                {
                    Format = Format.D16_UNorm,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = backBufferTexture.Description.Width,
                    Height = backBufferTexture.Description.Height,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                }))
            {
                D3D11Texture2D backBufferVdTexture = new D3D11Texture2D(backBufferTexture);
                D3D11Texture2D depthVdTexture = new D3D11Texture2D(depthBufferTexture);
                FramebufferDescription desc = new FramebufferDescription(depthVdTexture, backBufferVdTexture);
                _swapChainFramebuffer = new D3D11Framebuffer(_device, ref desc);
            }
        }

        public override ResourceFactory ResourceFactory { get; }

        public override Framebuffer SwapchainFramebuffer => _swapChainFramebuffer;

        public override void ExecuteCommands(CommandList cb)
        {
            D3D11CommandList d3d11Cb = Util.AssertSubtype<CommandList, D3D11CommandList>(cb);
            _immediateContext.ExecuteCommandList(d3d11Cb.DeviceCommandList, false);
        }

        public override void SwapBuffers()
        {
            _swapChain.Present(0, PresentFlags.None);
        }

        public override void Dispose()
        {
            DeviceDebug deviceDebug = _device.QueryInterface<DeviceDebug>();
            if (deviceDebug != null)
            {
                deviceDebug.ReportLiveDeviceObjects(ReportingLevel.Summary);
                deviceDebug.ReportLiveDeviceObjects(ReportingLevel.Detail);
            }
        }
    }
}
