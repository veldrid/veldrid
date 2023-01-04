using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace Veldrid
{
    /// <summary>
    /// Can render draw lists produced by ImGui, using a <see cref="CommandBuffer"/>.
    /// Also provides functions for updating ImGui input.
    /// </summary>
    public class ImGuiCBRenderer : IDisposable
    {
        private GraphicsDevice _gd;
        private readonly uint _bufferCount;
        private readonly Assembly _assembly;
        private ColorSpaceHandling _colorSpaceHandling;

        // Device objects
        private Texture _fontTexture;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private ResourceLayout _layout;
        private ResourceLayout _textureLayout;
        private Pipeline _pipeline;
        private ResourceSet _fontTextureResourceSet;

        // Multi-buffered resources
        private DeviceBuffer[] _vertexBuffers;
        private DeviceBuffer[] _indexBuffers;
        private DeviceBuffer[] _projMatrixBuffers;
        private ResourceSet[] _mainResourceSets;

        private IntPtr _fontAtlasID = (IntPtr)1;
        private bool _controlDown;
        private bool _shiftDown;
        private bool _altDown;

        private int _windowWidth;
        private int _windowHeight;
        private Vector2 _scaleFactor = Vector2.One;

        // Image trackers
        private readonly Dictionary<TextureView, ResourceSetInfo> _setsByView
            = new Dictionary<TextureView, ResourceSetInfo>();
        private readonly Dictionary<Texture, TextureView> _autoViewsByTexture
            = new Dictionary<Texture, TextureView>();
        private readonly Dictionary<IntPtr, ResourceSetInfo> _viewsById = new Dictionary<IntPtr, ResourceSetInfo>();
        private readonly List<IDisposable> _ownedResources = new List<IDisposable>();
        private int _lastAssignedID = 100;
        private bool _frameBegun;
        private uint _frameIndex;

        private readonly InputState _inputState = new InputState();

        /// <summary>
        /// Constructs a new ImGuiCBRenderer.
        /// </summary>
        /// <param name="gd">The GraphicsDevice used to create and update resources.</param>
        /// <param name="outputDescription">The output format.</param>
        /// <param name="width">The initial width of the rendering target. Can be resized.</param>
        /// <param name="height">The initial height of the rendering target. Can be resized.</param>
        /// <param name="colorSpaceHandling">Identifies how the renderer should treat vertex colors.</param>
        public ImGuiCBRenderer(
            GraphicsDevice gd,
            OutputDescription outputDescription,
            int width, int height,
            ColorSpaceHandling colorSpaceHandling,
            uint bufferCount)
        {
            _gd = gd;
            _bufferCount = bufferCount;
            _assembly = typeof(ImGuiCBRenderer).GetTypeInfo().Assembly;
            _colorSpaceHandling = colorSpaceHandling;
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGui.GetIO().Fonts.AddFontDefault();
            if (_gd.Features.DrawBaseVertex)
            {
                ImGui.GetIO().BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            }

            CreateDeviceResources(gd, outputDescription, _colorSpaceHandling);
            SetOpenTKKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            _frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription, ColorSpaceHandling colorSpaceHandling)
        {
            _gd = gd;
            _colorSpaceHandling = colorSpaceHandling;
            ResourceFactory factory = gd.ResourceFactory;

            _vertexBuffers = new DeviceBuffer[_bufferCount];
            for (uint i = 0; i < _bufferCount; i++)
            {
                _vertexBuffers[i] = factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
                _vertexBuffers[i].Name = "ImGui.NET Vertex Buffer";
            }

            _indexBuffers = new DeviceBuffer[_bufferCount];
            for (uint i = 0; i < _bufferCount; i++)
            {
                _indexBuffers[i] = factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
                _indexBuffers[i].Name = "ImGui.NET Index Buffer";
            }

            _projMatrixBuffers = new DeviceBuffer[_bufferCount];
            for (uint i = 0; i < _bufferCount; i++)
            {
                _projMatrixBuffers[i] = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
                _projMatrixBuffers[i].Name = "ImGui.NET Projection Buffer";
            }

            byte[] vertexShaderBytes = LoadEmbeddedShaderCode(gd.ResourceFactory, "imgui-vertex", ShaderStages.Vertex, _colorSpaceHandling);
            byte[] fragmentShaderBytes = LoadEmbeddedShaderCode(gd.ResourceFactory, "imgui-frag", ShaderStages.Fragment, _colorSpaceHandling);
            _vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, _gd.BackendType == GraphicsBackend.Vulkan ? "main" : "VS"));
            _fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, _gd.BackendType == GraphicsBackend.Vulkan ? "main" : "FS"));

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                    new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
            };

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("FontSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("FontTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                new DepthStencilStateDescription(false, false, ComparisonKind.Always),
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    vertexLayouts,
                    new[] { _vertexShader, _fragmentShader },
                    new[]
                    {
                        new SpecializationConstant(0, gd.IsClipSpaceYInverted),
                        new SpecializationConstant(1, _colorSpaceHandling == ColorSpaceHandling.Legacy),
                    }),
                new ResourceLayout[] { _layout, _textureLayout },
                outputDescription,
                ResourceBindingModel.Default);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _mainResourceSets = new ResourceSet[_bufferCount];
            for (uint i = 0; i < _bufferCount; i++)
            {
                _mainResourceSets[i] = factory.CreateResourceSet(new ResourceSetDescription(_layout,
                    _projMatrixBuffers[i],
                    gd.PointSampler));
            }

            RecreateFontDeviceTexture(gd);
        }

        /// <summary>
        /// Gets or creates a handle for a texture to be drawn with ImGui.
        /// Pass the returned handle to Image() or ImageButton().
        /// </summary>
        public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView)
        {
            if (!_setsByView.TryGetValue(textureView, out ResourceSetInfo rsi))
            {
                ResourceSet resourceSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, textureView));
                rsi = new ResourceSetInfo(GetNextImGuiBindingID(), resourceSet);

                _setsByView.Add(textureView, rsi);
                _viewsById.Add(rsi.ImGuiBinding, rsi);
                _ownedResources.Add(resourceSet);
            }

            return rsi.ImGuiBinding;
        }

        public void RemoveImGuiBinding(TextureView textureView)
        {
            if (_setsByView.TryGetValue(textureView, out ResourceSetInfo rsi))
            {
                _setsByView.Remove(textureView);
                _viewsById.Remove(rsi.ImGuiBinding);
                _ownedResources.Remove(rsi.ResourceSet);
                rsi.ResourceSet.Dispose();
            }
        }

        private IntPtr GetNextImGuiBindingID()
        {
            int newID = _lastAssignedID++;
            return (IntPtr)newID;
        }

        /// <summary>
        /// Gets or creates a handle for a texture to be drawn with ImGui.
        /// Pass the returned handle to Image() or ImageButton().
        /// </summary>
        public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture)
        {
            if (!_autoViewsByTexture.TryGetValue(texture, out TextureView textureView))
            {
                textureView = factory.CreateTextureView(texture);
                _autoViewsByTexture.Add(texture, textureView);
                _ownedResources.Add(textureView);
            }

            return GetOrCreateImGuiBinding(factory, textureView);
        }

        public void RemoveImGuiBinding(Texture texture)
        {
            if (_autoViewsByTexture.TryGetValue(texture, out TextureView textureView))
            {
                _autoViewsByTexture.Remove(texture);
                _ownedResources.Remove(textureView);
                textureView.Dispose();
                RemoveImGuiBinding(textureView);
            }
        }

        /// <summary>
        /// Retrieves the shader texture binding for the given helper handle.
        /// </summary>
        public ResourceSet GetImageResourceSet(IntPtr imGuiBinding)
        {
            if (!_viewsById.TryGetValue(imGuiBinding, out ResourceSetInfo rsi))
            {
                throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding.ToString());
            }

            return rsi.ResourceSet;
        }

        public void ClearCachedImageResources()
        {
            foreach (IDisposable resource in _ownedResources)
            {
                resource.Dispose();
            }

            _ownedResources.Clear();
            _setsByView.Clear();
            _viewsById.Clear();
            _autoViewsByTexture.Clear();
            _lastAssignedID = 100;
        }

        private byte[] LoadEmbeddedShaderCode(
            ResourceFactory factory,
            string name,
            ShaderStages stage,
            ColorSpaceHandling colorSpaceHandling)
        {
            switch (factory.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    {
                        if (stage == ShaderStages.Vertex && colorSpaceHandling == ColorSpaceHandling.Legacy) { name += "-legacy"; }
                        string resourceName = name + ".hlsl.bytes";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.OpenGL:
                    {
                        if (stage == ShaderStages.Vertex && colorSpaceHandling == ColorSpaceHandling.Legacy) { name += "-legacy"; }
                        string resourceName = name + ".glsl";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.OpenGLES:
                    {
                        if (stage == ShaderStages.Vertex && colorSpaceHandling == ColorSpaceHandling.Legacy) { name += "-legacy"; }
                        string resourceName = name + ".glsles";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.Vulkan:
                    {
                        string resourceName = name + ".spv";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.Metal:
                    {
                        string resourceName = name + ".metallib";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private string GetEmbeddedResourceText(string resourceName)
        {
            using (StreamReader sr = new StreamReader(_assembly.GetManifestResourceStream(resourceName)))
            {
                return sr.ReadToEnd();
            }
        }

        private byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            using (Stream s = _assembly.GetManifestResourceStream(resourceName))
            {
                byte[] ret = new byte[s.Length];
                s.Read(ret, 0, (int)s.Length);
                return ret;
            }
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture() => RecreateFontDeviceTexture(_gd);

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture(GraphicsDevice gd)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            // Build
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

            // Store our identifier
            io.Fonts.SetTexID(_fontAtlasID);

            _fontTexture?.Dispose();
            _fontTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)width,
                (uint)height,
                1,
                1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled));
            _fontTexture.Name = "ImGui.NET Font Texture";
            gd.UpdateTexture(
                _fontTexture,
                (IntPtr)pixels,
                (uint)(bytesPerPixel * width * height),
                0,
                0,
                0,
                (uint)width,
                (uint)height,
                1,
                0,
                0);

            _fontTextureResourceSet?.Dispose();
            _fontTextureResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _fontTexture));

            io.Fonts.ClearTexData();
        }

        public unsafe void Render(GraphicsDevice gd, CommandBuffer cb)
        {
            if (_frameBegun)
            {
                _frameIndex = (_frameIndex + 1) % _bufferCount;
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData(), gd, cb);
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(float deltaSeconds, InputSnapshot snapshot)
        {
            _inputState.Clear();
            _inputState.AddSnapshot(snapshot);
            _inputState.WheelDelta = 0f;

            Update(deltaSeconds, _inputState.View);

        }

        public void Update(float deltaSeconds, InputStateView inputState)
        {
            UpdateImGuiInput(inputState);

            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);

            _frameBegun = true;
            ImGui.NewFrame();
        }

        private unsafe void UpdateImGuiInput(InputStateView inputState)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            // Determine if any of the mouse buttons were pressed during this snapshot period, even if they are no longer held.
            bool leftPressed = false;
            bool middlePressed = false;
            bool rightPressed = false;
            for (int i = 0; i < inputState.MouseEvents.Count; i++)
            {
                MouseEvent me = inputState.MouseEvents[i];
                if (me.Down)
                {
                    switch (me.MouseButton)
                    {
                        case MouseButton.Left:
                            leftPressed = true;
                            break;
                        case MouseButton.Middle:
                            middlePressed = true;
                            break;
                        case MouseButton.Right:
                            rightPressed = true;
                            break;
                    }
                }
            }

            io.MouseDown[0] = leftPressed || inputState.IsMouseDown(MouseButton.Left);
            io.MouseDown[1] = rightPressed || inputState.IsMouseDown(MouseButton.Right);
            io.MouseDown[2] = middlePressed || inputState.IsMouseDown(MouseButton.Middle);
            io.MousePos = inputState.MousePosition;
            io.MouseWheel = inputState.WheelDelta;

            IReadOnlyList<char> keyCharPresses = inputState.KeyCharPresses;
            for (int i = 0; i < keyCharPresses.Count; i++)
            {
                char c = keyCharPresses[i];
                ImGui.GetIO().AddInputCharacter(c);
            }

            IReadOnlyList<KeyEvent> keyEvents = inputState.KeyEvents;
            for (int i = 0; i < keyEvents.Count; i++)
            {
                KeyEvent keyEvent = keyEvents[i];
                io.KeysDown[(int)keyEvent.Key] = keyEvent.Down;
                if (keyEvent.Key == Key.ControlLeft)
                {
                    _controlDown = keyEvent.Down;
                }
                if (keyEvent.Key == Key.ShiftLeft)
                {
                    _shiftDown = keyEvent.Down;
                }
                if (keyEvent.Key == Key.AltLeft)
                {
                    _altDown = keyEvent.Down;
                }
            }

            io.KeyCtrl = _controlDown;
            io.KeyAlt = _altDown;
            io.KeyShift = _shiftDown;
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private unsafe void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        private unsafe void UpdateImGuiInput(InputSnapshot snapshot)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            // Determine if any of the mouse buttons were pressed during this snapshot period, even if they are no longer held.
            bool leftPressed = false;
            bool middlePressed = false;
            bool rightPressed = false;
            for (int i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                MouseEvent me = snapshot.MouseEvents[i];
                if (me.Down)
                {
                    switch (me.MouseButton)
                    {
                        case MouseButton.Left:
                            leftPressed = true;
                            break;
                        case MouseButton.Middle:
                            middlePressed = true;
                            break;
                        case MouseButton.Right:
                            rightPressed = true;
                            break;
                    }
                }
            }

            io.MouseDown[0] = leftPressed || snapshot.IsMouseDown(MouseButton.Left);
            io.MouseDown[1] = rightPressed || snapshot.IsMouseDown(MouseButton.Right);
            io.MouseDown[2] = middlePressed || snapshot.IsMouseDown(MouseButton.Middle);
            io.MousePos = snapshot.MousePosition;
            io.MouseWheel = snapshot.WheelDelta;

            IReadOnlyList<char> keyCharPresses = snapshot.KeyCharPresses;
            for (int i = 0; i < keyCharPresses.Count; i++)
            {
                char c = keyCharPresses[i];
                ImGui.GetIO().AddInputCharacter(c);
            }

            IReadOnlyList<KeyEvent> keyEvents = snapshot.KeyEvents;
            for (int i = 0; i < keyEvents.Count; i++)
            {
                KeyEvent keyEvent = keyEvents[i];
                io.KeysDown[(int)keyEvent.Key] = keyEvent.Down;
                if (keyEvent.Key == Key.ControlLeft)
                {
                    _controlDown = keyEvent.Down;
                }
                if (keyEvent.Key == Key.ShiftLeft)
                {
                    _shiftDown = keyEvent.Down;
                }
                if (keyEvent.Key == Key.AltLeft)
                {
                    _altDown = keyEvent.Down;
                }
            }

            io.KeyCtrl = _controlDown;
            io.KeyAlt = _altDown;
            io.KeyShift = _shiftDown;
        }

        private static unsafe void SetOpenTKKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.BackSpace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
        }

        private unsafe void RenderImDrawData(ImDrawDataPtr draw_data, GraphicsDevice gd, CommandBuffer cb)
        {
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * sizeof(ImDrawVert));
            if (totalVBSize > _vertexBuffers[_frameIndex].SizeInBytes)
            {
                _vertexBuffers[_frameIndex].Dispose();
                _vertexBuffers[_frameIndex] = gd.ResourceFactory.CreateBuffer(
                    (uint)(totalVBSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.Dynamic);
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (totalIBSize > _indexBuffers[_frameIndex].SizeInBytes)
            {
                _indexBuffers[_frameIndex].Dispose();
                _indexBuffers[_frameIndex] = gd.ResourceFactory.CreateBuffer(
                    (uint)(totalIBSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.Dynamic);
            }

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                _vertexBuffers[_frameIndex].Update(
                    vertexOffsetInVertices * (uint)sizeof(ImDrawVert),
                    cmd_list.VtxBuffer.Data,
                    (uint)(cmd_list.VtxBuffer.Size * sizeof(ImDrawVert)));

                _indexBuffers[_frameIndex].Update(
                    indexOffsetInElements * sizeof(ushort),
                    cmd_list.IdxBuffer.Data,
                    (uint)(cmd_list.IdxBuffer.Size * sizeof(ushort)));

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            {
                var io = ImGui.GetIO();

                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    io.DisplaySize.X,
                    io.DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                _projMatrixBuffers[_frameIndex].Update(0, ref mvp);
            }

            cb.BindIndexBuffer(_indexBuffers[_frameIndex], IndexFormat.UInt16);
            cb.BindPipeline(_pipeline);
            cb.BindGraphicsResourceSet(0, _mainResourceSets[_frameIndex]);

            draw_data.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (pcmd.TextureId != IntPtr.Zero)
                        {
                            if (pcmd.TextureId == _fontAtlasID)
                            {
                                cb.BindGraphicsResourceSet(1, _fontTextureResourceSet);
                            }
                            else
                            {
                                cb.BindGraphicsResourceSet(1, GetImageResourceSet(pcmd.TextureId));
                            }
                        }

                        cb.SetScissorRect(
                            0,
                            (uint)pcmd.ClipRect.X,
                            (uint)pcmd.ClipRect.Y,
                            (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                            (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                        cb.BindVertexBuffer(0, _vertexBuffers[_frameIndex], (uint)(vtx_offset * sizeof(ImDrawVert)));
                        cb.DrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, 0, 0);
                    }
                }
                idx_offset += cmd_list.IdxBuffer.Size;
                vtx_offset += cmd_list.VtxBuffer.Size;
            }
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            for (uint i = 0; i < _bufferCount; i++)
            {
                _vertexBuffers[i].Dispose();
                _indexBuffers[i].Dispose();
                _projMatrixBuffers[i].Dispose();
                _mainResourceSets[i].Dispose();
            }
            _fontTexture.Dispose();
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
            _layout.Dispose();
            _textureLayout.Dispose();
            _pipeline.Dispose();
            _fontTextureResourceSet.Dispose();

            foreach (IDisposable resource in _ownedResources)
            {
                resource.Dispose();
            }
        }

        private struct ResourceSetInfo
        {
            public readonly IntPtr ImGuiBinding;
            public readonly ResourceSet ResourceSet;

            public ResourceSetInfo(IntPtr imGuiBinding, ResourceSet resourceSet)
            {
                ImGuiBinding = imGuiBinding;
                ResourceSet = resourceSet;
            }
        }
    }
}
