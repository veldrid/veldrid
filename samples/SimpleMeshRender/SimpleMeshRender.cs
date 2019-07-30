using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Veldrid.SPIRV;
using Veldrid.StbImage;
using Veldrid.Utilities;

namespace Veldrid.SampleGallery
{
    public class SimpleMeshRender : Example
    {
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private uint _indexCount;
        private Pipeline _pipeline;
        private Vector3 _modelPos = new Vector3(0, 0, 0);
        private Camera _camera;
        private SkyboxRenderer _skyboxRenderer;

        // Per-frame resources
        private DeviceBuffer[] _cameraInfoBuffers;
        private DeviceBuffer[] _uniformBuffers;
        private ResourceSet[] _resourceSets;
        private CommandBuffer[] _frameCBs;

        public override async Task LoadResourcesAsync()
        {
            List<Task> tasks = new List<Task>();

            Console.WriteLine($"Loading cat model.");
            using (Stream catFS = OpenEmbeddedAsset("cat.obj"))
            {
                ObjParser objParser = new ObjParser();
                ObjFile model = objParser.Parse(catFS);
                ConstructedMeshInfo firstMesh = model.GetFirstMesh();
                _vertexBuffer = firstMesh.CreateVertexBuffer(Factory, Device);

                int indexCount;
                _indexBuffer = firstMesh.CreateIndexBuffer(Factory, Device, out indexCount);
                _indexCount = (uint)indexCount;
            }

            Console.WriteLine($"Loading cat texture.");
            Texture catTexture = null;
            using (Stream catDiffFS = OpenEmbeddedAsset("cat_diff.png"))
            {
                catTexture = StbTextureLoader.Load(Device, Factory, catDiffFS, false, true);
            }

            Console.WriteLine($"Loading main mesh Pipeline.");
            ResourceLayout layout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UniformState", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Smp", ResourceKind.Sampler, ShaderStages.Fragment)));

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("vsin_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3, 0),
                new VertexElementDescription("vsin_uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2, 24));

            (Shader[] shaders, SpirvReflection reflection) = ShaderUtil.LoadEmbeddedShaderSet(
                typeof(SimpleMeshRender).Assembly, Factory, "SimpleMeshRender");
            ShaderSetDescription shadersDesc = new ShaderSetDescription(
                new[] { vertexLayout },
                shaders);

            _pipeline = Factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                Device.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyGreaterEqual
                    : DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shadersDesc,
                layout,
                Framebuffers[0].OutputDescription,
                reflection.VertexElements,
                reflection.ResourceLayouts));
            _camera = new Camera(Device, Framebuffers[0].Width, Framebuffers[0].Height);
            _camera.Position = new Vector3(0, 1, 3);

            GalleryConfig.Global.CameraInfoLayout = Device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));

            _uniformBuffers = new DeviceBuffer[Driver.BufferCount];
            _resourceSets = new ResourceSet[Driver.BufferCount];
            _uniformBuffers = new DeviceBuffer[Driver.BufferCount];
            _cameraInfoBuffers = new DeviceBuffer[Driver.BufferCount];
            GalleryConfig.Global.CameraInfoSets = new ResourceSet[Driver.BufferCount];

            for (uint i = 0; i < Driver.BufferCount; i++)
            {
                _uniformBuffers[i] = Factory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer));
                _resourceSets[i] = Factory.CreateResourceSet(
                    new ResourceSetDescription(layout, _uniformBuffers[i], catTexture, Device.LinearSampler));
                _cameraInfoBuffers[i] = Factory.CreateBuffer(
                    new BufferDescription((uint)Unsafe.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
                GalleryConfig.Global.CameraInfoSets[i] = Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                    GalleryConfig.Global.CameraInfoLayout, _cameraInfoBuffers[i]));
            }

            Console.WriteLine("Loading skybox renderer.");
            using (var face0 = OpenEmbeddedAsset("miramar_ft.png"))
            using (var face1 = OpenEmbeddedAsset("miramar_bk.png"))
            using (var face5 = OpenEmbeddedAsset("miramar_lf.png"))
            using (var face4 = OpenEmbeddedAsset("miramar_rt.png"))
            using (var face2 = OpenEmbeddedAsset("miramar_up.png"))
            using (var face3 = OpenEmbeddedAsset("miramar_dn.png"))
            {
                _skyboxRenderer = new SkyboxRenderer(Device, new[] { face0, face1, face2, face3, face4, face5 });
            }

            RecordFrameCommands();
        }

        protected override void OnGallerySizeChangedCore()
        {
            _camera.ViewSizeChanged(Framebuffers[0].Width, Framebuffers[0].Height);
            RecordFrameCommands();
        }

        private void RecordFrameCommands()
        {
            Util.DisposeAll(_frameCBs);
            _frameCBs = Enumerable.Range(0, (int)Driver.BufferCount).Select(frameIndex =>
            {
                CommandBuffer cb = Factory.CreateCommandBuffer(CommandBufferFlags.Reusable);
                cb.Name = $"SimpleMeshRender CB {frameIndex}";
                cb.BeginRenderPass(
                    Framebuffers[frameIndex],
                    LoadAction.Clear,
                    StoreAction.Store,
                    RgbaFloat.Red,
                    Device.IsDepthRangeZeroToOne ? 0f : 1f);
                cb.BindPipeline(_pipeline);
                cb.BindVertexBuffer(0, _vertexBuffer);
                cb.BindIndexBuffer(_indexBuffer, IndexFormat.UInt16);
                cb.BindGraphicsResourceSet(0, _resourceSets[frameIndex]);
                cb.DrawIndexed(_indexCount);

                _skyboxRenderer.Render(cb, (uint)frameIndex);
                cb.EndRenderPass();
                return cb;
            }).ToArray();
        }

        public override CommandBuffer[] Render(double deltaSeconds)
        {
            _camera.Update((float)deltaSeconds);
            Device.UpdateBuffer(_cameraInfoBuffers[Driver.FrameIndex], 0, _camera.GetCameraInfo());

            (Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world) uniformState =
                (
                _camera.ProjectionMatrix,
                _camera.ViewMatrix,
                Matrix4x4.CreateWorld(_modelPos, Vector3.UnitX, Vector3.UnitY)
                );
            _uniformBuffers[Driver.FrameIndex].Update(0, ref uniformState);

            return new[] { _frameCBs[Driver.FrameIndex] };
        }

        private Stream OpenEmbeddedAsset(string name)
            => typeof(SimpleMeshRender).Assembly.GetManifestResourceStream(name);
    }
}
