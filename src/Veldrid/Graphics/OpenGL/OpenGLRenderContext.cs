using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using Veldrid.Platform;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLRenderContext : RenderContext, IDisposable
    {
        private readonly OpenGLResourceFactory _resourceFactory;
        private readonly GraphicsContext _openGLGraphicsContext;
        private readonly OpenGLDefaultFramebuffer _defaultFramebuffer;
        private readonly int _vertexArrayID;

        public DebugSeverity MinimumLogSeverity { get; set; } = DebugSeverity.DebugSeverityLow;

        public OpenGLRenderContext(OpenTKWindow window)
            : base(window)
        {
            _resourceFactory = new OpenGLResourceFactory();

#if DEBUG
            _openGLGraphicsContext = new GraphicsContext(GraphicsMode.Default, window.OpenTKWindowInfo, 4, 3, GraphicsContextFlags.Debug);
#else
            _openGLGraphicsContext = new GraphicsContext(GraphicsMode.Default, window.OpenTKWindowInfo);
#endif
            _openGLGraphicsContext.MakeCurrent(window.OpenTKWindowInfo);

            _openGLGraphicsContext.LoadAll();

            // NOTE: I am binding a single VAO globally. This may or may not be a good idea.
            _vertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayID);

            _defaultFramebuffer = new OpenGLDefaultFramebuffer();

            SetInitialStates();
            OnWindowResized();

            PostContextCreated();

#if DEBUG
            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(DebugCallback, IntPtr.Zero);
#endif
        }

        private void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (severity >= MinimumLogSeverity)
            {
                string messageString = Marshal.PtrToStringAnsi(message, length);
                Console.WriteLine($"GL DEBUG MESSAGE: {source}, {type}, {id}. {severity}: {messageString}");
            }
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Documentation indicates that this needs to be called on OSX for proper behavior.
                _openGLGraphicsContext.Update(((OpenTKWindow)Window).OpenTKWindowInfo);
            }
        }

        protected override void PlatformSetViewport(int x, int y, int width, int height)
        {
            GL.Viewport(x, y, width, height);
        }

        protected override void PlatformSetDefaultFramebuffer()
        {
            SetFramebuffer(_defaultFramebuffer);
        }

        protected override void PlatformSetScissorRectangle(System.Drawing.Rectangle rectangle)
        {
            GL.Scissor(
                rectangle.Left,
                Window.Height - rectangle.Bottom,
                rectangle.Width,
                rectangle.Height);
        }

        protected override void PlatformClearMaterialResourceBindings()
        {
        }

        protected override void PlatformDispose()
        {
            _openGLGraphicsContext.Dispose();
        }
    }
}
