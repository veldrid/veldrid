using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Veldrid.Platform;
using System.Runtime.InteropServices;
using OpenTK;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLRenderContext : RenderContext, IDisposable
    {
        private readonly OpenGLResourceFactory _resourceFactory;
        private readonly GraphicsContext _openGLGraphicsContext;
        private readonly OpenGLDefaultFramebuffer _defaultFramebuffer;
        private readonly int _vertexArrayID;
        private PrimitiveType _primitiveType = PrimitiveType.Triangles;

        public DebugSeverity MinimumLogSeverity { get; set; } = DebugSeverity.DebugSeverityLow;

        public OpenGLRenderContext(OpenTKWindow window,
#if DEBUG
        bool debugContext = true)
#else
        bool debugContext = false)
#endif
            : base(window)
        {
            _resourceFactory = new OpenGLResourceFactory();

            if (debugContext)
            {
                _openGLGraphicsContext = new GraphicsContext(GraphicsMode.Default, window.OpenTKWindowInfo, 4, 3, GraphicsContextFlags.Debug);
            }
            else
            {
                _openGLGraphicsContext = new GraphicsContext(GraphicsMode.Default, window.OpenTKWindowInfo);
            }
            _openGLGraphicsContext.MakeCurrent(window.OpenTKWindowInfo);

            _openGLGraphicsContext.LoadAll();

            // NOTE: I am binding a single VAO globally. This may or may not be a good idea.
            _vertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayID);

            _defaultFramebuffer = new OpenGLDefaultFramebuffer(Window);

            SetInitialStates();
            OnWindowResized();

            PostContextCreated();

            if (debugContext)
            {
                GL.Enable(EnableCap.DebugOutput);
                GL.DebugMessageCallback(DebugCallback, IntPtr.Zero);
            }
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

            GL.DrawElements(_primitiveType, count, elementsType, new IntPtr(startingIndex * indexSize));
        }

        public override void DrawIndexedPrimitives(int count, int startingIndex, int startingVertex)
        {
            var elementsType = ((OpenGLIndexBuffer)IndexBuffer).ElementsType;
            int indexSize = OpenGLFormats.GetIndexFormatSize(elementsType);
            GL.DrawElementsBaseVertex(_primitiveType, count, elementsType, new IntPtr(startingIndex * indexSize), startingVertex);
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

        protected override void PlatformSetPrimitiveTopology(PrimitiveTopology primitiveTopology)
        {
            _primitiveType = OpenGLFormats.ConvertPrimitiveTopology(primitiveTopology);
        }

        protected override void PlatformSetDefaultFramebuffer()
        {
            SetFramebuffer(_defaultFramebuffer);
        }

        protected override void PlatformSetScissorRectangle(System.Drawing.Rectangle rectangle)
        {
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(
                rectangle.Left,
                Window.Height - rectangle.Bottom,
                rectangle.Width,
                rectangle.Height);
        }

        public override void ClearScissorRectangle()
        {
            GL.Disable(EnableCap.ScissorTest);
        }

        protected override void PlatformSetVertexBuffer(int slot, VertexBuffer vb)
        {
            // TODO: This is going to cause vertex attributes to be set at least twice,
            // because they are also set when the Material is initially applied.
            ((OpenGLMaterial)Material)?.SetVertexAttributes(slot, (OpenGLVertexBuffer)vb);
        }

        protected override void PlatformSetIndexBuffer(IndexBuffer ib)
        {
            ((OpenGLIndexBuffer)ib).Apply();
            //((OpenGLMaterial)Material)?.SetVertexAttributes();
        }

        protected override void PlatformSetMaterial(Material material)
        {
            ((OpenGLMaterial)material).Apply(VertexBuffers);
        }

        protected override void PlatformSetFramebuffer(Framebuffer framebuffer)
        {
            ((OpenGLFramebufferBase)framebuffer).Apply();
        }

        protected override void PlatformSetBlendstate(BlendState blendState)
        {
            ((OpenGLBlendState)blendState).Apply();
        }

        protected override void PlatformSetDepthStencilState(DepthStencilState depthStencilState)
        {
            ((OpenGLDepthStencilState)depthStencilState).Apply();
        }

        protected override void PlatformSetRasterizerState(RasterizerState rasterizerState)
        {
            ((OpenGLRasterizerState)rasterizerState).Apply();
        }

        protected override void PlatformClearMaterialResourceBindings()
        {
        }

        protected override void PlatformDispose()
        {
            _openGLGraphicsContext.Dispose();
        }

        protected override System.Numerics.Vector2 GetTopLeftUvCoordinate()
        {
            return new System.Numerics.Vector2(0, 1);
        }

        protected override System.Numerics.Vector2 GetBottomRightUvCoordinate()
        {
            return new System.Numerics.Vector2(1, 0);
        }
    }
}
