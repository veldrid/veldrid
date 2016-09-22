using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class ColoredCubeRenderer : SwappableRenderItem, IDisposable
    {
        private readonly DynamicDataProvider<Matrix4x4> _modelViewProvider = new DynamicDataProvider<Matrix4x4>();

        private static VertexBuffer s_vb;
        private static IndexBuffer s_ib;
        private static Material s_material;

        private static RenderContext s_currentContext;

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One * .75f;

        public ColoredCubeRenderer(AssetDatabase ad, RenderContext context)
        {
            if (context != s_currentContext)
            {
                if (s_currentContext == null)
                {
                    InitializeContextObjects(context);
                }
                else
                {
                    ChangeRenderContext(ad, context);
                }
            }
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext context)
        {
            if (s_currentContext != context)
            {
                Dispose();
                InitializeContextObjects(context);
            }
        }

        private void InitializeContextObjects(RenderContext context)
        {
            s_currentContext = context;
            ResourceFactory factory = context.ResourceFactory;

            s_vb = factory.CreateVertexBuffer(VertexPositionColor.SizeInBytes * s_cubeVertices.Length, false);
            VertexDescriptor desc = new VertexDescriptor(VertexPositionColor.SizeInBytes, VertexPositionColor.ElementCount, 0, IntPtr.Zero);
            s_vb.SetVertexData(s_cubeVertices, desc);

            s_ib = factory.CreateIndexBuffer(sizeof(int) * s_cubeIndices.Length, false);
            s_ib.SetIndices(s_cubeIndices);

            MaterialVertexInput materialInputs = new MaterialVertexInput(
                VertexPositionColor.SizeInBytes,
                new MaterialVertexInputElement[]
                {
                    new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                    new MaterialVertexInputElement("in_color", VertexSemanticType.Color, VertexElementFormat.Float4)
                });

            MaterialInputs<MaterialGlobalInputElement> globalInputs = new MaterialInputs<MaterialGlobalInputElement>(
                new MaterialGlobalInputElement[]
                {
                    new MaterialGlobalInputElement("ProjectionMatrixBuffer", MaterialInputType.Matrix4x4, "ProjectionMatrix")
                });

            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(
                new MaterialPerObjectInputElement[]
                {
                    new MaterialPerObjectInputElement("ModelViewMatrixBuffer", MaterialInputType.Matrix4x4, _modelViewProvider.DataSizeInBytes)
                });

            s_material = factory.CreateMaterial(
                context,
                VertexShaderSource,
                FragmentShaderSource,
                materialInputs,
                globalInputs,
                perObjectInputs,
                MaterialTextureInputs.Empty);
        }

        public IEnumerable<string> GetStagesParticipated()
        {
            yield return "Standard";
        }

        public void Render(RenderContext context, string pipelineStage)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            _modelViewProvider.Data =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateRotationX((rotationAmount * .5f) * Position.X)
                * Matrix4x4.CreateRotationY(rotationAmount)
                * Matrix4x4.CreateRotationZ((rotationAmount * .33f) * Position.Z)
                * Matrix4x4.CreateTranslation(Position)
                * Matrix4x4.CreateTranslation((float)Math.Sin(rotationAmount) * Vector3.UnitY)
                * ((ConstantBufferDataProvider<Matrix4x4>)((ChangeableProvider)context.GetNamedGlobalBufferProviderPair("ViewMatrix").DataProvider).DataProvider).Data;

            context.SetVertexBuffer(s_vb);
            context.SetIndexBuffer(s_ib);
            context.SetMaterial(s_material);
            s_material.ApplyPerObjectInput(_modelViewProvider);

            context.DrawIndexedPrimitives(s_cubeIndices.Length, 0);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return new RenderOrderKey();
        }

        public void Dispose()
        {
            s_vb.Dispose();
            s_ib.Dispose();
            s_material.Dispose();
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            float radius = 3f * (Scale.X * Scale.X) / 2f;
            var boundingSphere = new BoundingSphere(Position, radius);
            return visibleFrustum.Contains(boundingSphere) == ContainmentType.Disjoint;
        }

        private static readonly VertexPositionColor[] s_cubeVertices = new VertexPositionColor[]
        {
            // Top
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Red),
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.Red),
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.Red),
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Red),
            // Bottom
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Grey),
            // Left
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Blue),
            // Right
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.White),
            // Back
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.Yellow),
            // Front
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Green),
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.Green),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.Green),
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Green)
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

        private static readonly string VertexShaderSource = "simple-vertex";
        private static readonly string FragmentShaderSource = "simple-frag";
    }
}
