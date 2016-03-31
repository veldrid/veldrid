using System;
using System.Numerics;

namespace Veldrid.Graphics.Pipeline
{
    public class StandardPipelineStage : PipelineStage
    {
        private readonly RenderQueue _renderQueue = new RenderQueue();

        public string Name { get; }

        public bool Enabled { get; set; } = true;

        public RenderContext RenderContext { get; private set; }

        public StandardPipelineStage(string name)
        {
            Name = name;
        }

        public void ExecuteStage(VisibiltyManager visibilityManager)
        {
            RenderContext.SetDefaultFramebuffer();
            RenderContext.SetViewport(0, 0, RenderContext.Window.Width, RenderContext.Window.Height);
            _renderQueue.Clear();
            visibilityManager.CollectVisibleObjects(_renderQueue, Name, Vector3.Zero, Vector3.Zero);
            _renderQueue.Sort();

            foreach (RenderItem item in _renderQueue)
            {
                item.Render(RenderContext, Name);
            }
        }

        public void ChangeRenderContext(RenderContext rc)
        {
            RenderContext = rc;
        }
    }
}
