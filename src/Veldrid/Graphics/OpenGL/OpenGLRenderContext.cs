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

        private readonly OpenGLResourceFactory _resourceFactory;
        private readonly GraphicsContext _openGLGraphicsContext;
        private readonly OpenGLExtensions _extensions;
        private readonly OpenGLDefaultFramebuffer _defaultFramebuffer;
        private readonly int _maxConstantBufferSlots;
        private readonly OpenGLConstantBuffer[] _constantBuffersBySlot;
        private readonly OpenGLConstantBuffer[] _newConstantBuffersBySlot; // CB's bound during draw call preparation
        private int _newConstantBuffersCount;
        private readonly int _vertexArrayID;
        private readonly int _maxVertexAttributeSlots;
        private readonly int[] _vertexAttribDivisors;
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
            HashSet<string> extensions = new HashSet<string>();
            for (int i = 0; i < extensionCount; i++)
            {
                extensions.Add(GL.GetString(StringNameIndexed.Extensions, i));
            }
            _extensions = new OpenGLExtensions(extensions);

            _maxConstantBufferSlots = GL.GetInteger(GetPName.MaxUniformBufferBindings);
            _constantBuffersBySlot = new OpenGLConstantBuffer[_maxConstantBufferSlots];
            _newConstantBuffersBySlot = new OpenGLConstantBuffer[_maxConstantBufferSlots];

            _maxVertexAttributeSlots = GL.GetInteger(GetPName.MaxVertexAttribs);
            _vertexAttribDivisors = new int[_maxVertexAttributeSlots];
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

        protected override GraphicsBackend PlatformGetGraphicsBackend() => GraphicsBackend.OpenGL;

        public OpenGLExtensions Extensions => _extensions;

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
            if (slot > _maxConstantBufferSlots)
            {
                throw new VeldridException($"Too many constant buffers used. Limit is {_maxConstantBufferSlots}.");
            }

            // Bind Constant Buffer to slot
            if (_constantBuffersBySlot[slot] == cb)
            {
                if (_newConstantBuffersBySlot[slot] != null)
                {
                    _newConstantBuffersCount -= 1;
                }
                _newConstantBuffersBySlot[slot] = null;
            }
            else
            {
                if (_newConstantBuffersBySlot[slot] == null)
                {
                    _newConstantBuffersCount += 1;
                }

                _newConstantBuffersBySlot[slot] = cb;
            }

            // Bind slot to uniform block location. Performs internal caching to avoid GL calls.
            shaderSet.BindConstantBuffer(slot, blockLocation, cb);
        }

        private void CommitNewConstantBufferBindings()
        {
            if (_newConstantBuffersCount > 0)
            {
                if (_extensions.ARB_MultiBind)
                {
                    CommitNewConstantBufferBindings_MultiBind();
                }
                else
                {
                    CommitNewConstantBufferBindings_SingleBind();
                }

                Array.Clear(_newConstantBuffersBySlot, 0, _newConstantBuffersBySlot.Length);
                _newConstantBuffersCount = 0;
            }
        }

        private unsafe void CommitNewConstantBufferBindings_MultiBind()
        {
            Debug.Assert(_extensions.ARB_MultiBind);
            int* buffers = stackalloc int[_maxConstantBufferSlots];
            IntPtr* sizes = stackalloc IntPtr[_maxConstantBufferSlots];
            IntPtr* offsets = stackalloc IntPtr[_maxConstantBufferSlots];
            int currentIndex = 0; // Index into stack allocated buffers.
            int currentBaseSlot = -1;
            int remainingBuffers = _newConstantBuffersCount;

            void AddBinding(OpenGLConstantBuffer cb)
            {
                buffers[currentIndex] = cb.BufferID;
                sizes[currentIndex] = new IntPtr(cb.BufferSize);

                currentIndex += 1;
            }

            void EmitBindings()
            {
                int count = currentIndex;
                GL.BindBuffersRange(BufferRangeTarget.UniformBuffer, currentBaseSlot, count, buffers, offsets, sizes);
                Utilities.CheckLastGLError();
                currentIndex = 0;
                currentBaseSlot = -1;
                remainingBuffers -= count;
                Debug.Assert(remainingBuffers >= 0);
            }

            for (int slot = 0; slot < _maxConstantBufferSlots; slot++)
            {
                OpenGLConstantBuffer cb = _newConstantBuffersBySlot[slot];
                if (cb != null)
                {
                    AddBinding(cb);
                    if (currentBaseSlot == -1)
                    {
                        currentBaseSlot = slot;
                    }
                    _constantBuffersBySlot[slot] = cb;
                }
                else if (currentIndex != 0)
                {
                    EmitBindings();
                    if (remainingBuffers == 0)
                    {
                        return;
                    }
                }
            }

            if (currentIndex != 0)
            {
                EmitBindings();
            }
        }

        private void CommitNewConstantBufferBindings_SingleBind()
        {
            int remainingBindings = _newConstantBuffersCount;
            for (int slot = 0; slot < _maxConstantBufferSlots; slot++)
            {
                if (remainingBindings == 0)
                {
                    return;
                }

                OpenGLConstantBuffer cb = _newConstantBuffersBySlot[slot];
                if (cb != null)
                {
                    GL.BindBufferRange(BufferRangeTarget.UniformBuffer, slot, cb.BufferID, IntPtr.Zero, cb.BufferSize);
                    remainingBindings -= 1;
                }
            }
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
            OpenGLTexture boundTexture = (OpenGLTexture)textureBinding.BoundTexture;
            if (!_boundTexturesBySlot.TryGetValue(slot, out DeviceTexture oldBoundTexture) || oldBoundTexture != textureBinding.BoundTexture)
            {
                if (_extensions.ARB_DirectStateAccess)
                {
                    GL.BindTextureUnit(slot, boundTexture.ID);
                }
                else
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + slot);
                    boundTexture.Bind();
                }
                _boundTexturesBySlot[slot] = boundTexture;
            }

            int uniformLocation = ShaderTextureBindingSlots.GetUniformLocation(slot);
            ShaderSet.UpdateTextureUniform(uniformLocation, slot); // Performs internal caching.

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
                SetVertexAttributes(ShaderSet.InputLayout, VertexBuffers);
                _vertexLayoutChanged = false;
            }

            CommitNewConstantBufferBindings();
        }

        private void SetVertexAttributes(OpenGLVertexInputLayout inputlayout, VertexBuffer[] vertexBuffers)
        {
            int totalSlotsBound = 0;
            for (int i = 0; i < inputlayout.VBLayoutsBySlot.Length; i++)
            {
                OpenGLVertexInput input = inputlayout.VBLayoutsBySlot[i];
                ((OpenGLVertexBuffer)vertexBuffers[i]).Apply();
                for (int slot = 0; slot < input.Elements.Length; slot++)
                {
                    ref OpenGLVertexInputElement element = ref input.Elements[slot]; // Large structure -- use by reference.
                    int actualSlot = totalSlotsBound + slot;
                    if (actualSlot >= _vertexAttributesBound)
                    {
                        GL.EnableVertexAttribArray(actualSlot);
                    }

                    GL.VertexAttribPointer(actualSlot, element.ElementCount, element.Type, element.Normalized, input.VertexSizeInBytes, element.Offset);

                    int stepRate = element.InstanceStepRate;
                    if (_vertexAttribDivisors[actualSlot] != stepRate)
                    {
                        GL.VertexAttribDivisor(actualSlot, stepRate);
                        _vertexAttribDivisors[actualSlot] = stepRate;
                    }
                }

                totalSlotsBound += input.Elements.Length;
            }

            for (int extraSlot = totalSlotsBound; extraSlot < _vertexAttributesBound; extraSlot++)
            {
                GL.DisableVertexAttribArray(extraSlot);
            }

            _vertexAttributesBound = totalSlotsBound;
        }

        private new OpenGLTextureBindingSlots ShaderTextureBindingSlots => (OpenGLTextureBindingSlots)base.ShaderTextureBindingSlots;

        private new OpenGLShaderConstantBindingSlots ShaderConstantBindingSlots => (OpenGLShaderConstantBindingSlots)base.ShaderConstantBindingSlots;

        private new OpenGLShaderSet ShaderSet => (OpenGLShaderSet)base.ShaderSet;
    }
}
