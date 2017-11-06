using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Veldrid.NeoDemo
{
    public class SceneContext
    {
        public UniformBuffer ProjectionMatrixBuffer { get; private set; }
        public UniformBuffer ViewMatrixBuffer { get; private set; }
        public UniformBuffer LightInfoBuffer { get; private set; }
        public UniformBuffer LightViewProjectionBuffer0 { get; internal set; }
        public UniformBuffer LightViewProjectionBuffer1 { get; internal set; }
        public UniformBuffer LightViewProjectionBuffer2 { get; internal set; }
        public UniformBuffer DepthLimitsBuffer { get; internal set; }
        public UniformBuffer CameraInfoBuffer { get; private set; }
        public UniformBuffer PointLightsBuffer { get; private set; }

        public Texture NearShadowMapTexture { get; private set; }
        public TextureView NearShadowMapView { get; private set; }
        public Framebuffer NearShadowMapFramebuffer { get; private set; }

        public Texture MidShadowMapTexture { get; private set; }
        public TextureView MidShadowMapView { get; private set; }
        public Framebuffer MidShadowMapFramebuffer { get; private set; }

        public Texture FarShadowMapTexture { get; private set; }
        public TextureView FarShadowMapView { get; private set; }
        public Framebuffer FarShadowMapFramebuffer { get; private set; }

        // MainSceneView and Duplicator resource sets both use this.
        public ResourceLayout TextureSamplerResourceLayout { get; private set; }

        public Texture MainSceneColorTexture { get; private set; }
        public Texture MainSceneDepthTexture { get; private set; }
        public TextureView MainSceneColorView { get; private set; }
        public Framebuffer MainSceneFramebuffer { get; private set; }
        public ResourceSet MainSceneViewResourceSet { get; private set; }

        public Texture DuplicatorTarget0 { get; private set; }
        public TextureView DuplicatorTargetView0 { get; private set; }
        public ResourceSet DuplicatorTargetSet0 { get; internal set; }
        public Texture DuplicatorTarget1 { get; private set; }
        public TextureView DuplicatorTargetView1 { get; private set; }
        public ResourceSet DuplicatorTargetSet1 { get; internal set; }
        public Framebuffer DuplicatorFramebuffer { get; private set; }

        public Camera Camera { get; set; }
        public DirectionalLight DirectionalLight { get; } = new DirectionalLight();

        public virtual void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;
            ProjectionMatrixBuffer = factory.CreateUniformBuffer(new BufferDescription(64));
            ViewMatrixBuffer = factory.CreateUniformBuffer(new BufferDescription(64));
            LightViewProjectionBuffer0 = factory.CreateUniformBuffer(new BufferDescription(64, true));
            gd.SetResourceName(LightViewProjectionBuffer0, "LightViewProjectionBuffer0");
            LightViewProjectionBuffer1 = factory.CreateUniformBuffer(new BufferDescription(64, true));
            gd.SetResourceName(LightViewProjectionBuffer1, "LightViewProjectionBuffer1");
            LightViewProjectionBuffer2 = factory.CreateUniformBuffer(new BufferDescription(64, true));
            gd.SetResourceName(LightViewProjectionBuffer2, "LightViewProjectionBuffer2");
            DepthLimitsBuffer = factory.CreateUniformBuffer(new BufferDescription((uint)Unsafe.SizeOf<DepthCascadeLimits>()));
            LightInfoBuffer = factory.CreateUniformBuffer(new BufferDescription((uint)Unsafe.SizeOf<DirectionalLightInfo>()));
            CameraInfoBuffer = factory.CreateUniformBuffer(new BufferDescription((uint)Unsafe.SizeOf<CameraInfo>()));
            if (Camera != null)
            {
                UpdateCameraBuffers(cl);
            }

            PointLightsBuffer = factory.CreateUniformBuffer(new BufferDescription((uint)Unsafe.SizeOf<PointLightsInfo.Blittable>()));

            PointLightsInfo pli = new PointLightsInfo();
            pli.NumActiveLights = 4;
            pli.PointLights = new PointLightInfo[4]
            {
                new PointLightInfo { Color = new Vector3(1f, 1f, 1f), Position = new Vector3(-50, 5, 0), Range = 75f },
                new PointLightInfo { Color = new Vector3(1f, .75f, .9f), Position = new Vector3(0, 5, 0), Range = 100f },
                new PointLightInfo { Color = new Vector3(1f, 1f, 0.6f), Position = new Vector3(50, 5, 0), Range = 40f },
                new PointLightInfo { Color = new Vector3(0.75f, 0.75f, 1f), Position = new Vector3(25, 5, 45), Range = 150f },
            };

            cl.UpdateBuffer(PointLightsBuffer, 0, pli.GetBlittable());

            NearShadowMapTexture = factory.CreateTexture(new TextureDescription(2048, 2048, 1, 1, 1, PixelFormat.R16_UNorm, TextureUsage.DepthStencil | TextureUsage.Sampled));
            gd.SetResourceName(NearShadowMapTexture, "Near Shadow Map");
            NearShadowMapView = factory.CreateTextureView(new TextureViewDescription(NearShadowMapTexture));
            NearShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(NearShadowMapTexture));

            MidShadowMapTexture = factory.CreateTexture(new TextureDescription(2048, 2048, 1, 1, 1, PixelFormat.R16_UNorm, TextureUsage.DepthStencil | TextureUsage.Sampled));
            MidShadowMapView = factory.CreateTextureView(new TextureViewDescription(MidShadowMapTexture));
            MidShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(MidShadowMapTexture));

            FarShadowMapTexture = factory.CreateTexture(new TextureDescription(4096, 4096, 1, 1, 1, PixelFormat.R16_UNorm, TextureUsage.DepthStencil | TextureUsage.Sampled));
            FarShadowMapView = factory.CreateTextureView(new TextureViewDescription(FarShadowMapTexture));
            FarShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(FarShadowMapTexture));

            TextureSamplerResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.Texture, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            RecreateWindowSizedResources(gd, cl);
        }

        public virtual void DestroyDeviceObjects()
        {
            ProjectionMatrixBuffer.Dispose();
            ViewMatrixBuffer.Dispose();
            LightInfoBuffer.Dispose();
            LightViewProjectionBuffer0.Dispose();
            LightViewProjectionBuffer1.Dispose();
            LightViewProjectionBuffer2.Dispose();
            NearShadowMapView.Dispose();
            NearShadowMapFramebuffer.Dispose();
            NearShadowMapTexture.Dispose();
            MidShadowMapView.Dispose();
            MidShadowMapFramebuffer.Dispose();
            MidShadowMapTexture.Dispose();
            FarShadowMapView.Dispose();
            FarShadowMapFramebuffer.Dispose();
            FarShadowMapTexture.Dispose();
            DepthLimitsBuffer.Dispose();
            CameraInfoBuffer.Dispose();
            PointLightsBuffer.Dispose();
            MainSceneColorTexture.Dispose();
            MainSceneColorView.Dispose();
            MainSceneDepthTexture.Dispose();
            MainSceneFramebuffer.Dispose();
            MainSceneViewResourceSet.Dispose();
            DuplicatorTarget0.Dispose();
            DuplicatorTarget1.Dispose();
            DuplicatorTargetView0.Dispose();
            DuplicatorTargetView1.Dispose();
            DuplicatorTargetSet0.Dispose();
            DuplicatorTargetSet1.Dispose();
            DuplicatorFramebuffer.Dispose();
            TextureSamplerResourceLayout.Dispose();
        }

        public void SetCurrentScene(Scene scene)
        {
            Camera = scene.Camera;
        }

        public void UpdateCameraBuffers(CommandList cl)
        {
            cl.UpdateBuffer(ProjectionMatrixBuffer, 0, Camera.ProjectionMatrix);
            cl.UpdateBuffer(ViewMatrixBuffer, 0, Camera.ViewMatrix);
            cl.UpdateBuffer(CameraInfoBuffer, 0, Camera.GetCameraInfo());
        }

        internal void RecreateWindowSizedResources(GraphicsDevice gd, CommandList cl)
        {
            MainSceneColorTexture?.Dispose();
            MainSceneDepthTexture?.Dispose();
            MainSceneColorView?.Dispose();
            MainSceneViewResourceSet?.Dispose();
            MainSceneFramebuffer?.Dispose();
            DuplicatorTarget0?.Dispose();
            DuplicatorTarget1?.Dispose();
            DuplicatorTargetView0?.Dispose();
            DuplicatorTargetView1?.Dispose();
            DuplicatorTargetSet0?.Dispose();
            DuplicatorTargetSet1?.Dispose();
            DuplicatorFramebuffer?.Dispose();

            ResourceFactory factory = gd.ResourceFactory;
            TextureDescription colorTargetDesc = new TextureDescription(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.RenderTarget | TextureUsage.Sampled);

            MainSceneColorTexture = factory.CreateTexture(ref colorTargetDesc);
            MainSceneColorView = factory.CreateTextureView(MainSceneColorTexture);
            MainSceneDepthTexture = factory.CreateTexture(new TextureDescription(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                1,
                PixelFormat.R16_UNorm,
                TextureUsage.DepthStencil));
            MainSceneFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(MainSceneDepthTexture, MainSceneColorTexture));
            MainSceneViewResourceSet = factory.CreateResourceSet(new ResourceSetDescription(TextureSamplerResourceLayout, MainSceneColorView, gd.PointSampler));

            DuplicatorTarget0 = factory.CreateTexture(ref colorTargetDesc);
            DuplicatorTargetView0 = factory.CreateTextureView(DuplicatorTarget0);
            DuplicatorTarget1 = factory.CreateTexture(ref colorTargetDesc);
            DuplicatorTargetView1 = factory.CreateTextureView(DuplicatorTarget1);
            DuplicatorTargetSet0 = factory.CreateResourceSet(new ResourceSetDescription(TextureSamplerResourceLayout, DuplicatorTargetView0, gd.PointSampler));
            DuplicatorTargetSet1 = factory.CreateResourceSet(new ResourceSetDescription(TextureSamplerResourceLayout, DuplicatorTargetView1, gd.PointSampler));

            FramebufferDescription fbDesc = new FramebufferDescription(null, DuplicatorTarget0, DuplicatorTarget1);
            DuplicatorFramebuffer = factory.CreateFramebuffer(ref fbDesc);
        }
    }
}
