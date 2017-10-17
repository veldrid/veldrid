using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Vd2;
using VdSdl2;

namespace TestApp
{
    class Program
    {
        private static UniformBuffer _viewUB;
        private static UniformBuffer _projUB;
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static Camera _camera;
        private static double _desiredFrameLengthSeconds = 1.0 / 60.0;
        private static bool _limitFrameRate = false;
        private static FrameTimeAverager _fta = new FrameTimeAverager(0.666);

        static unsafe void Main(string[] args)
        {
            _window = new Sdl2Window("TestApp", 50, 50, 960, 540, SDL_WindowFlags.Shown | SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable, false);
            _window.Visible = true;
            InputTracker.Init(_window);
            _camera = new Camera(_window.Width, _window.Height);

            _gd = Hacks.CreateD3D11(_window.Handle, _window.Width, _window.Height);

            ResourceFactory rf = _gd.ResourceFactory;

            _viewUB = rf.CreateUniformBuffer(new BufferDescription(64));
            _projUB = rf.CreateUniformBuffer(new BufferDescription(64));

            CommandList sharedInit = rf.CreateCommandList(new CommandListDescription());
            VertexBuffer vb = rf.CreateVertexBuffer(new BufferDescription { SizeInBytes = 1024 });
            IndexBuffer ib = rf.CreateIndexBuffer(new IndexBufferDescription { Format = IndexFormat.UInt16, SizeInBytes = 1024 });
            (var vertices, var indices) = PrimitiveShapes.Box(2, 2, 2, 2);
            Texture2D surfaceTexture = rf.CreateTexture2D(new TextureDescription(256, 256, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled));
            RgbaFloat[] pinks = Enumerable.Repeat(RgbaFloat.Pink, 256 * 256).ToArray();

            sharedInit.Begin();
            sharedInit.UpdateBuffer(vb, 0, ref vertices[0], (uint)(Unsafe.SizeOf<VertexPositionNormalTexture>() * vertices.Length));
            sharedInit.UpdateBuffer(ib, 0, ref indices[0], (uint)(2 * indices.Length));

            fixed (RgbaFloat* pinkPtr = pinks)
            {
                sharedInit.UpdateTexture2D(surfaceTexture, (IntPtr)(pinkPtr), 256 * 256 * 16, 0, 0, 256, 256, 0, 0);
            }
            sharedInit.End();
            _gd.ExecuteCommands(sharedInit);

            Pipeline p = CreateBoxPipeline(rf);

            TextureView texView = rf.CreateTextureView(new TextureViewDescription(surfaceTexture));

            ResourceLayout resourceLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("World", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("View", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Projection", ResourceKind.Uniform, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.Texture2D, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            UniformBuffer worldUB = rf.CreateUniformBuffer(new BufferDescription(64));
            Matrix4x4 world = Matrix4x4.Identity;
            CommandList initCB = rf.CreateCommandList(new CommandListDescription());
            initCB.Begin();
            initCB.UpdateBuffer(worldUB, 0, ref world, 64);
            initCB.End();
            _gd.ExecuteCommands(initCB);
            initCB.Dispose();

            ResourceSet set = rf.CreateResourceSet(new ResourceSetDescription(resourceLayout, worldUB, _viewUB, _projUB, texView, _gd.PointSampler));

            CommandList renderCB = rf.CreateCommandList(new CommandListDescription());
            renderCB.Begin();
            renderCB.SetFramebuffer(_gd.SwapchainFramebuffer);
            renderCB.SetViewport(0, new Viewport(0, 0, _window.Width, _window.Height, 0, 1));
            renderCB.SetScissorRect(0, 0, 0, (uint)_window.Width, (uint)_window.Height);

            renderCB.SetVertexBuffer(0, vb, (uint)Unsafe.SizeOf<VertexPositionNormalTexture>());
            renderCB.SetIndexBuffer(ib);
            renderCB.SetPipeline(p);
            renderCB.SetResourceSet(set);
            renderCB.Draw((uint)indices.Length, 1, 0, 0, 0);
            renderCB.End();

            const int multiBoxDimensions = 30;
            const int numBoxes = multiBoxDimensions * multiBoxDimensions;
            CommandList[] multiBoxInits = new CommandList[numBoxes];
            CommandList[] multiBoxRenderCBs = new CommandList[numBoxes];

            Parallel.For(0, multiBoxDimensions, x =>
            {
                Parallel.For(0, multiBoxDimensions, y =>
                {
                    int index = y * multiBoxDimensions + x;
                    Matrix4x4 mWorld = 
                        Matrix4x4.CreateScale(0.2f) 
                        * Matrix4x4.CreateRotationX(x / (multiBoxDimensions / 2)) * Matrix4x4.CreateRotationY(y / (multiBoxDimensions / 2))
                        * Matrix4x4.CreateTranslation(x - (multiBoxDimensions / 2), y - (multiBoxDimensions / 2), -25);
                    CommandList mInit = GetInitCommandList(rf);
                    UniformBuffer mWorldUB = rf.CreateUniformBuffer(new BufferDescription(64));
                    mInit.Begin();
                    mInit.UpdateBuffer(mWorldUB, 0, ref mWorld, 64);
                    mInit.End();
                    multiBoxInits[index] = mInit;

                    ResourceSet mSet = rf.CreateResourceSet(new ResourceSetDescription(resourceLayout, mWorldUB, _viewUB, _projUB, texView, _gd.PointSampler));

                    CommandList mRender = rf.CreateCommandList(new CommandListDescription());
                    mRender.Begin();
                    mRender.SetFramebuffer(_gd.SwapchainFramebuffer);
                    mRender.SetViewport(0, new Viewport(0, 0, _window.Width, _window.Height, 0, 1));
                    mRender.SetScissorRect(0, 0, 0, (uint)_window.Width, (uint)_window.Height);
                    mRender.SetVertexBuffer(0, vb, (uint)Unsafe.SizeOf<VertexPositionNormalTexture>());
                    mRender.SetIndexBuffer(ib);
                    mRender.SetPipeline(p);
                    mRender.SetResourceSet(mSet);
                    mRender.Draw((uint)indices.Length, 1, 0, 0, 0);
                    mRender.End();
                    multiBoxRenderCBs[index] = mRender;
                });
            });

            foreach (CommandList mInitCB in multiBoxInits)
            {
                _gd.ExecuteCommands(mInitCB);
            }

            CommandList mainCB = rf.CreateCommandList(new CommandListDescription());
            mainCB.Begin();
            mainCB.SetFramebuffer(_gd.SwapchainFramebuffer);
            renderCB.SetViewport(0, new Viewport(0, 0, _window.Width, _window.Height, 0, 1));
            renderCB.SetScissorRect(0, 0, 0, (uint)_window.Width, (uint)_window.Height);
            mainCB.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            mainCB.ClearDepthTarget(1f);
            mainCB.End();

            CommandList perFrameCB = rf.CreateCommandList(new CommandListDescription());

            long previousFrameTicks = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (_window.Exists)
            {
                long currentFrameTicks = sw.ElapsedTicks;
                double deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

                while (_limitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
                {
                    currentFrameTicks = sw.ElapsedTicks;
                    deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                }

                previousFrameTicks = currentFrameTicks;

                InputSnapshot snapshot = _window.PumpEvents();
                InputTracker.UpdateFrameInput();
                Update(deltaSeconds, snapshot);

                UpdateViewProjection(perFrameCB);
                _gd.ExecuteCommands(mainCB);
                _gd.ExecuteCommands(renderCB);
                foreach (CommandList mRenderCB in multiBoxRenderCBs)
                {
                    _gd.ExecuteCommands(mRenderCB);
                }

                _gd.SwapBuffers();

            }

            _gd.Dispose();
        }

        private static unsafe CommandList GetInitCommandList(ResourceFactory rf)
        {
            //return Task.Factory.StartNew(() => rf.CreateCommandBuffer(), TaskCreationOptions.LongRunning).Result;

            return rf.CreateCommandList();
        }

        private static void Update(double deltaSeconds, InputSnapshot snapshot)
        {
            _fta.AddTime(deltaSeconds);
            _window.Title = _fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms");
        }

        private static void UpdateViewProjection(CommandList cb)
        {
            _camera.Update(1f / 60f);
            _camera.WindowResized(_window.Width, _window.Height);
            Matrix4x4 view = _camera.ViewMatrix;
            Matrix4x4 proj = _camera.ProjectionMatrix;

            cb.Begin();
            cb.UpdateBuffer(_viewUB, 0, ref view, 64);
            cb.UpdateBuffer(_projUB, 0, ref proj, 64);
            cb.End();
            _gd.ExecuteCommands(cb);
        }

        private static Pipeline CreateBoxPipeline(ResourceFactory rf)
        {
            PipelineDescription pd = new PipelineDescription();
            pd.BlendState = BlendStateDescription.SingleOverrideBlend;
            pd.DepthStencilStateDescription = DepthStencilStateDescription.LessEqual;
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
