using System;
using System.Diagnostics;
using System.Numerics;
using Veldrid.Sdl2;

namespace Veldrid.NeoDemo
{
    public class ImGuiRenderable : Renderable, IUpdateable
    {
        private ImGuiCBRenderer _imguiRenderer;
        private int _width;
        private int _height;
        private readonly uint _bufferCount;

        public ImGuiRenderable(int width, int height, uint bufferCount)
        {
            _width = width;
            _height = height;
            _bufferCount = bufferCount;
        }

        public void WindowResized(int width, int height) => _imguiRenderer.WindowResized(width, height);

        public override void CreateDeviceObjects(GraphicsDevice gd, SceneContext sc)
        {
            if (_imguiRenderer == null)
            {
                _imguiRenderer = new ImGuiCBRenderer(gd, sc.MainSceneFramebuffer.OutputDescription, _width, _height, ColorSpaceHandling.Linear, _bufferCount);
            }
            else
            {
                _imguiRenderer.CreateDeviceResources(gd, sc.MainSceneFramebuffer.OutputDescription, ColorSpaceHandling.Linear);
            }
        }

        public override void DestroyDeviceObjects()
        {
            _imguiRenderer.Dispose();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return new RenderOrderKey(ulong.MaxValue);
        }

        public override void Render(GraphicsDevice gd, CommandBuffer cb, SceneContext sc, RenderPasses renderPass)
        {
            Debug.Assert(renderPass == RenderPasses.Overlay);
            _imguiRenderer.Render(gd, cb);
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandBuffer cl, SceneContext sc)
        {
        }

        public override RenderPasses RenderPasses => RenderPasses.Overlay;

        public void Update(float deltaSeconds)
        {
            _imguiRenderer.Update(deltaSeconds, InputTracker.FrameSnapshot);
        }
    }
}
