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
        private int _vertexAttributesBound;
        private bool _vertexLayoutChanged;

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
            RenderCapabilities = new RenderCapabilities(true, true);

            if (debugContext)
            {
                _openGLGraphicsContext = new GraphicsContext(GraphicsMode.Default, window.OpenTKWindowInfo, 4, 3, GraphicsContextFlags.Debug);
            }
            else
            {
                _openGLGraphicsContext = new GraphicsContext(GraphicsMode.Default, window.OpenTKWindowInfo, 3, 3, GraphicsContextFlags.ForwardCompatible);
            }
            _openGLGraphicsContext.MakeCurrent(window.OpenTKWindowInfo);

            _openGLGraphicsContext.LoadAll();

            // NOTE: I am binding a single VAO globally. This may or may not be a good idea.
            _vertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayID);

            _defaultFramebuffer = new OpenGLDefaultFramebuffer(Window);

            SetInitialStates();
            OnWindowResized(Window.Width, Window.Height);

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
                System.Diagnostics.Debug.WriteLine($"GL DEBUG MESSAGE: {source}, {type}, {id}. {severity}: {messageString}");
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
            PreDrawCommand();
            var elementsType = ((OpenGLIndexBuffer)IndexBuffer).ElementsType;
            int indexSize = OpenGLFormats.GetIndexFormatSize(elementsType);

            GL.DrawElements(_primitiveType, count, elementsType, new IntPtr(startingIndex * indexSize));
        }

        public override void DrawIndexedPrimitives(int count, int startingIndex, int startingVertex)
        {
            PreDrawCommand();
            var elementsType = ((OpenGLIndexBuffer)IndexBuffer).ElementsType;
            int indexSize = OpenGLFormats.GetIndexFormatSize(elementsType);
            GL.DrawElementsBaseVertex(_primitiveType, count, elementsType, new IntPtr(startingIndex * indexSize), startingVertex);
        }

        public override void DrawInstancedPrimitives(int indexCount, int instanceCount, int startingIndex)
        {
            PreDrawCommand();
            var elementsType = ((OpenGLIndexBuffer)IndexBuffer).ElementsType;
            int indexSize = OpenGLFormats.GetIndexFormatSize(elementsType);
            GL.DrawElementsInstanced(_primitiveType, indexCount, elementsType, new IntPtr(startingIndex * indexSize), instanceCount);
        }

        public override void DrawInstancedPrimitives(int indexCount, int instanceCount, int startingIndex, int startingVertex)
        {
            PreDrawCommand();
            var elementsType = ((OpenGLIndexBuffer)IndexBuffer).ElementsType;
            int indexSize = OpenGLFormats.GetIndexFormatSize(elementsType);
            GL.DrawElementsInstancedBaseVertex(
                _primitiveType,
                indexCount,
                elementsType,
                new IntPtr(startingIndex * indexSize),
                instanceCount,
                startingVertex);
        }

        private void SetInitialStates()
        {
            GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        protected override void PlatformResize(int width, int height)
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

        protected override void PlatformSetScissorRectangle(Rectangle rectangle)
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
            _vertexLayoutChanged = true;
        }

        protected override void PlatformSetIndexBuffer(IndexBuffer ib)
        {
            ((OpenGLIndexBuffer)ib).Apply();
            _vertexLayoutChanged = true;
        }

        protected override void PlatformSetShaderSet(ShaderSet shaderSet)
        {
            OpenGLShaderSet glShaderSet = (OpenGLShaderSet)shaderSet;
            GL.UseProgram(glShaderSet.ProgramID);
            _vertexLayoutChanged = true;
        }

        protected override void PlatformSetShaderConstantBindings(ShaderConstantBindings shaderConstantBindings)
        {
            shaderConstantBindings.Apply();
        }

        protected override void PlatformSetShaderTextureBindingSlots(ShaderTextureBindingSlots bindingSlots)
        {
        }

        protected override void PlatformSetTexture(int slot, ShaderTextureBinding textureBinding)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            ((OpenGLTexture)textureBinding.BoundTexture).Bind();
            int uniformLocation = ShaderTextureBindingSlots.GetUniformLocation(slot);
            GL.Uniform1(uniformLocation, slot);
        }

        protected override void PlatformSetFramebuffer(Framebuffer framebuffer)
        {
            OpenGLFramebufferBase baseFramebuffer = (OpenGLFramebufferBase)framebuffer;
            if (baseFramebuffer is OpenGLFramebuffer)
            {
                OpenGLFramebuffer glFramebuffer = (OpenGLFramebuffer)baseFramebuffer;
                if (!glFramebuffer.HasDepthAttachment || !DepthStencilState.IsDepthEnabled)
                {
                    GL.Disable(EnableCap.DepthTest);
                    GL.DepthMask(false);
                }
                else
                {
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthMask(DepthStencilState.IsDepthWriteEnabled);
                }
            }

            baseFramebuffer.Apply();
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

        public override RenderCapabilities RenderCapabilities { get; }

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

        private void PreDrawCommand()
        {
            if (_vertexLayoutChanged)
            {
                _vertexAttributesBound = ((OpenGLVertexInputLayout)ShaderSet.InputLayout).SetVertexAttributes(VertexBuffers, _vertexAttributesBound);
                _vertexLayoutChanged = false;
            }
        }

        private new OpenGLTextureBindingSlots ShaderTextureBindingSlots => (OpenGLTextureBindingSlots)base.ShaderTextureBindingSlots;
    }
}
