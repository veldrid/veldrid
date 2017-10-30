using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Veldrid.D3D11
{
    internal class D3D11GraphicsDevice : GraphicsDevice
    {
        private readonly SharpDX.Direct3D11.Device _device;
        private readonly DeviceContext _immediateContext;
        private readonly SwapChain _swapChain;
        private D3D11Framebuffer _swapChainFramebuffer;

        public override GraphicsBackend BackendType => GraphicsBackend.Direct3D11;

        public override ResourceFactory ResourceFactory { get; }

        public override Framebuffer SwapchainFramebuffer => _swapChainFramebuffer;

        public SharpDX.Direct3D11.Device Device => _device;

        public List<D3D11CommandList> CommandListsReferencingSwapchain { get; internal set; } = new List<D3D11CommandList>();

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
#if DEBUG
            DeviceCreationFlags creationFlags = DeviceCreationFlags.Debug;
#else
            DeviceCreationFlags creationFlags = DeviceCreationFlags.None;
#endif 
            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                creationFlags,
                swapChainDescription,
                out _device,
                out _swapChain);
            _immediateContext = _device.ImmediateContext;

            Factory factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAll);

            ResourceFactory = new D3D11ResourceFactory(this);
            RecreateSwapchainFramebuffer(width, height);

            PostContextCreated();
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
                _swapChainFramebuffer.IsSwapchainFramebuffer = true;
            }
        }

        public override void ExecuteCommands(CommandList cl)
        {
            D3D11CommandList d3d11CL = Util.AssertSubtype<CommandList, D3D11CommandList>(cl);
            _immediateContext.ExecuteCommandList(d3d11CL.DeviceCommandList, false);
            d3d11CL.DeviceCommandList.Dispose();
            d3d11CL.DeviceCommandList = null;
            CommandListsReferencingSwapchain.Remove(d3d11CL);
        }

        public override void SwapBuffers()
        {
            _swapChain.Present(0, PresentFlags.None);
        }

        public override void SetResourceName(DeviceResource resource, string name)
        {
            switch (resource)
            {
                case D3D11Buffer buffer:
                    buffer.Buffer.DebugName = name;
                    break;
                case D3D11CommandList commandList:
                    commandList.DeviceContext.DebugName = name;
                    break;
                case D3D11Framebuffer framebuffer:
                    for (int i = 0; i < framebuffer.RenderTargetViews.Length; i++)
                    {
                        framebuffer.RenderTargetViews[i].DebugName = string.Format("{0}_RTV{1}", name, i);
                    }
                    if (framebuffer.DepthStencilView != null)
                    {
                        framebuffer.DepthStencilView.DebugName = string.Format("{0}_DSV", name);
                    }
                    break;
                case D3D11Sampler sampler:
                    sampler.DeviceSampler.DebugName = name;
                    break;
                case D3D11Shader shader:
                    shader.DeviceShader.DebugName = name;
                    break;
                case D3D11Texture2D tex2D:
                    tex2D.DeviceTexture.DebugName = name;
                    break;
                case D3D11TextureCube texCube:
                    texCube.DeviceTexture.DebugName = name;
                    break;
                case D3D11TextureView texView:
                    texView.ShaderResourceView.DebugName = name;
                    break;
            }
        }

        public override void Dispose()
        {
            DeviceDebug deviceDebug = _device.QueryInterfaceOrNull<DeviceDebug>();
            if (deviceDebug != null)
            {
                deviceDebug.ReportLiveDeviceObjects(ReportingLevel.Summary);
                deviceDebug.ReportLiveDeviceObjects(ReportingLevel.Detail);
            }
        }

        public override void WaitForIdle()
        {
        }
    }
}
