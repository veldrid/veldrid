using OpenTK;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DRenderContext : RenderContext
    {

        private SharpDX.Direct3D11.Device _device;
        private SwapChain _swapChain;
        private DeviceContext _deviceContext;

        private RenderTargetView _backBufferView;
        private DepthStencilView _depthStencilView;
        private BlendState _blendState;
        private DepthStencilState _depthState;
        private RasterizerState _rasterizerState;
        private SamplerState _samplerState;

        public D3DRenderContext()
        {

            CreateAndInitializeDevice();

            ResourceFactory = new D3DResourceFactory(_device);
        }

        public override ResourceFactory ResourceFactory { get; }


        protected override void PlatformClearBuffer()
        {
            Clear(ClearColor);
        }

        public override void DrawIndexedPrimitives(int startingVertex, int indexCount)
        {
            _device.ImmediateContext.DrawIndexed(indexCount, startingVertex, 0);
        }

        protected override void PlatformSwapBuffers()
        {
            _swapChain.Present(0, PresentFlags.None);
        }

        private void CreateAndInitializeDevice()
        {
            var swapChainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                IsWindowed = true,
                ModeDescription = new ModeDescription(NativeWindow.ClientSize.Width, NativeWindow.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = NativeWindow.WindowInfo.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            DeviceCreationFlags flags = DeviceCreationFlags.None;
#if DEBUG
            //flags |= DeviceCreationFlags.Debug;
#endif
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, flags, swapChainDescription, out _device, out _swapChain);
            _deviceContext = _device.ImmediateContext;
            var factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(NativeWindow.WindowInfo.Handle, WindowAssociationFlags.IgnoreAll);

            CreateRasterizerState();
            CreateDepthBufferState();
            CreateSamplerState();
            CreateBlendState();
            OnWindowResized();
            SetRegularTargets();

            _deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
        }

        private void SetRegularTargets()
        {
            // Setup targets and viewport for rendering
            _deviceContext.Rasterizer.SetViewport(0, 0, NativeWindow.ClientSize.Width, NativeWindow.ClientSize.Height);
            _deviceContext.OutputMerger.SetTargets(_depthStencilView, _backBufferView);
        }

        private void CreateBlendState()
        {
            _blendState = new BlendState(_device, BlendStateDescription.Default());
        }

        private void CreateDepthBufferState()
        {
            DepthStencilStateDescription description = DepthStencilStateDescription.Default();
            description.DepthComparison = Comparison.LessEqual;
            description.IsDepthEnabled = true;

            _depthState = new DepthStencilState(_device, description);
        }

        private void CreateRasterizerState()
        {
            var desc = RasterizerStateDescription.Default();
            desc.IsMultisampleEnabled = true;
            desc.CullMode = CullMode.Back;
            _rasterizerState = new RasterizerState(_device, desc);
        }

        public void CreateSamplerState()
        {
            SamplerStateDescription description = SamplerStateDescription.Default();
            description.Filter = Filter.MinMagMipLinear;
            description.AddressU = TextureAddressMode.Wrap;
            description.AddressV = TextureAddressMode.Wrap;
            _samplerState = new SamplerState(_device, description);
        }

        private void SetAllDeviceStates()
        {
            _deviceContext.Rasterizer.State = _rasterizerState;
            _deviceContext.OutputMerger.SetBlendState(_blendState);
            _deviceContext.OutputMerger.SetDepthStencilState(_depthState);
            _deviceContext.PixelShader.SetSampler(0, _samplerState);
        }

        private unsafe void Clear(RgbaFloat color)
        {
            // Clear the back buffer
            _deviceContext.ClearRenderTargetView(_backBufferView, *(RawColor4*)&color);

            // Clear the depth buffer
            _deviceContext.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        private void OnWindowResized(object sender, EventArgs e)
        {
            OnWindowResized();
        }

        protected override void HandleWindowResize()
        {
            if (_backBufferView != null)
            {
                _backBufferView.Dispose();
            }
            if (_depthStencilView != null)
            {
                _depthStencilView.Dispose();
            }

            _swapChain.ResizeBuffers(1, NativeWindow.ClientSize.Width, NativeWindow.ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);

            // Get the backbuffer from the swapchain
            using (var backBufferTexture = _swapChain.GetBackBuffer<Texture2D>(0))
            {
                // Backbuffer
                _backBufferView = new RenderTargetView(_device, backBufferTexture);
            }

            // Depth buffer

            using (var zbufferTexture = new Texture2D(_device, new Texture2DDescription()
            {
                Format = Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = Math.Max(1, NativeWindow.ClientSize.Width),
                Height = Math.Max(1, NativeWindow.ClientSize.Height),
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            }))
            {
                // Create the depth buffer view
                _depthStencilView = new DepthStencilView(_device, zbufferTexture);
            }

            SetRegularTargets();
        }
    }
}
