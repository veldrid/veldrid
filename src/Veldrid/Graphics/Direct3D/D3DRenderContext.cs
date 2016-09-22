using System;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Veldrid.Platform;
using System.Drawing;
using System.Numerics;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DRenderContext : RenderContext
    {
        private readonly ShaderResourceView[] _emptySRVs = new ShaderResourceView[MaxShaderResourceViewBindings];
        private const int MaxShaderResourceViewBindings = 10;

        private SharpDX.Direct3D11.Device _device;
        private SwapChain _swapChain;
        private DeviceContext _deviceContext;
        private D3DFramebuffer _defaultFramebuffer;
        private SamplerState _regularSamplerState;
        private SamplerState _shadowMapSampler;
        private PrimitiveTopology _primitiveTopology;
        private int _syncInterval;

        private const DeviceCreationFlags DefaultDeviceFlags
#if DEBUG
            = DeviceCreationFlags.Debug;
#else
            = DeviceCreationFlags.None;
#endif

        public D3DRenderContext(Window window) : this(window, DefaultDeviceFlags) { }

        public D3DRenderContext(Window window, DeviceCreationFlags flags)
            : base(window)
        {
            CreateAndInitializeDevice(flags);
            CreateAndSetSamplers();
            ResourceFactory = new D3DResourceFactory(_device);
            PostContextCreated();
        }

        public D3DRenderContext(Window window, SharpDX.Direct3D11.Device existingDevice, SwapChain existingSwapchain)
            : base(window)
        {
            _swapChain = existingSwapchain;
            _device = existingDevice;
            _deviceContext = _device.ImmediateContext;
            _syncInterval = 1;

            OnWindowResized();
            SetFramebuffer(_defaultFramebuffer);
            _deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            CreateAndSetSamplers();
            ResourceFactory = new D3DResourceFactory(_device);
            PostContextCreated();
        }

        public override ResourceFactory ResourceFactory { get; }

        public SharpDX.Direct3D11.Device Device => _device;

        protected unsafe override void PlatformClearBuffer()
        {
            RgbaFloat clearColor = ClearColor;
            if (CurrentFramebuffer.RenderTargetView != null)
            {
                _deviceContext.ClearRenderTargetView(CurrentFramebuffer.RenderTargetView, *(RawColor4*)&clearColor);
            }
            if (CurrentFramebuffer.DepthStencilView != null)
            {
                _deviceContext.ClearDepthStencilView(CurrentFramebuffer.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            }
        }

        public override void DrawIndexedPrimitives(int count, int startingIndex) => DrawIndexedPrimitives(count, startingIndex, 0);
        public override void DrawIndexedPrimitives(int count, int startingIndex, int startingVertex)
        {
            _deviceContext.DrawIndexed(count, startingIndex, startingVertex);
        }

        protected override void PlatformSwapBuffers()
        {
            _swapChain.Present(_syncInterval, PresentFlags.None);
        }

        private void CreateAndInitializeDevice(DeviceCreationFlags creationFlags)
        {
            var swapChainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                IsWindowed = true,
                ModeDescription = new ModeDescription(Window.Width, Window.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = Window.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                creationFlags,
                swapChainDescription,
                out _device,
                out _swapChain);

            _deviceContext = _device.ImmediateContext;
            var factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(Window.Handle, WindowAssociationFlags.IgnoreAll);

            OnWindowResized();
            SetFramebuffer(_defaultFramebuffer);
            _deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            _syncInterval = 0;
        }

        private void CreateAndSetSamplers()
        {
            SamplerStateDescription regularDesc = SamplerStateDescription.Default();
            regularDesc.AddressU = TextureAddressMode.Wrap;
            regularDesc.AddressV = TextureAddressMode.Wrap;
            _regularSamplerState = new SamplerState(_device, regularDesc);
            _deviceContext.PixelShader.SetSampler(0, _regularSamplerState);

            SamplerStateDescription shadowSamplerDesc = SamplerStateDescription.Default();
            shadowSamplerDesc.Filter = Filter.MinMagMipPoint;
            shadowSamplerDesc.BorderColor = new RawColor4(1f, 1f, 1f, 1f);
            shadowSamplerDesc.AddressU = TextureAddressMode.Border;
            shadowSamplerDesc.AddressV = TextureAddressMode.Border;
            _shadowMapSampler = new SamplerState(_device, shadowSamplerDesc);
            _deviceContext.PixelShader.SetSampler(1, _shadowMapSampler);
        }


        protected override void PlatformSetViewport(int left, int top, int width, int height)
        {
            _deviceContext.Rasterizer.SetViewport(left, top, width, height);
        }

        protected override void PlatformSetPrimitiveTopology(PrimitiveTopology primitiveTopology)
        {
            if (_primitiveTopology != primitiveTopology)
            {
                _primitiveTopology = primitiveTopology;
                var d3dTopology = D3DFormats.ConvertPrimitiveTopology(primitiveTopology);
                _deviceContext.InputAssembler.PrimitiveTopology = d3dTopology;
            }
        }

        protected override void PlatformResize()
        {
            RecreateDefaultFramebuffer();
        }

        private void RecreateDefaultFramebuffer()
        {
            if (_defaultFramebuffer != null)
            {
                _defaultFramebuffer.Dispose();
            }

            _swapChain.ResizeBuffers(2, Window.Width, Window.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            using (var backBufferTexture = _swapChain.GetBackBuffer<Texture2D>(0))
            using (var depthBufferTexture = new Texture2D(_device, new Texture2DDescription()
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
                bool currentlyBound = CurrentFramebuffer == null || CurrentFramebuffer == _defaultFramebuffer;
                // Create the depth buffer view
                _defaultFramebuffer = new D3DFramebuffer(
                    _device,
                    new D3DTexture2D(_device, backBufferTexture),
                    new D3DTexture2D(_device, depthBufferTexture),
                    backBufferTexture.Description.Width,
                    backBufferTexture.Description.Height);
                if (currentlyBound)
                {
                    SetFramebuffer(_defaultFramebuffer);
                }
            }
        }

        protected override void PlatformSetDefaultFramebuffer()
        {
            SetFramebuffer(_defaultFramebuffer);
        }

        protected override void PlatformSetScissorRectangle(Rectangle rectangle)
        {
            _deviceContext.Rasterizer.SetScissorRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        protected override void PlatformSetVertexBuffer(int slot, VertexBuffer vb)
        {
            ((D3DVertexBuffer)vb).Apply(slot);
        }

        protected override void PlatformSetIndexBuffer(IndexBuffer ib)
        {
            ((D3DIndexBuffer)ib).Apply();
        }

        protected override void PlatformSetMaterial(Material material)
        {
            ((D3DMaterial)material).Apply();
        }

        protected override void PlatformSetFramebuffer(Framebuffer framebuffer)
        {
            ((D3DFramebuffer)framebuffer).Apply();
        }

        protected override void PlatformSetBlendstate(BlendState blendState)
        {
            ((D3DBlendState)blendState).Apply();
        }

        protected override void PlatformSetDepthStencilState(DepthStencilState depthStencilState)
        {
            ((D3DDepthStencilState)depthStencilState).Apply();
        }

        protected override void PlatformSetRasterizerState(RasterizerState rasterizerState)
        {
            ((D3DRasterizerState)rasterizerState).Apply();
        }

        protected override void PlatformDispose()
        {
            _defaultFramebuffer.Dispose();
            _swapChain.Dispose();
            _device.Dispose();
        }

        protected override void PlatformClearMaterialResourceBindings()
        {
            if (Material != null)
            {
                ((D3DMaterial)Material).ClearTextureBindings();
            }
            _deviceContext.PixelShader.SetShaderResources(0, _emptySRVs.Length, _emptySRVs);
        }

        protected override Vector2 GetTopLeftUvCoordinate()
        {
            return Vector2.Zero;
        }

        protected override Vector2 GetBottomRightUvCoordinate()
        {
            return Vector2.One;
        }

        private new D3DFramebuffer CurrentFramebuffer => (D3DFramebuffer)base.CurrentFramebuffer;
    }
}
