using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using Veldrid.Platform;

namespace Veldrid.Graphics
{
    public abstract class RenderContext : IDisposable
    {
        private readonly Vector2 _topLeftUvCoordinate;
        private readonly Vector2 _bottomRightUvCoordinate;

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Material _material;

        private int _needsResizing;

        private RenderQueue _renderQueue = new RenderQueue();
        private Rectangle _scissorRectangle;
        private Viewport _viewport;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;

        public Dictionary<string, ConstantBufferDataProvider> DataProviders { get; } = new Dictionary<string, ConstantBufferDataProvider>();
        public Dictionary<string, ContextDeviceBinding<DeviceTexture>> TextureProviders { get; } = new Dictionary<string, ContextDeviceBinding<DeviceTexture>>();

        public RenderContext(Window window)
        {
            Window = window;
            window.Resized += () => _needsResizing = 1;

            _topLeftUvCoordinate = GetTopLeftUvCoordinate();
            _bottomRightUvCoordinate = GetBottomRightUvCoordinate();
        }

        public Window Window { get; }

        public Framebuffer CurrentFramebuffer { get; private set; }

        public Viewport Viewport
        {
            get { return _viewport; }
            set
            {
                SetViewport(value.X, value.Y, value.Width, value.Height);
            }
        }

        public RasterizerState RasterizerState
        {
            get { return _rasterizerState; }
            set { SetRasterizerState(value); }
        }

        public BlendState BlendState
        {
            get { return _blendState; }
            set { SetBlendState(value); }
        }

        public virtual RgbaFloat ClearColor { get; set; } = RgbaFloat.CornflowerBlue;

        public abstract ResourceFactory ResourceFactory { get; }

        public event Action WindowResized;

        public void SetVertexBuffer(VertexBuffer vb)
        {
            if (vb != _vertexBuffer)
            {
                PlatformSetVertexBuffer(vb);
                _vertexBuffer = vb;
            }
        }


        public IndexBuffer IndexBuffer => _indexBuffer;
        public void SetIndexBuffer(IndexBuffer ib)
        {
            if (ib != _indexBuffer)
            {
                PlatformSetIndexBuffer(ib);
                _indexBuffer = ib;
            }
        }

        public void SetMaterial(Material material)
        {
            if (material != _material)
            {
                PlatformSetMaterial(material);
                _material = material;
            }
        }

        public abstract void DrawIndexedPrimitives(int count, int startingIndex);
        public abstract void DrawIndexedPrimitives(int count, int startingIndex, int startingVertex);

        public void ClearBuffer()
        {
            if (Interlocked.CompareExchange(ref _needsResizing, 0, 1) == 1)
            {
                OnWindowResized();
            }

            PlatformClearBuffer();
            NullInputs();
        }

        public void ClearBuffer(RgbaFloat color)
        {
            RgbaFloat previousColor = ClearColor;
            ClearColor = color;
            ClearBuffer();
            ClearColor = previousColor;
        }

        public void SetFramebuffer(Framebuffer framebuffer)
        {
            if (CurrentFramebuffer != framebuffer)
            {
                _material = null;
                PlatformClearMaterialResourceBindings();
                CurrentFramebuffer = framebuffer;
                PlatformSetFramebuffer(framebuffer);
            }
        }

        public void SetScissorRectangle(int left, int top, int right, int bottom)
            => SetScissorRectangle(new Rectangle(left, top, right, bottom));
        public void SetScissorRectangle(Rectangle r)
        {
            if (_scissorRectangle != r)
            {
                _scissorRectangle = r;
                PlatformSetScissorRectangle(_scissorRectangle);
            }
        }

        public virtual void ClearScissorRectangle()
        {
            SetScissorRectangle(0, 0, int.MaxValue, int.MaxValue);
        }

        public void SetDefaultFramebuffer()
        {
            PlatformSetDefaultFramebuffer();
        }

        private void NullInputs()
        {
            _vertexBuffer = null;
            _indexBuffer = null;
            _material = null;
        }

        public void SwapBuffers()
        {
            PlatformSwapBuffers();
        }

        private BlendState _additiveBlend;
        public BlendState AdditiveBlend
            => _additiveBlend ?? (_additiveBlend = ResourceFactory.CreateCustomBlendState(true, Blend.SourceAlpha, Blend.One, BlendFunction.Add));

        private BlendState _alphaBlend;
        public BlendState AlphaBlend
            => _alphaBlend ?? (_alphaBlend = ResourceFactory.CreateCustomBlendState(true, Blend.SourceAlpha, Blend.InverseSourceAlpha, BlendFunction.Add));

        private BlendState _overrideBlend;
        public BlendState OverrideBlend
            => _overrideBlend ?? (_overrideBlend = ResourceFactory.CreateCustomBlendState(true, Blend.One, Blend.Zero, BlendFunction.Add));

        public void SetBlendState(BlendState blendState)
        {
            if (_blendState != blendState)
            {
                _blendState = blendState;
                PlatformSetBlendstate(blendState);
            }
        }

        private DepthStencilState _defaultDepthStencilState;
        public DepthStencilState DefaultDepthStencilState
            => _defaultDepthStencilState ?? (_defaultDepthStencilState
                = ResourceFactory.CreateDepthStencilState(true, DepthComparison.LessEqual));

        public void SetDepthStencilState(DepthStencilState depthStencilState)
        {
            if (_depthStencilState != depthStencilState)
            {
                _depthStencilState = depthStencilState;
                PlatformSetDepthStencilState(depthStencilState);
            }
        }

        private RasterizerState _defaultRasterizerState;
        public RasterizerState DefaultRasterizerState
            => _defaultRasterizerState ?? (_defaultRasterizerState
                = ResourceFactory.CreateRasterizerState(FaceCullingMode.Back, TriangleFillMode.Solid, true, true));

        public void SetRasterizerState(RasterizerState rasterizerState)
        {
            if (_rasterizerState != rasterizerState)
            {
                _rasterizerState = rasterizerState;
                PlatformSetRasterizerState(rasterizerState);
            }
        }

        public void SetViewport(Viewport viewport) => SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);

        public void SetViewport(int x, int y, int width, int height)
        {
            if (_viewport.X != x || _viewport.Y != y || _viewport.Width != width || _viewport.Height != height)
            {
                PlatformSetViewport(x, y, width, height);
                _viewport = new Viewport(x, y, width, height);
            }
        }

        public Vector2 TopLeftUv => _topLeftUvCoordinate;
        public Vector2 BottomRightUv => _bottomRightUvCoordinate;

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

        protected void OnWindowResized()
        {
            PlatformResize();
            WindowResized?.Invoke();
        }

        protected void PostContextCreated()
        {
            SetBlendState(AlphaBlend);
            SetDepthStencilState(DefaultDepthStencilState);
            SetRasterizerState(DefaultRasterizerState);
            ClearScissorRectangle();
        }

        protected abstract Vector2 GetTopLeftUvCoordinate();

        protected abstract Vector2 GetBottomRightUvCoordinate();

        protected abstract void PlatformSetScissorRectangle(Rectangle rectangle);

        protected abstract void PlatformSetDefaultFramebuffer();

        protected abstract void PlatformClearBuffer();

        protected abstract void PlatformSwapBuffers();

        protected abstract void PlatformResize();

        protected abstract void PlatformSetVertexBuffer(VertexBuffer vb);

        protected abstract void PlatformSetIndexBuffer(IndexBuffer ib);

        protected abstract void PlatformSetMaterial(Material material);

        protected abstract void PlatformSetFramebuffer(Framebuffer framebuffer);

        protected abstract void PlatformSetBlendstate(BlendState blendState);

        protected abstract void PlatformSetDepthStencilState(DepthStencilState depthStencilState);

        protected abstract void PlatformSetRasterizerState(RasterizerState rasterizerState);

        protected abstract void PlatformSetViewport(int x, int y, int width, int height);

        protected abstract void PlatformClearMaterialResourceBindings();

        protected abstract void PlatformDispose();

        public void Dispose()
        {
            _defaultDepthStencilState.Dispose();
            _defaultRasterizerState.Dispose();
            _additiveBlend?.Dispose();
            _overrideBlend?.Dispose();
            _alphaBlend?.Dispose();
            PlatformDispose();
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
}
