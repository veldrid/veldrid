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
        private ResourceSet _resourceSet;
        private CommandList _cl;
        private DeviceBuffer _uniformBuffer;
        private Vector3 _modelPos = new Vector3(0, 0, 0);
        private Camera _camera;
        private SkyboxRenderer _skyboxRenderer;
        private DeviceBuffer _cameraInfoBuffer;
        private bool _done;

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

            _cl = Factory.CreateCommandList();

            _uniformBuffer = Factory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer));
            ResourceLayout layout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UniformState", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Smp", ResourceKind.Sampler, ShaderStages.Fragment)));
            _resourceSet = Factory.CreateResourceSet(
                new ResourceSetDescription(layout, _uniformBuffer, catTexture, Device.LinearSampler));

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
                Framebuffer.OutputDescription));
            _camera = new Camera(Device, Framebuffer.Width, Framebuffer.Height);
            _camera.Position = new Vector3(0, 1, 3);

            _cameraInfoBuffer = Factory.CreateBuffer(
                new BufferDescription((uint)Unsafe.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            GalleryConfig.Global.CameraInfoLayout = Device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));
            GalleryConfig.Global.CameraInfoSet = Device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                GalleryConfig.Global.CameraInfoLayout, _cameraInfoBuffer));

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
            _camera.ViewSizeChanged(Framebuffer.Width, Framebuffer.Height);
        }

        public override void Render(double deltaSeconds)
        {
            _camera.Update((float)deltaSeconds);

            (Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world) uniformState =
                (
                _camera.ProjectionMatrix,
                _camera.ViewMatrix,
                Matrix4x4.CreateWorld(_modelPos, Vector3.UnitX, Vector3.UnitY)
                );

            _cl.Begin();
            if (!_done)
            {
                Device.UpdateBuffer(_cameraInfoBuffer, 0, _camera.GetCameraInfo());
                _cl.UpdateBuffer(_uniformBuffer, 0, ref uniformState);
                _done = true;
            }
            _cl.SetFramebuffer(Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0.05f, 1f));
            _cl.ClearDepthStencil(Device.IsDepthRangeZeroToOne ? 0f : 1f);
            _cl.SetPipeline(_pipeline);
            _cl.SetVertexBuffer(0, _vertexBuffer);
            _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _cl.SetGraphicsResourceSet(0, _resourceSet);


            //for (float y = -10; y <= 10; y += 2.5f)
            //    for (float x = -10; x <= 10; x += 2.5f)
                {
              //      uniformState.world = Matrix4x4.CreateWorld(_modelPos + new Vector3(x, y, 0), Vector3.UnitX, Vector3.UnitY);
              //      _cl.UpdateBuffer(_uniformBuffer, 0, ref uniformState);
                    _cl.DrawIndexed(_indexCount);
                }

            _skyboxRenderer.Render(_cl);

            _cl.End();
            Device.SubmitCommands(_cl);
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
