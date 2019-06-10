using System;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace CommandBufferDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                false,
                PixelFormat.R16_UNorm,
                false,
                ResourceBindingModel.Improved,
                true,
                true,
                true);
#if DEBUG
            options.Debug = true;
#endif
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(
                    Sdl2Native.SDL_WINDOWPOS_CENTERED, Sdl2Native.SDL_WINDOWPOS_CENTERED,
                    1280, 720,
                    WindowState.Normal,
                    "CommandBuffers"),
                options,
                GraphicsBackend.Vulkan,
                out Sdl2Window window,
                out GraphicsDevice gd);
            bool windowResized = false;
            window.Resized += () => windowResized = true;

            Swapchain sc = gd.MainSwapchain;

            Pipeline p = CreateQuadPipeline(gd.ResourceFactory, sc.Framebuffers[0].OutputDescription);

            PerFrame[] fr = CreateSwapchainResources(gd, sc, p);

            uint frameIndex = 0;

            frameIndex = gd.AcquireNextImage(sc, fr[0].ImageAcquired, null);
            while (window.Exists)
            {
                InputSnapshot input = window.PumpEvents();
                if (!window.Exists) { break; }
                if (windowResized)
                {
                    windowResized = false;
                    sc.Resize();
                    fr = CreateSwapchainResources(gd, sc, p);
                    frameIndex = gd.AcquireNextImage(sc, fr[0].ImageAcquired, null);
                }

                gd.WaitForFence(fr[frameIndex].Fence);
                gd.ResetFence(fr[frameIndex].Fence);
                gd.SubmitCommands(
                    fr[frameIndex].CB,
                    fr[frameIndex].ImageAcquired,
                    fr[frameIndex].RenderComplete,
                    fr[frameIndex].Fence);
                gd.Present(sc, fr[frameIndex].RenderComplete, frameIndex);
                uint nextFrame = (frameIndex + 1) % (uint)fr.Length;
                frameIndex = gd.AcquireNextImage(sc, fr[nextFrame].ImageAcquired, null);
            }
        }

        private static PerFrame[] CreateSwapchainResources(GraphicsDevice gd, Swapchain sc, Pipeline p)
        {
            PerFrame[] fr = new PerFrame[sc.BufferCount];
            for (uint i = 0; i < fr.Length; i++)
            {
                fr[i].CB = gd.ResourceFactory.CreateCommandBuffer(CommandBufferFlags.Reusable);
                fr[i].Fence = gd.ResourceFactory.CreateFence(signaled: true);
                fr[i].ImageAcquired = gd.ResourceFactory.CreateSemaphore();
                fr[i].RenderComplete = gd.ResourceFactory.CreateSemaphore();

                RenderEncoder rp = fr[i].CB.BeginRenderPass(sc.Framebuffers[i], LoadAction.Clear, StoreAction.Store, s_colors[i], 1.0f);
                rp.BindPipeline(p);
                rp.Draw(3);
                rp.End();
            }

            return fr;
        }

        private static Pipeline CreateQuadPipeline(ResourceFactory factory, OutputDescription outputs)
        {
            const string VS =
@"
#version 450
#extension GL_KHR_vulkan_glsl : enable

layout (location = 0) out vec4 fsin_color;

const vec2 QuadInfos[3] = 
{
    vec2(-0.75, -0.75),
    vec2(0, 0.75),
    vec2(0.75, -0.75),
};

const vec4 Colors[3] = 
{
    vec4(1, 0, 0, 1),
    vec4(0, 1, 0, 1),
    vec4(0, 0, 1, 1),
};

void main()
{
    gl_Position = vec4(QuadInfos[gl_VertexIndex], 0, 1);
    fsin_color = Colors[gl_VertexIndex];
}
";

            const string FS =
@"
#version 450
layout (location = 0) in vec4 fsin_color;
layout (location = 0) out vec4 fsout_color;
void main()
{
    fsout_color = fsin_color;
}
";
            return factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleStrip,
                new ShaderSetDescription(
                    Array.Empty<VertexLayoutDescription>(),
                    factory.CreateFromSpirv(
                        new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VS), "main"),
                        new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FS), "main"))),
                Array.Empty<ResourceLayout>(),
                outputs));
        }

        private static readonly RgbaFloat[] s_colors =
        {
            new RgbaFloat(1, 0, 0, 1),
            new RgbaFloat(1, 0.2f, 0, 1),
            new RgbaFloat(1, 0.3f, 0.2f, 1),
        };
    }

    public struct PerFrame
    {
        public CommandBuffer CB;
        public Semaphore ImageAcquired;
        public Semaphore RenderComplete;
        public Fence Fence;
    }
}
