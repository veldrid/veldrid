using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class GeometryShaderBox : SwappableRenderItem
    {
        private readonly DynamicDataProvider<Matrix4x4> _worldProvider;
        private readonly string[] s_stages = { "Standard" };
        private readonly int _indexCount = 1;

        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private ShaderSet _shaderSet;
        private VertexInputLayout _inputLayout;
        private ShaderConstantBindings _constantBindings;

        public Vector3 Position { get; set; }

        public GeometryShaderBox(AssetDatabase ad, RenderContext rc)
        {
            InitializeContextObjects(ad, rc);
            _worldProvider = new DynamicDataProvider<Matrix4x4>(Matrix4x4.CreateTranslation(Position));
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            ClearDeviceResources();
            InitializeContextObjects(ad, rc);
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return false;
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return RenderOrderKey.Create(Vector3.Distance(Position, viewPosition), 0);
        }

        public IEnumerable<string> GetStagesParticipated()
        {
            return s_stages;
        }

        public void Render(RenderContext rc, string pipelineStage)
        {
            rc.VertexBuffer = _vb;
            rc.IndexBuffer = _ib;
            rc.DrawIndexedPrimitives(_indexCount);
        }

        private void InitializeContextObjects(AssetDatabase ad, RenderContext rc)
        {
            _vb = rc.ResourceFactory.CreateVertexBuffer(new[] { new VertexPosition(Vector3.Zero) }, new VertexDescriptor(12, 1), false);
            _ib = rc.ResourceFactory.CreateIndexBuffer(new[] { 0 }, false);
        }

        private void ClearDeviceResources()
        {
            _vb.Dispose();
            _ib.Dispose();
        }
    }
}