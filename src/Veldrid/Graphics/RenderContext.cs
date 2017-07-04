using System;
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Graphics
{
    /// <summary>Represents a graphics device. Provides functionality for creating and managing
    /// device resources, querying and controlling device state, and low-level drawing operations.</summary>
    public abstract class RenderContext : IDisposable
    {
        public const int MaxVertexBuffers = 16;
        public const int MaxTextures = 10;
        public const int MaxRenderTargets = 8;

        private readonly Vector2 _topLeftUvCoordinate;
        private readonly Vector2 _bottomRightUvCoordinate;

        // Device State
        private Framebuffer _framebuffer;
        private VertexBuffer[] _vertexBuffers = new VertexBuffer[MaxVertexBuffers];
        private IndexBuffer _indexBuffer;
        private Rectangle _scissorRectangle;
        private Viewport _viewport;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;
        private ShaderSet _shaderSet;
        private ShaderConstantBindingSlots _constantBindings;
        private ShaderTextureBindingSlots _textureBindingSlots;

        protected readonly Dictionary<int, DeviceTexture> _boundTexturesBySlot = new Dictionary<int, DeviceTexture>();
        protected readonly Dictionary<int, BoundSamplerStateInfo> _boundSamplersBySlot = new Dictionary<int, BoundSamplerStateInfo>();

        public RenderContext()
        {
            _topLeftUvCoordinate = GetTopLeftUvCoordinate();
            _bottomRightUvCoordinate = GetBottomRightUvCoordinate();
        }

        /// <summary>Gets or sets the color which is used when clearing the Framebuffer when ClearBuffer() is called.</summary>
        public virtual RgbaFloat ClearColor { get; set; } = RgbaFloat.CornflowerBlue;

        /// <summary>The ResourceFactory associated with this RenderContext.</summary>
        public abstract ResourceFactory ResourceFactory { get; }

        /// <summary>An event which fires when the window is resized.
        /// When a resize is detected, the next call to ClearBuffer will trigger this event.</summary>
        public event Action<int, int> WindowResized;

        /// <summary>Gets or sets the VertexBuffer in slot 0.</summary>
        public VertexBuffer VertexBuffer
        {
            get { return _vertexBuffers[0]; }
            set { SetVertexBufferCore(0, value); }
        }

        public void SetSamplerState(int slot, SamplerState samplerState)
        {
            if (!_boundSamplersBySlot.TryGetValue(slot, out BoundSamplerStateInfo bssi) || bssi.SamplerState != samplerState)
            {
                bool mipmap = false;
                if (_boundTexturesBySlot.TryGetValue(slot, out DeviceTexture boundTex) && boundTex != null)
                {
                    mipmap = boundTex.MipLevels != 1;
                }

                PlatformSetSamplerState(slot, samplerState, mipmap);
                _boundSamplersBySlot[slot] = new BoundSamplerStateInfo(samplerState, mipmap);
            }
        }

        public SamplerState PointSampler
            => _pointSampler ?? (_pointSampler = ResourceFactory.CreateSamplerState(
                SamplerAddressMode.Wrap, SamplerAddressMode.Wrap, SamplerAddressMode.Wrap,
                SamplerFilter.MinMagMipPoint, 1, RgbaFloat.Black, DepthComparison.Always, 0, int.MaxValue, 0));
        private SamplerState _pointSampler;

        public SamplerState Anisox4Sampler
            => _anisox4Sampler ?? (_anisox4Sampler = ResourceFactory.CreateSamplerState(
                SamplerAddressMode.Wrap, SamplerAddressMode.Wrap, SamplerAddressMode.Wrap,
                SamplerFilter.Anisotropic, 4, RgbaFloat.Black, DepthComparison.Always, 0, int.MaxValue, 0));
        private SamplerState _anisox4Sampler;

        /// <summary>Changes the active VertexBuffer.</summary>
        public void SetVertexBuffer(int slot, VertexBuffer vb)
        {
            if (slot < 0 || slot >= MaxVertexBuffers)
            {
                throw new ArgumentOutOfRangeException("Slot must be between 0 and " + MaxVertexBuffers);
            }

            SetVertexBufferCore(slot, vb);
        }

        private void SetVertexBufferCore(int slot, VertexBuffer vb)
        {
            if (vb != _vertexBuffers[slot])
            {
                PlatformSetVertexBuffer(slot, vb);
                _vertexBuffers[slot] = vb;
            }
        }

        /// <summary>Gets or sets the active IndexBuffer.</summary>
        public IndexBuffer IndexBuffer
        {
            get { return _indexBuffer; }
            set
            {
                if (value != _indexBuffer)
                {
                    PlatformSetIndexBuffer(value);
                    _indexBuffer = value;
                }
            }
        }

        public ShaderSet ShaderSet
        {
            get { return _shaderSet; }
            set
            {
                if (_shaderSet != value)
                {
                    PlatformSetShaderSet(value);
                    _shaderSet = value;
                }
            }
        }

        public ShaderConstantBindingSlots ShaderConstantBindingSlots
        {
            get { return _constantBindings; }
            set
            {
                if (_constantBindings != value)
                {
                    PlatformSetShaderConstantBindings(value);
                    _constantBindings = value;
                }
            }
        }

        public void SetConstantBuffer(int slot, ConstantBuffer cb)
        {
            if (_constantBindings == null)
            {
                throw new InvalidOperationException(
                    "Cannot call SetConstantBuffer when ShaderConstantBindingSlots has not been set.");
            }

            PlatformSetConstantBuffer(slot, cb);
        }

        public ShaderTextureBindingSlots ShaderTextureBindingSlots
        {
            get { return _textureBindingSlots; }
            set
            {
                if (_textureBindingSlots != value)
                {
                    PlatformSetShaderTextureBindingSlots(value);
                    _textureBindingSlots = value;
                }
            }
        }

        public void SetTexture(int slot, ShaderTextureBinding textureBinding)
        {
            if (_textureBindingSlots == null)
            {
                throw new InvalidOperationException("Cannot call SetTexture when TextureBindingSlots has not been set.");
            }

            PlatformSetTexture(slot, textureBinding);
        }

        /// <summary>
        /// Draws indexed primitives.
        /// </summary>
        /// <param name="count">The number of indices to draw.</param>
        public void DrawIndexedPrimitives(int count)
        {
            DrawIndexedPrimitives(count, 0);
        }

        /// <summary>
        /// Draws indexed primitives, starting from the given index.
        /// </summary>
        /// <param name="count">The number of indices to draw.</param>
        /// <param name="startingIndex">The index to start with.</param>
        public abstract void DrawIndexedPrimitives(int count, int startingIndex);

        /// <summary>
        /// Draws indexed primitives, starting from the given index and vertex.
        /// </summary>
        /// <param name="count">The number of indices to draw.</param>
        /// <param name="startingIndex">The index to start with.</param>
        /// <param name="startingVertex">A base vertex value added to each index.</param>
        public abstract void DrawIndexedPrimitives(int count, int startingIndex, int startingVertex);

        /// <summary>
        /// Draws instanced primitives.
        /// </summary>
        /// <param name="indexCount">The number of indices to draw, per-instance.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        public void DrawInstancedPrimitives(int indexCount, int instanceCount)
        {
            DrawInstancedPrimitives(indexCount, instanceCount, 0);
        }

        /// <summary>
        /// Draws instanced primitives.
        /// </summary>
        /// <param name="indexCount">The number of indices to draw, per-instance.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        /// <param name="primitiveTopology">The <see cref="PrimitiveTopology"/> to render with.</param>
        public void DrawInstancedPrimitives(int indexCount, int instanceCount, PrimitiveTopology primitiveTopology)
        {
            PlatformSetPrimitiveTopology(primitiveTopology);
            DrawInstancedPrimitives(indexCount, instanceCount);
            PlatformSetPrimitiveTopology(PrimitiveTopology.TriangleList);
        }

        /// <summary>
        /// Draws instanced primitives, starting from the given index.
        /// </summary>
        /// <param name="indexCount">The number of indices to draw, per-instance.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        /// <param name="startingIndex">The index number to start with.</param>
        public abstract void DrawInstancedPrimitives(int indexCount, int instanceCount, int startingIndex);

        /// <summary>
        /// Draws instanced primitives, starting from the given index and vertex.
        /// </summary>
        /// <param name="indexCount">The number of indices to draw, per-instance.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        /// <param name="startingIndex">The index number to start with.</param>
        /// <param name="startingVertex">A base vertex value added to each index.</param>
        public abstract void DrawInstancedPrimitives(int indexCount, int instanceCount, int startingIndex, int startingVertex);

        public void DrawIndexedPrimitives(int count, int startingIndex, PrimitiveTopology primitiveTopology)
        {
            PlatformSetPrimitiveTopology(primitiveTopology);
            DrawIndexedPrimitives(count, startingIndex);
            PlatformSetPrimitiveTopology(PrimitiveTopology.TriangleList);
        }

        /// <summary>Clears the current Framebuffer's color and depth buffers.
        /// The color is cleared to the value stored in ClearColor. </summary>
        public void ClearBuffer()
        {
            PlatformClearBuffer();
            NullInputs();
        }

        /// <summary>Clears the current Framebuffer's color and depth buffers.
        /// The color is cleared to the given RgbaFloat value.</summary>
        public void ClearBuffer(RgbaFloat color)
        {
            RgbaFloat previousColor = ClearColor;
            ClearColor = color;
            ClearBuffer();
            ClearColor = previousColor;
        }

        /// <summary>Gets or sets the current active Framebuffer.</summary>
        public Framebuffer CurrentFramebuffer
        {
            get { return _framebuffer; }
            set { SetFramebuffer(value); }
        }

        /// <summary>Changes the current Framebuffer.</summary>
        public void SetFramebuffer(Framebuffer framebuffer)
        {
            if (_framebuffer != framebuffer)
            {
                _framebuffer = framebuffer;
                PlatformSetFramebuffer(framebuffer);
            }
        }

        /// <summary>Changes the current scissor rectangle.
        /// This will only have an effect if a RasterizerState is active with IsScissorTestEnabled set to True.</summary>
        public void SetScissorRectangle(int left, int top, int right, int bottom)
            => SetScissorRectangle(new Rectangle(left, top, right - left, bottom - top));

        /// <summary>Changes the current scissor rectangle.
        /// This will only have an effect if a RasterizerState is active with IsScissorTestEnabled set to True.</summary>
        public void SetScissorRectangle(Rectangle r)
        {
            if (_scissorRectangle != r)
            {
                _scissorRectangle = r;
                PlatformSetScissorRectangle(_scissorRectangle);
            }
        }

        /// <summary>Sets the scissor rectangle area to (0, 0, int.MaxValue, int.MaxValue).</summary>
        public virtual void ClearScissorRectangle()
        {
            SetScissorRectangle(0, 0, int.MaxValue, int.MaxValue);
        }

        /// <summary>Makes the window's default Framebuffer active.</summary>
        public void SetDefaultFramebuffer()
        {
            PlatformSetDefaultFramebuffer();
        }

        /// <summary>Swaps the graphics context's buffers and presents the new rendered image.</summary>
        public void SwapBuffers()
        {
            PlatformSwapBuffers();
        }

        /// <summary>A BlendState providing basic override additive blending.</summary>
        public BlendState AdditiveBlend
            => _additiveBlend ?? (_additiveBlend = ResourceFactory.CreateCustomBlendState(true, Blend.SourceAlpha, Blend.One, BlendFunction.Add));
        private BlendState _additiveBlend;

        /// <summary>A BlendState providing SrcAlpha->InvSrcAlpha additive blending.</summary>
        public BlendState AlphaBlend
            => _alphaBlend ?? (_alphaBlend = ResourceFactory.CreateCustomBlendState(true, Blend.SourceAlpha, Blend.InverseSourceAlpha, BlendFunction.Add));
        private BlendState _alphaBlend;

        /// <summary>A BlendState providing full-override blending. This is the default <see cref="Veldrid.Graphics.BlendState"/>.</summary>
        public BlendState OverrideBlend
            => _overrideBlend ?? (_overrideBlend = ResourceFactory.CreateCustomBlendState(true, Blend.One, Blend.Zero, BlendFunction.Add));
        private BlendState _overrideBlend;

        /// <summary>Gets or sets the current active BlendState.</summary>
        public BlendState BlendState
        {
            get { return _blendState; }
            set { SetBlendState(value); }
        }

        /// <summary>Changes the active BlendState.</summary>
        public void SetBlendState(BlendState blendState)
        {
            if (_blendState != blendState)
            {
                _blendState = blendState;
                PlatformSetBlendstate(blendState);
            }
        }

        /// <summary>Gets the default DepthStencilState.</summary>
        public DepthStencilState DefaultDepthStencilState
            => _defaultDepthStencilState ?? (_defaultDepthStencilState
                = ResourceFactory.CreateDepthStencilState(true, DepthComparison.LessEqual, true));
        private DepthStencilState _defaultDepthStencilState;

        /// <summary>Gets or sets the active DepthStencilState.</summary>
        public DepthStencilState DepthStencilState
        {
            get { return _depthStencilState; }
            set { SetDepthStencilState(value); }
        }

        /// <summary>Changes the active DepthStencilState.</summary>
        public void SetDepthStencilState(DepthStencilState depthStencilState)
        {
            if (_depthStencilState != depthStencilState)
            {
                _depthStencilState = depthStencilState;
                PlatformSetDepthStencilState(depthStencilState);
            }
        }

        /// <summary>Gets the default RasterizerState.</summary>
        public RasterizerState DefaultRasterizerState
            => _defaultRasterizerState ?? (_defaultRasterizerState
                = ResourceFactory.CreateRasterizerState(FaceCullingMode.Back, TriangleFillMode.Solid, true, true));
        private RasterizerState _defaultRasterizerState;

        /// <summary>Gets or sets the current active RasterizerState.</summary>
        public RasterizerState RasterizerState
        {
            get { return _rasterizerState; }
            set { SetRasterizerState(value); }
        }

        /// <summary>Changes the active RasterizerState.</summary>
        public void SetRasterizerState(RasterizerState rasterizerState)
        {
            if (_rasterizerState != rasterizerState)
            {
                _rasterizerState = rasterizerState;
                PlatformSetRasterizerState(rasterizerState);
            }
        }

        /// <summary>Gets or sets the current viewport.</summary>
        public Viewport Viewport
        {
            get { return _viewport; }
            set { SetViewport(value.X, value.Y, value.Width, value.Height); }
        }

        /// <summary>Changes the active viewport.</summary>
        public void SetViewport(Viewport viewport) => SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);

        /// <summary>Changes the active viewport.</summary>
        public void SetViewport(int x, int y, int width, int height)
        {
            if (_viewport.X != x || _viewport.Y != y || _viewport.Width != width || _viewport.Height != height)
            {
                PlatformSetViewport(x, y, width, height);
                _viewport = new Viewport(x, y, width, height);
            }
        }

        public abstract RenderCapabilities RenderCapabilities { get; }

        /// <summary>Gets the top left UV coordinate of a standard plane.</summary>
        public Vector2 TopLeftUv => _topLeftUvCoordinate;

        /// <summary>Gets the bottom right UV coordinate of a standard plane.</summary>
        public Vector2 BottomRightUv => _bottomRightUvCoordinate;

        // TODO: REMOVE THIS.
        public void NotifyWindowResized(int width, int height)
        {
            OnWindowResized(width, height);
        }

        protected void OnWindowResized(int width, int height)
        {
            PlatformResize(width, height);
            WindowResized?.Invoke(width, height);
        }

        protected void PostContextCreated()
        {
            SetBlendState(OverrideBlend);
            SetDepthStencilState(DefaultDepthStencilState);
            SetRasterizerState(DefaultRasterizerState);
            ClearScissorRectangle();
        }

        protected VertexBuffer[] VertexBuffers => _vertexBuffers;

        protected abstract Vector2 GetTopLeftUvCoordinate();

        protected abstract Vector2 GetBottomRightUvCoordinate();

        protected abstract void PlatformSetScissorRectangle(Rectangle rectangle);

        protected abstract void PlatformSetDefaultFramebuffer();

        protected abstract void PlatformClearBuffer();

        protected abstract void PlatformSwapBuffers();

        protected abstract void PlatformResize(int width, int height);

        protected abstract void PlatformSetVertexBuffer(int slot, VertexBuffer vb);

        protected abstract void PlatformSetIndexBuffer(IndexBuffer ib);

        protected abstract void PlatformSetFramebuffer(Framebuffer framebuffer);

        protected abstract void PlatformSetBlendstate(BlendState blendState);

        protected abstract void PlatformSetDepthStencilState(DepthStencilState depthStencilState);

        protected abstract void PlatformSetRasterizerState(RasterizerState rasterizerState);

        protected abstract void PlatformSetViewport(int x, int y, int width, int height);

        protected abstract void PlatformSetPrimitiveTopology(PrimitiveTopology primitiveTopology);

        protected abstract void PlatformSetShaderSet(ShaderSet shaderSet);

        protected abstract void PlatformSetShaderConstantBindings(ShaderConstantBindingSlots shaderConstantBindings);

        protected abstract void PlatformSetConstantBuffer(int slot, ConstantBuffer cb);

        protected abstract void PlatformSetShaderTextureBindingSlots(ShaderTextureBindingSlots bindingSlots);

        protected abstract void PlatformSetTexture(int slot, ShaderTextureBinding textureBinding);

        protected abstract void PlatformSetSamplerState(int slot, SamplerState samplerState, bool mipmapped);

        protected abstract void PlatformClearMaterialResourceBindings();

        protected abstract void PlatformDispose();

        ///<summary>Disposes all resources owned by this RenderContext.</summary>
        public void Dispose()
        {
            _defaultDepthStencilState.Dispose();
            _defaultRasterizerState.Dispose();
            _additiveBlend?.Dispose();
            _overrideBlend?.Dispose();
            _alphaBlend?.Dispose();
            PlatformDispose();
        }

        private void NullInputs()
        {
            for (int i = 0; i < MaxVertexBuffers; i++)
            {
                _vertexBuffers[i] = null;
            }

            _indexBuffer = null;
            _constantBindings = null;
        }

        protected struct BoundSamplerStateInfo
        {
            public SamplerState SamplerState { get; }
            public bool Mipmapped { get; }

            public BoundSamplerStateInfo(SamplerState samplerState, bool mipmapped)
            {
                SamplerState = samplerState;
                Mipmapped = mipmapped;
            }
        }
    }
}
