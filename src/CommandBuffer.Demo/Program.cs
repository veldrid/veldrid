using System;
using System.Numerics;
using System.Runtime.CompilerServices;
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
            WindowCreateInfo windowCI = new WindowCreateInfo
            {
                X = Sdl2Native.SDL_WINDOWPOS_CENTERED,
                Y = Sdl2Native.SDL_WINDOWPOS_CENTERED,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowInitialState = WindowState.Normal,
                WindowTitle = "Veldrid NeoDemo"
            };
            Sdl2Window window = VeldridStartup.CreateWindow(windowCI);

            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(false, null, false, ResourceBindingModel.Improved, true, true, true);
#if DEBUG
            gdOptions.Debug = true;
#endif
            GraphicsDevice gd = GraphicsDevice.Create(gdOptions, GraphicsBackend.Direct3D11);
            SwapchainSource ss = VeldridStartup.GetSwapchainSource(window);
            Swapchain sc = gd.ResourceFactory.CreateSwapchain(
                new SwapchainDescription(ss, (uint)window.Width, (uint)window.Height, PixelFormat.R16_UNorm, false, true));
            bool windowResized = false;
            window.Resized += () => windowResized = true;

            (Pipeline p, ResourceLayout layout) = CreateQuadPipeline(gd.ResourceFactory, sc.Framebuffers[0].OutputDescription);
            uint bufferSpace = Math.Max((uint)Unsafe.SizeOf<Vector4>(), gd.UniformBufferMinOffsetAlignment);
            DeviceBuffer offsetsBuffer = gd.ResourceFactory.CreateBuffer(
                bufferSpace * sc.BufferCount, BufferUsage.UniformBuffer | BufferUsage.Dynamic);
            ResourceSet rs = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout,
                new DeviceBufferRange(offsetsBuffer, 0, 16)));

            StandardFrameLoop loop = new StandardFrameLoop(gd, sc);
            while (window.Exists)
            {
                InputSnapshot input = window.PumpEvents();
                if (!window.Exists) { break; }
                if (windowResized)
                {
                    windowResized = false;
                    loop.ResizeSwapchain((uint)window.Width, (uint)window.Height);
                }

                loop.RunFrame((CommandBuffer cb, uint frameIndex, Framebuffer fb) =>
                {
                    float t = Environment.TickCount / 2000.0f;
                    Vector2 offsets = new Vector2(MathF.Cos(t), MathF.Sin(t)) * 0.3f;
                    uint frameOffset = frameIndex * bufferSpace;
                    offsetsBuffer.Update(frameOffset, new Vector4(offsets, 0, 0));

                    cb.BeginRenderPass(fb, LoadAction.Clear, StoreAction.Store, s_colors[frameIndex], 1.0f);
                    cb.BindPipeline(p);
                    Span<uint> bindOffsets = stackalloc uint[1];
                    bindOffsets[0] = frameOffset;
                    cb.BindGraphicsResourceSet(0, rs, bindOffsets);
                    cb.Draw(3);
                    cb.EndRenderPass();
                });
            }
        }

        private static (Pipeline, ResourceLayout) CreateQuadPipeline(ResourceFactory factory, OutputDescription outputs)
        {
            const string VS =
@"
#version 450
#extension GL_KHR_vulkan_glsl : enable

layout (location = 0) out vec4 fsin_color;

layout (set = 0, binding = 0) uniform OffsetInfo
{
    vec2 offsets;
    vec2 _padding0;
};

const vec2 QuadInfos[3] =
{
    vec2(-0.35, -0.35),
    vec2(0, 0.35),
    vec2(0.35, -0.35),
};

const vec4 Colors[3] = 
{
    vec4(1, 0, 0, 1),
    vec4(0, 1, 0, 1),
    vec4(0, 0, 1, 1),
};

void main()
{
    gl_Position = vec4(QuadInfos[gl_VertexIndex] + offsets, 0, 1);
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
            ResourceLayout layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("OffsetsInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex, ResourceLayoutElementOptions.DynamicBinding)));

            Pipeline pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleStrip,
                new ShaderSetDescription(
                    Array.Empty<VertexLayoutDescription>(),
                    factory.CreateFromSpirv(
                        new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VS), "main"),
                        new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FS), "main"))),
                layout,
                outputs));
            return (pipeline, layout);
        }

        private static readonly RgbaFloat[] s_colors =
        {
            new RgbaFloat(0, 0, 0.03f, 1),
            new RgbaFloat(0, 0, 0.03f, 1),
            new RgbaFloat(0, 0, 0.03f, 1),
        };
    }
}
