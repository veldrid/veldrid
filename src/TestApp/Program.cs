using OpenTK.Graphics;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vd2;

namespace TestApp
{
    class Program
    {
        //private static UniformBuffer _viewUB;
        //private static UniformBuffer _projUB;
        private static OpenTK.NativeWindow _nw;
        private static GraphicsDevice _gd;

        static unsafe void Main(string[] args)
        {
            _nw = new OpenTK.NativeWindow(
                960,
                540,
                "TestApp",
                OpenTK.GameWindowFlags.Default,
                GraphicsMode.Default,
                OpenTK.DisplayDevice.Default);
            _nw.Visible = true;

            _gd = Hacks.CreateD3D11(_nw.WindowInfo.Handle, _nw.Width, _nw.Height);

            ResourceFactory rf = _gd.ResourceFactory;

            //_viewUB = rf.CreateUniformBuffer(new BufferDescription(64));
            //_projUB = rf.CreateUniformBuffer(new BufferDescription(64));

            CommandBuffer sharedInit = rf.CreateCommandBuffer(new CommandBufferDescription());
            VertexBuffer vb = rf.CreateVertexBuffer(new BufferDescription { SizeInBytes = 1024 });
            IndexBuffer ib = rf.CreateIndexBuffer(new IndexBufferDescription { Format = IndexFormat.UInt16, SizeInBytes = 1024 });
            (var vertices, var indices) = PrimitiveShapes.Box(2, 2, 2, 2);
            Texture2D surfaceTexture = rf.CreateTexture2D(new TextureDescription(1, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled));
            RgbaFloat pink = RgbaFloat.Pink;

            sharedInit.Begin();
            sharedInit.UpdateBuffer(vb, ref vertices[0], Unsafe.SizeOf<VertexPositionNormalTexture>() * vertices.Length, 0);
            sharedInit.UpdateBuffer(ib, ref indices[0], 2 * indices.Length, 0);
            sharedInit.UpdateTexture2D(surfaceTexture, (IntPtr)(&pink), 16, 0, 0, 1, 1, 0, 0);
            sharedInit.End();
            _gd.ExecuteCommands(sharedInit);

            Pipeline p = CreateBoxPipeline(rf);

            TextureView texView = rf.CreateTextureView(new TextureViewDescription(surfaceTexture));

            Sampler sampler = rf.CreateSampler(SamplerDescription.Aniso4x);

            ResourceLayout resourceLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                //new ResourceLayoutElementDescription("World", ResourceKind.Uniform, ShaderStages.Vertex),
                //new ResourceLayoutElementDescription("View", ResourceKind.Uniform, ShaderStages.Vertex),
                //new ResourceLayoutElementDescription("Projection", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("MVP", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.Texture2D, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            const int numItems = 1;
            CommandBuffer[] initCBs = new CommandBuffer[numItems];
            CommandBuffer[] renderCBs = new CommandBuffer[numItems];
            //Parallel.For(0, numItems, i =>
            //{
            int i = 0;
            UniformBuffer worldUB = rf.CreateUniformBuffer(new BufferDescription(64));
            Matrix4x4 world = Matrix4x4.Identity;
            Matrix4x4 view = Matrix4x4.CreateLookAt(Vector3.UnitZ * 5f + Vector3.UnitX * 3 + Vector3.UnitY * 2, Vector3.Zero, Vector3.UnitY);
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(1f, (float)_nw.Width / _nw.Height, .1f, 1000f);
            Matrix4x4 total = world * view * proj;

            CommandBuffer initCB = rf.CreateCommandBuffer(new CommandBufferDescription());
            initCB.Begin();
            initCB.UpdateBuffer(worldUB, ref total, 64, 0);
            initCB.End();
            initCBs[i] = initCB;

            ResourceSet set = rf.CreateResourceSet(new ResourceSetDescription(resourceLayout, worldUB, texView, sampler));

            CommandBuffer renderCB = rf.CreateCommandBuffer(new CommandBufferDescription());
            renderCB.Begin();
            renderCB.SetFramebuffer(_gd.SwapchainFramebuffer);
            renderCB.SetViewport(0, new Viewport(0, 0, _nw.Width, _nw.Height, 0, 1));
            renderCB.SetScissorRect(0, 0, 0, (uint)_nw.Width, (uint)_nw.Height);

            renderCB.BindVertexBuffer(0, vb, (uint)Unsafe.SizeOf<VertexPositionNormalTexture>());
            renderCB.BindIndexBuffer(ib);
            renderCB.BindPipeline(p);
            renderCB.BindResourceSet(set);
            renderCB.Draw((uint)indices.Length, 1, 0, 0, 0);
            renderCB.End();
            //renderCBs[i] = renderCB;
            //});

            //foreach (CommandBuffer initCB in initCBs)
            //{
            _gd.ExecuteCommands(initCB);
            //}

            CommandBuffer mainCB = rf.CreateCommandBuffer(new CommandBufferDescription());
            mainCB.Begin();
            mainCB.SetFramebuffer(_gd.SwapchainFramebuffer);
            renderCB.SetViewport(0, new Viewport(0, 0, _nw.Width, _nw.Height, 0, 1));
            renderCB.SetScissorRect(0, 0, 0, (uint)_nw.Width, (uint)_nw.Height);
            mainCB.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            mainCB.ClearDepthTarget(1f);
            ////foreach (CommandBuffer renderCB in renderCBs)
            //{
            mainCB.ExecuteCommands(renderCB);
            //}
            mainCB.End();

            CommandBuffer perFrameCB = rf.CreateCommandBuffer(new CommandBufferDescription());

            while (_nw.Exists)
            {
                _nw.ProcessEvents();

                //UpdateViewProjection(perFrameCB);
                _gd.ExecuteCommands(mainCB);
                //_gd.ExecuteCommands(renderCB);
                _gd.SwapBuffers();
            }
        }

        //private static void UpdateViewProjection(CommandBuffer cb)
        //{
        //    Matrix4x4 view = Matrix4x4.CreateLookAt(Vector3.UnitZ * 5f + Vector3.UnitX * 3 + Vector3.UnitY * 2, Vector3.Zero, Vector3.UnitY);
        //    Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(1f, (float)_nw.Width / _nw.Height, .1f, 1000f);
        //    cb.Begin();
        //    cb.UpdateBuffer(_viewUB, ref view, 64, 0);
        //    cb.UpdateBuffer(_projUB, ref proj, 64, 0);
        //    cb.End();
        //    _gd.ExecuteCommands(cb);
        //}

        private static Pipeline CreateBoxPipeline(ResourceFactory rf)
        {
            PipelineDescription pd = new PipelineDescription();
            pd.BlendState = BlendStateDescription.SingleAdditiveBlend;
            pd.DepthStencilState = DepthStencilStateDescription.LessEqual;
            pd.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pd.RasterizerState = RasterizerStateDescription.Default;
            pd.ShaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                     new VertexLayoutDescription(
                        new VertexElementDescription("in_position", VertexElementFormat.Float3, VertexElementSemantic.Position),
                        new VertexElementDescription("in_normal", VertexElementFormat.Float3, VertexElementSemantic.Normal),
                        new VertexElementDescription("in_texCoords", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate))
                },
                new ShaderStageDescription[]
                {
                    new ShaderStageDescription(ShaderStages.Vertex, LoadShader(rf, ShaderStages.Vertex, "Box-vertex"), "VS"),
                    new ShaderStageDescription(ShaderStages.Fragment, LoadShader(rf, ShaderStages.Fragment, "Box-fragment"), "FS")
                });

            return rf.CreatePipeline(ref pd);
        }

        private static Shader LoadShader(ResourceFactory rf, ShaderStages stage, string name)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Shaders", name + ".hlsl.bytes");
            return rf.CreateShader(new ShaderDescription { ShaderBytes = File.ReadAllBytes(path), Stage = stage });
        }
    }
}
