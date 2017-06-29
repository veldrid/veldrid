using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class TeapotRenderer : SwappableRenderItem, IDisposable
    {
        private readonly BoundingSphere _centeredBounds;

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Material _material;
        private ConstantBuffer _worldBuffer;
        private ConstantBuffer _inverseTransposeWorldBuffer;
        private ShaderTextureBinding _textureBinding;

        private static ConstructedMeshInfo _teapotMesh;

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        private static ConstructedMeshInfo LoadTeapotMesh()
        {
            if (_teapotMesh == null)
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Models", "Teapot.obj");
                using (var fs = File.OpenRead(path))
                {
                    _teapotMesh = new ObjParser().Parse(fs).GetFirstMesh();
                }
            }

            return _teapotMesh;
        }

        public TeapotRenderer(RenderContext rc)
        {
            _centeredBounds = BoundingSphere.CreateFromPoints(LoadTeapotMesh().Vertices);

            InitializeContextObjects(rc);
        }

        private void InitializeContextObjects(RenderContext rc)
        {
            var factory = rc.ResourceFactory;
            var mesh = LoadTeapotMesh();

            _vertexBuffer = factory.CreateVertexBuffer(mesh.Vertices.Length * VertexPositionNormalTexture.SizeInBytes, false);
            _vertexBuffer.SetVertexData(mesh.Vertices, new VertexDescriptor(VertexPositionNormalTexture.SizeInBytes, 3, 0, IntPtr.Zero));

            _indexBuffer = factory.CreateIndexBuffer(mesh.Indices.Length * sizeof(int), false);
            _indexBuffer.SetIndices(mesh.Indices, IndexFormat.UInt16);

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
                new ShaderConstantDescription("LightBuffer", ShaderConstantType.Custom, Unsafe.SizeOf<DirectionalLightBuffer>()),
                new ShaderConstantDescription("WorldMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderConstantDescription("InverseTransposeWorldMatrixBuffer", ShaderConstantType.Matrix4x4),
            };

            ShaderTextureInput[] textureInputs = new[] { new ShaderTextureInput(0, "surfaceTexture") };

            _material = factory.CreateMaterial(
                rc,
                "textured-vertex",
                "lit-frag",
                materialInputs,
                constants,
                textureInputs);

            _worldBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);
            _inverseTransposeWorldBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);

            DeviceTexture2D deviceTex = s_cubeTexture.CreateDeviceTexture(factory);
            _textureBinding = factory.CreateShaderTextureBinding(deviceTex);
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            Dispose();
            InitializeContextObjects(rc);
        }

        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _material.Dispose();
            _worldBuffer.Dispose();
            _inverseTransposeWorldBuffer.Dispose();
        }

        private Matrix4x4 CalculateInverseTranspose(Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Matrix4x4.Invert(m, out inverted);
            return Matrix4x4.Transpose(inverted);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return new RenderOrderKey();
        }

        public IList<string> GetStagesParticipated() => CommonStages.Standard;

        public void Render(RenderContext rc, string pipelineStage)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            Matrix4x4 worldData =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateFromQuaternion(Rotation)
                * Matrix4x4.CreateTranslation(Position);
            _worldBuffer.SetData(ref worldData, 64);

            var inverseTransposeData = Utilities.CalculateInverseTranspose(worldData);
            _inverseTransposeWorldBuffer.SetData(ref inverseTransposeData, 64);

            rc.VertexBuffer = _vertexBuffer;
            rc.IndexBuffer = _indexBuffer;
            _material.Apply(rc);
            rc.SetConstantBuffer(0, SharedDataProviders.ProjectionMatrixBuffer);
            rc.SetConstantBuffer(1, SharedDataProviders.ViewMatrixBuffer);
            rc.SetConstantBuffer(2, SharedDataProviders.DirectionalLightBuffer);
            rc.SetConstantBuffer(3, _worldBuffer);
            rc.SetConstantBuffer(4, _inverseTransposeWorldBuffer);
            rc.SetTexture(0, _textureBinding);

            rc.DrawIndexedPrimitives(_teapotMesh.Indices.Length, 0);
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            BoundingSphere sphere = new BoundingSphere(_centeredBounds.Center + Position, _centeredBounds.Radius * Scale.X);
            return visibleFrustum.Contains(sphere) == ContainmentType.Disjoint;
        }

        private static readonly ImageSharpTexture s_cubeTexture = new ImageSharpTexture(AppContext.BaseDirectory + "/Assets/Textures/CubeTexture.png");
    }
}
