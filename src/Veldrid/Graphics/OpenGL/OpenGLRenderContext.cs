using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System.Runtime.InteropServices;
using OpenTK;
using Veldrid.Platform;
using System.Diagnostics;
using System.Collections.Generic;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLRenderContext : RenderContext, IDisposable
    {
        private const int MaxConstantBufferSlots = 30; // Real limit?

        private readonly OpenGLResourceFactory _resourceFactory;
        private readonly GraphicsContext _openGLGraphicsContext;
        private readonly HashSet<string> _extensions;
        private readonly OpenGLDefaultFramebuffer _defaultFramebuffer;
        private readonly OpenGLConstantBuffer[] _constantBuffersBySlot = new OpenGLConstantBuffer[MaxConstantBufferSlots];
        private readonly int _vertexArrayID;
        private PrimitiveType _primitiveType = PrimitiveType.Triangles;
        private int _vertexAttributesBound;
        private bool _vertexLayoutChanged;
        private Action _swapBufferFunc;
        private DebugProc _debugMessageCallback;

        public OpenGLRenderContext(Window window, OpenGLPlatformContextInfo platformContext)
        {
            _resourceFactory = new OpenGLResourceFactory();
            RenderCapabilities = new RenderCapabilities(true, true);
            _swapBufferFunc = platformContext.SwapBuffer;
            GraphicsContext.GetAddressDelegate getAddressFunc = s => platformContext.GetProcAddress(s);
            GraphicsContext.GetCurrentContextDelegate getCurrentContextFunc = () => new ContextHandle(platformContext.GetCurrentContext());
            _openGLGraphicsContext = new GraphicsContext(new ContextHandle(platformContext.ContextHandle), getAddressFunc, getCurrentContextFunc);

            _openGLGraphicsContext.LoadAll();

            // NOTE: I am binding a single VAO globally.
            _vertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayID);

            _defaultFramebuffer = new OpenGLDefaultFramebuffer(window.Width, window.Height);
            OnWindowResized(window.Width, window.Height);

            SetInitialStates();

            PostContextCreated();

            int extensionCount = GL.GetInteger(GetPName.NumExtensions);
            _extensions = new HashSet<string>();
            for (int i = 0; i < extensionCount; i++)
            {
                _extensions.Add(GL.GetString(StringNameIndexed.Extensions, i));
            }
        }

        public void EnableDebugCallback() => EnableDebugCallback(DebugSeverity.DebugSeverityNotification);
        public void EnableDebugCallback(DebugSeverity minimumSeverity) => EnableDebugCallback(DefaultDebugCallback(minimumSeverity));
        public void EnableDebugCallback(DebugProc callback)
        {
            GL.Enable(EnableCap.DebugOutput);
            // The debug callback delegate must be persisted, otherwise errors will occur
            // when the OpenGL drivers attempt to call it after it has been collected.
            _debugMessageCallback = callback;
            GL.DebugMessageCallback(_debugMessageCallback, IntPtr.Zero);
        }

        private DebugProc DefaultDebugCallback(DebugSeverity minimumSeverity)
        {
            return (DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam) =>
            {
                if (severity >= minimumSeverity)
                {
                    string messageString = Marshal.PtrToStringAnsi(message, length);
                    System.Diagnostics.Debug.WriteLine($"GL DEBUG MESSAGE: {source}, {type}, {id}. {severity}: {messageString}");
                }
            };
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
                Color4 openTKColor = new Color4(value.R, value.G, value.B, value.A);
                GL.ClearColor(openTKColor);
            }
        }

        protected override void PlatformClearBuffer()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        protected override void PlatformSwapBuffers()
        {
            if (_swapBufferFunc != null)
            {
                _swapBufferFunc();
            }
            else
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
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        protected override void PlatformResize(int width, int height)
        {
            _defaultFramebuffer.Width = width;
            _defaultFramebuffer.Height = height;
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
                Viewport.Height - rectangle.Bottom,
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

        protected override void PlatformSetShaderConstantBindings(ShaderConstantBindingSlots shaderConstantBindings)
        {
        }

        protected override void PlatformSetConstantBuffer(int slot, ConstantBuffer cb)
        {
            OpenGLShaderConstantBindingSlots.UniformBinding binding = ShaderConstantBindingSlots.GetUniformBindingForSlot(slot);
            if (binding.BlockLocation != -1)
            { 
                BindUniformBlock(ShaderSet, slot, binding.BlockLocation, (OpenGLConstantBuffer)cb);
            }
            else
            {
                Debug.Assert(binding.StorageAdapter != null);
                SetUniformLocationDataSlow((OpenGLConstantBuffer)cb, binding.StorageAdapter);
            }
        }

        private void BindUniformBlock(OpenGLShaderSet shaderSet, int slot, int blockLocation, OpenGLConstantBuffer cb)
        {
            if (slot > MaxConstantBufferSlots)
            {
                throw new InvalidOperationException($"Too many constant buffers used. Limit is {MaxConstantBufferSlots}.");
            }

            // Bind Constant Buffer to slot
            if (_constantBuffersBySlot[slot] != cb)
            {
                // TODO: Defer this work and use GL.BindBuffersRange
                GL.BindBufferRange(BufferRangeTarget.UniformBuffer, slot, cb.BufferID, IntPtr.Zero, cb.BufferSize);
                _constantBuffersBySlot[slot] = cb;
            }

            // Bind slot to uniform block location.
            shaderSet.BindConstantBuffer(slot, blockLocation, cb);
        }

        private unsafe void SetUniformLocationDataSlow(OpenGLConstantBuffer cb, OpenGLUniformStorageAdapter storageAdapter)
        {
            // NOTE: This is slow -- avoid using uniform locations in shader code. Prefer uniform blocks.
            int dataSizeInBytes = cb.BufferSize;
            byte* data = stackalloc byte[dataSizeInBytes];
            cb.GetData((IntPtr)data, dataSizeInBytes);
            storageAdapter.SetData((IntPtr)data, dataSizeInBytes);
        }

        protected override void PlatformSetShaderTextureBindingSlots(ShaderTextureBindingSlots bindingSlots)
        {
        }

        protected override void PlatformSetTexture(int slot, ShaderTextureBinding textureBinding)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            OpenGLTexture boundTexture = (OpenGLTexture)textureBinding.BoundTexture;
            boundTexture.Bind();
            int uniformLocation = ShaderTextureBindingSlots.GetUniformLocation(slot);
            GL.Uniform1(uniformLocation, slot);

            _boundTexturesBySlot[slot] = boundTexture;
            EnsureSamplerMipmapState(slot, boundTexture.MipLevels != 1);
        }

        protected override void PlatformSetSamplerState(int slot, SamplerState samplerState, bool mipmapped)
        {
            OpenGLSamplerState glSamplerState = (OpenGLSamplerState)samplerState;
            glSamplerState.Apply(slot, mipmapped);
        }

        private void EnsureSamplerMipmapState(int slot, bool mipmap)
        {
            if (_boundSamplersBySlot.TryGetValue(slot, out BoundSamplerStateInfo info))
            {
                if (info.SamplerState != null && info.Mipmapped != mipmap)
                {
                    ((OpenGLSamplerState)info.SamplerState).Apply(slot, mipmap);
                    _boundSamplersBySlot[slot] = new BoundSamplerStateInfo(info.SamplerState, mipmap);
                }
            }
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

        private new OpenGLShaderConstantBindingSlots ShaderConstantBindingSlots => (OpenGLShaderConstantBindingSlots)base.ShaderConstantBindingSlots;

        private new OpenGLShaderSet ShaderSet => (OpenGLShaderSet)base.ShaderSet;
    }
}
