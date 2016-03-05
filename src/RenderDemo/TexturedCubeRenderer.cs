using System;
using System.Numerics;
using Veldrid.Graphics;

namespace RenderDemo
{
    public class TexturedCubeRenderer
    {
        private static VertexBuffer s_vb;
        private static IndexBuffer s_ib;
        private static Material s_material;

        private Vector3 _position = Vector3.Zero;

        private DynamicDataProvider<Matrix4x4> _worldProvider = new DynamicDataProvider<Matrix4x4>();

        public TexturedCubeRenderer(RenderContext context)
        {
            ResourceFactory factory = context.ResourceFactory;

            if (s_vb == null)
            {
                s_vb = factory.CreateVertexBuffer(VertexPositionTexture.SizeInBytes * s_cubeVertices.Length);
                VertexDescriptor desc = new VertexDescriptor(VertexPositionTexture.SizeInBytes, VertexPositionTexture.ElementCount, 0, IntPtr.Zero);
                s_vb.SetVertexData(s_cubeVertices, desc);

                s_ib = factory.CreateIndexBuffer(sizeof(int) * s_cubeIndices.Length);
                s_ib.SetIndices(s_cubeIndices, 0, IntPtr.Zero);

                MaterialVertexInput materialInputs = new MaterialVertexInput(
                    VertexPositionTexture.SizeInBytes,
                    new MaterialVertexInputElement[]
                    {
                    new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                    new MaterialVertexInputElement("in_texCoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2)
                    });

                MaterialInputs<MaterialGlobalInputElement> globalInputs = new MaterialInputs<MaterialGlobalInputElement>(
                    new MaterialGlobalInputElement[]
                    {
                    new MaterialGlobalInputElement("projectionMatrixUniform", MaterialInputType.Matrix4x4, context.ProjectionMatrixProvider),
                    new MaterialGlobalInputElement("viewMatrixUniform", MaterialInputType.Matrix4x4, context.ViewMatrixProvider),
                    });

                MaterialInputs<MaterialPerObjectInputElement> perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(
                    new MaterialPerObjectInputElement[]
                    {
                    new MaterialPerObjectInputElement("worldMatrixUniform", MaterialInputType.Matrix4x4, _worldProvider.DataSizeInBytes),
                    });

                MaterialTextureInputs textureInputs = new MaterialTextureInputs(
                    new MaterialTextureInputElement[] {
                    new MaterialTextureInputElement("surfaceTexture", s_cubeTexture)
                    });

                s_material = factory.CreateMaterial(
                    VertexShaderSource,
                    FragmentShaderSource,
                    materialInputs,
                    globalInputs,
                    perObjectInputs,
                    textureInputs);
            }
        }

        public void Render(RenderContext context)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            _worldProvider.Data =
                Matrix4x4.CreateScale(2.5f)
                * Matrix4x4.CreateRotationY(rotationAmount)
                * Matrix4x4.CreateTranslation(_position);

            context.SetVertexBuffer(s_vb);
            context.SetIndexBuffer(s_ib);
            context.SetMaterial(s_material);
            s_material.ApplyPerObjectInput(_worldProvider);

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
        private static readonly ImageProcessorTexture s_cubeTexture = new ImageProcessorTexture(AppContext.BaseDirectory + "/Textures/CubeTexture.png");
    }
}
