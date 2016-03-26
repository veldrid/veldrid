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
        private readonly MaterialVertexInput _vertexInput;
        private readonly MaterialInputs<MaterialGlobalInputElement> _shadowGlobalInputs;
        private readonly MaterialInputs<MaterialPerObjectInputElement> _perObjectInputs;
        private readonly MaterialTextureInputs _textureInputs;
        private readonly TextureData _surfaceTextureData;

        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _shadowPassMaterial;
        private Material _regularPassMaterial;

        public ShadowCaster(RenderContext rc, VertexPositionNormalTexture[] vertices, int[] indices, TextureData surfaceTexture)
        {
            _vertices = vertices;
            _indices = indices;
            _surfaceTextureData = surfaceTexture;
            _vertexInput = new MaterialVertexInput(
                VertexPositionNormalTexture.SizeInBytes,
                new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                new MaterialVertexInputElement("in_normal", VertexSemanticType.Normal, VertexElementFormat.Float3),
                new MaterialVertexInputElement("in_texcoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2));

            _shadowGlobalInputs = new MaterialInputs<MaterialGlobalInputElement>(
                new MaterialGlobalInputElement("ProjectionMatrix", MaterialInputType.Matrix4x4, rc.DataProviders["ProjectionMatrix"]),
                new MaterialGlobalInputElement("ViewMatrix", MaterialInputType.Matrix4x4, rc.DataProviders["ViewMatrix"]));

            _perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(
                new MaterialPerObjectInputElement("WorldMatrix", MaterialInputType.Matrix4x4, _worldProvider.DataSizeInBytes));

            _textureInputs = new MaterialTextureInputs(
                new MaterialTextureInputElement("SurfaceTexture", _surfaceTextureData));

            InitializeContextObjects(rc);
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
                "shadow-vert",
                "shadow-frag",
                _vertexInput,
                _shadowGlobalInputs,
                _perObjectInputs,
                _textureInputs);
        }

        public RenderOrderKey GetRenderOrderKey()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetStagesParticipated()
        {
            yield return "ShadowMap";
            yield return "Standard";
        }

        public void Render(RenderContext context, string pipelineStage)
        {
            if (pipelineStage == "ShadowMap")
            {
                _shadowPassMaterial.Apply();
                _shadowPassMaterial.ApplyPerObjectInput(_worldProvider);
            }
            else
            {
                Debug.Assert(pipelineStage == "Standard");
                _regularPassMaterial.Apply();
                _regularPassMaterial.ApplyPerObjectInput(_worldProvider);
            }

            _vb.Apply();
            _ib.Apply();

            context.DrawIndexedPrimitives(_indices.Length, 0);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}