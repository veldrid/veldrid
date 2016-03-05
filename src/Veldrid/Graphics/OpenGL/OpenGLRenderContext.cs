using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using Veldrid.Platform;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLRenderContext : RenderContext
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

            int major, minor;
            GL.GetInteger(GetPName.MajorVersion, out major);
            GL.GetInteger(GetPName.MinorVersion, out minor);
            Console.WriteLine($"Created OpenGL Context. Version: {major}.{minor}");
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


        public override void DrawIndexedPrimitives(int startingIndex, int indexCount)
        {
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private void OnNativeWindowResized(object sender, EventArgs e)
        {
            _openGLGraphicsContext.Update(((OpenTKWindow)Window).OpenTKWindowInfo);
            OnWindowResized();
        }

        private void SetInitialStates()
        {
            GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.PolygonSmooth);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        protected override void PlatformResize()
        {
            GL.Viewport(0, 0, Window.Width, Window.Height);
        }

        protected override void PlatformSetDefaultFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
