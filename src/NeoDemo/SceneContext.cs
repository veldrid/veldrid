using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.NeoDemo.Objects;

namespace Veldrid.NeoDemo
{
    public class SceneContext
    {
        public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
        public DeviceBuffer ViewMatrixBuffer { get; private set; }
        public DeviceBuffer LightInfoBuffer { get; private set; }
        public DeviceBuffer LightViewProjectionBuffer0 { get; internal set; }
        public DeviceBuffer LightViewProjectionBuffer1 { get; internal set; }
        public DeviceBuffer LightViewProjectionBuffer2 { get; internal set; }
        public DeviceBuffer DepthLimitsBuffer { get; internal set; }
        public DeviceBuffer CameraInfoBuffer { get; private set; }
        public DeviceBuffer PointLightsBuffer { get; private set; }

        public CascadedShadowMaps ShadowMaps { get; private set; } = new CascadedShadowMaps();
        public TextureView NearShadowMapView => ShadowMaps.NearShadowMapView;
        public TextureView MidShadowMapView => ShadowMaps.MidShadowMapView;
        public TextureView FarShadowMapView => ShadowMaps.FarShadowMapView;
        public Framebuffer NearShadowMapFramebuffer => ShadowMaps.NearShadowMapFramebuffer;
        public Framebuffer MidShadowMapFramebuffer => ShadowMaps.MidShadowMapFramebuffer;
        public Framebuffer FarShadowMapFramebuffer => ShadowMaps.FarShadowMapFramebuffer;
        public Texture ShadowMapTexture => ShadowMaps.NearShadowMap; // Only used for size.

        public Texture ReflectionColorTexture { get; private set; }
        public Texture ReflectionDepthTexture { get; private set; }
        public TextureView ReflectionColorView { get; private set; }
        public Framebuffer ReflectionFramebuffer { get; private set; }
        public DeviceBuffer ReflectionViewProjBuffer { get; private set; }

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
        public DeviceBuffer MirrorClipPlaneBuffer { get; private set; }
        public DeviceBuffer NoClipPlaneBuffer { get; private set; }

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
                UpdateCameraBuffers(cl);
            }

            PointLightsBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<PointLightsInfo.Blittable>(), BufferUsage.UniformBuffer));

            PointLightsInfo pli = new PointLightsInfo();
            pli.NumActiveLights = 4;
            pli.PointLights = new PointLightInfo[4]
            {
                new PointLightInfo { Color = new Vector3(.6f, .6f, .6f), Position = new Vector3(-50, 5, 0), Range = 75f },
                new PointLightInfo { Color = new Vector3(.6f, .35f, .4f), Position = new Vector3(0, 5, 0), Range = 100f },
                new PointLightInfo { Color = new Vector3(.6f, .6f, 0.35f), Position = new Vector3(50, 5, 0), Range = 40f },
                new PointLightInfo { Color = new Vector3(0.4f, 0.4f, .6f), Position = new Vector3(25, 5, 45), Range = 150f },
            };

            cl.UpdateBuffer(PointLightsBuffer, 0, pli.GetBlittable());

            TextureSamplerResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            uint ReflectionMapSize = 2048;
            ReflectionColorTexture = factory.CreateTexture(TextureDescription.Texture2D(ReflectionMapSize, ReflectionMapSize, 12, 1, PixelFormat.R16_G16_B16_A16_Float, TextureUsage.RenderTarget | TextureUsage.Sampled | TextureUsage.GenerateMipmaps));
            ReflectionDepthTexture = factory.CreateTexture(TextureDescription.Texture2D(ReflectionMapSize, ReflectionMapSize, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil));
            ReflectionColorView = factory.CreateTextureView(ReflectionColorTexture);
            ReflectionFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(ReflectionDepthTexture, ReflectionColorTexture));
            ReflectionViewProjBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            MirrorClipPlaneBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer));
            gd.UpdateBuffer(MirrorClipPlaneBuffer, 0, new ClipPlaneInfo(MirrorMesh.Plane, true));
            NoClipPlaneBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer));
            gd.UpdateBuffer(NoClipPlaneBuffer, 0, new ClipPlaneInfo());

            RecreateWindowSizedResources(gd, cl);

            ShadowMaps.CreateDeviceResources(gd);
        }

        public virtual void DestroyDeviceObjects()
        {
            ProjectionMatrixBuffer.Dispose();
            ViewMatrixBuffer.Dispose();
            LightInfoBuffer.Dispose();
            LightViewProjectionBuffer0.Dispose();
            LightViewProjectionBuffer1.Dispose();
            LightViewProjectionBuffer2.Dispose();
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
            ReflectionColorTexture.Dispose();
            ReflectionDepthTexture.Dispose();
            ReflectionColorView.Dispose();
            ReflectionFramebuffer.Dispose();
            ReflectionViewProjBuffer.Dispose();
            MirrorClipPlaneBuffer.Dispose();
            NoClipPlaneBuffer.Dispose();
            ShadowMaps.DestroyDeviceObjects();
        }

        public void SetCurrentScene(Scene scene)
        {
            Camera = scene.Camera;
        }

        public unsafe void UpdateCameraBuffers(CommandList cl)
        {
            cl.UpdateBuffer(ProjectionMatrixBuffer, 0, Camera.ProjectionMatrix);
            cl.UpdateBuffer(ViewMatrixBuffer, 0, Camera.ViewMatrix);
            cl.UpdateBuffer(CameraInfoBuffer, 0, Camera.GetCameraInfo());
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

            gd.GetPixelFormatSupport(
                PixelFormat.R16_G16_B16_A16_Float,
                TextureType.Texture2D,
                TextureUsage.RenderTarget,
                out PixelFormatProperties properties);

            TextureSampleCount sampleCount = MainSceneSampleCount;
            while (!properties.IsSampleCountSupported(sampleCount))
            {
                sampleCount = sampleCount - 1;
            }

            TextureDescription mainColorDesc = TextureDescription.Texture2D(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                PixelFormat.R16_G16_B16_A16_Float,
                TextureUsage.RenderTarget | TextureUsage.Sampled,
                sampleCount);

            MainSceneColorTexture = factory.CreateTexture(ref mainColorDesc);
            if (sampleCount != TextureSampleCount.Count1)
            {
                mainColorDesc.SampleCount = TextureSampleCount.Count1;
                MainSceneResolvedColorTexture = factory.CreateTexture(ref mainColorDesc);
            }
            else
            {
                MainSceneResolvedColorTexture = MainSceneColorTexture;
            }
            MainSceneResolvedColorView = factory.CreateTextureView(MainSceneResolvedColorTexture);
            MainSceneDepthTexture = factory.CreateTexture(TextureDescription.Texture2D(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                PixelFormat.R32_Float,
                TextureUsage.DepthStencil,
                sampleCount));
            MainSceneFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(MainSceneDepthTexture, MainSceneColorTexture));
            MainSceneViewResourceSet = factory.CreateResourceSet(new ResourceSetDescription(TextureSamplerResourceLayout, MainSceneResolvedColorView, gd.PointSampler));

            TextureDescription colorTargetDesc = TextureDescription.Texture2D(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                PixelFormat.R16_G16_B16_A16_Float,
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

    public class CascadedShadowMaps
    {
        public Texture NearShadowMap { get; private set; }
        public TextureView NearShadowMapView { get; private set; }
        public Framebuffer NearShadowMapFramebuffer { get; private set; }

        public Texture MidShadowMap { get; private set; }
        public TextureView MidShadowMapView { get; private set; }
        public Framebuffer MidShadowMapFramebuffer { get; private set; }

        public Texture FarShadowMap { get; private set; }
        public TextureView FarShadowMapView { get; private set; }
        public Framebuffer FarShadowMapFramebuffer { get; private set; }

        public void CreateDeviceResources(GraphicsDevice gd)
        {
            var factory = gd.ResourceFactory;
            TextureDescription desc = TextureDescription.Texture2D(2048, 2048, 1, 1, PixelFormat.D32_Float_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled);
            NearShadowMap = factory.CreateTexture(desc);
            NearShadowMap.Name = "Near Shadow Map";
            NearShadowMapView = factory.CreateTextureView(NearShadowMap);
            NearShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(NearShadowMap, 0), Array.Empty<FramebufferAttachmentDescription>()));

            MidShadowMap = factory.CreateTexture(desc);
            MidShadowMapView = factory.CreateTextureView(new TextureViewDescription(MidShadowMap, 0, 1, 0, 1));
            MidShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(MidShadowMap, 0), Array.Empty<FramebufferAttachmentDescription>()));

            FarShadowMap = factory.CreateTexture(desc);
            FarShadowMapView = factory.CreateTextureView(new TextureViewDescription(FarShadowMap, 0, 1, 0, 1));
            FarShadowMapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(FarShadowMap, 0), Array.Empty<FramebufferAttachmentDescription>()));
        }

        public void DestroyDeviceObjects()
        {
            NearShadowMap.Dispose();
            NearShadowMapView.Dispose();
            NearShadowMapFramebuffer.Dispose();

            MidShadowMap.Dispose();
            MidShadowMapView.Dispose();
            MidShadowMapFramebuffer.Dispose();

            FarShadowMap.Dispose();
            FarShadowMapView.Dispose();
            FarShadowMapFramebuffer.Dispose();
        }
    }
}
