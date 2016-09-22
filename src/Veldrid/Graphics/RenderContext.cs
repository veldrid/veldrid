using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using Veldrid.Platform;

namespace Veldrid.Graphics
{
    /// <summary>Represents a graphics device. Provides functionality for creating and managing
    /// device resources, querying and controlling device state, and low-level drawing operations.
    public abstract class RenderContext : IDisposable
    {
        public const int MaxVertexBuffers = 16;

        private readonly Vector2 _topLeftUvCoordinate;
        private readonly Vector2 _bottomRightUvCoordinate;

        private int _needsResizing;

        // Device State
        private Framebuffer _framebuffer;
        private VertexBuffer[] _vertexBuffers = new VertexBuffer[MaxVertexBuffers];
        private IndexBuffer _indexBuffer;
        private Material _material;
        private Rectangle _scissorRectangle;
        private Viewport _viewport;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;

        /// <summary>Storage for shader texture input providers.</summary>
        public Dictionary<string, ContextDeviceBinding<DeviceTexture>> TextureProviders { get; } = new Dictionary<string, ContextDeviceBinding<DeviceTexture>>();

        private readonly Dictionary<string, BufferProviderPair> _bufferProviderPairs = new Dictionary<string, BufferProviderPair>();

        public RenderContext(Window window)
        {
            Window = window;
            window.Resized += () => _needsResizing = 1;

            _topLeftUvCoordinate = GetTopLeftUvCoordinate();
            _bottomRightUvCoordinate = GetBottomRightUvCoordinate();
        }

        /// <summary>The window to which this RenderContext renders.</summary>
        public Window Window { get; }

        /// <summary>Gets or sets the color which is used when clearing the Framebuffer when ClearBuffer() is called.</summary>
        public virtual RgbaFloat ClearColor { get; set; } = RgbaFloat.CornflowerBlue;

        /// <summary>The ResourceFactory associated with this RenderContext.</summary>
        public abstract ResourceFactory ResourceFactory { get; }

        /// <summary>An event which fires when the window is resized.
        /// When a resize is detected, the next call to ClearBuffer will trigger this event.</summary>
        public event Action WindowResized;

        /// <summary>Gets or sets the VertexBuffer in slot 0.</summary>
        public VertexBuffer VertexBuffer
        {
            get { return _vertexBuffers[0]; }
            set { SetVertexBufferCore(0, value); }
        }

        public void SetVertexBuffer(VertexBuffer vb)
        {
            SetVertexBufferCore(0, vb);
        }

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
            set { SetIndexBuffer(value); }
        }

        /// <summary>Changes the active IndexBuffer.</summary>
        public void SetIndexBuffer(IndexBuffer ib)
        {
            if (ib != _indexBuffer)
            {
                PlatformSetIndexBuffer(ib);
                _indexBuffer = ib;
            }
        }

        /// <summary>Gets or sets the active Material.</summary>
        public Material Material
        {
            get { return _material; }
            set { SetMaterial(value); }
        }

        /// <summary>Changes the active Material.</summary>
        public void SetMaterial(Material material)
        {
            if (material != _material)
            {
                PlatformSetMaterial(material);
                _material = material;
            }
            else
            {
                // TODO: Fix this abstraction.
                PlatformClearMaterialResourceBindings();
                material.UseDefaultTextures();
            }
        }

        /// <summary>Draws indexed primitives, starting from the given index.</summary>
        public abstract void DrawIndexedPrimitives(int count, int startingIndex);

        /// <summary>Draws indexed primitives, starting from the given index and vertex.</summary>
        public abstract void DrawIndexedPrimitives(int count, int startingIndex, int startingVertex);

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
            if (Interlocked.CompareExchange(ref _needsResizing, 0, 1) == 1)
            {
                OnWindowResized();
            }

            PlatformClearBuffer();
            NullInputs();
            FlushConstantBufferData();
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
                _material = null;
                PlatformClearMaterialResourceBindings();
                _framebuffer = framebuffer;
                PlatformSetFramebuffer(framebuffer);
            }
        }

        /// <summary>Changes the current scissor rectangle.
        /// This will only have an effect if a RasterizerState is active with IsScissorTestEnabled set to True.</summary>
        public void SetScissorRectangle(int left, int top, int right, int bottom)
            => SetScissorRectangle(new Rectangle(left, top, right, bottom));

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

        /// <summary>Sets the scissor rectangle area to (0, 0, int.MaxValue, int.MaxValue).
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

        /// <summary>A BlendState providing basic override additive blending.<summary>
        public BlendState AdditiveBlend
            => _additiveBlend ?? (_additiveBlend = ResourceFactory.CreateCustomBlendState(true, Blend.SourceAlpha, Blend.One, BlendFunction.Add));
        private BlendState _additiveBlend;

        /// <summary>A BlendState providing SrcAlpha->InvSrcAlpha additive blending.<summary>
        public BlendState AlphaBlend
            => _alphaBlend ?? (_alphaBlend = ResourceFactory.CreateCustomBlendState(true, Blend.SourceAlpha, Blend.InverseSourceAlpha, BlendFunction.Add));
        private BlendState _alphaBlend;

        /// <summary>A BlendState providing full-override blending.<summary>
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
                = ResourceFactory.CreateDepthStencilState(true, DepthComparison.LessEqual));
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

        /// <summary>Gets the top left UV coordinate of a standard plane.</summary>
        public Vector2 TopLeftUv => _topLeftUvCoordinate;

        /// <summary>Gets the bottom right UV coordinate of a standard plane.</summary>
        public Vector2 BottomRightUv => _bottomRightUvCoordinate;

        /// <summary>Gets a DeviceTexture binding by name.</summary>
        public ContextDeviceBinding<DeviceTexture> GetTextureContextBinding(string name)
        {
            ContextDeviceBinding<DeviceTexture> value;
            if (!TextureProviders.TryGetValue(name, out value))
            {
                value = new ContextDeviceBinding<DeviceTexture>();
                TextureProviders.Add(name, value);
            }

            return value;
        }

        public void RegisterGlobalDataProvider(string name, ConstantBufferDataProvider provider)
        {
            BufferProviderPair pair;
            if (_bufferProviderPairs.TryGetValue(name, out pair))
            {
                ChangeableProvider changeable = (ChangeableProvider)pair.DataProvider;
                changeable.DataProvider = provider;
            }
            else
            {
                var constantBuffer = ResourceFactory.CreateConstantBuffer(provider.DataSizeInBytes);
                var newProvider = provider is ChangeableProvider ? provider : new ChangeableProvider(provider);
                _bufferProviderPairs.Add(name, new BufferProviderPair(constantBuffer, newProvider));
            }
        }

        public BufferProviderPair GetNamedGlobalBufferProviderPair(string name)
        {
            BufferProviderPair pair;
            if (!_bufferProviderPairs.TryGetValue(name, out pair))
            {
                throw new InvalidOperationException("No provider registered with name " + name);
            }

            return pair;
        }

        public IEnumerable<KeyValuePair<string, BufferProviderPair>> GetAllGlobalBufferProviderPairs()
        {
            return _bufferProviderPairs;
        }

        protected void OnWindowResized()
        {
            PlatformResize();
            WindowResized?.Invoke();
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

        protected abstract void PlatformResize();

        protected abstract void PlatformSetVertexBuffer(int slot, VertexBuffer vb);

        protected abstract void PlatformSetIndexBuffer(IndexBuffer ib);

        protected abstract void PlatformSetMaterial(Material material);

        protected abstract void PlatformSetFramebuffer(Framebuffer framebuffer);

        protected abstract void PlatformSetBlendstate(BlendState blendState);

        protected abstract void PlatformSetDepthStencilState(DepthStencilState depthStencilState);

        protected abstract void PlatformSetRasterizerState(RasterizerState rasterizerState);

        protected abstract void PlatformSetViewport(int x, int y, int width, int height);

        protected abstract void PlatformSetPrimitiveTopology(PrimitiveTopology primitiveTopology);

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
            _material = null;
        }

        private void FlushConstantBufferData()
        {
            foreach (var kvp in _bufferProviderPairs)
            {
                kvp.Value.UpdateData();
            }
        }
    }

    public class ContextDeviceBinding<T>
    {
        private T _value;
        private bool _valid;

        public T Value
        {
            get
            {
                if (!_valid)
                {
                    throw new InvalidOperationException($"No value has been bound to context binding of type {typeof(T).FullName}");
                }

                return _value;
            }
            set
            {
                _value = value;
                _valid = true;
            }
        }

        public ContextDeviceBinding(T value)
        {
            _value = value;
            _valid = true;
        }

        public ContextDeviceBinding()
        {
        }
    }

    public class ChangeableProvider : ConstantBufferDataProvider
    {
        private ConstantBufferDataProvider _dataProvider;

        public ConstantBufferDataProvider DataProvider
        {
            get { return _dataProvider; }
            set
            {
                _dataProvider.DataChanged -= OnParentDataChanged;
                _dataProvider = value;
                _dataProvider.DataChanged += OnParentDataChanged;
                OnParentDataChanged();
            }
        }

        public event Action DataChanged;

        public ChangeableProvider(ConstantBufferDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            DataSizeInBytes = dataProvider.DataSizeInBytes;
            dataProvider.DataChanged += OnParentDataChanged;
        }

        private void OnParentDataChanged()
        {
            DataChanged?.Invoke();
        }

        public int DataSizeInBytes { get; }

        public void SetData(ConstantBuffer buffer)
        {
            _dataProvider.SetData(buffer);
        }
    }

    public class BufferProviderPair : IDisposable
    {
        public readonly ConstantBuffer ConstantBuffer;
        public readonly ConstantBufferDataProvider DataProvider;

        private bool _dirty;

        public BufferProviderPair(ConstantBuffer buffer, ConstantBufferDataProvider provider)
        {
            ConstantBuffer = buffer;
            DataProvider = provider;
            provider.DataChanged += OnDataChanged;
            _dirty = true;
            UpdateData();
        }

        private void OnDataChanged()
        {
            _dirty = true;
        }

        public void UpdateData()
        {
            if (_dirty)
            {
                DataProvider.SetData(ConstantBuffer);
                _dirty = false;
            }
        }

        public void Dispose()
        {
            DataProvider.DataChanged -= OnDataChanged;
        }
    }
}
