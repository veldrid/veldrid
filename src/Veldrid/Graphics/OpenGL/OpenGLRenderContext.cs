using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using Veldrid.Platform;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLRenderContext : RenderContext, IDisposable
    {
        private readonly OpenGLResourceFactory _resourceFactory;
        private readonly GraphicsContext _openGLGraphicsContext;

        public OpenGLRenderContext(OpenTKWindow window)
            : base(window)
        {
            _resourceFactory = new OpenGLResourceFactory();

            _openGLGraphicsContext = new GraphicsContext(GraphicsMode.Default, window.OpenTKWindowInfo);
            _openGLGraphicsContext.MakeCurrent(window.OpenTKWindowInfo);

            _openGLGraphicsContext.LoadAll();

            SetInitialStates();
            OnWindowResized();

            PostContextCreated();
        }

        public override ResourceFactory ResourceFactory => _resourceFactory;

        public override RgbaFloat ClearColor
        {
            get
            {
                return base.ClearColor;
            }
            set
            {
                base.ClearColor = value;
                Color4 openTKColor = RgbaFloat.ToOpenTKColor(value);
                GL.ClearColor(openTKColor);
            }
        }

        protected override void PlatformClearBuffer()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        protected override void PlatformSwapBuffers()
        {
            if (Window.Exists)
            {
                _openGLGraphicsContext.SwapBuffers();
            }
        }

        public override void DrawIndexedPrimitives(int count, int startingIndex)
        {
            var elementsType = ((OpenGLIndexBuffer)IndexBuffer).ElementsType;
            int indexSize = OpenGLFormats.GetIndexFormatSize(elementsType);

            GL.DrawElements(PrimitiveType.Triangles, count, elementsType, new IntPtr(startingIndex * indexSize));
        }

        public override void DrawIndexedPrimitives(int count, int startingIndex, int startingVertex)
        {
            var elementsType = ((OpenGLIndexBuffer)IndexBuffer).ElementsType;
            int indexSize = OpenGLFormats.GetIndexFormatSize(elementsType);
            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, count, elementsType, new IntPtr(startingIndex * indexSize), startingVertex);
        }

        private void SetInitialStates()
        {
            GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
            GL.Enable(EnableCap.PolygonSmooth);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        protected override void PlatformResize()
        {
            _openGLGraphicsContext.Update(((OpenTKWindow)Window).OpenTKWindowInfo);
            GL.Viewport(0, 0, Window.Width, Window.Height);
        }

        protected override void PlatformSetDefaultFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        protected override void PlatformSetScissorRectangle(Rectangle rectangle)
        {
            GL.Scissor(
                rectangle.Left,
                Window.Height - rectangle.Bottom,
                rectangle.Width,
                rectangle.Height);
        }

        public void Dispose()
        {
            _openGLGraphicsContext.Dispose();
        }
    }
}
