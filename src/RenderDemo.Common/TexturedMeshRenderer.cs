using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class TexturedMeshRenderer : SwappableRenderItem, IDisposable
    {
        private readonly VertexPositionNormalTexture[] _vertices;
        private readonly ushort[] _indices;
        private readonly TextureData _texture;
        private BoundingSphere _centeredBounds;

        private RenderContext _currentContext;
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;
        private ConstantBuffer _worldBuffer;
        private ConstantBuffer _inverseTransposeWorldBuffer;
        private ShaderTextureBinding _textureBinding;

        private static RasterizerState s_wireframeRasterizerState;

        public Vector3 Position { get; internal set; }
        public Vector3 Scale { get; internal set; } = new Vector3(1f);

        public TexturedMeshRenderer(AssetDatabase ad, RenderContext context, VertexPositionNormalTexture[] vertices, ushort[] indices, TextureData texture)
        {
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

            VertexInputDescription materialInputs = new VertexInputDescription(
                VertexPositionNormalTexture.SizeInBytes,
                new VertexInputElement[]
                {
                    new VertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                    new VertexInputElement("in_normal", VertexSemanticType.Normal, VertexElementFormat.Float3),
                    new VertexInputElement("in_texCoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2)
                });

            ShaderConstantDescription[] constants = new[]
            {
                new ShaderConstantDescription("ProjectionMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderConstantDescription("ViewMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderConstantDescription("LightBuffer", Unsafe.SizeOf<DirectionalLightBuffer>()),
                new ShaderConstantDescription("WorldMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderConstantDescription("InverseTransposeWorldMatrixBuffer", ShaderConstantType.Matrix4x4),
            };

            ShaderTextureInput[] textureInputs = new[] { new ShaderTextureInput(0, "surfaceTexture") };

            _material = factory.CreateMaterial(
                context,
                VertexShaderSource,
                FragmentShaderSource,
                materialInputs,
                constants,
                textureInputs);

            _worldBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);
            _inverseTransposeWorldBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);

            DeviceTexture2D texture = _texture.CreateDeviceTexture(factory);
            _textureBinding = factory.CreateShaderTextureBinding(texture);

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
            Matrix4x4 worldMat =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateTranslation(Position);
            _worldBuffer.SetData(ref worldMat, 64);
            Matrix4x4 inverseTransposeWorld = Utilities.CalculateInverseTranspose(worldMat);
            _inverseTransposeWorldBuffer.SetData(ref inverseTransposeWorld, 64);

            rc.VertexBuffer = _vb;
            rc.IndexBuffer = _ib;

            _material.Apply(rc);
            rc.SetConstantBuffer(0, SharedDataProviders.ProjectionMatrixBuffer);
            rc.SetConstantBuffer(1, SharedDataProviders.ViewMatrixBuffer);
            rc.SetConstantBuffer(2, SharedDataProviders.DirectionalLightBuffer);
            rc.SetConstantBuffer(3, _worldBuffer);
            rc.SetConstantBuffer(4, _inverseTransposeWorldBuffer);
            rc.SetTexture(0, _textureBinding);
            rc.SetSamplerState(0, rc.PointSampler);

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
            _worldBuffer.Dispose();
            _inverseTransposeWorldBuffer.Dispose();
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
