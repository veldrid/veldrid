using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class TexturedMeshRenderer : SwappableRenderItem, IDisposable
    {
        private readonly DynamicDataProvider<Matrix4x4> _worldProvider;
        private readonly DependantDataProvider<Matrix4x4> _inverseTransposeWorldProvider;
        private readonly ConstantBufferDataProvider[] _perObjectProviders;
        private readonly VertexPositionNormalTexture[] _vertices;
        private readonly ushort[] _indices;
        private readonly TextureData _texture;
        private BoundingSphere _centeredBounds;

        private RenderContext _currentContext;
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;
        private SamplerState _samplerState;

        private static RasterizerState s_wireframeRasterizerState;

        public Vector3 Position { get; internal set; }
        public Vector3 Scale { get; internal set; } = new Vector3(1f);

        public TexturedMeshRenderer(AssetDatabase ad, RenderContext context, VertexPositionNormalTexture[] vertices, ushort[] indices, TextureData texture)
        {
            _worldProvider = new DynamicDataProvider<Matrix4x4>();
            _inverseTransposeWorldProvider = new DependantDataProvider<Matrix4x4>(_worldProvider, CalculateInverseTranspose);
            _perObjectProviders = new ConstantBufferDataProvider[] { _worldProvider, _inverseTransposeWorldProvider };
            _vertices = vertices;
            _indices = indices;
            _texture = texture;

            _centeredBounds = BoundingSphere.CreateFromPoints(vertices);

            if (context != _currentContext)
            {
                if (_currentContext == null)
                {
                    InitializeContextObjects(context);
                }
                else
                {
                    ChangeRenderContext(ad, context);
                }
            }
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            if (_currentContext != rc)
            {
                Dispose();
                InitializeContextObjects(rc);
            }
        }

        private void InitializeContextObjects(RenderContext context)
        {
            _currentContext = context;
            ResourceFactory factory = context.ResourceFactory;

            _vb = factory.CreateVertexBuffer(VertexPositionNormalTexture.SizeInBytes * _vertices.Length, false);
            VertexDescriptor desc = new VertexDescriptor(VertexPositionNormalTexture.SizeInBytes, VertexPositionNormalTexture.ElementCount, 0, IntPtr.Zero);
            _vb.SetVertexData(_vertices, desc);

            _ib = factory.CreateIndexBuffer(sizeof(ushort) * _indices.Length, false);
            _ib.SetIndices(_indices, IndexFormat.UInt16);

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
                new MaterialTextureInputElement[]
                {
                    new TextureDataInputElement("surfaceTexture", _texture)
                });

            _material = factory.CreateMaterial(
                context,
                VertexShaderSource,
                FragmentShaderSource,
                materialInputs,
                globalInputs,
                perObjectInputs,
                textureInputs);

            _samplerState = factory.CreateSamplerState(
                SamplerAddressMode.Wrap, SamplerAddressMode.Wrap, SamplerAddressMode.Wrap,
                SamplerFilter.MinMagMipPoint,
                1,
                RgbaFloat.Pink,
                DepthComparison.Always,
                0,
                int.MaxValue,
                0);

            s_wireframeRasterizerState = factory.CreateRasterizerState(FaceCullingMode.None, TriangleFillMode.Wireframe, true, true);
        }

        private Matrix4x4 CalculateInverseTranspose(Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Matrix4x4.Invert(m, out inverted);
            return Matrix4x4.Transpose(inverted);
        }

        public IList<string> GetStagesParticipated() => CommonStages.Standard;

        public void Render(RenderContext rc, string pipelineStage)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            _worldProvider.Data =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateTranslation(Position);

            rc.VertexBuffer = _vb;
            rc.IndexBuffer = _ib;
            rc.Material = _material;
            _material.ApplyPerObjectInputs(_perObjectProviders);
            rc.SetSamplerState(0, _samplerState);

            rc.DrawIndexedPrimitives(_indices.Length, 0);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return new RenderOrderKey();
        }

        public void Dispose()
        {
            _vb.Dispose();
            _ib.Dispose();
            _material.Dispose();
            _samplerState.Dispose();
            s_wireframeRasterizerState.Dispose();
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            BoundingSphere sphere = new BoundingSphere(_centeredBounds.Center + Position, _centeredBounds.Radius * Scale.X);
            return visibleFrustum.Contains(sphere) == ContainmentType.Disjoint;
        }

        private static readonly string VertexShaderSource = "textured-vertex";
        private static readonly string FragmentShaderSource = "lit-frag";
    }
}
