namespace Offscreen
{
    class Program
    {
        static void Main(string[] args)
        {
            new OffscreenApplication().Run();
        }
    }
}

// using System.IO;
// using System.Numerics;
// using Veldrid.Sdl2;
// using Veldrid.StartupUtilities;
// using Veldrid.MetalBindings;
// using System;
// using System.Diagnostics;
// using Veldrid.Utilities;
// using Veldrid.ImageSharp;

// namespace Veldrid.Metal
// {
//     internal class TexturedCube : SampleApplication
//     {
//         private DeviceBuffer _projectionBuffer;
//         private DeviceBuffer _viewBuffer;
//         private DeviceBuffer _worldBuffer;
//         private DeviceBuffer _vertexBuffer;
//         private DeviceBuffer _indexBuffer;
//         private CommandList _cl;
//         private Texture _surfaceTexture;
//         private TextureView _surfaceTextureView;
//         private Pipeline _pipeline;
//         private ResourceSet _projViewSet;
//         private ResourceSet _worldTextureSet;
//         private Stopwatch _sw;

//         public static void Main()
//         {
//             new TexturedCube().Run();
//         }

//         public TexturedCube()
//         {
//             _sw = Stopwatch.StartNew();
//         }

//         protected override void CreateResources(ResourceFactory factory)
//         {
//             _cl = factory.CreateCommandList();
//             _cl.Begin();
//             _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
//             _projectionBuffer.Name = "Projection";
//             _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
//             _viewBuffer.Name = "View";
//             _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
//             _worldBuffer.Name = "World";

//             VertexPositionTexture[] vertices = GetCubeVertices();
//             _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VertexPositionTexture.SizeInBytes * vertices.Length), BufferUsage.VertexBuffer));
//             _cl.UpdateBuffer(_vertexBuffer, 0, vertices);

//             ushort[] indices = GetCubeIndices();
//             _indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(ushort) * (uint)indices.Length, BufferUsage.IndexBuffer));
//             _cl.UpdateBuffer(_indexBuffer, 0, indices);

//             ImageSharpTexture stoneImage = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures", "spnza_bricks_a_diff.png"));
//             _surfaceTexture = stoneImage.CreateDeviceTexture(_gd, factory);
//             _surfaceTextureView = factory.CreateTextureView(_surfaceTexture);

//             _cl.End();
//             _gd.SubmitCommands(_cl);
//             _gd.WaitForIdle();

//             ShaderSetDescription shaderSet = new ShaderSetDescription(
//                 new[]
//                 {
//                     new VertexLayoutDescription(
//                         new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
//                         new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
//                 },
//                 new[]
//                 {
//                     LoadShader(factory, "Cube", ShaderStages.Vertex, "VS"),
//                     LoadShader(factory, "Cube", ShaderStages.Fragment, "FS")
//                 });

//             ResourceLayout projViewLayout = factory.CreateResourceLayout(
//                 new ResourceLayoutDescription(
//                     new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
//                     new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

//             ResourceLayout worldTextureLayout = factory.CreateResourceLayout(
//                 new ResourceLayoutDescription(
//                     new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex),
//                     new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
//                     new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

//             _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
//                 BlendStateDescription.SingleOverrideBlend,
//                 DepthStencilStateDescription.DepthOnlyLessEqual,
//                 RasterizerStateDescription.Default,
//                 PrimitiveTopology.TriangleList,
//                 shaderSet,
//                 new[] { projViewLayout, worldTextureLayout },
//                 _gd.SwapchainFramebuffer.OutputDescription));

//             _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
//                 projViewLayout,
//                 _projectionBuffer,
//                 _viewBuffer));

//             _worldTextureSet = factory.CreateResourceSet(new ResourceSetDescription(
//                 worldTextureLayout,
//                 _worldBuffer,
//                 _surfaceTextureView,
//                 _gd.Aniso4xSampler));
//         }

//         protected override void Draw()
//         {
//             long ticks = _sw.ElapsedMilliseconds;
//             _cl.Begin();

//             _cl.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreatePerspectiveFieldOfView(
//                 1.0f,
//                 (float)_window.Width / _window.Height,
//                 0.5f,
//                 100f));

//             _cl.UpdateBuffer(_viewBuffer, 0, Matrix4x4.CreateLookAt(Vector3.UnitZ * 2.5f, Vector3.Zero, Vector3.UnitY));

//             Matrix4x4 rotation =
//                 Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, (ticks / 1000f))
//                 * Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (ticks / 3000f));
//             _cl.UpdateBuffer(_worldBuffer, 0, ref rotation);

//             _cl.SetFramebuffer(_gd.SwapchainFramebuffer);
//             _cl.SetFullViewports();
//             _cl.ClearColorTarget(0, RgbaFloat.Black);
//             _cl.ClearDepthStencil(1f);
//             _cl.SetPipeline(_pipeline);
//             _cl.SetVertexBuffer(0, _vertexBuffer);
//             _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
//             _cl.SetGraphicsResourceSet(0, _projViewSet);
//             _cl.SetGraphicsResourceSet(1, _worldTextureSet);
//             _cl.DrawIndexed(36, 1, 0, 0, 0);

//             _cl.End();
//             _gd.SubmitCommands(_cl);
//             _gd.SwapBuffers();
//         }

//         private static VertexPositionTexture[] GetCubeVertices()
//         {
//             VertexPositionTexture[] vertices = new VertexPositionTexture[]
//             {
//                 // Top
//                 new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
//                 new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
//                 new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1)),
//                 new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1)),
//                 // Bottom                                                             
//                 new VertexPositionTexture(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0)),
//                 new VertexPositionTexture(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0)),
//                 new VertexPositionTexture(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1)),
//                 new VertexPositionTexture(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1)),
//                 // Left                                                               
//                 new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
//                 new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
//                 new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
//                 new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
//                 // Right                                                              
//                 new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
//                 new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
//                 new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
//                 new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
//                 // Back                                                               
//                 new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
//                 new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
//                 new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
//                 new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
//                 // Front                                                              
//                 new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
//                 new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
//                 new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
//                 new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
//             };

//             return vertices;
//         }

//         private static ushort[] GetCubeIndices()
//         {
//             ushort[] indices =
//             {
//                 0,1,2, 0,2,3,
//                 4,5,6, 4,6,7,
//                 8,9,10, 8,10,11,
//                 12,13,14, 12,14,15,
//                 16,17,18, 16,18,19,
//                 20,21,22, 20,22,23,
//             };

//             return indices;
//         }
//     }
// }