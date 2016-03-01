using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK;
using System.Numerics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLRenderContext : RenderContext
    {
        private readonly OpenGLResourceFactory _resourceFactory;
        private readonly GraphicsContext _openGLGraphicsContext;
        private readonly NativeWindow _window;

        public OpenGLRenderContext(NativeWindow window)
        {
            _resourceFactory = new OpenGLResourceFactory();

            _window = window;
            _window.Resize += OnNativeWindowResized;
            WindowInfo = new OpenTKWindowInfo(_window);

            _openGLGraphicsContext = new GraphicsContext(GraphicsMode.Default, _window.WindowInfo);
            _openGLGraphicsContext.MakeCurrent(_window.WindowInfo);
            _openGLGraphicsContext.LoadAll();

            SetInitialStates();
            OnWindowResized();

            int major, minor;
            GL.GetInteger(GetPName.MajorVersion, out major);
            GL.GetInteger(GetPName.MinorVersion, out minor);
            Console.WriteLine($"Created OpenGL Context. Version: {major}.{minor}");
        }

        public override ResourceFactory ResourceFactory => _resourceFactory;

        public override WindowInfo WindowInfo { get; }

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

        public override void ClearBuffer()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void SwapBuffers()
        {
            _openGLGraphicsContext.SwapBuffers();
        }


        public override void DrawIndexedPrimitives(int startingIndex, int indexCount)
        {
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private void OnNativeWindowResized(object sender, EventArgs e)
        {
            _openGLGraphicsContext.Update(_window.WindowInfo);
            OnWindowResized();
        }

        private void SetInitialStates()
        {
            GL.ClearColor(Color.CornflowerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.PolygonSmooth);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        protected override void HandleWindowResize()
        {
            GL.Viewport(0, 0, _window.Width, _window.Height);
        }
    }
}
