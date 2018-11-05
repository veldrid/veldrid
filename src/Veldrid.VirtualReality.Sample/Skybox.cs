using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;

namespace Veldrid.VirtualReality.Sample
{
    internal class Skybox
    {
        private readonly Image<Rgba32> _front;
        private readonly Image<Rgba32> _back;
        private readonly Image<Rgba32> _left;
        private readonly Image<Rgba32> _right;
        private readonly Image<Rgba32> _top;
        private readonly Image<Rgba32> _bottom;

        // Context objects
        private ResourceLayout _layout;
        private DeviceBuffer _vb;
        private DeviceBuffer _ib;
        private Pipeline _pipeline;
        private DeviceBuffer _ubo;
        private ResourceSet _resourceSet;
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public Skybox(
            Image<Rgba32> front, Image<Rgba32> back, Image<Rgba32> left,
            Image<Rgba32> right, Image<Rgba32> top, Image<Rgba32> bottom)
        {
            _front = front;
            _back = back;
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
        }

        public void CreateDeviceObjects(GraphicsDevice gd, OutputDescription outputs)
        {
            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription((uint)(s_vertices.Length * 12), BufferUsage.VertexBuffer));
            gd.UpdateBuffer(_vb, 0, s_vertices);

            _ib = factory.CreateBuffer(new BufferDescription((uint)(s_indices.Length * 2), BufferUsage.IndexBuffer));
            gd.UpdateBuffer(_ib, 0, s_indices);

            ImageSharpCubemapTexture imageSharpCubemapTexture = new ImageSharpCubemapTexture(_front, _back, _top, _bottom, _right, _left, true);

            Texture textureCube = imageSharpCubemapTexture.CreateDeviceTexture(gd, factory);
            TextureView textureView = factory.CreateTextureView(new TextureViewDescription(textureCube));

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
            };

            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.ASCII.GetBytes(VertexShader), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.ASCII.GetBytes(FragmentShader), "main"));
            _disposables.Add(shaders[0]);
            _disposables.Add(shaders[1]);

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UBO", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("CubeTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("CubeSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(vertexLayouts, shaders),
                new ResourceLayout[] { _layout },
                outputs);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _ubo = factory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                _layout,
                _ubo,
                textureView,
                gd.PointSampler));
        }

        public void Render(CommandList cl, Framebuffer fb, Matrix4x4 proj, Matrix4x4 view)
        {
            cl.UpdateBuffer(_ubo, 0, new UBO(proj, view, Matrix4x4.Identity));
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _resourceSet);
            float depth = 1;
            cl.SetViewport(0, new Viewport(0, 0, fb.Width, fb.Height, depth, depth));
            cl.DrawIndexed((uint)s_indices.Length, 1, 0, 0, 0);

            cl.SetViewport(0, new Viewport(0, 0, fb.Width, fb.Height, 0, 1));
        }

        private static readonly Vector3[] s_vertices = new Vector3[]
        {
            // Top
            new Vector3(-20.0f,20.0f,-20.0f),
            new Vector3(20.0f,20.0f,-20.0f),
            new Vector3(20.0f,20.0f,20.0f),
            new Vector3(-20.0f,20.0f,20.0f),
            // Bottom
            new Vector3(-20.0f,-20.0f,20.0f),
            new Vector3(20.0f,-20.0f,20.0f),
            new Vector3(20.0f,-20.0f,-20.0f),
            new Vector3(-20.0f,-20.0f,-20.0f),
            // Left
            new Vector3(-20.0f,20.0f,-20.0f),
            new Vector3(-20.0f,20.0f,20.0f),
            new Vector3(-20.0f,-20.0f,20.0f),
            new Vector3(-20.0f,-20.0f,-20.0f),
            // Right
            new Vector3(20.0f,20.0f,20.0f),
            new Vector3(20.0f,20.0f,-20.0f),
            new Vector3(20.0f,-20.0f,-20.0f),
            new Vector3(20.0f,-20.0f,20.0f),
            // Back
            new Vector3(20.0f,20.0f,-20.0f),
            new Vector3(-20.0f,20.0f,-20.0f),
            new Vector3(-20.0f,-20.0f,-20.0f),
            new Vector3(20.0f,-20.0f,-20.0f),
            // Front
            new Vector3(-20.0f,20.0f,20.0f),
            new Vector3(20.0f,20.0f,20.0f),
            new Vector3(20.0f,-20.0f,20.0f),
            new Vector3(-20.0f,-20.0f,20.0f),
        };

        private static readonly ushort[] s_indices = new ushort[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        };

        internal const string VertexShader =
@"
#version 450

layout (set = 0, binding = 0) uniform WVP
{
    mat4 Proj;
    mat4 View;
    mat4 World;
};

layout(location = 0) in vec3 vsin_Position;
layout(location = 0) out vec3 fsin_0;

void main()
{
    mat4 view3x3 = mat4(
        View[0][0], View[0][1], View[0][2], 0,
        View[1][0], View[1][1], View[1][2], 0,
        View[2][0], View[2][1], View[2][2], 0,
        0, 0, 0, 1);
    vec4 pos = Proj * view3x3 * vec4(vsin_Position, 1.0f);
    gl_Position = vec4(pos.x, pos.y, pos.w, pos.w);
    fsin_0 = vsin_Position;
}
";

        internal const string FragmentShader =
@"
#version 450

layout(set = 0, binding = 1) uniform textureCube CubeTexture;
layout(set = 0, binding = 2) uniform sampler CubeSampler;

layout(location = 0) in vec3 fsin_0;
layout(location = 0) out vec4 OutputColor;

void main()
{
    OutputColor = texture(samplerCube(CubeTexture, CubeSampler), fsin_0);
}
";
    }
}
