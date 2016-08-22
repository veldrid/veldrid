using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class Skybox : SwappableRenderItem
    {
        private readonly ImageProcessorTexture _front;
        private readonly ImageProcessorTexture _back;
        private readonly ImageProcessorTexture _left;
        private readonly ImageProcessorTexture _right;
        private readonly ImageProcessorTexture _top;
        private readonly ImageProcessorTexture _bottom;

        // Context objects
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;
        private ShaderTextureBinding _cubemapBinding;
        private RasterizerState _rasterizerState;
        private ConstantBufferDataProvider _perObjectInput;

        public Skybox(RenderContext rc, AssetDatabase ad) : this(rc, ad,
            ad.LoadAsset<ImageProcessorTexture>("Textures/cloudtop/cloudtop_ft.png"),
            ad.LoadAsset<ImageProcessorTexture>("Textures/cloudtop/cloudtop_bk.png"),
            ad.LoadAsset<ImageProcessorTexture>("Textures/cloudtop/cloudtop_lf.png"),
            ad.LoadAsset<ImageProcessorTexture>("Textures/cloudtop/cloudtop_rt.png"),
            ad.LoadAsset<ImageProcessorTexture>("Textures/cloudtop/cloudtop_up.png"),
            ad.LoadAsset<ImageProcessorTexture>("Textures/cloudtop/cloudtop_dn.png"))
        { }

        public Skybox(RenderContext rc, AssetDatabase ad,
            ImageProcessorTexture front, ImageProcessorTexture back, ImageProcessorTexture left,
            ImageProcessorTexture right, ImageProcessorTexture top, ImageProcessorTexture bottom)
        {
            _front = front;
            _back = back;
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
            ChangeRenderContext(ad, rc);
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            var factory = rc.ResourceFactory;

            _vb = factory.CreateVertexBuffer(s_vertices.Length * VertexPosition.SizeInBytes, false);
            _vb.SetVertexData(s_vertices, new VertexDescriptor(VertexPosition.SizeInBytes, 1, 0, IntPtr.Zero));

            _ib = factory.CreateIndexBuffer(s_indices.Length * sizeof(int), false);
            _ib.SetIndices(s_indices);

            _material = ad.LoadAsset<MaterialAsset>("MaterialAsset/Skybox.json").Create(ad, rc);

            var viewProvider = (ConstantBufferDataProvider<Matrix4x4>)((ChangeableProvider)rc.GetNamedGlobalBufferProviderPair("ViewMatrix").DataProvider).DataProvider;
            _perObjectInput = new DependantDataProvider<Matrix4x4>(
                viewProvider,
                Utilities.ConvertToMatrix3x3);

            using (var frontPin = _front.Pixels.Pin())
            using (var backPin = _back.Pixels.Pin())
            using (var leftPin = _left.Pixels.Pin())
            using (var rightPin = _right.Pixels.Pin())
            using (var topPin = _top.Pixels.Pin())
            using (var bottomPin = _bottom.Pixels.Pin())
            {
                var cubemapTexture = factory.CreateCubemapTexture(
                    frontPin.Ptr,
                    backPin.Ptr,
                    leftPin.Ptr,
                    rightPin.Ptr,
                    topPin.Ptr,
                    bottomPin.Ptr,
                    _front.Width,
                    _front.Height,
                    RgbaFloat.SizeInBytes,
                    PixelFormat.R32_G32_B32_A32_Float);
                _cubemapBinding = factory.CreateShaderTextureBinding(cubemapTexture);
            }

            _rasterizerState = factory.CreateRasterizerState(FaceCullingMode.None, TriangleFillMode.Solid, false, false);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            // Render the skybox last.
            return new RenderOrderKey(ulong.MaxValue);
        }

        public IEnumerable<string> GetStagesParticipated()
        {
            yield return "Standard";
        }

        public void Render(RenderContext rc, string pipelineStage)
        {
            rc.SetVertexBuffer(_vb);
            rc.SetIndexBuffer(_ib);
            rc.SetMaterial(_material);
            RasterizerState previousRasterState = rc.RasterizerState;
            rc.SetRasterizerState(_rasterizerState);
            _material.UseTexture(0, _cubemapBinding);
            _material.ApplyPerObjectInput(_perObjectInput);
            rc.DrawIndexedPrimitives(s_indices.Length, 0);
            rc.SetRasterizerState(previousRasterState);
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return false;
        }

        private static readonly VertexPosition[] s_vertices = new VertexPosition[]
        {
            // Top
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            // Bottom
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            // Left
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            // Right
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            // Back
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            // Front
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
        };

        private static readonly int[] s_indices = new int[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        };
    }
}