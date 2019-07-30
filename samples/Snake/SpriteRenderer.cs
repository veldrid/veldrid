using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.SampleGallery;
using Veldrid.SPIRV;
using Veldrid.StbImage;

namespace Snake
{
    public class SpriteRenderer
    {
        private readonly List<SpriteInfo> _draws = new List<SpriteInfo>();

        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _textBuffer;
        private DeviceBuffer _orthoBuffer;
        private ResourceLayout _orthoLayout;
        private ResourceSet _orthoSet;
        private ResourceLayout _texLayout;
        private Pipeline _pipeline;

        private Dictionary<string, (Texture, ResourceSet)> _loadedImages
            = new Dictionary<string, (Texture, ResourceSet)>();
        private ResourceSet _textSet;

        public SpriteRenderer(GraphicsDevice gd)
        {
            ResourceFactory factory = gd.ResourceFactory;

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(1000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _textBuffer = factory.CreateBuffer(new BufferDescription(QuadVertex.VertexSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _orthoBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            _orthoLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("OrthographicProjection", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
            _orthoSet = factory.CreateResourceSet(new ResourceSetDescription(_orthoLayout, _orthoBuffer));

            _texLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SpriteTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SpriteSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            (Shader[] shaders, SpirvReflection reflection) = ShaderUtil.LoadEmbeddedShaderSet(
                typeof(SpriteRenderer).Assembly, factory, "Sprite");
            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleStrip,
                new ShaderSetDescription(
                    new VertexLayoutDescription[]
                    {
                        new VertexLayoutDescription(
                            QuadVertex.VertexSize,
                            1,
                            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                            new VertexElementDescription("Size", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                            new VertexElementDescription("Tint", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm),
                            new VertexElementDescription("Rotation", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1))
                    },
                    shaders),
                new[] { _orthoLayout, _texLayout },
                GalleryConfig.Global.MainFBOutput,
                reflection.VertexElements,
                reflection.ResourceLayouts));
        }

        public void AddSprite(Vector2 position, Vector2 size, string spriteName)
            => AddSprite(position, size, spriteName, RgbaByte.White, 0f);
        public void AddSprite(Vector2 position, Vector2 size, string spriteName, RgbaByte tint, float rotation)
        {
            _draws.Add(new SpriteInfo(spriteName, new QuadVertex(position, size, tint, rotation)));
        }

        private ResourceSet Load(GraphicsDevice gd, string spriteName)
        {
            if (!_loadedImages.TryGetValue(spriteName, out (Texture, ResourceSet) ret))
            {
                Texture tex;
                using (var stream = typeof(SpriteRenderer).Assembly.GetManifestResourceStream(spriteName))
                {
                    tex = StbTextureLoader.Load(gd, gd.ResourceFactory, stream, false, false);
                }

                ResourceSet set = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                    _texLayout,
                    tex,
                    gd.PointSampler));
                ret = (tex, set);
                _loadedImages.Add(spriteName, ret);
            }

            return ret.Item2;
        }

        public void Draw(GraphicsDevice gd, CommandBuffer cb)
        {
            if (_draws.Count == 0)
            {
                return;
            }

            float width = gd.MainSwapchain.Framebuffer.Width;
            float height = gd.MainSwapchain.Framebuffer.Height;
            _orthoBuffer.Update(
                0,
                Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, 0, 1));

            EnsureBufferSize(gd, (uint)_draws.Count * QuadVertex.VertexSize);
            QuadVertex[] vertData = new QuadVertex[_draws.Count];
            for (int i = 0; i < _draws.Count; i++)
            {
                vertData[i] = _draws[i].Quad;
            }
            _vertexBuffer.Update(0, vertData);

            cb.BindPipeline(_pipeline);
            cb.BindGraphicsResourceSet(0, _orthoSet);

            uint vbOffset = 0;
            for (int i = 0; i < _draws.Count;)
            {
                uint batchStart = (uint)i;
                string spriteName = _draws[i].SpriteName;
                ResourceSet rs = Load(gd, spriteName);
                cb.BindGraphicsResourceSet(1, rs);
                uint batchSize = 0;
                do
                {
                    i += 1;
                    batchSize += 1;
                }
                while (i < _draws.Count && _draws[i].SpriteName == spriteName);

                cb.BindVertexBuffer(0, _vertexBuffer, vbOffset);
                cb.Draw(4, batchSize, 0, 0);

                vbOffset += batchSize * QuadVertex.VertexSize;
            }

            _draws.Clear();
        }

        internal void RenderText(GraphicsDevice gd, CommandList cl, TextureView textureView, Vector2 pos)
        {
            cl.SetPipeline(_pipeline);
            cl.SetVertexBuffer(0, _textBuffer);
            cl.SetGraphicsResourceSet(0, _orthoSet);
            if (_textSet == null)
            {
                _textSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_texLayout, textureView, gd.PointSampler));
            }
            cl.SetGraphicsResourceSet(1, _textSet);
            Texture target = textureView.Target;
            cl.UpdateBuffer(_textBuffer, 0, new QuadVertex(pos, new Vector2(target.Width, target.Height)));
            cl.Draw(4, 1, 0, 0);
        }

        private void EnsureBufferSize(GraphicsDevice gd, uint size)
        {
            if (_vertexBuffer.SizeInBytes < size)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = gd.ResourceFactory.CreateBuffer(
                    new BufferDescription(size, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            }
        }

        private struct SpriteInfo
        {
            public SpriteInfo(string spriteName, QuadVertex quad)
            {
                SpriteName = spriteName;
                Quad = quad;
            }

            public string SpriteName { get; }
            public QuadVertex Quad { get; }
        }

        private struct QuadVertex
        {
            public const uint VertexSize = 24;

            public Vector2 Position;
            public Vector2 Size;
            public RgbaByte Tint;
            public float Rotation;

            public QuadVertex(Vector2 position, Vector2 size) : this(position, size, RgbaByte.White, 0f) { }
            public QuadVertex(Vector2 position, Vector2 size, RgbaByte tint, float rotation)
            {
                Position = position;
                Size = size;
                Tint = tint;
                Rotation = rotation;
            }
        }
    }
}
