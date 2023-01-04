using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Veldrid.SampleGallery
{
    public class Gallery
    {
        private IGalleryDriver _driver;
        private GraphicsDevice _gd;
        private Swapchain _mainSwapchain;
        private ImGuiCBRenderer _imguiRenderer;
        private Example _example;
        private TextureBlitter _blitter;
        private CommandBuffer[] _cbs = Array.Empty<CommandBuffer>();
        private CommandBuffer[] _postProcessCBs;
        private string _loadedExampleName;
        private readonly Dictionary<string, Func<Example>> _availableExamples
            = new Dictionary<string, Func<Example>>();

        public Gallery(IGalleryDriver driver)
        {
            _driver = driver;
            _gd = driver.Device;
            _mainSwapchain = _driver.MainSwapchain;

            _driver.Resized += () =>
            {
                if (_driver.SupportsImGui)
                {
                    _imguiRenderer.WindowResized((int)_driver.Width, (int)_driver.Height);
                }
            };

            _driver.Update += Update;
            _driver.Render += Render;

            if (_driver.SupportsImGui)
            {
                _imguiRenderer = new ImGuiCBRenderer(
                    _gd,
                    _mainSwapchain.Framebuffer.OutputDescription,
                    (int)_driver.Width, (int)_driver.Height,
                    ColorSpaceHandling.Linear,
                    _mainSwapchain.BufferCount);
            }

            PixelFormat format = _mainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
            bool srgbOutput = format == PixelFormat.B8_G8_R8_A8_UNorm_SRgb || format == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
            _blitter = new TextureBlitter(_gd, _gd.ResourceFactory, _mainSwapchain.Framebuffer.OutputDescription, srgbOutput);
            GalleryConfig.Global.BlitterLayout = _blitter.ResourceLayout;

            _postProcessCBs = new CommandBuffer[_mainSwapchain.BufferCount];
            for (uint i = 0; i < _mainSwapchain.BufferCount; i++)
            {
                _postProcessCBs[i] = _gd.ResourceFactory.CreateCommandBuffer();
                _postProcessCBs[i].Name = $"Gallery Post Process Frame {i}";
            }
        }

        public void RegisterExample(string name, Func<Example> load)
            => _availableExamples.Add(name, load);

        public void LoadExample(string name)
        {
            Console.WriteLine($"Loading example \"{name}\".");
            if (_loadedExampleName != name
                && _availableExamples.TryGetValue(name, out Func<Example> load))
            {
                if (_example != null)
                {
                    _example.Shutdown();
                }

                _example = load();
                _example.Initialize(_driver);
                _example.LoadResourcesAsync().Wait();
                _loadedExampleName = name;
            }
            else
            {
                // TODO: Show error message
            }
        }

        private void Update(double deltaSeconds)
        {
            InputTracker.UpdateFrameInput(_driver.GetInputState());
            if (_driver.SupportsImGui)
            {
                _imguiRenderer.Update((float)deltaSeconds, _driver.GetInputState());
                InputTracker.WantCaptureMouse = ImGui.GetIO().WantCaptureMouse;
                InputTracker.WantCaptureKeyboard = ImGui.GetIO().WantCaptureKeyboard;

                string exampleName = null;
                if (ImGui.BeginMainMenuBar())
                {
                    if (ImGui.BeginMenu("Load"))
                    {
                        foreach (KeyValuePair<string, Func<Example>> kvp in _availableExamples)
                        {
                            if (_loadedExampleName == kvp.Key)
                            {
                                ImGui.PushStyleColor(ImGuiCol.Text, RgbaFloat.Cyan.ToVector4());
                            }
                            if (ImGui.MenuItem(kvp.Key))
                            {
                                exampleName = kvp.Key;
                            }
                            if (_loadedExampleName == kvp.Key)
                            {
                                ImGui.PopStyleColor();
                            }
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Settings"))
                    {
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Debug"))
                    {
                        ImGui.Button($"Mouse pos: {ImGui.GetIO().MousePos}");
                        ImGui.Button($"Framerate: {ImGui.GetIO().Framerate}");
                        ImGui.Button($"Backend: {_driver.Device.BackendType}");
                        ImGui.EndMenu();
                    }

                    _driver.DrawMainMenuBars();
                    _example.DrawMainMenuBars();

                    ImGui.EndMainMenuBar();
                }

                if (exampleName != null)
                {
                    LoadExample(exampleName);
                }
            }
        }

        private CommandBuffer[] Render(double deltaSeconds)
        {
            CommandBuffer[] exampleCBs = _example?.Render(deltaSeconds); // _example.Framebuffer now contains output.
            Util.EnsureArraySize(ref _cbs, exampleCBs.Length + 1);
            Array.Copy(exampleCBs, _cbs, exampleCBs.Length);
            _cbs[_cbs.Length - 1] = _postProcessCBs[_driver.FrameIndex];

            CommandBuffer cb = _cbs[_cbs.Length - 1];
            cb.MemoryBarrier(
                _example.Framebuffers[_driver.FrameIndex].ColorTargets[0].Target, 0, 1, 0, 1,
                ShaderStages.Fragment,
                ShaderStages.Fragment);

            cb.BeginRenderPass(
                _mainSwapchain.Framebuffers[_driver.FrameIndex],
                LoadAction.Clear,
                StoreAction.Store,
                RgbaFloat.Pink,
                1f);
            _blitter.Render(cb, _example.BlitterSets[_driver.FrameIndex], Vector2.Zero, Vector2.One);
            if (_driver.SupportsImGui)
            {
                _imguiRenderer.Render(_gd, cb);
            }
            cb.EndRenderPass();

            return _cbs;
        }

        public static GraphicsDeviceOptions GetPreferredOptions()
        {
#if DEBUG
            bool isDebugBuild = true;
#else
            bool isDebugBuild = false;
#endif

            return new GraphicsDeviceOptions(
                debug: isDebugBuild,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: true,
                enableCommandBuffers: true);

        }
    }
}
