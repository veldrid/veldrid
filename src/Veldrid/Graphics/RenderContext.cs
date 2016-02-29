using System;
using System.Numerics;

namespace Veldrid.Graphics
{
    public abstract class RenderContext
    {
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
            SetViewport();

            WindowResized?.Invoke();
        }

        public abstract WindowInfo WindowInfo { get; }
        public abstract void DrawIndexedPrimitives(int startingVertex, int vertexCount);
        public abstract void BeginFrame();
        public abstract void SwapBuffers();

        protected abstract void SetViewport();
    }
}
