using System;
using System.Numerics;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class TexturedCubeRenderer : RenderItem
    {
        private static VertexBuffer s_vb;
        private static IndexBuffer s_ib;
        private static Material s_material;

        private readonly DynamicDataProvider<Matrix4x4> _worldProvider;
        private readonly DependantDataProvider<Matrix4x4> _inverseTransposeWorldProvider;
        private readonly ConstantBufferDataProvider[] _perObjectProviders;

        public Vector3 Position { get; internal set; }

        public TexturedCubeRenderer(RenderContext context)
        {
            _worldProvider = new DynamicDataProvider<Matrix4x4>();
            _inverseTransposeWorldProvider = new DependantDataProvider<Matrix4x4>(_worldProvider, CalculateInverseTranspose);
            _perObjectProviders = new ConstantBufferDataProvider[] { _worldProvider, _inverseTransposeWorldProvider };

            ResourceFactory factory = context.ResourceFactory;

            if (s_vb == null)
            {
                s_vb = factory.CreateVertexBuffer(VertexPositionNormalTexture.SizeInBytes * s_cubeVertices.Length, false);
                VertexDescriptor desc = new VertexDescriptor(VertexPositionNormalTexture.SizeInBytes, VertexPositionNormalTexture.ElementCount, 0, IntPtr.Zero);
                s_vb.SetVertexData(s_cubeVertices, desc);

                s_ib = factory.CreateIndexBuffer(sizeof(int) * s_cubeIndices.Length, false);
                s_ib.SetIndices(s_cubeIndices);

                MaterialVertexInput materialInputs = new MaterialVertexInput(
                    VertexPositionNormalTexture.SizeInBytes,
                    new MaterialVertexInputElement[]
                    {
                        new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                        new MaterialVertexInputElement("in_normal", VertexSemanticType.Normal, VertexElementFormat.Float3),
                        new MaterialVertexInputElement("in_texCoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2)
                    });

                MaterialInputs<MaterialGlobalInputElement> globalInputs = new MaterialInputs<MaterialGlobalInputElement>(
                    new MaterialGlobalInputElement[]
                    {
                        new MaterialGlobalInputElement("projectionMatrixUniform", MaterialInputType.Matrix4x4, context.ProjectionMatrixProvider),
                        new MaterialGlobalInputElement("viewMatrixUniform", MaterialInputType.Matrix4x4, context.ViewMatrixProvider),
                        new MaterialGlobalInputElement("LightBuffer", MaterialInputType.Custom, context.DataProviders["LightBuffer"]),
                    });

                MaterialInputs<MaterialPerObjectInputElement> perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(
                    new MaterialPerObjectInputElement[]
                    {
                        new MaterialPerObjectInputElement("worldMatrixUniform", MaterialInputType.Matrix4x4, _worldProvider.DataSizeInBytes),
                        new MaterialPerObjectInputElement("inverseTransposeWorldMatrixUniform", MaterialInputType.Matrix4x4, _inverseTransposeWorldProvider.DataSizeInBytes),
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

        private Matrix4x4 CalculateInverseTranspose(Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Matrix4x4.Invert(m, out inverted);
            return Matrix4x4.Transpose(inverted);
        }

        public void Render(RenderContext context)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            _worldProvider.Data =
                Matrix4x4.CreateScale(1.5f)
                * Matrix4x4.CreateRotationY(rotationAmount)
                * Matrix4x4.CreateTranslation(Position);

            context.SetVertexBuffer(s_vb);
            context.SetIndexBuffer(s_ib);
            context.SetMaterial(s_material);
            s_material.ApplyPerObjectInputs(_perObjectProviders);

            context.DrawIndexedPrimitives(s_cubeIndices.Length, 0);
        }

        public RenderOrderKey GetRenderOrderKey()
        {
            return new RenderOrderKey();
        }

        private static readonly VertexPositionNormalTexture[] s_cubeVertices = new VertexPositionNormalTexture[]
        {
            // Top
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,-.5f),     new Vector3(0,1,0),     new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,-.5f),      new Vector3(0,1,0),     new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,.5f),       new Vector3(0,1,0),     new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,.5f),      new Vector3(0,1,0),     new Vector2(0, 1)),
            // Bottom                                                             
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,.5f),     new Vector3(0,1,0),     new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,.5f),      new Vector3(0,1,0),     new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,-.5f),     new Vector3(0,1,0),     new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,-.5f),    new Vector3(0,1,0),     new Vector2(0, 1)),
            // Left                                                               
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,-.5f),     new Vector3(-1,0,0),    new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,.5f),      new Vector3(-1,0,0),    new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,.5f),     new Vector3(-1,0,0),    new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,-.5f),    new Vector3(-1,0,0),    new Vector2(0, 1)),
            // Right                                                              
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,.5f),       new Vector3(1,0,0),     new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,-.5f),      new Vector3(1,0,0),     new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,-.5f),     new Vector3(1,0,0),     new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,.5f),      new Vector3(1,0,0),     new Vector2(0, 1)),
            // Back                                                               
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,-.5f),      new Vector3(0,0,-1),    new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,-.5f),     new Vector3(0,0,-1),    new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,-.5f),    new Vector3(0,0,-1),    new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,-.5f),     new Vector3(0,0,-1),    new Vector2(0, 1)),
            // Front                                                              
            new VertexPositionNormalTexture(new Vector3(-.5f,.5f,.5f),      new Vector3(0,0,1),     new Vector2(0, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,.5f,.5f),       new Vector3(0,0,1),     new Vector2(1, 0)),
            new VertexPositionNormalTexture(new Vector3(.5f,-.5f,.5f),      new Vector3(0,0,1),     new Vector2(1, 1)),
            new VertexPositionNormalTexture(new Vector3(-.5f,-.5f,.5f),     new Vector3(0,0,1),     new Vector2(0, 1)),
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
        private static readonly string FragmentShaderSource = "lit-frag";
        private static readonly ImageProcessorTexture s_cubeTexture = new ImageProcessorTexture(AppContext.BaseDirectory + "/Textures/CubeTexture.png");
    }
}
