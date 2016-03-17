using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace Veldrid.Graphics
{
    public abstract class RenderContext
    {
        private readonly float _fieldOfViewRadians = 1.05f;

        private readonly Vector3 _cameraPosition = new Vector3(0, 3, 5);
        private readonly Vector3 _cameraDirection = new Vector3(0, -3, -5);

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Material _material;

        private int _needsResizing;
        private RenderQueue _renderQueue = new RenderQueue();
        private Rectangle _scissorRectangle;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;

        public Dictionary<string, ConstantBufferDataProvider> DataProviders { get; } = new Dictionary<string, ConstantBufferDataProvider>();

        public RenderContext(Window window)
        {
            Window = window;
            window.Resized += () => _needsResizing = 1;
        }

        public Window Window { get; }

        public Framebuffer CurrentFramebuffer { get; private set; }

        public virtual RgbaFloat ClearColor { get; set; } = RgbaFloat.CornflowerBlue;

        public abstract ResourceFactory ResourceFactory { get; }

        public DynamicDataProvider<Matrix4x4> ProjectionMatrixProvider { get; }
            = new DynamicDataProvider<Matrix4x4>();

        public event Action WindowResized;

        public void RenderFrame(VisibiltyManager visiblityManager)
        {
            _renderQueue.Clear();
            visiblityManager.CollectVisibleObjects(_renderQueue, _cameraPosition, _cameraDirection);
            foreach (var item in _renderQueue)
            {
                // TODO: Investigate putting this "Draw" concept into a separate, higher-level abstraction.
                // It's a bit awkward passing "this" into the renderable items.
                item.Render(this);
            }
        }

        public void SetVertexBuffer(VertexBuffer vb)
        {
            if (vb != _vertexBuffer)
            {
                vb.Apply();
                _vertexBuffer = vb;
            }
        }

        public IndexBuffer IndexBuffer => _indexBuffer;
        public void SetIndexBuffer(IndexBuffer ib)
        {
            if (ib != _indexBuffer)
            {
                ib.Apply();
                _indexBuffer = ib;
            }
        }

        public void SetMaterial(Material material)
        {
            if (material != _material)
            {
                material.Apply();
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

        public void SetFramebuffer(Framebuffer framebuffer)
        {
            if (CurrentFramebuffer != framebuffer)
            {
                CurrentFramebuffer = framebuffer;
                framebuffer.Apply();
            }
        }

        public void SetScissorRectangle(int left, int top, int right, int bottom)
        {
            Rectangle r = new Rectangle(left, top, right, bottom);
            if (_scissorRectangle != r)
            {
                _scissorRectangle = r;
                PlatformSetScissorRectangle(_scissorRectangle);
            }
        }

        protected abstract void PlatformSetScissorRectangle(Rectangle rectangle);

        public void ClearScissorRectangle()
        {
            _scissorRectangle = default(Rectangle);
            PlatformClearScissorRectangle();
        }

        protected abstract void PlatformClearScissorRectangle();

        public void SetDefaultFramebuffer()
        {
            PlatformSetDefaultFramebuffer();
        }

        protected abstract void PlatformSetDefaultFramebuffer();

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
                _blendState.Apply();
            }
        }

        private DepthStencilState _defaultDepthStencilState;
        public DepthStencilState DefaultDepthStencilState
            => _defaultDepthStencilState ?? (_defaultDepthStencilState = ResourceFactory.CreateDepthStencilState(true, DepthComparison.LessEqual));

        public void SetDepthStencilState(DepthStencilState depthStencilState)
        {
            if (_depthStencilState != depthStencilState)
            {
                _depthStencilState = depthStencilState;
                _depthStencilState.Apply();
            }
        }

        protected void OnWindowResized()
        {
            ProjectionMatrixProvider.Data = Matrix4x4.CreatePerspectiveFieldOfView(
                _fieldOfViewRadians,
                Window.Width / (float)Window.Height,
                1f,
                1000f);

            PlatformResize();
            WindowResized?.Invoke();
        }

        protected void PostContextCreated()
        {
            SetBlendState(AlphaBlend);
            SetDepthStencilState(DefaultDepthStencilState);
        }

        protected abstract void PlatformClearBuffer();

        protected abstract void PlatformSwapBuffers();

        protected abstract void PlatformResize();
    }
}
