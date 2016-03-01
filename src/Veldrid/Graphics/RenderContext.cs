using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Veldrid.Graphics
{
    public abstract class RenderContext
    {
        private OpenTK.NativeWindow _nativeWindow;
        private OpenTKWindowInfo _windowInfo;
        private volatile bool v_needsResizing;

        public RenderContext()
        {
            ManualResetEvent initializationEvent = new ManualResetEvent(false);
            Task.Factory.StartNew(WindowOwnerRoutine, initializationEvent);
            initializationEvent.WaitOne();
            initializationEvent.Dispose();
        }

        private void WindowOwnerRoutine(object state)
        {
            ManualResetEvent initializationEvent = (ManualResetEvent)state;

            _nativeWindow = new OpenTK.NativeWindow();
            _nativeWindow.Visible = true;
            _nativeWindow.X = 100;
            _nativeWindow.Y = 100;
            _nativeWindow.Resize += (s, e) => v_needsResizing = true;
            _windowInfo = new OpenTKWindowInfo(_nativeWindow);
            initializationEvent.Set();

            while (_nativeWindow.Exists)
            {
                _nativeWindow.ProcessEvents();
            }
        }

        public WindowInfo WindowInfo => _windowInfo;

        public virtual RgbaFloat ClearColor { get; set; } = RgbaFloat.CornflowerBlue;

        public abstract ResourceFactory ResourceFactory { get; }

        public DynamicDataProvider<Matrix4x4> ViewMatrixProvider { get; }
            = new DynamicDataProvider<Matrix4x4>(
                Matrix4x4.CreateLookAt(new Vector3(0, 3, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0)));

        public DynamicDataProvider<Matrix4x4> ProjectionMatrixProvider { get; }
            = new DynamicDataProvider<Matrix4x4>();

        public event Action WindowResized;

        private readonly float _fieldOfViewRadians = 1.05f;

        public void SetVertexBuffer(VertexBuffer vb)
        {
            vb.Apply();
        }

        public void SetIndexBuffer(IndexBuffer ib)
        {
            ib.Apply();
        }

        public void SetMaterial(Material material)
        {
            material.Apply();
        }

        protected void OnWindowResized()
        {
            ProjectionMatrixProvider.Data = Matrix4x4.CreatePerspectiveFieldOfView(
                _fieldOfViewRadians,
                (float)WindowInfo.Width / (float)WindowInfo.Height,
                1f,
                1000f);

            HandleWindowResize();

            WindowResized?.Invoke();
        }

        public abstract void DrawIndexedPrimitives(int startingIndex, int indexCount);
        public void ClearBuffer()
        {
            if (v_needsResizing)
            {
                OnWindowResized();
            }

            PlatformClearBuffer();
        }

        protected OpenTK.NativeWindow NativeWindow => _nativeWindow;

        protected abstract void PlatformClearBuffer();

        public abstract void SwapBuffers();

        protected abstract void HandleWindowResize();
    }
}
