using AssetPrimitives;
using AssetProcessor;
using ImGuiNET;
using SampleBase;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SampleGallery;
using Veldrid.SPIRV;
using Veldrid.Utilities;
using Vulkan.Xlib;

namespace Instancing
{
    public class InstancingExample : Example
    {
        // Shared resources
        private ResourceSet[] _sharedResourceSets;
        private DeviceBuffer[] _cameraProjViewBuffers;
        private DeviceBuffer[] _lightInfoBuffers;

        // Resources for instanced rocks
        private Pipeline _instancePipeline;
        private Camera _camera;
        private uint _instanceCount;
        private DeviceBuffer _instanceVB;
        private ResourceSet _instanceTextureSet;
        private ModelResources _rockModel;

        // Resources for central planet
        private Pipeline _planetPipeline;
        private ResourceSet _planetTextureSet;
        private ModelResources _planetModel;

        // Resources for the background starfield
        private Pipeline _starfieldPipeline;
        private DeviceBuffer[] _viewInfoBuffers;
        private ResourceSet[] _viewInfoSets;

        // Dynamic data
        private Vector3 _lightDir;
        private bool _lightFromCamera = false; // Press F1 to switch where the directional light originates
        private DeviceBuffer[] _rotationInfoBuffers; // Contains the local and global rotation values.
        private float _localRotation = 0f; // Causes individual rocks to rotate around their centers
        private float _globalRotation = 0f; // Causes rocks to rotate around the global origin (where the planet is)
        private CommandBuffer[] _frameCBs;

        // Settings
        private bool _autoRotateCamera = true;
        private float _cameraAutoRotateAngle = 0f;
        private float _cameraAutoRotateSpeed = 0.25f;

        private T[] Buffered<T>(Func<T> generator)
        {
            return Enumerable.Range(0, (int)Driver.BufferCount).Select(i => generator()).ToArray();
        }

        protected override void OnGallerySizeChangedCore()
        {
            _camera.ViewSizeChanged(Driver.Width, Driver.Height);
            RecordFrameCommands();
        }

        public override async Task LoadResourcesAsync()
        {
            _camera = new Camera(Device, Driver.Width, Driver.Height);
            _instanceCount = 8000u;

            _camera.Position = new Vector3(-36f, 20f, 100f);
            _camera.Pitch = -0.3f;
            _camera.Yaw = 0.1f;

            _cameraProjViewBuffers = Buffered(() => Factory.CreateBuffer(
                new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>() * 2), BufferUsage.UniformBuffer | BufferUsage.Dynamic)));
            _lightInfoBuffers = Buffered(() => Factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic)));
            _rotationInfoBuffers = Buffered(() => Factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer | BufferUsage.Dynamic)));
            _lightDir = Vector3.Normalize(new Vector3(0.3f, -0.75f, -0.3f));

            VertexLayoutDescription sharedVertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

            bool etc2Supported = Device.GetPixelFormatSupport(
                PixelFormat.ETC2_R8_G8_B8_UNorm,
                TextureType.Texture2D,
                TextureUsage.Sampled);
            PixelFormat pixelFormat = etc2Supported ? PixelFormat.ETC2_R8_G8_B8_UNorm : PixelFormat.BC3_UNorm;

            KtxFileProcessor ktxProcessor = new KtxFileProcessor();
            string fileName = etc2Supported
                ? "texturearray_rocks_etc2_unorm.ktx"
                : "texturearray_rocks_bc3_unorm.ktx";
            byte[] rockTexData;
            using (Stream stream = OpenEmbeddedAsset(fileName))
            {
                rockTexData = await ktxProcessor.ProcessT(stream, "ktx");
            }
            Texture rockTexture = KtxFile.LoadTexture(Device, Factory, rockTexData, pixelFormat);
            TextureView rockTextureView = Factory.CreateTextureView(rockTexture);

            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions =
            {
                new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("RotationInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            };
            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            ResourceLayout sharedLayout = Factory.CreateResourceLayout(resourceLayoutDescription);

            ResourceLayoutElementDescription[] textureLayoutDescriptions =
            {
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Samp", ResourceKind.Sampler, ShaderStages.Fragment)
            };
            ResourceLayout textureLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(textureLayoutDescriptions));

            _sharedResourceSets = Enumerable.Range(0, (int)Driver.BufferCount).Select(frameID =>
            {
                BindableResource[] bindableResources = new BindableResource[]
                {
                    _lightInfoBuffers[frameID],
                    _cameraProjViewBuffers[frameID],
                    _rotationInfoBuffers[frameID]
                };
                ResourceSetDescription resourceSetDescription = new ResourceSetDescription(sharedLayout, bindableResources);
                return Factory.CreateResourceSet(resourceSetDescription);
            }).ToArray();

            BindableResource[] instanceBindableResources = { rockTextureView, Device.LinearSampler };
            _instanceTextureSet = Factory.CreateResourceSet(new ResourceSetDescription(textureLayout, instanceBindableResources));

            Console.WriteLine($"Loading rock01 model.");
            using (Stream rockFS = OpenEmbeddedAsset("rock01.obj"))
            {
                ObjParser objParser = new ObjParser();
                ObjFile model = objParser.Parse(rockFS);
                ConstructedMeshInfo firstMesh = model.GetFirstMesh();
                DeviceBuffer vertexBuffer = firstMesh.CreateVertexBuffer(Factory, Device);
                DeviceBuffer indexBuffer = firstMesh.CreateIndexBuffer(Factory, Device, out int indexCount);
                _rockModel = new ModelResources(vertexBuffer, indexBuffer, IndexFormat.UInt16, (uint)indexCount);
            }

            VertexLayoutDescription vertexLayoutPerInstance = new VertexLayoutDescription(
                new VertexElementDescription("InstancePosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("InstanceRotation", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("InstanceScale", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("InstanceTexArrayIndex", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1));
            vertexLayoutPerInstance.InstanceStepRate = 1;
            _instanceVB = Factory.CreateBuffer(new BufferDescription(InstanceInfo.Size * _instanceCount, BufferUsage.VertexBuffer));
            InstanceInfo[] infos = new InstanceInfo[_instanceCount];
            Random r = new Random();
            float orbitDistance = 50f;
            for (uint i = 0; i < _instanceCount / 2; i++)
            {
                float angle = (float)(r.NextDouble() * Math.PI * 2);
                infos[i] = new InstanceInfo(
                    new Vector3(
                        ((float)Math.Cos(angle) * orbitDistance) + (float)(-10 + r.NextDouble() * 20),
                        (float)(-1.5 + r.NextDouble() * 3),
                        ((float)Math.Sin(angle) * orbitDistance) + (float)(-10 + r.NextDouble() * 20)),
                    new Vector3(
                        (float)(r.NextDouble() * Math.PI * 2),
                        (float)(r.NextDouble() * Math.PI * 2),
                        (float)(r.NextDouble() * Math.PI * 2)),
                    new Vector3((float)(0.65 + r.NextDouble() * 0.35)),
                    r.Next(0, (int)rockTexture.ArrayLayers));
            }

            orbitDistance = 100f;
            for (uint i = _instanceCount / 2; i < _instanceCount; i++)
            {
                float angle = (float)(r.NextDouble() * Math.PI * 2);
                infos[i] = new InstanceInfo(
                    new Vector3(
                        ((float)Math.Cos(angle) * orbitDistance) + (float)(-10 + r.NextDouble() * 20),
                        (float)(-1.5 + r.NextDouble() * 3),
                        ((float)Math.Sin(angle) * orbitDistance) + (float)(-10 + r.NextDouble() * 20)),
                    new Vector3(
                        (float)(r.NextDouble() * Math.PI * 2),
                        (float)(r.NextDouble() * Math.PI * 2),
                        (float)(r.NextDouble() * Math.PI * 2)),
                    new Vector3((float)(0.65 + r.NextDouble() * 0.35)),
                    r.Next(0, (int)rockTexture.ArrayLayers));
            }

            Device.UpdateBuffer(_instanceVB, 0, infos);

            (Shader[] instanceShaders, SpirvReflection instanceReflection) = ShaderUtil.LoadEmbeddedShaderSet(
                typeof(InstancingExample).Assembly, Factory, "Instance");

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription()
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = new DepthStencilStateDescription(
                    depthTestEnabled: true,
                    depthWriteEnabled: true,
                    comparisonKind: Device.IsDepthRangeZeroToOne
                        ? ComparisonKind.GreaterEqual
                        : ComparisonKind.LessEqual
                ),
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new ResourceLayout[] { sharedLayout, textureLayout },
                ShaderSet = new ShaderSetDescription(
                    // The ordering of layouts directly impacts shader layout schemes
                    vertexLayouts: new VertexLayoutDescription[] { sharedVertexLayout, vertexLayoutPerInstance },
                    shaders: instanceShaders
                ),
                Outputs = Framebuffers[0].OutputDescription,
                ReflectedVertexElements = instanceReflection.VertexElements,
                ReflectedResourceLayouts = instanceReflection.ResourceLayouts
            };

            _instancePipeline = Factory.CreateGraphicsPipeline(pipelineDescription);

            (Shader[] planetShaders, SpirvReflection planetReflection) = ShaderUtil.LoadEmbeddedShaderSet(
                typeof(InstancingExample).Assembly, Factory, "Planet");

            // Create planet Pipeline
            // Almost everything is the same as the rock Pipeline,
            // except no instance vertex buffer is needed, and different shaders are used.
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                new[] { sharedVertexLayout },
                planetShaders);
            pipelineDescription.ReflectedVertexElements = planetReflection.VertexElements;
            pipelineDescription.ReflectedResourceLayouts = planetReflection.ResourceLayouts;
            _planetPipeline = Factory.CreateGraphicsPipeline(pipelineDescription);

            Console.WriteLine($"Loading sphere model.");
            using (Stream sphereFS = OpenEmbeddedAsset("sphere.obj"))
            {
                ObjParser objParser = new ObjParser();
                ObjFile model = objParser.Parse(sphereFS);
                ConstructedMeshInfo firstMesh = model.GetFirstMesh();
                DeviceBuffer vertexBuffer = firstMesh.CreateVertexBuffer(Factory, Device);
                DeviceBuffer indexBuffer = firstMesh.CreateIndexBuffer(Factory, Device, out int indexCount);

                _planetModel = new ModelResources(vertexBuffer, indexBuffer, IndexFormat.UInt16, (uint)indexCount);
            }

            string planetFileName = etc2Supported
                ? "lavaplanet_etc2_unorm.ktx"
                : "lavaplanet_bc3_unorm.ktx";
            byte[] planetTexData;
            using (Stream stream = OpenEmbeddedAsset(planetFileName))
            {
                planetTexData = await ktxProcessor.ProcessT(stream, "ktx");
            }
            Texture planetTexture = KtxFile.LoadTexture(Device, Factory, planetTexData, pixelFormat);
            TextureView planetTextureView = Factory.CreateTextureView(planetTexture);
            _planetTextureSet = Factory.CreateResourceSet(new ResourceSetDescription(textureLayout, planetTextureView, Device.Aniso4xSampler));

            // Starfield resources
            ResourceLayout invCameraInfoLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("InvCameraInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));
            _viewInfoBuffers = Buffered(
                () => Factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MatrixPair>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic)));
            _viewInfoSets = Enumerable.Range(0, (int)Driver.BufferCount)
                .Select(frameID => Factory.CreateResourceSet(new ResourceSetDescription(invCameraInfoLayout, _viewInfoBuffers[frameID]))).ToArray();

            (Shader[] starfieldShaders, SpirvReflection starfieldReflection) = ShaderUtil.LoadEmbeddedShaderSet(
                typeof(InstancingExample).Assembly, Factory, "Starfield");

            ShaderSetDescription starfieldShaderSetDesc = new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), starfieldShaders);

            _starfieldPipeline = Factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleList,
                starfieldShaderSetDesc,
                new[] { invCameraInfoLayout },
                Framebuffers[0].OutputDescription,
                null,
                starfieldReflection.ResourceLayouts));

            RecordFrameCommands();
        }

        private void RecordFrameCommands()
        {
            Util.DisposeAll(_frameCBs);
            _frameCBs = Enumerable.Range(0, (int)Driver.BufferCount).Select(frameIndex =>
            {
                CommandBuffer cb = Factory.CreateCommandBuffer(CommandBufferFlags.Reusable);
                cb.Name = $"InstancingExample CB {frameIndex}";
                cb.BeginRenderPass(
                    Framebuffers[frameIndex],
                    LoadAction.Clear,
                    StoreAction.Store,
                    RgbaFloat.Black,
                    Device.IsDepthRangeZeroToOne ? 0f : 1f);

                // First, draw the background starfield.
                cb.BindPipeline(_starfieldPipeline);
                cb.BindGraphicsResourceSet(0, _viewInfoSets[frameIndex]);
                cb.Draw(4);

                // Next, draw our orbiting rocks with instanced drawing.
                cb.BindPipeline(_instancePipeline);
                // Set uniforms
                cb.BindGraphicsResourceSet(0, _sharedResourceSets[frameIndex]);
                cb.BindGraphicsResourceSet(1, _instanceTextureSet);

                cb.BindVertexBuffer(0, _rockModel.VertexBuffer);
                cb.BindIndexBuffer(_rockModel.IndexBuffer, _rockModel.IndexFormat);
                cb.BindVertexBuffer(1, _instanceVB);

                // Issue a Draw command for two instances with 4 indices.
                cb.DrawIndexed(
                    indexCount: _rockModel.IndexCount,
                    instanceCount: _instanceCount,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0);

                // Next, we draw our central planet.
                cb.BindPipeline(_planetPipeline);
                cb.BindGraphicsResourceSet(1, _planetTextureSet);
                cb.BindVertexBuffer(0, _planetModel.VertexBuffer);
                cb.BindIndexBuffer(_planetModel.IndexBuffer, _planetModel.IndexFormat);

                // The planet is drawn with regular indexed drawing -- not instanced.
                cb.DrawIndexed(_planetModel.IndexCount);

                cb.EndRenderPass();

                return cb;
            }).ToArray();
        }

        public override void DrawMainMenuBars()
        {
            if (ImGui.BeginMenu("Settings"))
            {
                ImGui.Checkbox("Light frosm Camera", ref _lightFromCamera);
                ImGui.Checkbox("Auto-rotate camera", ref _autoRotateCamera);
                ImGui.SliderFloat("Auto-rotate speed", ref _cameraAutoRotateSpeed, 0.05f, 1f);

                ImGui.EndMenu();
            }
        }

        public override CommandBuffer[] Render(double delta)
        {
            if (InputTracker.GetKeyDown(Key.F2))
            {
                _lightFromCamera = !_lightFromCamera;
            }

            uint frameIndex = Driver.FrameIndex;

            _camera.Update((float)delta);

            if (_autoRotateCamera)
            {
                Vector3 cameraPos = new Vector3((float)Math.Cos(_cameraAutoRotateAngle), 0f, (float)Math.Sin(_cameraAutoRotateAngle));
                float distance = 125f + 30f * (float)Math.Sin(_cameraAutoRotateAngle);
                cameraPos *= distance;
                cameraPos.Y = 50 + (float)Math.Cos(_cameraAutoRotateAngle) * 15.0f;
                _camera.Position = cameraPos;
                Vector3 forward = Vector3.Normalize(-_camera.Position);

                float pitch = (float)Math.Asin(Vector3.Dot(forward, Vector3.UnitY));
                forward.Y = 0;
                forward = Vector3.Normalize(forward);
                float yaw = (float)Math.Asin(Vector3.Dot(forward, Vector3.UnitX));
                if (Vector3.Dot(forward, -Vector3.UnitZ) > 0)
                {
                    yaw = (2f * (float)Math.PI) - yaw;
                }
                else
                {
                    yaw = yaw - (float)Math.PI;
                }

                _camera.Pitch = pitch;
                _camera.Yaw = yaw;
            }

            // Update per-frame resources.
            Device.UpdateBuffer(_cameraProjViewBuffers[frameIndex], 0, new MatrixPair(_camera.ViewMatrix, _camera.ProjectionMatrix));

            if (_lightFromCamera)
            {
                Device.UpdateBuffer(_lightInfoBuffers[frameIndex], 0, new LightInfo(_camera.LookDirection, _camera.Position));
            }
            else
            {
                Device.UpdateBuffer(_lightInfoBuffers[frameIndex], 0, new LightInfo(_lightDir, _camera.Position));
            }

            if (_autoRotateCamera)
            {
                _cameraAutoRotateAngle += (float)delta * -_cameraAutoRotateSpeed;
            }

            _localRotation += (float)delta * ((float)Math.PI * 2 / 9);
            _globalRotation += -(float)delta * ((float)Math.PI * 2 / 240);
            Device.UpdateBuffer(_rotationInfoBuffers[frameIndex], 0, new Vector4(_localRotation, _globalRotation, 0, 0));

            Matrix4x4.Invert(_camera.ProjectionMatrix, out Matrix4x4 inverseProjection);
            Matrix4x4.Invert(_camera.ViewMatrix, out Matrix4x4 inverseView);
            Device.UpdateBuffer(_viewInfoBuffers[frameIndex], 0, new MatrixPair(
                inverseProjection,
                inverseView));

            return new[] { _frameCBs[Driver.FrameIndex] };
        }

        private Stream OpenEmbeddedAsset(string name)
            => typeof(InstancingExample).Assembly.GetManifestResourceStream(name);
    }

    public struct InstanceInfo
    {
        public static uint Size { get; } = (uint)Unsafe.SizeOf<InstanceInfo>();

        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public int TexArrayIndex;

        public InstanceInfo(Vector3 position, Vector3 rotation, Vector3 scale, int texArrayIndex)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            TexArrayIndex = texArrayIndex;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LightInfo
    {
        public Vector3 LightDirection;
        private float padding0;
        public Vector3 CameraPosition;
        private float padding1;

        public LightInfo(Vector3 lightDirection, Vector3 cameraPosition)
        {
            LightDirection = lightDirection;
            CameraPosition = cameraPosition;
            padding0 = 0;
            padding1 = 0;
        }
    }

    public struct MatrixPair
    {
        public Matrix4x4 First;
        public Matrix4x4 Second;

        public MatrixPair(Matrix4x4 first, Matrix4x4 second)
        {
            First = first;
            Second = second;
        }
    }

    //public class Camera
    //{
    //    private float _fov = 1f;
    //    private float _near = 1f;
    //    private float _far = 1000f;

    //    private Matrix4x4 _viewMatrix;
    //    private Matrix4x4 _projectionMatrix;

    //    private Vector3 _position = new Vector3(0, 3, 0);
    //    private Vector3 _lookDirection = new Vector3(0, -.3f, -1f);
    //    private float _moveSpeed = 10.0f;

    //    private float _yaw;
    //    private float _pitch;

    //    private Vector2 _previousMousePos;
    //    private float _windowWidth;
    //    private float _windowHeight;

    //    public event Action<Matrix4x4> ProjectionChanged;
    //    public event Action<Matrix4x4> ViewChanged;

    //    public Camera(float width, float height)
    //    {
    //        _windowWidth = width;
    //        _windowHeight = height;
    //        UpdatePerspectiveMatrix();
    //        UpdateViewMatrix();
    //    }

    //    public Matrix4x4 ViewMatrix => _viewMatrix;
    //    public Matrix4x4 ProjectionMatrix => _projectionMatrix;

    //    public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }

    //    public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
    //    public float FieldOfView => _fov;
    //    public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

    //    public float AspectRatio => _windowWidth / _windowHeight;

    //    public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
    //    public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

    //    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    //    public Vector3 Forward => GetLookDir();

    //    public void Update(float deltaSeconds)
    //    {
    //        float sprintFactor = InputTracker.GetKey(Key.ControlLeft)
    //            ? 0.1f
    //            : InputTracker.GetKey(Key.ShiftLeft)
    //                ? 2.5f
    //                : 1f;
    //        Vector3 motionDir = Vector3.Zero;
    //        if (InputTracker.GetKey(Key.A))
    //        {
    //            motionDir += -Vector3.UnitX;
    //        }
    //        if (InputTracker.GetKey(Key.D))
    //        {
    //            motionDir += Vector3.UnitX;
    //        }
    //        if (InputTracker.GetKey(Key.W))
    //        {
    //            motionDir += -Vector3.UnitZ;
    //        }
    //        if (InputTracker.GetKey(Key.S))
    //        {
    //            motionDir += Vector3.UnitZ;
    //        }
    //        if (InputTracker.GetKey(Key.Q))
    //        {
    //            motionDir += -Vector3.UnitY;
    //        }
    //        if (InputTracker.GetKey(Key.E))
    //        {
    //            motionDir += Vector3.UnitY;
    //        }

    //        if (motionDir != Vector3.Zero)
    //        {
    //            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
    //            motionDir = Vector3.Transform(motionDir, lookRotation);
    //            _position += motionDir * MoveSpeed * sprintFactor * deltaSeconds;
    //            UpdateViewMatrix();
    //        }

    //        Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
    //        _previousMousePos = InputTracker.MousePosition;

    //        if (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right))
    //        {
    //            Yaw += -mouseDelta.X * 0.01f;
    //            Pitch += -mouseDelta.Y * 0.01f;
    //            Pitch = Clamp(Pitch, -1.55f, 1.55f);

    //            UpdateViewMatrix();
    //        }
    //    }

    //    private float Clamp(float value, float min, float max)
    //    {
    //        return value > max
    //            ? max
    //            : value < min
    //                ? min
    //                : value;
    //    }

    //    public void WindowResized(float width, float height)
    //    {
    //        _windowWidth = width;
    //        _windowHeight = height;
    //        UpdatePerspectiveMatrix();
    //    }

    //    private void UpdatePerspectiveMatrix()
    //    {
    //        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _windowWidth / _windowHeight, _near, _far);
    //        ProjectionChanged?.Invoke(_projectionMatrix);
    //    }

    //    private void UpdateViewMatrix()
    //    {
    //        Vector3 lookDir = GetLookDir();
    //        _lookDirection = lookDir;
    //        _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY);
    //        ViewChanged?.Invoke(_viewMatrix);
    //    }

    //    private Vector3 GetLookDir()
    //    {
    //        Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
    //        Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
    //        return lookDir;
    //    }

    //    public CameraInfo GetCameraInfo() => new CameraInfo
    //    {
    //        CameraPosition_WorldSpace = _position,
    //        CameraLookDirection = _lookDirection
    //    };
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraInfo
    {
        public Vector3 CameraPosition_WorldSpace;
        private float _padding1;
        public Vector3 CameraLookDirection;
        private float _padding2;
    }
}
