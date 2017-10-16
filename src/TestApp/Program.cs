using OpenTK.Graphics;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vd2;

namespace TestApp
{
    class Program
    {
        private static UniformBuffer _viewUB;
        private static UniformBuffer _projUB;
        private static OpenTK.NativeWindow _nw;
        private static GraphicsDevice _gd;
        private static Camera _camera;

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
            InputTracker.Init(_nw);
            _camera = new Camera(_nw.Width, _nw.Height);

            _gd = Hacks.CreateD3D11(_nw.WindowInfo.Handle, _nw.Width, _nw.Height);

            ResourceFactory rf = _gd.ResourceFactory;

            _viewUB = rf.CreateUniformBuffer(new BufferDescription(64));
            _projUB = rf.CreateUniformBuffer(new BufferDescription(64));

            CommandBuffer sharedInit = rf.CreateCommandBuffer(new CommandBufferDescription());
            VertexBuffer vb = rf.CreateVertexBuffer(new BufferDescription { SizeInBytes = 1024 });
            IndexBuffer ib = rf.CreateIndexBuffer(new IndexBufferDescription { Format = IndexFormat.UInt16, SizeInBytes = 1024 });
            (var vertices, var indices) = PrimitiveShapes.Box(2, 2, 2, 2);
            Texture2D surfaceTexture = rf.CreateTexture2D(new TextureDescription(256, 256, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled));
            RgbaFloat[] pinks = Enumerable.Repeat(RgbaFloat.Pink, 256 * 256).ToArray();

            sharedInit.Begin();
            sharedInit.UpdateBuffer(vb, ref vertices[0], Unsafe.SizeOf<VertexPositionNormalTexture>() * vertices.Length, 0);
            sharedInit.UpdateBuffer(ib, ref indices[0], 2 * indices.Length, 0);

            fixed (RgbaFloat* pinkPtr = pinks)
            {
                sharedInit.UpdateTexture2D(surfaceTexture, (IntPtr)(pinkPtr), 256 * 256 * 16, 0, 0, 256, 256, 0, 0);
            }
            sharedInit.End();
            _gd.ExecuteCommands(sharedInit);

            Pipeline p = CreateBoxPipeline(rf);

            TextureView texView = rf.CreateTextureView(new TextureViewDescription(surfaceTexture));

            Sampler sampler = rf.CreateSampler(SamplerDescription.Point);

            ResourceLayout resourceLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("World", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("View", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Projection", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.Texture2D, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            UniformBuffer worldUB = rf.CreateUniformBuffer(new BufferDescription(64));
            Matrix4x4 world = Matrix4x4.Identity;
            CommandBuffer initCB = rf.CreateCommandBuffer(new CommandBufferDescription());
            initCB.Begin();
            initCB.UpdateBuffer(worldUB, ref world, 64, 0);
            initCB.End();
            _gd.ExecuteCommands(initCB);
            initCB.Dispose();

            ResourceSet set = rf.CreateResourceSet(new ResourceSetDescription(resourceLayout, worldUB, _viewUB, _projUB, texView, sampler));

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

            const int multiBoxDimensions = 30;
            const int numBoxes = multiBoxDimensions * multiBoxDimensions;
            CommandBuffer[] multiBoxInits = new CommandBuffer[numBoxes];
            CommandBuffer[] multiBoxRenderCBs = new CommandBuffer[numBoxes];

            Parallel.For(0, multiBoxDimensions, x =>
            {
                Parallel.For(0, multiBoxDimensions, y =>
                {
                    int index = y * multiBoxDimensions + x;
                    Matrix4x4 mWorld = Matrix4x4.CreateScale(0.2f) * Matrix4x4.CreateTranslation(x - (multiBoxDimensions / 2), y - (multiBoxDimensions / 2), -25);

                    CommandBuffer mInit = rf.CreateCommandBuffer(new CommandBufferDescription());
                    UniformBuffer mWorldUB = rf.CreateUniformBuffer(new BufferDescription(64));
                    mInit.Begin();
                    mInit.UpdateBuffer(mWorldUB, ref mWorld, 64, 0);
                    mInit.End();
                    multiBoxInits[index] = mInit;

                    ResourceSet mSet = rf.CreateResourceSet(new ResourceSetDescription(resourceLayout, mWorldUB, _viewUB, _projUB, texView, sampler));

                    CommandBuffer mRender = rf.CreateCommandBuffer(new CommandBufferDescription());
                    mRender.Begin();
                    mRender.SetFramebuffer(_gd.SwapchainFramebuffer);
                    mRender.SetViewport(0, new Viewport(0, 0, _nw.Width, _nw.Height, 0, 1));
                    mRender.SetScissorRect(0, 0, 0, (uint)_nw.Width, (uint)_nw.Height);
                    mRender.BindVertexBuffer(0, vb, (uint)Unsafe.SizeOf<VertexPositionNormalTexture>());
                    mRender.BindIndexBuffer(ib);
                    mRender.BindPipeline(p);
                    mRender.BindResourceSet(mSet);
                    mRender.Draw((uint)indices.Length, 1, 0, 0, 0);
                    mRender.End();
                    multiBoxRenderCBs[index] = mRender;
                });
            });

            foreach (CommandBuffer mInitCB in multiBoxInits)
            {
                _gd.ExecuteCommands(mInitCB);
            }

            CommandBuffer mainCB = rf.CreateCommandBuffer(new CommandBufferDescription());
            mainCB.Begin();
            mainCB.SetFramebuffer(_gd.SwapchainFramebuffer);
            renderCB.SetViewport(0, new Viewport(0, 0, _nw.Width, _nw.Height, 0, 1));
            renderCB.SetScissorRect(0, 0, 0, (uint)_nw.Width, (uint)_nw.Height);
            mainCB.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            mainCB.ClearDepthTarget(1f);
            mainCB.ExecuteCommands(renderCB);
            foreach (CommandBuffer mRenderCB in multiBoxRenderCBs)
            {
                mainCB.ExecuteCommands(mRenderCB);
            }
            mainCB.End();

            CommandBuffer perFrameCB = rf.CreateCommandBuffer(new CommandBufferDescription());

            while (_nw.Exists)
            {
                _nw.ProcessEvents();

                UpdateViewProjection(perFrameCB);
                _gd.ExecuteCommands(mainCB);
                _gd.SwapBuffers();

                InputTracker.UpdateFrameInput();
            }

            _gd.Dispose();
        }

        private static void UpdateViewProjection(CommandBuffer cb)
        {
            _camera.Update(1f / 60f);
            _camera.WindowResized(_nw.Width, _nw.Height);
            Matrix4x4 view = _camera.ViewMatrix;
            Matrix4x4 proj = _camera.ProjectionMatrix;

            cb.Begin();
            cb.UpdateBuffer(_viewUB, ref view, 64, 0);
            cb.UpdateBuffer(_projUB, ref proj, 64, 0);
            cb.End();
            _gd.ExecuteCommands(cb);
        }

        private static Pipeline CreateBoxPipeline(ResourceFactory rf)
        {
            PipelineDescription pd = new PipelineDescription();
            pd.BlendState = BlendStateDescription.SingleOverrideBlend;
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
