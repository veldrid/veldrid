using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo.ForwardRendering
{
    public class ShadowCaster : SwappableRenderItem, IDisposable, RayCastable
    {
        public string Name { get; set; } = "No Name";

        private readonly BoundingSphere _centeredBounds;

        private readonly DynamicDataProvider<Matrix4x4> _worldProvider = new DynamicDataProvider<Matrix4x4>();
        private readonly DependantDataProvider<Matrix4x4> _inverseTransposeWorldProvider;
        private readonly ConstantBufferDataProvider[] _perObjectProviders;

        private readonly MaterialAsset _shadowPassMaterialAsset;
        private readonly MaterialAsset _regularPassMaterialAsset;
        private readonly TextureData _overrideTextureData;

        private readonly string[] _stages = new string[] { "ShadowMap", "Standard" };

        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _shadowPassMaterial;
        private Material _regularPassMaterial;
        private DeviceTexture2D _overrideTexture;
        private ShaderTextureBinding _overrideTextureBinding;
        private readonly SimpleMeshDataProvider _meshData;

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        public ShadowCaster(
            RenderContext rc,
            AssetDatabase ad,
            VertexPositionNormalTexture[] vertices,
            ushort[] indices,
            MaterialAsset regularPassMaterial,
            TextureData overrideTexture = null)
        {
            _meshData = new SimpleMeshDataProvider(vertices, indices);

            _shadowPassMaterialAsset = ad.LoadAsset<MaterialAsset>("MaterialAsset/ShadowCaster_ShadowMap.json");
            _regularPassMaterialAsset = regularPassMaterial;
            _overrideTextureData = overrideTexture;

            _worldProvider = new DynamicDataProvider<Matrix4x4>();
            _inverseTransposeWorldProvider = new DependantDataProvider<Matrix4x4>(_worldProvider, Utilities.CalculateInverseTranspose);
            _perObjectProviders = new ConstantBufferDataProvider[] { _worldProvider, _inverseTransposeWorldProvider };

            _centeredBounds = BoundingSphere.CreateFromPoints(vertices);

            InitializeContextObjects(ad, rc);
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext context)
        {
            Dispose();
            InitializeContextObjects(ad, context);
        }

        private void InitializeContextObjects(AssetDatabase ad, RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
            _vb = factory.CreateVertexBuffer(_meshData.Vertices.Length * VertexPositionNormalTexture.SizeInBytes, false);
            _vb.SetVertexData(
                _meshData.Vertices,
                new VertexDescriptor(
                    VertexPositionNormalTexture.SizeInBytes,
                    VertexPositionNormalTexture.ElementCount,
                    0,
                    IntPtr.Zero));
            _ib = factory.CreateIndexBuffer(sizeof(int) * _meshData.Indices.Length, false);
            _ib.SetIndices(_meshData.Indices, IndexFormat.UInt16);

            _shadowPassMaterial = _shadowPassMaterialAsset.Create(ad, rc);
            _regularPassMaterial = _regularPassMaterialAsset.Create(ad, rc);
            if (_overrideTextureData != null)
            {
                _overrideTexture = _overrideTextureData.CreateDeviceTexture(factory);
                _overrideTextureBinding = factory.CreateShaderTextureBinding(_overrideTexture);
            }
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            float distance = Vector3.Distance(Position, viewPosition);
            return RenderOrderKey.Create(distance, _regularPassMaterial.GetHashCode());
        }

        public IList<string> GetStagesParticipated() => _stages;

        public void Render(RenderContext rc, string pipelineStage)
        {
            rc.VertexBuffer =_vb;
            rc.IndexBuffer =_ib;

            if (pipelineStage == "ShadowMap")
            {
                rc.Material =_shadowPassMaterial;
                _shadowPassMaterial.ApplyPerObjectInput(_worldProvider);
            }
            else
            {
                Debug.Assert(pipelineStage == "Standard");
                rc.Material = _regularPassMaterial;
                _regularPassMaterial.ApplyPerObjectInputs(_perObjectProviders);
                if (_overrideTextureBinding != null)
                {
                    _regularPassMaterial.UseTexture(0, _overrideTextureBinding);
                }
            }

            _worldProvider.Data =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateFromQuaternion(Rotation)
                * Matrix4x4.CreateTranslation(Position);

            rc.DrawIndexedPrimitives(_meshData.Indices.Length, 0);
        }

        private void Serialize<T>(ref T value)
        {
            JsonSerializer js = new JsonSerializer();
            js.TypeNameHandling = TypeNameHandling.All;
            var fileName = typeof(T).Name + ".json";

            using (var fs = File.CreateText(fileName))
            {
                js.Serialize(fs, value);
            }

            using (var fs = File.OpenText(fileName))
            {
                value = js.Deserialize<T>(new JsonTextReader(fs));
            }
        }

        public void Dispose()
        {
            _regularPassMaterial.Dispose();
            _shadowPassMaterial.Dispose();
            _overrideTexture?.Dispose();
            _overrideTextureBinding?.Dispose();
            _vb.Dispose();
            _ib.Dispose();
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            var boundingSphere = new BoundingSphere((_centeredBounds.Center * (Scale.X)) + Position, _centeredBounds.Radius * Scale.X);
            return visibleFrustum.Contains(boundingSphere) == ContainmentType.Disjoint;
        }

        public BoundingBox BoundingBox
        {
            get
            {
                return BoundingBox.CreateFromVertices(_meshData.Vertices, Rotation, Position, Scale);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Name, BoundingBox.GetCenter());
        }

        public int RayCast(Ray ray, List<float> distances)
        {
            return _meshData.RayCast(ray, distances);
        }
    }
}