using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public abstract class WireframeShapeRenderer : SwappableRenderItem, IDisposable
    {
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;
        private ConstantBuffer _worldBuffer;

        private static string[] s_stages = new string[] { "Standard" };
        private RasterizerState _wireframeState;

        protected List<WireframeVertex> _vertices = new List<WireframeVertex>();
        protected List<ushort> _indices = new List<ushort>();
        protected List<ushort> _wireframePolyfillIndices = new List<ushort>();

        public RgbaByte Color { get; set; } = RgbaByte.Cyan;

        public WireframeShapeRenderer(AssetDatabase ad, RenderContext rc, RgbaFloat color)
        {
            InitializeContextObjects(ad, rc);
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            Dispose();
            InitializeContextObjects(ad, rc);
        }

        private void InitializeContextObjects(AssetDatabase ad, RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
            _vb = factory.CreateVertexBuffer(1024, true);
            _ib = factory.CreateIndexBuffer(1024, true);

            Shader vs = factory.CreateShader(ShaderType.Vertex, "wireframe-vertex");
            Shader fs = factory.CreateShader(ShaderType.Fragment, "wireframe-frag");
            VertexInputLayout inputLayout = factory.CreateInputLayout(
                new VertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                new VertexInputElement("in_color", VertexSemanticType.Color, VertexElementFormat.Byte4));
            ShaderSet shaderSet = factory.CreateShaderSet(inputLayout, vs, fs);
            ShaderConstantBindingSlots cbs = factory.CreateShaderConstantBindingSlots(
                shaderSet,
                new ShaderConstantDescription("ProjectionMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderConstantDescription("ViewMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderConstantDescription("WorldMatrixBuffer", ShaderConstantType.Matrix4x4));
            ShaderTextureBindingSlots tbs = factory.CreateShaderTextureBindingSlots(shaderSet, Array.Empty<ShaderTextureInput>());
            _material = new Material(shaderSet, cbs, tbs);

            _worldBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);
            Matrix4x4 identity = Matrix4x4.Identity;
            _worldBuffer.SetData(ref identity, 64);

            _wireframeState = factory.CreateRasterizerState(FaceCullingMode.None, TriangleFillMode.Solid, true, true);
        }

        public abstract bool Cull(ref BoundingFrustum visibleFrustum);

        public void Dispose()
        {
            _vb?.Dispose();
            _ib?.Dispose();
            _material?.Dispose();
            _worldBuffer?.Dispose();
            _wireframeState?.Dispose();
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return RenderOrderKey.Create(_material.GetHashCode());
        }

        public IList<string> GetStagesParticipated()
        {
            return s_stages;
        }

        public void Render(RenderContext rc, string pipelineStage)
        {
            RasterizerState rasterState = rc.RasterizerState;
            UpdateBuffers(rc);

            rc.VertexBuffer = _vb;
            rc.IndexBuffer = _ib;
            _material.Apply(rc);
            rc.SetConstantBuffer(0, SharedDataProviders.ProjectionMatrixBuffer);
            rc.SetConstantBuffer(1, SharedDataProviders.ViewMatrixBuffer);
            rc.SetConstantBuffer(2, _worldBuffer);
            rc.RasterizerState = _wireframeState;
            rc.SetSamplerState(0, rc.PointSampler);
            rc.DrawIndexedPrimitives(_indices.Count, 0, PrimitiveTopology.LineList);
            rc.RasterizerState = rasterState;
        }

        private void UpdateBuffers(RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
            _vertices.Clear();
            _indices.Clear();
            AddVerticesAndIndices();

            _vb.Dispose();
            _ib.Dispose();

            // TODO: The vertex buffer should not need to be recreated every frame.
            _vb = factory.CreateVertexBuffer(_vertices.Count * WireframeVertex.SizeInBytes, false);
            _vb.SetVertexData(
                // TODO: This copying is very wasteful -- it happens every single frame.
                // Use a RawList type or similar.
                _vertices.ToArray(),
                new VertexDescriptor(
                    WireframeVertex.SizeInBytes,
                    WireframeVertex.ElementCount,
                    0,
                    IntPtr.Zero));
            // TODO: The index buffer should not need to be recreated every frame.
            _ib = factory.CreateIndexBuffer(sizeof(ushort) * _indices.Count, false);
            // TODO: This copying is very wasteful -- it happens every single frame.
            // Use a RawList type or similar.
            _ib.SetIndices(_indices.ToArray(), IndexFormat.UInt16);
        }

        protected abstract void AddVerticesAndIndices();

        protected struct WireframeVertex
        {
            public Vector3 Position;
            public RgbaByte Color;

            public WireframeVertex(Vector3 position, RgbaByte color) => (Position, Color) = (position, color);

            public const int SizeInBytes = 16;
            public const int ElementCount = 2;
        }
    }

    public class OctreeRenderer<T> : WireframeShapeRenderer
    {
        private OctreeNode<T> _octree;
        public OctreeNode<T> Octree
        {
            get { return _octree; }
            set { _octree = value; }
        }

        public OctreeRenderer(OctreeNode<T> octree, AssetDatabase ad, RenderContext rc) : base(ad, rc, RgbaFloat.Red)
        {
            _octree = octree;
        }

        public override bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return visibleFrustum.Contains(_octree.Bounds) == ContainmentType.Disjoint;
        }

        protected override void AddVerticesAndIndices()
        {
            AddVerticesAndIndices(_octree, _vertices, _indices);
        }

        private void AddVerticesAndIndices(OctreeNode<T> octree, List<WireframeVertex> vertices, List<ushort> indices)
        {
            // TODO: This is literally the exact same thing as the bounding box renderer, except recursive.
            ushort baseIndex = checked((ushort)vertices.Count);
            var bounds = octree.Bounds;

            vertices.Add(new WireframeVertex(new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Min.Z), Color));
            vertices.Add(new WireframeVertex(new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Min.Z), Color));
            vertices.Add(new WireframeVertex(new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Min.Z), Color));
            vertices.Add(new WireframeVertex(new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Min.Z), Color));
            vertices.Add(new WireframeVertex(new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Max.Z), Color));
            vertices.Add(new WireframeVertex(new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Max.Z), Color));
            vertices.Add(new WireframeVertex(new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Max.Z), Color));
            vertices.Add(new WireframeVertex(new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Max.Z), Color));

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 3));

            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 7));

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 1));

            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 3));

            foreach (var child in octree.Children)
            {
                AddVerticesAndIndices(child, vertices, indices);
            }
        }
    }

    public class FrustumWireframeRenderer : WireframeShapeRenderer
    {
        private BoundingFrustum _frustum;

        public FrustumWireframeRenderer(BoundingFrustum frustum, AssetDatabase ad, RenderContext rc)
            : base(ad, rc, RgbaFloat.Cyan)
        {
            _frustum = frustum;
        }

        public BoundingFrustum Frustum
        {
            get { return _frustum; }
            set { _frustum = value; }
        }

        public override bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return visibleFrustum.Contains(ref _frustum) == ContainmentType.Disjoint;
        }

        protected override void AddVerticesAndIndices()
        {
            ushort baseIndex = checked((ushort)_vertices.Count);
            FrustumCorners corners = _frustum.GetCorners();

            _vertices.Add(new WireframeVertex(corners.NearTopLeft, Color));
            _vertices.Add(new WireframeVertex(corners.NearTopRight, Color));
            _vertices.Add(new WireframeVertex(corners.NearBottomRight, Color));
            _vertices.Add(new WireframeVertex(corners.NearBottomLeft, Color));

            _vertices.Add(new WireframeVertex(corners.FarTopLeft, Color));
            _vertices.Add(new WireframeVertex(corners.FarTopRight, Color));
            _vertices.Add(new WireframeVertex(corners.FarBottomRight, Color));
            _vertices.Add(new WireframeVertex(corners.FarBottomLeft, Color));

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 3));

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 7));

            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 6));

            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 2));
        }
    }

    public class BoundingBoxWireframeRenderer : WireframeShapeRenderer
    {
        private BoundingBox _box;

        public BoundingBoxWireframeRenderer(BoundingBox box, AssetDatabase ad, RenderContext rc)
            : base(ad, rc, RgbaFloat.Cyan)
        {
            _box = box;
        }

        public BoundingBox Box
        {
            get { return _box; }
            set { _box = value; }
        }

        public override bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return visibleFrustum.Contains(_box) == ContainmentType.Disjoint;
        }

        protected override void AddVerticesAndIndices()
        {
            ushort baseIndex = checked((ushort)_vertices.Count);

            var min = _box.Min;
            var max = _box.Max;

            _vertices.Add(new WireframeVertex(new Vector3(min.X, min.Y, min.Z), Color));
            _vertices.Add(new WireframeVertex(new Vector3(min.X, max.Y, min.Z), Color));
            _vertices.Add(new WireframeVertex(new Vector3(max.X, max.Y, min.Z), Color));
            _vertices.Add(new WireframeVertex(new Vector3(max.X, min.Y, min.Z), Color));
            _vertices.Add(new WireframeVertex(new Vector3(min.X, min.Y, max.Z), Color));
            _vertices.Add(new WireframeVertex(new Vector3(min.X, max.Y, max.Z), Color));
            _vertices.Add(new WireframeVertex(new Vector3(max.X, max.Y, max.Z), Color));
            _vertices.Add(new WireframeVertex(new Vector3(max.X, min.Y, max.Z), Color));

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 3));

            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 7));

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 4));
            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 5));
            _indices.Add((ushort)(baseIndex + 1));

            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 6));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 7));
            _indices.Add((ushort)(baseIndex + 3));
        }
    }
}
