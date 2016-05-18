using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Assets;
using Veldrid.Graphics;
using Veldrid.RenderDemo.ForwardRendering;
using Veldrid.RenderDemo.Models;

namespace Veldrid.RenderDemo.Drawers
{
    public class ModelDrawer : Drawer<ObjMeshInfo>
    {
        private static readonly ConditionalWeakTable<ObjMeshInfo, PreviewScene> _previewScenes = new ConditionalWeakTable<ObjMeshInfo, PreviewScene>();

        public override bool Draw(string label, ref ObjMeshInfo obj, RenderContext rc)
        {
            throw new NotImplementedException();
        }

        private class PreviewScene
        {
            RenderContext _rc;
            private readonly Framebuffer _fb;

            private readonly PreviewModel _previewItem;
            private readonly PreviewModel _floor;

            private readonly DynamicDataProvider<Matrix4x4> _projection;
            private readonly DynamicDataProvider<Matrix4x4> _view;
            private readonly Dictionary<string, ConstantBufferDataProvider> _sceneProviders = new Dictionary<string, ConstantBufferDataProvider>();

            public PreviewScene(RenderContext rc, ObjMeshInfo previewItem)
            {
                _rc = rc;
                int width = 400;
                ResourceFactory factory = rc.ResourceFactory;
                _fb = factory.CreateFramebuffer(width, width);

                _floor = CreatePreviewModel(PlaneModel.Vertices, PlaneModel.Indices);
                _previewItem = CreatePreviewModel(previewItem.Vertices, previewItem.Indices);
            }

            public DeviceTexture RenderedScene => _fb.ColorTexture;

            private PreviewModel CreatePreviewModel(VertexPositionNormalTexture[] vertices, int[] indices, ShaderTextureBinding textureBinding = null)
            {
                AssetDatabase lfd = new LooseFileDatabase(Path.Combine(AppContext.BaseDirectory, "Assets"));
                VertexBuffer vb = _rc.ResourceFactory.CreateVertexBuffer(vertices.Length * VertexPositionNormalTexture.SizeInBytes, false);
                IndexBuffer ib = _rc.ResourceFactory.CreateIndexBuffer(indices.Length * sizeof(int), false);

                MaterialAsset shadowmapAsset = lfd.LoadAsset<MaterialAsset>("Assets/MaterialAsset/ShadowCaster_ShadowMap.json");
                MaterialAsset surfaceMaterial = lfd.LoadAsset<MaterialAsset>("Assets/MaterialAsset/ModelPreview.json");
                Material shadowmapMaterial = shadowmapAsset.Create(lfd, _rc, _sceneProviders);
                Material regularMaterial = surfaceMaterial.Create(lfd, _rc, _sceneProviders);

                return new PreviewModel(
                    vb,
                    ib,
                    indices.Length,
                    regularMaterial,
                    shadowmapMaterial,
                    new DynamicDataProvider<Matrix4x4>(Matrix4x4.Identity),
                    textureBinding);
            }

            public void RenderFrame()
            {
                _rc.SetFramebuffer(_fb);
                _rc.ClearBuffer();
                _floor.Render(false, _rc);
                _previewItem.Render(false, _rc);
            }
        }

        private class PreviewModel
        {
            private readonly VertexBuffer _vb;
            private readonly IndexBuffer _ib;
            private readonly int _elementCount;
            private readonly Material _shadowmapMaterial;
            private readonly Material _regularMaterial;
            private readonly ConstantBufferDataProvider _worldProvider;
            private readonly ConstantBufferDataProvider _inverseWorldProvider;
            private readonly ConstantBufferDataProvider[] _perObjectInputs;
            private readonly ShaderTextureBinding _textureBinding;

            public ConstantBufferDataProvider WorldMatrix => _worldProvider;

            public PreviewModel(
                VertexBuffer vb,
                IndexBuffer ib,
                int elementCount,
                Material regularMaterial,
                Material shadowmapMaterial,
                DynamicDataProvider<Matrix4x4> worldProvider,
                ShaderTextureBinding surfaceTextureBinding = null)
            {
                _vb = vb;
                _ib = ib;
                _elementCount = elementCount;
                _regularMaterial = regularMaterial;
                _shadowmapMaterial = shadowmapMaterial;
                _worldProvider = worldProvider;
                _inverseWorldProvider = new DependantDataProvider<Matrix4x4>(worldProvider, Utilities.CalculateInverseTranspose);
                _perObjectInputs = new ConstantBufferDataProvider[] { _worldProvider, _inverseWorldProvider };
                _textureBinding = surfaceTextureBinding;
            }

            public void Render(bool shadowmap, RenderContext rc)
            {
                rc.SetVertexBuffer(_vb);
                rc.SetIndexBuffer(_ib);
                Material mat = shadowmap ? _shadowmapMaterial : _regularMaterial;
                rc.SetMaterial(mat);
                mat.ApplyPerObjectInputs(_perObjectInputs);
                if (_textureBinding != null)
                {
                    mat.UseTexture(0, _textureBinding);
                }

                rc.DrawIndexedPrimitives(_elementCount, 0);
            }
        }
    }
}
