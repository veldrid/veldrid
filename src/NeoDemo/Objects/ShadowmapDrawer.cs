using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo.Objects
{
    public class ShadowmapDrawer : Renderable
    {
        private readonly Func<Sdl2Window> _windowGetter;
        private readonly DisposeCollector _disposeCollector = new DisposeCollector();

        private DeviceBuffer _vb;
        private DeviceBuffer _ib;
        private DeviceBuffer _orthographicBuffer;
        private DeviceBuffer _sizeInfoBuffer;
        private Pipeline _pipeline;
        private ResourceSet _resourceSet;

        private Vector2 _position;
        private Vector2 _size = new Vector2(100, 100);

        private readonly Func<TextureView> _bindingGetter;
        private SizeInfo? _si;
        private Matrix4x4? _ortho;

        public Vector2 Position { get => _position; set { _position = value; UpdateSizeInfoBuffer(); } }

        public Vector2 Size { get => _size; set { _size = value; UpdateSizeInfoBuffer(); } }

        private void UpdateSizeInfoBuffer()
        {
            _si = new SizeInfo { Size = _size, Position = _position };
        }

        public ShadowmapDrawer(Func<Sdl2Window> windowGetter, Func<TextureView> bindingGetter)
        {
            _windowGetter = windowGetter;
            OnWindowResized();
            _bindingGetter = bindingGetter;
        }

        public void OnWindowResized()
        {
            _ortho = Matrix4x4.CreateOrthographicOffCenter(0, _windowGetter().Width, _windowGetter().Height, 0, -1, 1);
        }

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;
            _vb = factory.CreateBuffer(new BufferDescription(s_quadVerts.SizeInBytes(), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, s_quadVerts);
            _ib = factory.CreateBuffer(new BufferDescription(s_quadIndices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, s_quadIndices);

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate,  VertexElementFormat.Float2))
            };

            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "ShadowmapPreviewShader");

            ResourceLayout layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("SizePos", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("TexSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                new DepthStencilStateDescription(false, true, ComparisonKind.Always),
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    vertexLayouts,
                    new[] { vs, fs },
                    ShaderHelper.GetSpecializations(gd)),
                new ResourceLayout[] { layout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _sizeInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<SizeInfo>(), BufferUsage.UniformBuffer));
            UpdateSizeInfoBuffer();
            _orthographicBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer));

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _orthographicBuffer,
                _sizeInfoBuffer,
                _bindingGetter(),
                gd.PointSampler));

            OnWindowResized();

            _disposeCollector.Add(_vb, _ib, layout, vs, fs, _pipeline, _sizeInfoBuffer, _orthographicBuffer, _resourceSet);
        }

        public override void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return RenderOrderKey.Create(_pipeline.GetHashCode(), 0);
        }

        public override RenderPasses RenderPasses => RenderPasses.Overlay;

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            if (_si.HasValue)
            {
                cl.UpdateBuffer(_sizeInfoBuffer, 0, _si.Value);
                _si = null;
            }

            if (_ortho.HasValue)
            {
                cl.UpdateBuffer(_orthographicBuffer, 0, _ortho.Value);
                _ortho = null;
            }
        }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _resourceSet);
            cl.DrawIndexed((uint)s_quadIndices.Length, 1, 0, 0, 0);
        }

        private static float[] s_quadVerts = new float[]
        {
            0, 0, 0, 0,
            1, 0, 1, 0,
            1, 1, 1, 1,
            0, 1, 0, 1
        };

        private static ushort[] s_quadIndices = new ushort[] { 0, 1, 2, 0, 2, 3 };

        public struct SizeInfo
        {
            public Vector2 Position;
            public Vector2 Size;
        }
    }
}
