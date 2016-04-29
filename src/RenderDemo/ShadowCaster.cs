using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class ShadowCaster : SwappableRenderItem, IDisposable
    {
        private readonly VertexPositionNormalTexture[] _vertices;
        private readonly int[] _indices;

        private readonly DynamicDataProvider<Matrix4x4> _worldProvider = new DynamicDataProvider<Matrix4x4>();
        private readonly DependantDataProvider<Matrix4x4> _inverseTransposeWorldProvider;
        private readonly ConstantBufferDataProvider[] _perObjectProviders;

        private readonly MaterialAsset _shadowPassMaterialAsset;
        private readonly MaterialAsset _regularPassMaterialAsset;

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

            _shadowPassMaterialAsset = AssetDatabase.Load<MaterialAsset>("ShadowCaster_ShadowMap");
            _regularPassMaterialAsset = AssetDatabase.Load<MaterialAsset>("ShadowCaster_RegularPass");

            _worldProvider = new DynamicDataProvider<Matrix4x4>();
            _inverseTransposeWorldProvider = new DependantDataProvider<Matrix4x4>(_worldProvider, CalculateInverseTranspose);
            _perObjectProviders = new ConstantBufferDataProvider[] { _worldProvider, _inverseTransposeWorldProvider };

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

        private void InitializeContextObjects(RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
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

            var surfaceTextureElement = new TextureDataInputElement("SurfaceTexture", _surfaceTextureData);
            _shadowPassMaterial = _shadowPassMaterialAsset.Create(rc);
            _regularPassMaterial = _regularPassMaterialAsset.Create(rc, surfaceTextureElement);
        }

        public RenderOrderKey GetRenderOrderKey()
        {
            return new RenderOrderKey();
        }

        public IEnumerable<string> GetStagesParticipated() => _stages;

        public void Render(RenderContext rc, string pipelineStage)
        {
            rc.SetVertexBuffer(_vb);
            rc.SetIndexBuffer(_ib);

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

            rc.DrawIndexedPrimitives(_indices.Length, 0);
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
            _vb.Dispose();
            _ib.Dispose();
        }
    }
}