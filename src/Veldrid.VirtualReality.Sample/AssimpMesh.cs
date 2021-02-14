using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;

using Matrix4x4 = System.Numerics.Matrix4x4;

namespace Veldrid.VirtualReality.Sample
{
    internal class AssimpMesh : IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<MeshPiece> _meshPieces = new List<MeshPiece>();
        private readonly Pipeline _pipeline;
        private readonly DeviceBuffer _wvpBuffer;
        private readonly Texture _texture;
        private readonly TextureView _view;
        private readonly ResourceSet _rs;

        public AssimpMesh(GraphicsDevice gd, OutputDescription outputs, string meshPath, string texturePath)
        {
            _gd = gd;
            ResourceFactory factory = gd.ResourceFactory;

            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.ASCII.GetBytes(vertexGlsl), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.ASCII.GetBytes(fragmentGlsl), "main"));
            _disposables.Add(shaders[0]);
            _disposables.Add(shaders[1]);

            ResourceLayout rl = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WVP", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Input", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("InputSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _disposables.Add(rl);

            VertexLayoutDescription positionLayoutDesc = new VertexLayoutDescription(
                new VertexElementDescription[]
                {
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                });

            VertexLayoutDescription texCoordLayoutDesc = new VertexLayoutDescription(
                new VertexElementDescription[]
                {
                    new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                });

            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { positionLayoutDesc, texCoordLayoutDesc }, new Shader[] { shaders[0], shaders[1] }),
                rl,
                outputs));
            _disposables.Add(_pipeline);

            _wvpBuffer = factory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _disposables.Add(_wvpBuffer);

            _texture = new ImageSharpTexture(texturePath, true, true).CreateDeviceTexture(gd, factory);
            _view = factory.CreateTextureView(_texture);
            _disposables.Add(_texture);
            _disposables.Add(_view);

            _rs = factory.CreateResourceSet(new ResourceSetDescription(rl, _wvpBuffer, _view, gd.Aniso4xSampler));
            _disposables.Add(_rs);

            AssimpContext ac = new AssimpContext();
            Scene scene = ac.ImportFile(meshPath);

            foreach (Mesh mesh in scene.Meshes)
            {
                DeviceBuffer positions = CreateDeviceBuffer(mesh.Vertices, BufferUsage.VertexBuffer);
                DeviceBuffer texCoords = CreateDeviceBuffer(
                    mesh.TextureCoordinateChannels[0].Select(v3=>new Vector2(v3.X, v3.Y)).ToArray(),
                    BufferUsage.VertexBuffer);
                DeviceBuffer indices = CreateDeviceBuffer(mesh.GetUnsignedIndices(), BufferUsage.IndexBuffer);

                _meshPieces.Add(new MeshPiece(positions, texCoords, indices));
            }
        }

        public DeviceBuffer CreateDeviceBuffer<T>(IList<T> list, BufferUsage usage) where T : unmanaged
        {
            DeviceBuffer buffer = _gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<T>() * list.Count), usage));
            _disposables.Add(buffer);
            _gd.UpdateBuffer(buffer, 0, list.ToArray());
            return buffer;
        }

        public void Render(CommandList cl, UBO ubo)
        {
            cl.UpdateBuffer(_wvpBuffer, 0, ubo);
            cl.SetPipeline(_pipeline);
            foreach (MeshPiece piece in _meshPieces)
            {
                cl.SetVertexBuffer(0, piece.Positions);
                cl.SetVertexBuffer(1, piece.TexCoords);
                cl.SetIndexBuffer(piece.Indices, IndexFormat.UInt32);
                cl.SetGraphicsResourceSet(0, _rs);
                cl.DrawIndexed(piece.IndexCount);
            }
        }

        private const string vertexGlsl =
@"
#version 450

layout (set = 0, binding = 0) uniform WVP
{
    mat4 Proj;
    mat4 View;
    mat4 World;
};

layout (location = 0) in vec3 vsin_Position;
layout (location = 1) in vec2 vsin_UV;

layout (location = 0) out vec2 fsin_UV;

void main()
{
    gl_Position = Proj * View * World * vec4(vsin_Position, 1);
    fsin_UV = vsin_UV;
}
";
        private const string fragmentGlsl =
@"
#version 450

layout(set = 0, binding = 1) uniform texture2D Input;
layout(set = 0, binding = 2) uniform sampler InputSampler;

layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    vec2 uv = fsin_UV;
    uv.y = 1 - uv.y;

    fsout_Color0 = texture(sampler2D(Input, InputSampler), uv);
}
";

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables) { disposable.Dispose(); }
            _disposables.Clear();
        }
    }

    internal class MeshPiece
    {
        public DeviceBuffer Positions { get; }
        public DeviceBuffer TexCoords { get; }
        public DeviceBuffer Indices { get; }
        public uint IndexCount { get; }

        public MeshPiece(DeviceBuffer positions, DeviceBuffer texCoords, DeviceBuffer indices)
        {
            Positions = positions;
            TexCoords = texCoords;
            Indices = indices;
            IndexCount = indices.SizeInBytes / sizeof(uint);
        }
    }

    internal struct UBO
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
        public Matrix4x4 World;

        public UBO(Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            Projection = projection;
            View = view;
            World = world;
        }
    }
}
