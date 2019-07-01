using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
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

        public override async Task LoadResourcesAsync()
        {
            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Run(() =>
            {
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
            }));

            Texture catTexture = null;
            tasks.Add(Task.Run(async () =>
            {
                using (Stream catDiffFS = OpenEmbeddedAsset("cat_diff.png"))
                {
                    catTexture = StbTextureLoader.Load(Device, Factory, catDiffFS, false, true);
                }
            }));

            Task.WaitAll(tasks.ToArray());

            ResourceLayout layout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UniformState", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Smp", ResourceKind.Sampler, ShaderStages.Fragment)));

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("vsin_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3, 0),
                new VertexElementDescription("vsin_uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2, 24));

            ShaderSetDescription shadersDesc = new ShaderSetDescription(
                new[] { vertexLayout },
                Factory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main")));

            _pipeline = Factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                Device.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyGreaterEqual
                    : DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shadersDesc,
                layout,
                Framebuffers[0].OutputDescription));
            _camera = new Camera(Device, Framebuffers[0].Width, Framebuffers[0].Height);
            _camera.Position = new Vector3(0, 1, 3);

            GalleryConfig.Global.CameraInfoLayout = Device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));

            _uniformBuffers = new DeviceBuffer[Gallery.BufferCount];
            _resourceSets = new ResourceSet[Gallery.BufferCount];
            _uniformBuffers = new DeviceBuffer[Gallery.BufferCount];
            _cameraInfoBuffers = new DeviceBuffer[Gallery.BufferCount];
            GalleryConfig.Global.CameraInfoSets = new ResourceSet[Gallery.BufferCount];

            for (uint i = 0; i < Gallery.BufferCount; i++)
            {
                _uniformBuffers[i] = Factory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer));
                _resourceSets[i] = Factory.CreateResourceSet(
                    new ResourceSetDescription(layout, _uniformBuffers[i], catTexture, Device.LinearSampler));
                _cameraInfoBuffers[i] = Factory.CreateBuffer(
                    new BufferDescription((uint)Unsafe.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
                GalleryConfig.Global.CameraInfoSets[i] = Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                    GalleryConfig.Global.CameraInfoLayout, _cameraInfoBuffers[i]));

            }

            using (var face0 = OpenEmbeddedAsset("miramar_ft.png"))
            using (var face1 = OpenEmbeddedAsset("miramar_bk.png"))
            using (var face5 = OpenEmbeddedAsset("miramar_lf.png"))
            using (var face4 = OpenEmbeddedAsset("miramar_rt.png"))
            using (var face2 = OpenEmbeddedAsset("miramar_up.png"))
            using (var face3 = OpenEmbeddedAsset("miramar_dn.png"))
            {
                _skyboxRenderer = new SkyboxRenderer(Device, new[] { face0, face1, face2, face3, face4, face5 });
            }
        }

        protected override void OnGallerySizeChangedCore()
        {
            _camera.ViewSizeChanged(Framebuffers[0].Width, Framebuffers[0].Height);
        }

        public override void Render(double deltaSeconds, CommandBuffer cb)
        {
            _camera.Update((float)deltaSeconds);
            Device.UpdateBuffer(_cameraInfoBuffers[Gallery.FrameIndex], 0, _camera.GetCameraInfo());

            (Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world) uniformState =
                (
                _camera.ProjectionMatrix,
                _camera.ViewMatrix,
                Matrix4x4.CreateWorld(_modelPos, Vector3.UnitX, Vector3.UnitY)
                );

            cb.UpdateBuffer(_uniformBuffers[Gallery.FrameIndex], 0, ref uniformState);
            cb.BeginRenderPass(
                Framebuffers[Gallery.FrameIndex],
                LoadAction.Clear,
                StoreAction.Store,
                RgbaFloat.Red,
                Device.IsDepthRangeZeroToOne ? 0f : 1f);
            cb.BindPipeline(_pipeline);
            cb.BindVertexBuffer(0, _vertexBuffer);
            cb.BindIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cb.BindGraphicsResourceSet(0, _resourceSets[Gallery.FrameIndex]);

            //for (float y = -10; y <= 10; y += 2.5f)
            //    for (float x = -10; x <= 10; x += 2.5f)
            {
                //      uniformState.world = Matrix4x4.CreateWorld(_modelPos + new Vector3(x, y, 0), Vector3.UnitX, Vector3.UnitY);
                //      _cl.UpdateBuffer(_uniformBuffer, 0, ref uniformState);
                cb.DrawIndexed(_indexCount);
            }

            _skyboxRenderer.Render(cb, Gallery.FrameIndex);
            cb.EndRenderPass();
        }

        private Stream OpenEmbeddedAsset(string name)
            => typeof(SimpleMeshRender).Assembly.GetManifestResourceStream(name);

        private const string VertexCode =
@"#version 450 core
layout(location = 0) in vec3 vsin_position;
layout(location = 1) in vec2 vsin_uv;
layout(location = 0) out vec2 fsin_uv;

layout(set = 0, binding = 0) uniform UniformState
{
    mat4 Projection;  
    mat4 View;  
    mat4 World;  
};

void main()
{
    gl_Position = Projection * View * World * vec4(vsin_position, 1);
    fsin_uv = vsin_uv;
}";
        private const string FragmentCode =
@"#version 450 core
layout(location = 0) in vec2 fsin_uv;
layout(location = 0) out vec4 fsout_color;
layout(set = 0, binding = 1) uniform texture2D Tex;
layout(set = 0, binding = 2) uniform sampler Smp;

void main()
{
    fsout_color = texture(sampler2D(Tex, Smp), fsin_uv);
}
";
    }
}
