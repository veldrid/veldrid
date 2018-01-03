using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace Offscreen
{
    public class OffscreenApplication : SampleApplication
    {
        private const uint OffscreenWidth = 1024;
        private const uint OffscreenHeight = 1024;

        private CommandList _cl;
        private Framebuffer _offscreenFB;
        private Pipeline _offscreenPipeline;
        private Model _dragonModel;
        private Model _planeModel;
        private Texture _colorMap;
        private TextureView _colorView;
        private Texture _offscreenColor;
        private TextureView _offscreenView;
        private VertexLayoutDescription _vertexLayout;
        private Pipeline _dragonPipeline;
        private Pipeline _mirrorPipeline;
        private Vector3 _cameraPos = new Vector3(0, 1, 6f);
        private Vector3 _rotation = new Vector3(-1.5f, 0, 0);
        private Vector3 _dragonPos = new Vector3(0, 1.5f, 0);
        private Vector3 _dragonRotation = new Vector3(0, 0, 0);
        private float _zoom = -6f;

        private DeviceBuffer _uniformBuffers_vsShared;
        private DeviceBuffer _uniformBuffers_vsMirror;
        private DeviceBuffer _uniformBuffers_vsOffScreen;
        private ResourceSet _offscreenResourceSet;
        private ResourceSet _dragonResourceSet;
        private ResourceSet _mirrorResourceSet;
        private Vector2 _mousePos;
        private float _zoomSpeed = 5f;
        private float _rotationSpeed = 1f;

        private int _startTicks;

        public OffscreenApplication()
        {
            _startTicks = Environment.TickCount;
        }

        protected override void CreateResources(ResourceFactory factory)
        {
            _vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3));

            _planeModel = new Model();
            _planeModel.LoadFromFile(
                _gd,
                factory,
                GetAssetPath("models/plane2.dae"),
                _vertexLayout,
                new Model.ModelCreateInfo(new Vector3(0.5f, 0.5f, 0.5f), Vector2.One, Vector3.Zero));

            _dragonModel = new Model();
            _dragonModel.LoadFromFile(
                _gd,
                factory,
                GetAssetPath("models/chinesedragon.dae"),
                _vertexLayout,
                new Model.ModelCreateInfo(new Vector3(0.3f, -0.3f, 0.3f), Vector2.One, Vector3.Zero));

            _colorMap = LoadTexture(
                _gd,
                factory,
                GetAssetPath("textures/darkmetal_bc3_unorm.ktx"),
                PixelFormat.BC3_UNorm);
            _colorView = factory.CreateTextureView(_colorMap);

            _offscreenColor = factory.CreateTexture(TextureDescription.Texture2D(
                OffscreenWidth, OffscreenHeight, 1, 1,
                 PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
            _offscreenView = factory.CreateTextureView(_offscreenColor);
            Texture offscreenDepth = factory.CreateTexture(TextureDescription.Texture2D(
                OffscreenWidth, OffscreenHeight, 1, 1, PixelFormat.R16_UNorm, TextureUsage.DepthStencil));
            _offscreenFB = factory.CreateFramebuffer(new FramebufferDescription(offscreenDepth, _offscreenColor));

            ShaderSetDescription phongShaders = new ShaderSetDescription(
                new[] { _vertexLayout },
                new[]
                {
                    LoadShader(factory, "Phong", ShaderStages.Vertex, "VS"),
                    LoadShader(factory, "Phong", ShaderStages.Fragment, "FS")
                });

            ResourceLayout phongLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UBO", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.Front, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology.TriangleList,
                phongShaders,
                phongLayout,
                _offscreenFB.OutputDescription);
            _offscreenPipeline = factory.CreateGraphicsPipeline(pd);

            pd.Outputs = _gd.SwapchainFramebuffer.OutputDescription;
            pd.RasterizerState = RasterizerStateDescription.Default;
            _dragonPipeline = factory.CreateGraphicsPipeline(pd);

            ResourceLayout mirrorLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UBO", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ReflectionMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ReflectionMapSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ColorMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ColorMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            ShaderSetDescription mirrorShaders = new ShaderSetDescription(
                new[] { _vertexLayout },
                new[]
                {
                    LoadShader(factory, "Mirror", ShaderStages.Vertex, "VS"),
                    LoadShader(factory, "Mirror", ShaderStages.Fragment, "FS")
                });

            GraphicsPipelineDescription mirrorPD = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology.TriangleList,
                mirrorShaders,
                mirrorLayout,
                _gd.SwapchainFramebuffer.OutputDescription);
            _mirrorPipeline = factory.CreateGraphicsPipeline(ref mirrorPD);

            _uniformBuffers_vsShared = factory.CreateBuffer(new BufferDescription(208, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _uniformBuffers_vsMirror = factory.CreateBuffer(new BufferDescription(208, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _uniformBuffers_vsOffScreen = factory.CreateBuffer(new BufferDescription(208, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            _offscreenResourceSet = factory.CreateResourceSet(new ResourceSetDescription(phongLayout, _uniformBuffers_vsOffScreen));
            _dragonResourceSet = factory.CreateResourceSet(new ResourceSetDescription(phongLayout, _uniformBuffers_vsShared));
            _mirrorResourceSet = factory.CreateResourceSet(new ResourceSetDescription(mirrorLayout,
                _uniformBuffers_vsMirror,
                _offscreenView,
                _gd.LinearSampler,
                _colorView,
                _gd.Aniso4xSampler));

            _cl = factory.CreateCommandList();
        }

        public struct UniformInfo
        {
            public Matrix4x4 Projection;
            public Matrix4x4 View;
            public Matrix4x4 Model;
            public Vector4 LightPos;
        }

        protected override void Draw()
        {
            int newTicks = Environment.TickCount;
            float frameTimer = (newTicks - _startTicks) / 1000f;
            _startTicks = newTicks;
            _dragonRotation.Y += frameTimer * 10f;

            UpdateUniformBuffers();
            UpdateUniformBufferOffscreen();

            _cl.Begin();
            DrawOffscreen();
            DrawMain();
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers();
        }

        private void DrawOffscreen()
        {
            _cl.SetFramebuffer(_offscreenFB);
            _cl.SetFullViewports();
            _cl.SetFullScissorRects();
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);

            _cl.SetPipeline(_offscreenPipeline);
            _cl.SetGraphicsResourceSet(0, _offscreenResourceSet);
            _cl.SetVertexBuffer(0, _dragonModel._vertexBuffer);
            _cl.SetIndexBuffer(_dragonModel._indexBuffer, IndexFormat.UInt32);
            _cl.DrawIndexed(_dragonModel._indexCount, 1, 0, 0, 0);
        }

        private void DrawMain()
        {
            _cl.SetFramebuffer(_gd.SwapchainFramebuffer);
            _cl.SetFullViewports();
            _cl.SetFullScissorRects();
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);

            _cl.SetPipeline(_mirrorPipeline);
            _cl.SetGraphicsResourceSet(0, _mirrorResourceSet);
            _cl.SetVertexBuffer(0, _planeModel._vertexBuffer);
            _cl.SetIndexBuffer(_planeModel._indexBuffer, IndexFormat.UInt32);
            _cl.DrawIndexed(_planeModel._indexCount, 1, 0, 0, 0);

            _cl.SetPipeline(_dragonPipeline);
            _cl.SetGraphicsResourceSet(0, _dragonResourceSet);
            _cl.SetVertexBuffer(0, _dragonModel._vertexBuffer);
            _cl.SetIndexBuffer(_dragonModel._indexBuffer, IndexFormat.UInt32);
            _cl.DrawIndexed(_dragonModel._indexCount, 1, 0, 0, 0);
        }

        public static float DegreesToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180f;
        }

        private void UpdateUniformBuffers()
        {
            UniformInfo ui = new UniformInfo { LightPos = new Vector4(0, 0, 0, 1) };

            ui.Projection = Matrix4x4.CreatePerspectiveFieldOfView(DegreesToRadians(60.0f), _window.Width / (float)_window.Height, 0.1f, 256.0f);

            Matrix4x4 cameraRot = Matrix4x4.CreateFromYawPitchRoll(
                DegreesToRadians(_rotation.Y),
                DegreesToRadians(_rotation.X),
                DegreesToRadians(_rotation.Z));
            Vector3 cameraLookDir = Vector3.Transform(-Vector3.UnitZ, cameraRot);
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(_cameraPos, _cameraPos + cameraLookDir, Vector3.UnitY);
            ui.View = viewMatrix;

            ui.Model = Matrix4x4.CreateRotationX(DegreesToRadians(_dragonRotation.X));
            ui.Model = Matrix4x4.CreateRotationY(DegreesToRadians(_dragonRotation.Y)) * ui.Model;
            ui.Model = Matrix4x4.CreateTranslation(_dragonPos) * ui.Model;

            _gd.UpdateBuffer(_uniformBuffers_vsShared, 0, ref ui);

            // Mirror
            ui.Model = Matrix4x4.Identity;
            _gd.UpdateBuffer(_uniformBuffers_vsMirror, 0, ref ui);
        }

        private void UpdateUniformBufferOffscreen()
        {
            UniformInfo ui = new UniformInfo { LightPos = new Vector4(0, 0, 0, 1) };

            ui.Projection = Matrix4x4.CreatePerspectiveFieldOfView(DegreesToRadians(60.0f), _window.Width / (float)_window.Height, 0.1f, 256.0f);

            Matrix4x4 cameraRot = Matrix4x4.CreateFromYawPitchRoll(
                DegreesToRadians(_rotation.Y),
                DegreesToRadians(_rotation.X),
                DegreesToRadians(_rotation.Z));
            Vector3 cameraLookDir = Vector3.Transform(-Vector3.UnitZ, cameraRot);
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(_cameraPos, _cameraPos + cameraLookDir, Vector3.UnitY);
            ui.View = viewMatrix;

            ui.Model = Matrix4x4.CreateRotationX(DegreesToRadians(_dragonRotation.X));
            ui.Model = Matrix4x4.CreateRotationY(DegreesToRadians(_dragonRotation.Y)) * ui.Model;
            ui.Model = Matrix4x4.CreateScale(new Vector3(1, -1, 1)) * ui.Model;
            ui.Model = Matrix4x4.CreateTranslation(_dragonPos) * ui.Model;

            _gd.UpdateBuffer(_uniformBuffers_vsOffScreen, 0, ref ui);
        }

        private unsafe Texture LoadTexture(
            GraphicsDevice gd,
            ResourceFactory factory,
            string assetPath,
            PixelFormat format)
        {
            KtxFile tex2D;
            using (FileStream fs = File.OpenRead(assetPath))
            {
                tex2D = KtxFile.Load(fs, false);
            }

            uint width = tex2D.Header.PixelWidth;
            uint height = tex2D.Header.PixelHeight;
            if (height == 0) height = width;

            uint mipLevels = tex2D.Header.NumberOfMipmapLevels;

            Texture ret = factory.CreateTexture(TextureDescription.Texture2D(
                width, height, mipLevels, 1,
                format, TextureUsage.Sampled));

            Texture stagingTex = factory.CreateTexture(TextureDescription.Texture2D(
                width, height, mipLevels, 1,
                format, TextureUsage.Staging));

            // Copy texture data into staging buffer
            for (uint level = 0; level < mipLevels; level++)
            {
                KtxMipmap mipmap = tex2D.Faces[0].Mipmaps[level];
                byte[] pixelData = mipmap.Data;
                fixed (byte* pixelDataPtr = &pixelData[0])
                {
                    gd.UpdateTexture(stagingTex, (IntPtr)pixelDataPtr, (uint)pixelData.Length,
                        0, 0, 0, mipmap.Width, mipmap.Height, 1, level, 0);
                }
            }

            CommandList copyCL = factory.CreateCommandList();
            copyCL.Begin();
            for (uint level = 0; level < mipLevels; level++)
            {
                uint levelWidth = tex2D.Faces[0].Mipmaps[level].Width;
                uint levelHeight = tex2D.Faces[0].Mipmaps[level].Height;
                copyCL.CopyTexture(stagingTex, 0, 0, 0, level, 0,
                    ret, 0, 0, 0, level, 0, levelWidth, levelHeight, 1, 1);
            }
            copyCL.End();
            gd.SubmitCommands(copyCL);

            gd.DisposeWhenIdle(copyCL);
            gd.DisposeWhenIdle(stagingTex);

            return ret;
        }

        private static string GetAssetPath(string name)
        {
            return Path.Combine(AppContext.BaseDirectory, name);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            int posx = (int)e.MousePosition.X;
            int posy = (int)e.MousePosition.Y;

            if (e.State.IsButtonDown(MouseButton.Right))
            {
                _zoom += (_mousePos.Y - posy) * .005f * _zoomSpeed;
                Matrix4x4 cameraRot = Matrix4x4.CreateFromYawPitchRoll(
                    DegreesToRadians(_rotation.Y),
                    DegreesToRadians(_rotation.X),
                    DegreesToRadians(_rotation.Z));
                Vector3 camForward = Vector3.Normalize(Vector3.Transform(-Vector3.UnitZ, cameraRot));
                _cameraPos += camForward * (_mousePos.Y - posy) * .005f * _zoomSpeed;
            }
            if (e.State.IsButtonDown(MouseButton.Left))
            {
                _rotation.X += (_mousePos.Y - posy) * 1.25f * _rotationSpeed;
                _rotation.Y += (_mousePos.X - posx) * 1.25f * _rotationSpeed;
            }
            if (e.State.IsButtonDown(MouseButton.Middle))
            {
                Matrix4x4 cameraRot = Matrix4x4.CreateFromYawPitchRoll(
                    DegreesToRadians(_rotation.Y),
                    DegreesToRadians(_rotation.X),
                    DegreesToRadians(_rotation.Z));
                Vector3 camForward = Vector3.Normalize(Vector3.Transform(-Vector3.UnitZ, cameraRot));
                Vector3 camRight = Vector3.Normalize(Vector3.Cross(camForward, Vector3.UnitY));
                Vector3 camUp = Vector3.Normalize(Vector3.Cross(camForward, -camRight));

                _cameraPos += (_mousePos.X - posx) * 0.01f * camRight;
                _cameraPos -= (_mousePos.Y - posy) * 0.01f * camUp;
                //_cameraPos.X += (_mousePos.X - posx) * 0.01f;
                //_cameraPos.Y -= (_mousePos.Y - posy) * 0.01f;
                _mousePos = new Vector2(posx, posy);
            }

            _mousePos = new Vector2(posx, posy);
        }
    }
}
