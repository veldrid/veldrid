using System;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Veldrid.Platform;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;

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
        private PrimitiveTopology _primitiveTopology;
        private int _syncInterval;

        private readonly Dictionary<D3DVertexInputLayout, InputLayout> _inputLayoutCache = new Dictionary<D3DVertexInputLayout, InputLayout>();
        private bool _vertexLayoutChanged;

        // Shader constant bindings
        private D3DConstantBuffer[] _vertexConstantBindings = new D3DConstantBuffer[15];
        private D3DConstantBuffer[] _geometryConstantBindings = new D3DConstantBuffer[15];
        private D3DConstantBuffer[] _fragmentConstantBindings = new D3DConstantBuffer[15];

        // Texture binding arrays
        private ShaderTextureBinding[] _vertexTextureBindings = new ShaderTextureBinding[MaxShaderResourceViewBindings];
        private ShaderTextureBinding[] _geometryShaderTextureBindings = new ShaderTextureBinding[MaxShaderResourceViewBindings];
        private ShaderTextureBinding[] _pixelShaderTextureBindings = new ShaderTextureBinding[MaxShaderResourceViewBindings];

        private D3DVertexInputLayout _inputLayout;
        private D3DVertexShader _vertexShader;
        private D3DGeometryShader _geometryShader;
        private D3DFragmentShader _fragmentShader;

        private const DeviceCreationFlags DefaultDeviceFlags
#if DEBUG
            = DeviceCreationFlags.Debug;
#else
            = DeviceCreationFlags.None;
#endif

        public D3DRenderContext(Window window) : this(window, DefaultDeviceFlags) { }

        public D3DRenderContext(Window window, DeviceCreationFlags flags)
        {
            CreateAndInitializeDevice(window, flags);
            ResourceFactory = new D3DResourceFactory(_device);
            RenderCapabilities = new RenderCapabilities(false, false);
            PostContextCreated();
        }

        public D3DRenderContext(Window window, SharpDX.Direct3D11.Device existingDevice, SwapChain existingSwapchain)
        {
            _swapChain = existingSwapchain;
            _device = existingDevice;
            _deviceContext = _device.ImmediateContext;
            _syncInterval = 1;

            OnWindowResized(window.Width, window.Height);
            SetFramebuffer(_defaultFramebuffer);
            _deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            ResourceFactory = new D3DResourceFactory(_device);
            PostContextCreated();
        }

        public override ResourceFactory ResourceFactory { get; }

        public SharpDX.Direct3D11.Device Device => _device;

        protected override GraphicsBackend PlatformGetGraphicsBackend() => GraphicsBackend.Direct3D11;

        protected unsafe override void PlatformClearBuffer()
        {
            RgbaFloat clearColor = ClearColor;
            for (int i = 0; i < CurrentFramebuffer.RenderTargetViews.Count; i++)
            {
                RenderTargetView rtv = CurrentFramebuffer.RenderTargetViews[i];
                if (rtv != null)
                {
                    _deviceContext.ClearRenderTargetView(rtv, *(RawColor4*)&clearColor);
                }
            }
            if (CurrentFramebuffer.DepthStencilView != null)
            {
                _deviceContext.ClearDepthStencilView(CurrentFramebuffer.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            }
        }

        public override void DrawIndexedPrimitives(int count, int startingIndex) => DrawIndexedPrimitives(count, startingIndex, 0);
        public override void DrawIndexedPrimitives(int count, int startingIndex, int startingVertex)
        {
            PreDrawCommand();
            _deviceContext.DrawIndexed(count, startingIndex, startingVertex);
        }


        public override void DrawInstancedPrimitives(int indexCount, int instanceCount, int startingIndex)
        {
            PreDrawCommand();
            DrawInstancedPrimitives(indexCount, instanceCount, startingIndex, 0);
        }

        public override void DrawInstancedPrimitives(int indexCount, int instanceCount, int startingIndex, int startingVertex)
        {
            PreDrawCommand();
            _deviceContext.DrawIndexedInstanced(indexCount, instanceCount, startingIndex, startingVertex, 0);
        }

        private void PreDrawCommand()
        {
            if (_vertexLayoutChanged)
            {
                _vertexLayoutChanged = false;
                InputLayout inputLayout = GetInputLayout(_vertexShader, _inputLayout);
                _deviceContext.InputAssembler.InputLayout = inputLayout;
            }
        }

        private InputLayout GetInputLayout(D3DVertexShader vertexShader, D3DVertexInputLayout d3dInputLayout)
        {
            if (!_inputLayoutCache.TryGetValue(d3dInputLayout, out InputLayout inputLayout))
            {
                inputLayout = D3DVertexInputLayout.CreateLayout(_device, d3dInputLayout.InputDescriptions, vertexShader.Bytecode);
                _inputLayoutCache.Add(d3dInputLayout, inputLayout);
            }

            return inputLayout;
        }

        protected override void PlatformSwapBuffers()
        {
            _swapChain.Present(_syncInterval, PresentFlags.None);
        }

        private void CreateAndInitializeDevice(Window window, DeviceCreationFlags creationFlags)
        {
            var swapChainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                IsWindowed = true,
                ModeDescription = new ModeDescription(window.Width, window.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = window.Handle,
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
            factory.MakeWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAll);

            OnWindowResized(window.Width, window.Height);
            SetFramebuffer(_defaultFramebuffer);
            _deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            _syncInterval = 0;
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
                var d3dTopology = D3DFormats.VeldridToD3DPrimitiveTopology(primitiveTopology);
                _deviceContext.InputAssembler.PrimitiveTopology = d3dTopology;
            }
        }

        protected override void PlatformResize(int width, int height)
        {
            RecreateDefaultFramebuffer(width, height);
        }

        private void RecreateDefaultFramebuffer(int width, int height)
        {
            if (_defaultFramebuffer != null)
            {
                _defaultFramebuffer.Dispose();
            }

            _swapChain.ResizeBuffers(2, width, height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

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

        protected override void PlatformSetShaderSet(ShaderSet shaderSet)
        {
            D3DShaderSet d3dShaders = ((D3DShaderSet)shaderSet);

            SetInputLayout(d3dShaders.InputLayout);
            SetVertexShader(d3dShaders.VertexShader);
            SetGeometryShader(d3dShaders.GeometryShader);
            SetPixelShader(d3dShaders.FragmentShader);
        }

        private void SetInputLayout(D3DVertexInputLayout inputLayout)
        {
            if (_inputLayout != inputLayout)
            {
                _inputLayout = inputLayout;
                _vertexLayoutChanged = true;
            }
        }

        private void SetVertexShader(D3DVertexShader vertexShader)
        {
            if (_vertexShader != vertexShader)
            {
                _vertexShader = vertexShader;
                _deviceContext.VertexShader.Set(vertexShader?.DeviceShader);
            }
        }

        private void SetGeometryShader(D3DGeometryShader geometryShader)
        {
            if (_geometryShader != geometryShader)
            {
                _geometryShader = geometryShader;
                _deviceContext.GeometryShader.Set(geometryShader?.DeviceShader);
            }
        }

        private void SetPixelShader(D3DFragmentShader fragmentShader)
        {
            if (_fragmentShader != fragmentShader)
            {
                _fragmentShader = fragmentShader;
                _deviceContext.PixelShader.Set(fragmentShader?.DeviceShader);
            }
        }

        protected override void PlatformSetShaderConstantBindings(ShaderConstantBindingSlots shaderConstantBindings)
        {
        }

        protected override void PlatformSetConstantBuffer(int slot, ConstantBuffer cb)
        {
            ShaderStageApplicabilityFlags applicability = ShaderConstantBindingSlots.GetApplicability(slot);

            if ((applicability & ShaderStageApplicabilityFlags.Vertex) == ShaderStageApplicabilityFlags.Vertex
                && _vertexConstantBindings[slot] != cb)
            {
                _deviceContext.VertexShader.SetConstantBuffer(slot, ((D3DConstantBuffer)cb).Buffer);
                _vertexConstantBindings[slot] = (D3DConstantBuffer)cb;
            }

            if ((applicability & ShaderStageApplicabilityFlags.Geometry) == ShaderStageApplicabilityFlags.Geometry
                && _geometryConstantBindings[slot] != cb)
            {
                _deviceContext.GeometryShader.SetConstantBuffer(slot, ((D3DConstantBuffer)cb).Buffer);
                _geometryConstantBindings[slot] = (D3DConstantBuffer)cb;
            }

            if ((applicability & ShaderStageApplicabilityFlags.Fragment) == ShaderStageApplicabilityFlags.Fragment
                && _fragmentConstantBindings[slot] != cb)
            {
                _deviceContext.PixelShader.SetConstantBuffer(slot, ((D3DConstantBuffer)cb).Buffer);
                _fragmentConstantBindings[slot] = (D3DConstantBuffer)cb;
            }
        }

        protected override void PlatformSetShaderTextureBindingSlots(ShaderTextureBindingSlots bindingSlots)
        {
        }

        protected override void PlatformSetTexture(int slot, ShaderTextureBinding binding)
        {
            ShaderStageApplicabilityFlags applicability = ShaderTextureBindingSlots.GetApplicabilityForSlot(slot);
            if ((applicability & ShaderStageApplicabilityFlags.Vertex) == ShaderStageApplicabilityFlags.Vertex)
            {
                SetTextureBinding(ShaderType.Vertex, slot, binding);
            }
            if ((applicability & ShaderStageApplicabilityFlags.Geometry) == ShaderStageApplicabilityFlags.Geometry)
            {
                SetTextureBinding(ShaderType.Geometry, slot, binding);
            }
            if ((applicability & ShaderStageApplicabilityFlags.Fragment) == ShaderStageApplicabilityFlags.Fragment)
            {
                SetTextureBinding(ShaderType.Fragment, slot, binding);
            }
        }

        protected override void PlatformSetSamplerState(int slot, SamplerState samplerState, bool mipmapped)
        {
            D3DSamplerState d3dSamplerState = (D3DSamplerState)samplerState;

            // TODO: This should respect applicability flags like textures do.
            _deviceContext.VertexShader.SetSampler(slot, d3dSamplerState.SamplerState);
            _deviceContext.GeometryShader.SetSampler(slot, d3dSamplerState.SamplerState);
            _deviceContext.PixelShader.SetSampler(slot, d3dSamplerState.SamplerState);
        }

        protected override void PlatformSetFramebuffer(Framebuffer framebuffer)
        {
            D3DFramebuffer d3dFramebuffer = (D3DFramebuffer)framebuffer;
            D3DTexture2D depthTexture = d3dFramebuffer.DepthTexture;
            UnbindIfBound(ShaderType.Vertex, depthTexture);
            UnbindIfBound(ShaderType.Geometry, depthTexture);
            UnbindIfBound(ShaderType.Fragment, depthTexture);

            for (int i = 0; i < MaxRenderTargets; i++)
            {
                DeviceTexture2D colorTexture = framebuffer.GetColorTexture(i);
                if (colorTexture != null)
                {
                    UnbindIfBound(ShaderType.Vertex, colorTexture);
                    UnbindIfBound(ShaderType.Geometry, colorTexture);
                    UnbindIfBound(ShaderType.Fragment, colorTexture);
                }
            }
            d3dFramebuffer.Apply();
        }

        /// <summary>
        /// Unbinds a texture if it is bound to any slots for the given shader type.
        /// Used to avoid binding a texture to a framebuffer while it is in use elsewhere, which is not valid.
        /// </summary>
        /// <param name="shaderType">The type of shader to unbind the texture from.</param>
        /// <param name="texture">The texture resource to unbind.</param>
        private void UnbindIfBound(ShaderType shaderType, DeviceTexture texture)
        {
            ShaderTextureBinding[] bindingsArray = GetTextureBindingsArray(shaderType);
            for (int i = 0; i < bindingsArray.Length; i++)
            {
                if (bindingsArray[i] != null && bindingsArray[i].BoundTexture == texture)
                {
                    bindingsArray[i] = null;
                    CommonShaderStage stage = GetShaderStage(shaderType);
                    stage.SetShaderResource(i, null);
                }
            }
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

        private void SetTextureBinding(ShaderType type, int slot, ShaderTextureBinding binding)
        {
            Debug.Assert(slot >= 0 && slot < MaxShaderResourceViewBindings);

            ShaderTextureBinding[] array = GetTextureBindingsArray(type);
            if (array[slot] != binding)
            {
                array[slot] = binding;
                D3DTextureBinding d3dBinding = (D3DTextureBinding)binding;
                CommonShaderStage shaderStage = GetShaderStage(type);
                shaderStage.SetShaderResource(slot, d3dBinding.ResourceView);
            }
        }

        private ShaderTextureBinding[] GetTextureBindingsArray(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.Vertex:
                    return _vertexTextureBindings;
                case ShaderType.Geometry:
                    return _geometryShaderTextureBindings;
                case ShaderType.Fragment:
                    return _pixelShaderTextureBindings;
                default:
                    throw Illegal.Value<ShaderType>();
            }
        }

        private CommonShaderStage GetShaderStage(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.Vertex:
                    return _deviceContext.VertexShader;
                case ShaderType.Geometry:
                    return _deviceContext.GeometryShader;
                case ShaderType.Fragment:
                    return _deviceContext.PixelShader;
                default:
                    throw Illegal.Value<ShaderType>();
            }
        }

        protected override void PlatformDispose()
        {
            _defaultFramebuffer.Dispose();
            _swapChain.Dispose();
            _device.Dispose();
        }

        protected override void PlatformClearMaterialResourceBindings()
        {
        }

        public override RenderCapabilities RenderCapabilities { get; }

        protected override Vector2 GetTopLeftUvCoordinate()
        {
            return Vector2.Zero;
        }

        protected override Vector2 GetBottomRightUvCoordinate()
        {
            return Vector2.One;
        }

        private new D3DFramebuffer CurrentFramebuffer => (D3DFramebuffer)base.CurrentFramebuffer;

        private new D3DShaderTextureBindingSlots ShaderTextureBindingSlots => (D3DShaderTextureBindingSlots)base.ShaderTextureBindingSlots;

        private new D3DShaderConstantBindingSlots ShaderConstantBindingSlots => (D3DShaderConstantBindingSlots)base.ShaderConstantBindingSlots;
    }
}
