using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Veldrid.NeoDemo
{
    public class SceneContext
    {
        public Buffer ProjectionMatrixBuffer { get; private set; }
        public Buffer ViewMatrixBuffer { get; private set; }
        public Buffer LightInfoBuffer { get; private set; }
        public Buffer LightViewProjectionBuffer0 { get; internal set; }
        public Buffer LightViewProjectionBuffer1 { get; internal set; }
        public Buffer LightViewProjectionBuffer2 { get; internal set; }
        public Buffer DepthLimitsBuffer { get; internal set; }
        public Buffer CameraInfoBuffer { get; private set; }
        public Buffer PointLightsBuffer { get; private set; }

        public Texture ShadowMapTexture { get; private set; }
        public TextureView NearShadowMapView { get; private set; }
        public Framebuffer NearShadowMapFramebuffer { get; private set; }

        public TextureView MidShadowMapView { get; private set; }
        public Framebuffer MidShadowMapFramebuffer { get; private set; }

        public TextureView FarShadowMapView { get; private set; }
        public Framebuffer FarShadowMapFramebuffer { get; private set; }

        // MainSceneView and Duplicator resource sets both use this.
        public ResourceLayout TextureSamplerResourceLayout { get; private set; }

        public Texture MainSceneColorTexture { get; private set; }
        public Texture MainSceneDepthTexture { get; private set; }
        public Framebuffer MainSceneFramebuffer { get; private set; }
        public Texture MainSceneResolvedColorTexture { get; private set; }
        public TextureView MainSceneResolvedColorView { get; private set; }
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
        public TextureSampleCount MainSceneSampleCount { get; internal set; }

        public virtual void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;
            ProjectionMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            ViewMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            LightViewProjectionBuffer0 = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            LightViewProjectionBuffer0.Name = "LightViewProjectionBuffer0";
            LightViewProjectionBuffer1 = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            LightViewProjectionBuffer1.Name = "LightViewProjectionBuffer1";
            LightViewProjectionBuffer2 = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            LightViewProjectionBuffer2.Name = "LightViewProjectionBuffer2";
            DepthLimitsBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<DepthCascadeLimits>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            LightInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<DirectionalLightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            CameraInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            if (Camera != null)
            {
                UpdateCameraBuffers(gd);
            }

            PointLightsBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<PointLightsInfo.Blittable>(), BufferUsage.UniformBuffer));

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

            ShadowMapTexture = factory.CreateTexture(new TextureDescription(2048, 2048, 1, 1, 3, PixelFormat.R16_UNorm, TextureUsage.DepthStencil | TextureUsage.Sampled));
            ShadowMapTexture.Name = "Shadow Map";
            NearShadowMapView = factory.CreateTextureView(new TextureViewDescription(ShadowMapTexture, 0, 1, 0, 1));
            NearShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(ShadowMapTexture, 0), Array.Empty<FramebufferAttachmentDescription>()));

            MidShadowMapView = factory.CreateTextureView(new TextureViewDescription(ShadowMapTexture, 0, 1, 1, 1));
            MidShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(ShadowMapTexture, 1), Array.Empty<FramebufferAttachmentDescription>()));

            FarShadowMapView = factory.CreateTextureView(new TextureViewDescription(ShadowMapTexture, 0, 1, 2, 1));
            FarShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(ShadowMapTexture, 2), Array.Empty<FramebufferAttachmentDescription>()));

            TextureSamplerResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
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
            ShadowMapTexture.Dispose();
            MidShadowMapView.Dispose();
            MidShadowMapFramebuffer.Dispose();
            FarShadowMapView.Dispose();
            FarShadowMapFramebuffer.Dispose();
            DepthLimitsBuffer.Dispose();
            CameraInfoBuffer.Dispose();
            PointLightsBuffer.Dispose();
            MainSceneColorTexture.Dispose();
            MainSceneResolvedColorTexture.Dispose();
            MainSceneResolvedColorView.Dispose();
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

        public unsafe void UpdateCameraBuffers(GraphicsDevice gd)
        {
            gd.UpdateBuffer(ProjectionMatrixBuffer, 0, Camera.ProjectionMatrix);

            MappedResource mappedView = gd.Map(ViewMatrixBuffer, MapMode.Write);
            Unsafe.Write(mappedView.Data.ToPointer(), Camera.ViewMatrix);
            gd.Unmap(ViewMatrixBuffer);

            MappedResource mappedCameraInfo = gd.Map(CameraInfoBuffer, MapMode.Write);
            Unsafe.Write(mappedCameraInfo.Data.ToPointer(), Camera.GetCameraInfo());
            gd.Unmap(CameraInfoBuffer);
        }

        internal void RecreateWindowSizedResources(GraphicsDevice gd, CommandList cl)
        {
            MainSceneColorTexture?.Dispose();
            MainSceneDepthTexture?.Dispose();
            MainSceneResolvedColorTexture?.Dispose();
            MainSceneResolvedColorView?.Dispose();
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

            TextureSampleCount mainSceneSampleCountCapped = (TextureSampleCount)Math.Min(
                (int)gd.GetSampleCountLimit(PixelFormat.R8_G8_B8_A8_UNorm, false),
                (int)MainSceneSampleCount);

            TextureDescription mainColorDesc = new TextureDescription(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.RenderTarget | TextureUsage.Sampled,
                mainSceneSampleCountCapped);

            MainSceneColorTexture = factory.CreateTexture(ref mainColorDesc);
            if (mainSceneSampleCountCapped != TextureSampleCount.Count1)
            {
                mainColorDesc.SampleCount = TextureSampleCount.Count1;
                MainSceneResolvedColorTexture = factory.CreateTexture(ref mainColorDesc);
            }
            else
            {
                MainSceneResolvedColorTexture = MainSceneColorTexture;
            }
            MainSceneResolvedColorView = factory.CreateTextureView(MainSceneResolvedColorTexture);
            MainSceneDepthTexture = factory.CreateTexture(new TextureDescription(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                1,
                PixelFormat.R16_UNorm,
                TextureUsage.DepthStencil,
                mainSceneSampleCountCapped));
            MainSceneFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(MainSceneDepthTexture, MainSceneColorTexture));
            MainSceneViewResourceSet = factory.CreateResourceSet(new ResourceSetDescription(TextureSamplerResourceLayout, MainSceneResolvedColorView, gd.PointSampler));

            TextureDescription colorTargetDesc = new TextureDescription(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.RenderTarget | TextureUsage.Sampled);
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
