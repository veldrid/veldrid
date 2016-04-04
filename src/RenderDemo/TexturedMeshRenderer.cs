using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class TexturedMeshRenderer : RenderItem, IDisposable
    {
        private readonly DynamicDataProvider<Matrix4x4> _worldProvider;
        private readonly DependantDataProvider<Matrix4x4> _inverseTransposeWorldProvider;
        private readonly ConstantBufferDataProvider[] _perObjectProviders;
        private readonly VertexPositionNormalTexture[] _vertices;
        private readonly int[] _indices;
        private readonly TextureData _texture;

        private RenderContext _currentContext;
        private VertexBuffer s_vb;
        private IndexBuffer s_ib;
        private Material s_material;

        public Vector3 Position { get; internal set; }
        public Vector3 Scale { get; internal set; } = new Vector3(1f);

        public TexturedMeshRenderer(RenderContext context, VertexPositionNormalTexture[] vertices, int[] indices, TextureData texture)
        {
            _worldProvider = new DynamicDataProvider<Matrix4x4>();
            _inverseTransposeWorldProvider = new DependantDataProvider<Matrix4x4>(_worldProvider, CalculateInverseTranspose);
            _perObjectProviders = new ConstantBufferDataProvider[] { _worldProvider, _inverseTransposeWorldProvider };
            _vertices = vertices;
            _indices = indices;
            _texture = texture;

            if (context != _currentContext)
            {
                if (_currentContext == null)
                {
                    InitializeContextObjects(context);
                }
                else
                {
                    ChangeRenderContext(context);
                }
            }
        }

        public void ChangeRenderContext(RenderContext rc)
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

            s_vb = factory.CreateVertexBuffer(VertexPositionNormalTexture.SizeInBytes * _vertices.Length, false);
            VertexDescriptor desc = new VertexDescriptor(VertexPositionNormalTexture.SizeInBytes, VertexPositionNormalTexture.ElementCount, 0, IntPtr.Zero);
            s_vb.SetVertexData(_vertices, desc);

            s_ib = factory.CreateIndexBuffer(sizeof(int) * _indices.Length, false);
            s_ib.SetIndices(_indices);

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
                    new MaterialGlobalInputElement("projectionMatrixUniform", MaterialInputType.Matrix4x4, context.DataProviders["ProjectionMatrix"]),
                    new MaterialGlobalInputElement("viewMatrixUniform", MaterialInputType.Matrix4x4, context.DataProviders["ViewMatrix"]),
                    new MaterialGlobalInputElement("LightBuffer", MaterialInputType.Custom, context.DataProviders["LightBuffer"]),
                });

            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(
                new MaterialPerObjectInputElement[]
                {
                    new MaterialPerObjectInputElement("WorldMatrix", MaterialInputType.Matrix4x4, _worldProvider.DataSizeInBytes),
                    new MaterialPerObjectInputElement("inverseTransposeWorldMatrixUniform", MaterialInputType.Matrix4x4, _inverseTransposeWorldProvider.DataSizeInBytes),
                });

            MaterialTextureInputs textureInputs = new MaterialTextureInputs(
                new MaterialTextureInputElement[]
                {
                    new TextureDataInputElement("surfaceTexture", _texture)
                });

            s_material = factory.CreateMaterial(
                context,
                VertexShaderSource,
                FragmentShaderSource,
                materialInputs,
                globalInputs,
                perObjectInputs,
                textureInputs);
        }

        private Matrix4x4 CalculateInverseTranspose(Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Matrix4x4.Invert(m, out inverted);
            return Matrix4x4.Transpose(inverted);
        }

        public IEnumerable<string> GetStagesParticipated()
        {
            yield return "Standard";
        }

        public void Render(RenderContext context, string pipelineStage)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            _worldProvider.Data =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateTranslation(Position);

            context.SetVertexBuffer(s_vb);
            context.SetIndexBuffer(s_ib);
            context.SetMaterial(s_material);
            s_material.ApplyPerObjectInputs(_perObjectProviders);

            context.DrawIndexedPrimitives(_indices.Length, 0);
        }

        public RenderOrderKey GetRenderOrderKey()
        {
            return new RenderOrderKey();
        }

        public void Dispose()
        {
            s_vb.Dispose();
            s_ib.Dispose();
            s_material.Dispose();
        }

        private static readonly string VertexShaderSource = "textured-vertex";
        private static readonly string FragmentShaderSource = "lit-frag";
    }
}
