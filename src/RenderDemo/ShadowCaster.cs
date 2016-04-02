using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class ShadowCaster : RenderItem, IDisposable
    {
        private readonly VertexPositionNormalTexture[] _vertices;
        private readonly int[] _indices;

        private readonly DynamicDataProvider<Matrix4x4> _worldProvider = new DynamicDataProvider<Matrix4x4>();
        private readonly DependantDataProvider<Matrix4x4> _inverseTransposeWorldProvider;
        private readonly ConstantBufferDataProvider[] _perObjectProviders;

        private readonly MaterialVertexInput _vertexInput;
        private readonly MaterialInputs<MaterialGlobalInputElement> _shadowGlobalInputs;
        private readonly MaterialInputs<MaterialGlobalInputElement> _regularPassGlobalInputs;
        private readonly MaterialInputs<MaterialPerObjectInputElement> _shadowMapPerObjectInputs;
        private readonly MaterialInputs<MaterialPerObjectInputElement> _perObjectInputs;
        private readonly MaterialTextureInputs _textureInputs;
        private readonly TextureData _surfaceTextureData;
        private readonly string[] _stages = new string[] { "ShadowMap", "Standard" };

        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _shadowPassMaterial;
        private Material _regularPassMaterial;

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        public ShadowCaster(RenderContext rc, VertexPositionNormalTexture[] vertices, int[] indices, TextureData surfaceTexture)
        {
            _vertices = vertices;
            _indices = indices;
            _surfaceTextureData = surfaceTexture;

            _worldProvider = new DynamicDataProvider<Matrix4x4>();
            _inverseTransposeWorldProvider = new DependantDataProvider<Matrix4x4>(_worldProvider, CalculateInverseTranspose);
            _perObjectProviders = new ConstantBufferDataProvider[] { _worldProvider, _inverseTransposeWorldProvider };

            _vertexInput = new MaterialVertexInput(
                VertexPositionNormalTexture.SizeInBytes,
                new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                new MaterialVertexInputElement("in_normal", VertexSemanticType.Normal, VertexElementFormat.Float3),
                new MaterialVertexInputElement("in_texCoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2));

            _shadowGlobalInputs = new MaterialInputs<MaterialGlobalInputElement>(
                new MaterialGlobalInputElement("ProjectionMatrix", MaterialInputType.Matrix4x4, rc.DataProviders["LightProjMatrix"]),
                new MaterialGlobalInputElement("ViewMatrix", MaterialInputType.Matrix4x4, rc.DataProviders["LightViewMatrix"]));

            _regularPassGlobalInputs = new MaterialInputs<MaterialGlobalInputElement>(
                    new MaterialGlobalInputElement("ProjectionMatrixBuffer", MaterialInputType.Matrix4x4, rc.DataProviders["ProjectionMatrix"]),
                    new MaterialGlobalInputElement("ViewMatrixBuffer", MaterialInputType.Matrix4x4, rc.DataProviders["ViewMatrix"]),
                    new MaterialGlobalInputElement("LightProjectionMatrixBuffer", MaterialInputType.Matrix4x4, rc.DataProviders["LightProjMatrix"]),
                    new MaterialGlobalInputElement("LightViewMatrixBuffer", MaterialInputType.Matrix4x4, rc.DataProviders["LightViewMatrix"]),
                    new MaterialGlobalInputElement("LightInfoBuffer", MaterialInputType.Float4, rc.DataProviders["LightInfo"]));

            _shadowMapPerObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(
                    new MaterialPerObjectInputElement("WorldMatrix", MaterialInputType.Matrix4x4, _worldProvider.DataSizeInBytes));

            _perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(
                    new MaterialPerObjectInputElement("WorldMatrixBuffer", MaterialInputType.Matrix4x4, _worldProvider.DataSizeInBytes),
                    new MaterialPerObjectInputElement("InverseTransposeWorldMatrixBuffer", MaterialInputType.Matrix4x4, _inverseTransposeWorldProvider.DataSizeInBytes));

            _textureInputs = new MaterialTextureInputs(
                new TextureDataInputElement("SurfaceTexture", _surfaceTextureData),
                new ContextTextureInputElement("ShadowMap"));


            InitializeContextObjects(rc);
        }

        private Matrix4x4 CalculateInverseTranspose(Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Matrix4x4.Invert(m, out inverted);
            return Matrix4x4.Transpose(inverted);
        }

        public void ChangeRenderContext(RenderContext context)
        {
            Dispose();
            InitializeContextObjects(context);
        }

        private void InitializeContextObjects(RenderContext context)
        {
            ResourceFactory factory = context.ResourceFactory;
            _vb = factory.CreateVertexBuffer(_vertices.Length * VertexPositionNormalTexture.SizeInBytes, false);
            _vb.SetVertexData(
                _vertices,
                new VertexDescriptor(
                    VertexPositionNormalTexture.SizeInBytes,
                    VertexPositionNormalTexture.ElementCount,
                    0,
                    IntPtr.Zero));
            _ib = factory.CreateIndexBuffer(sizeof(int) * _indices.Length, false);
            _ib.SetIndices(_indices);

            _shadowPassMaterial = factory.CreateMaterial(
                context,
                "shadowmap-vertex",
                "shadowmap-frag",
                _vertexInput,
                _shadowGlobalInputs,
                _shadowMapPerObjectInputs,
                MaterialTextureInputs.Empty);

            _regularPassMaterial = factory.CreateMaterial(
                context,
                "shadow-vertex",
                "shadow-frag",
                _vertexInput,
                _regularPassGlobalInputs,
                _perObjectInputs,
                _textureInputs);
        }

        public RenderOrderKey GetRenderOrderKey()
        {
            return new RenderOrderKey();
        }

        public IEnumerable<string> GetStagesParticipated() => _stages;

        public void Render(RenderContext rc, string pipelineStage)
        {
            if (pipelineStage == "ShadowMap")
            {
                rc.SetMaterial(_shadowPassMaterial);
                _shadowPassMaterial.ApplyPerObjectInput(_worldProvider);
            }
            else
            {
                Debug.Assert(pipelineStage == "Standard");
                rc.SetMaterial(_regularPassMaterial);
                _regularPassMaterial.ApplyPerObjectInputs(_perObjectProviders);
            }

            _worldProvider.Data =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateFromQuaternion(Rotation)
                * Matrix4x4.CreateTranslation(Position);

            rc.SetVertexBuffer(_vb);
            rc.SetIndexBuffer(_ib);

            rc.DrawIndexedPrimitives(_indices.Length, 0);
        }

        public void Dispose()
        {
            _regularPassMaterial.Dispose();
            _shadowPassMaterial.Dispose();
            _vb.Dispose();
            _ib.Dispose();
        }
    }
}