using ImGuiNET;
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
        private readonly int _indexCount = 1;
        private readonly Camera _camera;

        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;
        private readonly string _geometryShaderName;

        public Vector3 Position { get; set; }

        public GeometryShaderBox(AssetDatabase ad, RenderContext rc, Camera camera, string geometryShaderName = "cube-geometry")
        {
            _geometryShaderName = geometryShaderName;
            _worldProvider = new DynamicDataProvider<Matrix4x4>(Matrix4x4.CreateTranslation(Position));
            InitializeContextObjects(ad, rc);
            _camera = camera;
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

        public IList<string> GetStagesParticipated() => CommonStages.Standard;

        public void Render(RenderContext rc, string pipelineStage)
        {
            rc.VertexBuffer = _vb;
            rc.IndexBuffer = _ib;
            rc.Material = _material;
            _worldProvider.Data = Matrix4x4.CreateTranslation(Position);
            _material.ApplyPerObjectInput(_worldProvider);
            rc.DrawIndexedPrimitives(_indexCount, 0, PrimitiveTopology.PointList);
        }

        private void InitializeContextObjects(AssetDatabase ad, RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
            _vb = factory.CreateVertexBuffer(new[] { new VertexPosition(Vector3.Zero) }, new VertexDescriptor(12, 1), false);
            _ib = factory.CreateIndexBuffer(new[] { 0 }, false);
            Shader vertexShader = factory.CreateShader(ShaderType.Vertex, "geometry-vertex");
            Shader geometryShader = factory.CreateShader(ShaderType.Geometry, _geometryShaderName);
            Shader fragmentShader = factory.CreateShader(ShaderType.Fragment, "geometry-frag");
            VertexInputLayout inputLayout = factory.CreateInputLayout(
                vertexShader, new MaterialVertexInput(12, new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3)));
            ShaderSet shaderSet = factory.CreateShaderSet(inputLayout, vertexShader, geometryShader, fragmentShader);
            ShaderConstantBindings constantBindings = factory.CreateShaderConstantBindings(rc, shaderSet,
                new MaterialInputs<MaterialGlobalInputElement>(
                    new MaterialGlobalInputElement("ProjectionMatrixBuffer", MaterialInputType.Matrix4x4, "ProjectionMatrix"),
                    new MaterialGlobalInputElement("ViewMatrixBuffer", MaterialInputType.Matrix4x4, "ViewMatrix"),
                    new MaterialGlobalInputElement("CameraInfoBuffer", MaterialInputType.Custom, "CameraInfo")
                    ),
                new MaterialInputs<MaterialPerObjectInputElement>(
                    new MaterialPerObjectInputElement("WorldMatrixBuffer", MaterialInputType.Matrix4x4, _worldProvider.DataSizeInBytes)));
            ShaderTextureBindingSlots slots = factory.CreateShaderTextureBindingSlots(shaderSet, MaterialTextureInputs.Empty);
            _material = new Material(rc, shaderSet, constantBindings, slots, Array.Empty<DefaultTextureBindingInfo>());
        }

        private void ClearDeviceResources()
        {
            _vb.Dispose();
            _ib.Dispose();
            _material.Dispose();
        }
    }
}