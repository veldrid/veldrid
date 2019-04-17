using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AssetPrimitives;
using AssetProcessor;
using Veldrid.Utilities;
using Veldrid.SPIRV;
using System.Text;
using System.Numerics;

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
                ProcessedTexture tex;
                using (Stream catDiffFS = OpenEmbeddedAsset("cat_diff.png"))
                {
                    StbImageProcessor imageProcessor = new StbImageProcessor();
                    tex = await imageProcessor.ProcessT(catDiffFS, "png");
                }

                catTexture = tex.CreateDeviceTexture(Device, Factory, TextureUsage.Sampled);
            }));

            // Task.WhenAll(tasks);
            Task.WaitAll(tasks.ToArray());

            _cl = Factory.CreateCommandList();

            _uniformBuffer = Factory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer));
            ResourceLayout layout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UniformState", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Tex", ResourceKind.Sampler, ShaderStages.Fragment)));
            _resourceSet = Factory.CreateResourceSet(
                new ResourceSetDescription(layout, _uniformBuffer, catTexture, Device.LinearSampler));

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("vsin_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3, 0),
                new VertexElementDescription("vsin_uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2, 24));

            ShaderSetDescription shadersDesc = new ShaderSetDescription(
                new[] { vertexLayout },
                Factory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(Fragmentcode), "main")));

            _pipeline = Factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shadersDesc,
                layout,
                Framebuffer.OutputDescription));
            _camera = new Camera(Device, Framebuffer.Width, Framebuffer.Height);
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
            _cl.UpdateBuffer(_uniformBuffer, 0, ref uniformState);
            _cl.SetFramebuffer(Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0.05f, 1f));
            _cl.ClearDepthStencil(1.0f);
            _cl.SetPipeline(_pipeline);
            _cl.SetVertexBuffer(0, _vertexBuffer);
            _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _cl.SetGraphicsResourceSet(0, _resourceSet);
            _cl.DrawIndexed(_indexCount);
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
        private const string Fragmentcode =
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
