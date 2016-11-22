using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class TeapotRenderer : SwappableRenderItem, IDisposable
    {
        private readonly DynamicDataProvider<Matrix4x4> _worldProvider;
        private readonly DependantDataProvider<Matrix4x4> _inverseTransposeWorldProvider;
        private readonly ConstantBufferDataProvider[] _perObjectProviders;
        private readonly BoundingSphere _centeredBounds;

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Material _material;

        private static ConstructedMeshInfo _teapotMesh;

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        private static ConstructedMeshInfo LoadTeapotMesh()
        {
            if (_teapotMesh == null)
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Models", "Teapot.obj");
                using (var fs = File.OpenRead(path))
                {
                    _teapotMesh = new ObjParser().Parse(fs).GetFirstMesh();
                }
            }

            return _teapotMesh;
        }

        public TeapotRenderer(RenderContext rc)
        {
            _worldProvider = new DynamicDataProvider<Matrix4x4>();
            _inverseTransposeWorldProvider = new DependantDataProvider<Matrix4x4>(_worldProvider, CalculateInverseTranspose);
            _perObjectProviders = new ConstantBufferDataProvider[] { _worldProvider, _inverseTransposeWorldProvider };

            _centeredBounds = BoundingSphere.CreateFromPoints(LoadTeapotMesh().Vertices);

            InitializeContextObjects(rc);
        }

        private void InitializeContextObjects(RenderContext rc)
        {
            var factory = rc.ResourceFactory;
            var mesh = LoadTeapotMesh();

            _vertexBuffer = factory.CreateVertexBuffer(mesh.Vertices.Length * VertexPositionNormalTexture.SizeInBytes, false);
            _vertexBuffer.SetVertexData(mesh.Vertices, new VertexDescriptor(VertexPositionNormalTexture.SizeInBytes, 3, 0, IntPtr.Zero));

            _indexBuffer = factory.CreateIndexBuffer(mesh.Indices.Length * sizeof(int), false);
            _indexBuffer.SetIndices(mesh.Indices);

            MaterialVertexInput materialInputs = new MaterialVertexInput(
                VertexPositionNormalTexture.SizeInBytes,
                new MaterialVertexInputElement[]
                {
                    new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                    new MaterialVertexInputElement("in_normal", VertexSemanticType.Normal, VertexElementFormat.Float3),
                    new MaterialVertexInputElement("in_texCoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2)
                });

            MaterialInputs<MaterialGlobalInputElement> globalInputs = new MaterialInputs<MaterialGlobalInputElement>(
                new MaterialGlobalInputElement[]
                {
                    new MaterialGlobalInputElement("ProjectionMatrixBuffer", MaterialInputType.Matrix4x4, "ProjectionMatrix"),
                    new MaterialGlobalInputElement("ViewMatrixBuffer", MaterialInputType.Matrix4x4, "ViewMatrix"),
                    new MaterialGlobalInputElement("LightBuffer", MaterialInputType.Custom, "LightBuffer"),
                });

            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(
                new MaterialPerObjectInputElement[]
                {
                    new MaterialPerObjectInputElement("WorldMatrixBuffer", MaterialInputType.Matrix4x4, _worldProvider.DataSizeInBytes),
                    new MaterialPerObjectInputElement("InverseTransposeWorldMatrixBuffer", MaterialInputType.Matrix4x4, _inverseTransposeWorldProvider.DataSizeInBytes),
                });

            MaterialTextureInputs textureInputs = new MaterialTextureInputs(
                new TextureDataInputElement("surfaceTexture", s_cubeTexture));

            _material = factory.CreateMaterial(
                rc,
                "textured-vertex",
                "lit-frag",
                materialInputs,
                globalInputs,
                perObjectInputs,
                textureInputs);
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            Dispose();
            InitializeContextObjects(rc);
        }

        public void Dispose()
        {
            ((IDisposable)_vertexBuffer).Dispose();
            ((IDisposable)_indexBuffer).Dispose();
            ((IDisposable)_material).Dispose();
        }

        private Matrix4x4 CalculateInverseTranspose(Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Matrix4x4.Invert(m, out inverted);
            return Matrix4x4.Transpose(inverted);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return new RenderOrderKey();
        }

        public IEnumerable<string> GetStagesParticipated()
        {
            yield return "Standard";
        }

        public unsafe void Render(RenderContext context, string pipelineStage)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            _worldProvider.Data =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateFromQuaternion(Rotation)
                * Matrix4x4.CreateTranslation(Position);

            context.SetVertexBuffer(_vertexBuffer);
            context.SetIndexBuffer(_indexBuffer);
            context.SetMaterial(_material);
            _material.ApplyPerObjectInputs(_perObjectProviders);

            context.DrawIndexedPrimitives(_teapotMesh.Indices.Length, 0);
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            BoundingSphere sphere = new BoundingSphere(_centeredBounds.Center + Position, _centeredBounds.Radius * Scale.X);
            return visibleFrustum.Contains(sphere) == ContainmentType.Disjoint;
        }

        private static readonly ImageSharpTexture s_cubeTexture = new ImageSharpTexture(AppContext.BaseDirectory + "/Assets/Textures/CubeTexture.png");
    }
}
