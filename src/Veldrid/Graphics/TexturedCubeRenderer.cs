using System;
using System.Numerics;

namespace Veldrid.Graphics
{
    public class TexturedCubeRenderer
    {
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;

        private Vector3 _position = Vector3.Zero;

        private DynamicDataProvider<Matrix4x4> _worldProvider = new DynamicDataProvider<Matrix4x4>();

        public TexturedCubeRenderer(RenderContext context)
        {
            ResourceFactory factory = context.ResourceFactory;

            _vb = factory.CreateVertexBuffer();
            VertexDescriptor desc = new VertexDescriptor(VertexPositionTexture.SizeInBytes, VertexPositionTexture.ElementCount, 0, IntPtr.Zero);
            _vb.SetVertexData(s_cubeVertices, desc);

            _ib = factory.CreateIndexBuffer();
            _ib.SetIndices(s_cubeIndices, 0, IntPtr.Zero);

            MaterialVertexInput materialInputs = new MaterialVertexInput(
                VertexPositionTexture.SizeInBytes,
                new MaterialVertexInputElement[]
                {
                    new MaterialVertexInputElement("in_position", VertexElementFormat.Float3),
                    new MaterialVertexInputElement("in_texCoord", VertexElementFormat.Float2)
                });

            MaterialGlobalInputs globalInputs = new MaterialGlobalInputs(
                new MaterialGlobalInputElement[]
                {
                    new MaterialGlobalInputElement("projectionMatrixUniform", MaterialGlobalInputType.Matrix4x4, context.ProjectionMatrixProvider),
                    new MaterialGlobalInputElement("viewMatrixUniform", MaterialGlobalInputType.Matrix4x4, context.ViewMatrixProvider),
                    new MaterialGlobalInputElement("worldMatrixUniform", MaterialGlobalInputType.Matrix4x4, _worldProvider),
                });

            MaterialTextureInputs textureInputs = new MaterialTextureInputs(
                new MaterialTextureInputElement[] {
                    new MaterialTextureInputElement("surfaceTexture", s_cubeTexture)
                });

            _material = factory.CreateMaterial(VertexShaderSource, FragmentShaderSource, materialInputs, globalInputs, textureInputs);
        }

        public unsafe void Render(RenderContext context)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            _worldProvider.Data =
                Matrix4x4.CreateScale(2.5f)
                * Matrix4x4.CreateRotationY(rotationAmount)
                * Matrix4x4.CreateTranslation(_position);

            context.SetVertexBuffer(_vb);
            context.SetIndexBuffer(_ib);
            context.SetMaterial(_material);

            context.DrawIndexedPrimitives(0, s_cubeIndices.Length);
        }

        private static readonly VertexPositionTexture[] s_cubeVertices = new VertexPositionTexture[]
        {
            // Top
            new VertexPositionTexture(new Vector3(-.5f,.5f,-.5f),    new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(.5f,.5f,-.5f),     new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(.5f,.5f,.5f),      new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-.5f,.5f,.5f),     new Vector2(0, 1)),
            // Bottom
            new VertexPositionTexture(new Vector3(-.5f,-.5f,.5f),    new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(.5f,-.5f,.5f),     new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(.5f,-.5f,-.5f),    new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-.5f,-.5f,-.5f),   new Vector2(0, 1)),
            // Left
            new VertexPositionTexture(new Vector3(-.5f,.5f,-.5f),    new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-.5f,.5f,.5f),     new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-.5f,-.5f,.5f),    new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-.5f,-.5f,-.5f),   new Vector2(0, 1)),
            // Right
            new VertexPositionTexture(new Vector3(.5f,.5f,.5f),      new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(.5f,.5f,-.5f),     new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(.5f,-.5f,-.5f),    new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(.5f,-.5f,.5f),     new Vector2(0, 1)),
            // Back
            new VertexPositionTexture(new Vector3(.5f,.5f,-.5f),     new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-.5f,.5f,-.5f),    new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-.5f,-.5f,-.5f),   new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(.5f,-.5f,-.5f),    new Vector2(0, 1)),
            // Front
            new VertexPositionTexture(new Vector3(-.5f,.5f,.5f),     new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(.5f,.5f,.5f),      new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(.5f,-.5f,.5f),     new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-.5f,-.5f,.5f),    new Vector2(0, 1)),
        };

        private static readonly int[] s_cubeIndices = new int[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        };

        private static readonly string VertexShaderSource = "textured-vertex";
        private static readonly string FragmentShaderSource = "textured-frag";
        private static readonly ImageProcessorTexture s_cubeTexture = new ImageProcessorTexture("Textures/CubeTexture.png");
    }
}
