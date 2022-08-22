using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.IO;
using System.Text;

namespace Veldrid
{
    /// <summary>
    /// Can render draw lists produced by ImGui.
    /// Also provides functions for updating ImGui input.
    /// </summary>
    public class ImGuiRenderer : IDisposable
    {
        private GraphicsDevice _gd;
        private readonly Assembly _assembly;
        private ColorSpaceHandling _colorSpaceHandling;

        // Device objects
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _projMatrixBuffer;
        private Texture _fontTexture;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private ResourceLayout _layout;
        private ResourceLayout _textureLayout;
        private Pipeline _pipeline;
        private ResourceSet _mainResourceSet;
        private ResourceSet _fontTextureResourceSet;
        private IntPtr _fontAtlasID = (IntPtr)1;

        private int _windowWidth;
        private int _windowHeight;
        private Vector2 _scaleFactor = Vector2.One;

        // Image trackers
        private readonly Dictionary<TextureView, ResourceSetInfo> _setsByView = new();
        private readonly Dictionary<Texture, TextureView> _autoViewsByTexture = new();
        private readonly Dictionary<IntPtr, ResourceSetInfo> _viewsById = new();
        private readonly List<IDisposable> _ownedResources = new();
        private int _lastAssignedID = 100;
        private bool _frameBegun;
        private bool _disposed;

        /// <summary>
        /// Constructs a new ImGuiRenderer.
        /// </summary>
        /// <param name="gd">The GraphicsDevice used to create and update resources.</param>
        /// <param name="outputDescription">The output format.</param>
        /// <param name="width">The initial width of the rendering target. Can be resized.</param>
        /// <param name="height">The initial height of the rendering target. Can be resized.</param>
        public ImGuiRenderer(GraphicsDevice gd, OutputDescription outputDescription, int width, int height)
            : this(gd, outputDescription, width, height, ColorSpaceHandling.Legacy)
        {
        }

        /// <summary>
        /// Constructs a new ImGuiRenderer.
        /// </summary>
        /// <param name="gd">The GraphicsDevice used to create and update resources.</param>
        /// <param name="outputDescription">The output format.</param>
        /// <param name="width">The initial width of the rendering target. Can be resized.</param>
        /// <param name="height">The initial height of the rendering target. Can be resized.</param>
        /// <param name="colorSpaceHandling">Identifies how the renderer should treat vertex colors.</param>
        public ImGuiRenderer(GraphicsDevice gd, OutputDescription outputDescription, int width, int height, ColorSpaceHandling colorSpaceHandling)
        {
            _gd = gd;
            _assembly = typeof(ImGuiRenderer).GetTypeInfo().Assembly;
            _colorSpaceHandling = colorSpaceHandling;
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGui.GetIO().Fonts.AddFontDefault();
            ImGui.GetIO().Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

            CreateDeviceResources(gd, outputDescription);

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
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _projMatrixBuffer.Dispose();
            _fontTexture.Dispose();
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
            _layout.Dispose();
            _textureLayout.Dispose();
            _pipeline.Dispose();
            _mainResourceSet.Dispose();
            _fontTextureResourceSet.Dispose();

            foreach (IDisposable resource in _ownedResources)
            {
                resource.Dispose();
            }
        }

        public void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription)
        {
            CreateDeviceResources(gd, outputDescription, _colorSpaceHandling);
        }

        public void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription, ColorSpaceHandling colorSpaceHandling)
        {
            _gd = gd;
            _colorSpaceHandling = colorSpaceHandling;
            ResourceFactory factory = gd.ResourceFactory;
            _vertexBuffer = factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.DynamicWrite));
            _vertexBuffer.Name = "ImGui.NET Vertex Buffer";
            _indexBuffer = factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.DynamicWrite));
            _indexBuffer.Name = "ImGui.NET Index Buffer";

            _projMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.DynamicWrite));
            _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";

            byte[] vertexShaderBytes = LoadEmbeddedShaderCode(gd.ResourceFactory, "imgui-vertex", ShaderStages.Vertex, _colorSpaceHandling);
            byte[] fragmentShaderBytes = LoadEmbeddedShaderCode(gd.ResourceFactory, "imgui-frag", ShaderStages.Fragment, _colorSpaceHandling);
            _vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, _gd.BackendType == GraphicsBackend.Vulkan ? "main" : "VS"));
            _vertexShader.Name = "ImGui.NET Vertex Shader";
            _fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, _gd.BackendType == GraphicsBackend.Vulkan ? "main" : "FS"));
            _fragmentShader.Name = "ImGui.NET Fragment Shader";

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
            _layout.Name = "ImGui.NET Resource Layout";

            _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("FontTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));
            _textureLayout.Name = "ImGui.NET Texture Layout";

            GraphicsPipelineDescription pd = new(
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
            _pipeline = factory.CreateGraphicsPipeline(pd);
            _pipeline.Name = "ImGui.NET Pipeline";

            _mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout,
                _projMatrixBuffer,
                gd.PointSampler));
            _mainResourceSet.Name = "ImGui.NET Main Resource Set";

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
                resourceSet.Name = $"ImGui.NET {textureView.Name} Resource Set";
                rsi = new ResourceSetInfo(GetNextImGuiBindingID(), resourceSet);

                _setsByView.Add(textureView, rsi);
                _viewsById.Add(rsi.ImGuiBinding, rsi);
                _ownedResources.Add(resourceSet);
            }

            return rsi.ImGuiBinding;
        }

        public void RemoveImGuiBinding(TextureView textureView)
        {
            if (_setsByView.Remove(textureView, out ResourceSetInfo rsi))
            {
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
                textureView.Name = $"ImGui.NET {texture.Name} View";
                _autoViewsByTexture.Add(texture, textureView);
                _ownedResources.Add(textureView);
            }

            return GetOrCreateImGuiBinding(factory, textureView);
        }

        public void RemoveImGuiBinding(Texture texture)
        {
            if (_autoViewsByTexture.Remove(texture, out TextureView textureView))
            {
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
                    if (stage == ShaderStages.Vertex && colorSpaceHandling == ColorSpaceHandling.Legacy)
                        name += "-legacy";
                    string resourceName = name + ".hlsl.bytes";
                    return GetEmbeddedResourceBytes(resourceName);
                }
                case GraphicsBackend.OpenGL:
                {
                    if (stage == ShaderStages.Vertex && colorSpaceHandling == ColorSpaceHandling.Legacy)
                        name += "-legacy";
                    string resourceName = name + ".glsl";
                    return GetEmbeddedResourceBytes(resourceName);
                }
                case GraphicsBackend.OpenGLES:
                {
                    if (stage == ShaderStages.Vertex && colorSpaceHandling == ColorSpaceHandling.Legacy)
                        name += "-legacy";
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

        private byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            using Stream s = _assembly.GetManifestResourceStream(resourceName);
            byte[] ret = new byte[s.Length];
            int offset = 0;
            do
            {
                int read = s.Read(ret, offset, ret.Length - offset);
                offset += read;
                if (read == 0)
                    break;
            }
            while (offset < ret.Length);

            if (offset != ret.Length)
            {
                throw new EndOfStreamException();
            }
            return ret;
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
            _fontTextureResourceSet.Name = "ImGui.NET Font Texture Resource Set";

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public unsafe void Render(GraphicsDevice gd, CommandList cl)
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData(), gd, cl);
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(float deltaSeconds, InputSnapshot snapshot)
        {
            BeginUpdate(deltaSeconds);
            UpdateImGuiInput(snapshot);
            EndUpdate();
        }

        /// <summary>
        /// Called before we handle the input in <see cref="Update(float, InputSnapshot)"/>.
        /// This render ImGui and update the state.
        /// </summary>
        protected void BeginUpdate(float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
        }

        /// <summary>
        /// Called at the end of <see cref="Update(float, InputSnapshot)"/>.
        /// This tells ImGui that we are on the next frame.
        /// </summary>
        protected void EndUpdate()
        {
            _frameBegun = true;
            ImGui.NewFrame();
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

        private static bool TryMapKey(Key key, out ImGuiKey result)
        {
            static ImGuiKey keyToImGuiKeyShortcut(Key keyToConvert, Key startKey1, ImGuiKey startKey2)
            {
                int changeFromStart1 = (int)keyToConvert - (int)startKey1;
                return startKey2 + changeFromStart1;
            }

            if (key >= Key.F1 && key <= Key.F12)
            {
                result = keyToImGuiKeyShortcut(key, Key.F1, ImGuiKey.F1);
                return true;
            }
            else if (key >= Key.Keypad0 && key <= Key.Keypad9)
            {
                result = keyToImGuiKeyShortcut(key, Key.Keypad0, ImGuiKey.Keypad0);
                return true;
            }
            else if (key >= Key.A && key <= Key.Z)
            {
                result = keyToImGuiKeyShortcut(key, Key.A, ImGuiKey.A);
                return true;
            }
            else if (key >= Key.Num0 && key <= Key.Num9)
            {
                result = keyToImGuiKeyShortcut(key, Key.Num0, ImGuiKey._0);
                return true;
            }

            switch (key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    result = ImGuiKey.ModShift;
                    return true;
                case Key.LeftControl:
                case Key.RightControl:
                    result = ImGuiKey.ModCtrl;
                    return true;
                case Key.LeftAlt:
                case Key.RightAlt:
                    result = ImGuiKey.ModAlt;
                    return true;
                case Key.LeftGui:
                case Key.RightGui:
                    result = ImGuiKey.ModSuper;
                    return true;
                case Key.Menu:
                    result = ImGuiKey.Menu;
                    return true;
                case Key.Up:
                    result = ImGuiKey.UpArrow;
                    return true;
                case Key.Down:
                    result = ImGuiKey.DownArrow;
                    return true;
                case Key.Left:
                    result = ImGuiKey.LeftArrow;
                    return true;
                case Key.Right:
                    result = ImGuiKey.RightArrow;
                    return true;
                case Key.Return:
                    result = ImGuiKey.Enter;
                    return true;
                case Key.Escape:
                    result = ImGuiKey.Escape;
                    return true;
                case Key.Space:
                    result = ImGuiKey.Space;
                    return true;
                case Key.Tab:
                    result = ImGuiKey.Tab;
                    return true;
                case Key.Backspace:
                    result = ImGuiKey.Backspace;
                    return true;
                case Key.Insert:
                    result = ImGuiKey.Insert;
                    return true;
                case Key.Delete:
                    result = ImGuiKey.Delete;
                    return true;
                case Key.PageUp:
                    result = ImGuiKey.PageUp;
                    return true;
                case Key.PageDown:
                    result = ImGuiKey.PageDown;
                    return true;
                case Key.Home:
                    result = ImGuiKey.Home;
                    return true;
                case Key.End:
                    result = ImGuiKey.End;
                    return true;
                case Key.CapsLock:
                    result = ImGuiKey.CapsLock;
                    return true;
                case Key.ScrollLock:
                    result = ImGuiKey.ScrollLock;
                    return true;
                case Key.PrintScreen:
                    result = ImGuiKey.PrintScreen;
                    return true;
                case Key.Pause:
                    result = ImGuiKey.Pause;
                    return true;
                case Key.NumLockClear:
                    result = ImGuiKey.NumLock;
                    return true;
                case Key.KeypadDivide:
                    result = ImGuiKey.KeypadDivide;
                    return true;
                case Key.KeypadMultiply:
                    result = ImGuiKey.KeypadMultiply;
                    return true;
                case Key.KeypadMemorySubtract:
                    result = ImGuiKey.KeypadSubtract;
                    return true;
                case Key.KeypadMemoryAdd:
                    result = ImGuiKey.KeypadAdd;
                    return true;
                case Key.KeypadDecimal:
                    result = ImGuiKey.KeypadDecimal;
                    return true;
                case Key.KeypadEnter:
                    result = ImGuiKey.KeypadEnter;
                    return true;
                case Key.Grave:
                    result = ImGuiKey.GraveAccent;
                    return true;
                case Key.Minus:
                    result = ImGuiKey.Minus;
                    return true;
                case Key.KeypadPlus:
                    result = ImGuiKey.Equal;
                    return true;
                case Key.LeftBracket:
                    result = ImGuiKey.LeftBracket;
                    return true;
                case Key.RightBracket:
                    result = ImGuiKey.RightBracket;
                    return true;
                case Key.Semicolon:
                    result = ImGuiKey.Semicolon;
                    return true;
                case Key.Apostrophe:
                    result = ImGuiKey.Apostrophe;
                    return true;
                case Key.Comma:
                    result = ImGuiKey.Comma;
                    return true;
                case Key.Period:
                    result = ImGuiKey.Period;
                    return true;
                case Key.Slash:
                    result = ImGuiKey.Slash;
                    return true;
                case Key.Backslash:
                case Key.NonUsBackslash:
                    result = ImGuiKey.Backslash;
                    return true;
                default:
                    result = ImGuiKey.GamepadBack;
                    return false;
            }
        }

        private static void UpdateImGuiInput(InputSnapshot snapshot)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            Vector2 mousePos = snapshot.MousePosition;
            io.AddMousePosEvent(mousePos.X, mousePos.Y);

            MouseButton snapMouseDown = snapshot.MouseDown;
            io.AddMouseButtonEvent(0, (snapMouseDown & MouseButton.Left) != 0);
            io.AddMouseButtonEvent(1, (snapMouseDown & MouseButton.Right) != 0);
            io.AddMouseButtonEvent(2, (snapMouseDown & MouseButton.Middle) != 0);
            io.AddMouseButtonEvent(3, (snapMouseDown & MouseButton.Button1) != 0);
            io.AddMouseButtonEvent(4, (snapMouseDown & MouseButton.Button2) != 0);

            Vector2 wheelDelta = snapshot.WheelDelta;
            io.AddMouseWheelEvent(wheelDelta.X, wheelDelta.Y);

            foreach (Rune rune in snapshot.InputEvents)
            {
                io.AddInputCharacter((uint)rune.Value);
            }

            foreach(KeyEvent keyEvent in snapshot.KeyEvents)
            {
                if (TryMapKey(keyEvent.Physical, out ImGuiKey imguikey))
                {
                    io.AddKeyEvent(imguikey, keyEvent.Down);
                }
            }
        }

        private unsafe void RenderImDrawData(ImDrawDataPtr draw_data, GraphicsDevice gd, CommandList cl)
        {
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * sizeof(ImDrawVert));
            if (totalVBSize > _vertexBuffer.SizeInBytes)
            {
                string name = _vertexBuffer.Name;
                _vertexBuffer.Dispose();
                _vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalVBSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.DynamicWrite));
                _vertexBuffer.Name = name;
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (totalIBSize > _indexBuffer.SizeInBytes)
            {
                string name = _indexBuffer.Name;
                _indexBuffer.Dispose();
                _indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalIBSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.DynamicWrite));
                _indexBuffer.Name = name;
            }

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                cl.UpdateBuffer(
                    _vertexBuffer,
                    vertexOffsetInVertices * (uint)sizeof(ImDrawVert),
                    cmd_list.VtxBuffer.Data,
                    (uint)(cmd_list.VtxBuffer.Size * sizeof(ImDrawVert)));

                cl.UpdateBuffer(
                    _indexBuffer,
                    indexOffsetInElements * sizeof(ushort),
                    cmd_list.IdxBuffer.Data,
                    (uint)(cmd_list.IdxBuffer.Size * sizeof(ushort)));

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            {
                ImGuiIOPtr io = ImGui.GetIO();

                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    io.DisplaySize.X,
                    io.DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                cl.UpdateBuffer(_projMatrixBuffer, 0, ref mvp);
            }

            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _mainResourceSet);

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
                                cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                            }
                            else
                            {
                                cl.SetGraphicsResourceSet(1, GetImageResourceSet(pcmd.TextureId));
                            }
                        }

                        cl.SetScissorRect(
                            0,
                            (uint)pcmd.ClipRect.X,
                            (uint)pcmd.ClipRect.Y,
                            (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                            (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                        cl.DrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, (int)(pcmd.VtxOffset + vtx_offset), 0);
                    }
                }

                idx_offset += cmd_list.IdxBuffer.Size;
                vtx_offset += cmd_list.VtxBuffer.Size;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DestroyDeviceObjects();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
