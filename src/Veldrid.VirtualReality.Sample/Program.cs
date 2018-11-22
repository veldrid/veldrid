using ImGuiNET;
using SixLabors.ImageSharp;
using System;
using System.Numerics;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Veldrid.VirtualReality.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Sdl2Window window = VeldridStartup.CreateWindow(
                new WindowCreateInfo(
                    Sdl2Native.SDL_WINDOWPOS_CENTERED, Sdl2Native.SDL_WINDOWPOS_CENTERED,
                    1280, 720,
                    WindowState.Normal,
                    "Veldrid.VirtualReality Sample"));

            VRContext vrContext = VRContext.CreateOpenVR();

            GraphicsBackend backend = GraphicsBackend.Vulkan;

            bool debug = false;
#if DEBUG
            debug = true;
#endif

            GraphicsDeviceOptions gdo = new GraphicsDeviceOptions(debug, null, true, ResourceBindingModel.Improved, true, true, true);

            if (backend == GraphicsBackend.Vulkan)
            {
                // Oculus runtime causes validation errors.
                gdo.Debug = false;
            }

            (GraphicsDevice gd, Swapchain sc) = CreateDeviceAndSwapchain(window, vrContext, backend, gdo);
            window.Resized += () => sc.Resize((uint)window.Width, (uint)window.Height);

            vrContext.Initialize(gd);

            ImGuiRenderer igr = new ImGuiRenderer(gd, sc.Framebuffer.OutputDescription, window.Width, window.Height, true);
            window.Resized += () => igr.WindowResized(window.Width, window.Height);

            AssimpMesh mesh = new AssimpMesh(
                gd,
                vrContext.LeftEyeFramebuffer.OutputDescription,
                @"E:\projects\ascendance\demo\demo3d\Assets\cat.obj",
                @"E:\projects\ascendance\demo\demo3d\Assets\cat_spec.png");

            Skybox skybox = new Skybox(
                Image.Load(@"E:\Assets\envmap_miramar\miramar_ft.png"),
                Image.Load(@"E:\Assets\envmap_miramar\miramar_bk.png"),
                Image.Load(@"E:\Assets\envmap_miramar\miramar_lf.png"),
                Image.Load(@"E:\Assets\envmap_miramar\miramar_rt.png"),
                Image.Load(@"E:\Assets\envmap_miramar\miramar_up.png"),
                Image.Load(@"E:\Assets\envmap_miramar\miramar_dn.png"));
            skybox.CreateDeviceObjects(gd, vrContext.LeftEyeFramebuffer.OutputDescription);

            CommandList windowCL = gd.ResourceFactory.CreateCommandList();
            CommandList eyesCL = gd.ResourceFactory.CreateCommandList();

            MirrorTextureEyeSource eyeSource = MirrorTextureEyeSource.BothEyes;

            while (window.Exists)
            {
                InputSnapshot snapshot = window.PumpEvents();
                if (!window.Exists) { break; }

                igr.Update(1f / 60f, snapshot);

                if (ImGui.BeginMainMenuBar())
                {
                    if (ImGui.BeginMenu("Settings"))
                    {
                        if (ImGui.BeginMenu("Mirror Texture"))
                        {
                            if (ImGui.MenuItem("Both Eyes", null, eyeSource == MirrorTextureEyeSource.BothEyes))
                            {
                                eyeSource = MirrorTextureEyeSource.BothEyes;
                            }
                            if (ImGui.MenuItem("Left Eye", null, eyeSource == MirrorTextureEyeSource.LeftEye))
                            {
                                eyeSource = MirrorTextureEyeSource.LeftEye;
                            }
                            if (ImGui.MenuItem("Right Eye", null, eyeSource == MirrorTextureEyeSource.RightEye))
                            {
                                eyeSource = MirrorTextureEyeSource.RightEye;
                            }

                            ImGui.EndMenu();
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMainMenuBar();
                }

                windowCL.Begin();
                windowCL.SetFramebuffer(sc.Framebuffer);
                windowCL.ClearColorTarget(0, new RgbaFloat(0f, 0f, 0.2f, 1f));
                vrContext.RenderMirrorTexture(windowCL, sc.Framebuffer, eyeSource);
                igr.Render(gd, windowCL);
                windowCL.End();
                gd.SubmitCommands(windowCL);
                gd.SwapBuffers(sc);

                HmdPoseState poses = vrContext.WaitForPoses();

                // Render Eyes
                eyesCL.Begin();

                eyesCL.PushDebugGroup("Left Eye");
                RenderEye(eyesCL, vrContext.LeftEyeFramebuffer, mesh, skybox, poses.LeftEyeProjection, poses.LeftEyeView);
                eyesCL.PopDebugGroup();

                eyesCL.PushDebugGroup("Right Eye");
                RenderEye(eyesCL, vrContext.RightEyeFramebuffer, mesh, skybox, poses.RightEyeProjection, poses.RightEyeView);
                eyesCL.PopDebugGroup();

                eyesCL.End();
                gd.SubmitCommands(eyesCL);

                vrContext.SubmitFrame();
            }

            gd.Dispose();
        }

        private static void RenderEye(CommandList cl, Framebuffer fb, AssimpMesh mesh, Skybox skybox, Matrix4x4 proj, Matrix4x4 view)
        {
            cl.SetFramebuffer(fb);
            cl.ClearDepthStencil(1f);
            cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

            mesh.Render(cl, new UBO(
                proj,
                view,
                Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(0f, -1, -2f)));

            mesh.Render(cl, new UBO(
                proj,
                view,
                Matrix4x4.CreateScale(0.66f) * Matrix4x4.CreateTranslation(-0.5f, -1, -2f)));

            mesh.Render(cl, new UBO(
                proj,
                view,
                Matrix4x4.CreateScale(1.5f) * Matrix4x4.CreateTranslation(0.5f, -1, -2f)));

            skybox.Render(cl, fb, proj, view);
        }

        private static (GraphicsDevice gd, Swapchain sc) CreateDeviceAndSwapchain(
            Sdl2Window window,
            VRContext vrc,
            GraphicsBackend backend,
            GraphicsDeviceOptions gdo)
        {
            if (backend == GraphicsBackend.Vulkan)
            {
                (string[] instance, string[] device) = vrc.GetRequiredVulkanExtensions();
                VulkanDeviceOptions vdo = new VulkanDeviceOptions(instance, device);
                GraphicsDevice gd = GraphicsDevice.CreateVulkan(gdo, vdo);
                Swapchain sc = gd.ResourceFactory.CreateSwapchain(new SwapchainDescription(
                    VeldridStartup.GetSwapchainSource(window),
                    (uint)window.Width, (uint)window.Height,
                    gdo.SwapchainDepthFormat, gdo.SyncToVerticalBlank, true));
                return (gd, sc);
            }
            else
            {
                GraphicsDevice gd = VeldridStartup.CreateGraphicsDevice(window, gdo, backend);
                Swapchain sc = gd.MainSwapchain;
                return (gd, sc);
            }
        }
    }
}
