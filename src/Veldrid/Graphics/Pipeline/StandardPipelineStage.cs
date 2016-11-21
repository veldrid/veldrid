using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Graphics.Pipeline
{
    public class StandardPipelineStage : PipelineStage
    {
        private readonly RenderQueue _renderQueue = new RenderQueue();

        public string Name { get; }

        public bool Enabled { get; set; } = true;

        public RenderContext RenderContext { get; private set; }

        public Framebuffer OverrideFramebuffer { get; set; }

        public BoundingFrustum CameraFrustum
        {
            get { return _cameraFrustum; }
            set { _cameraFrustum = value; }
        }

        public Comparer<RenderItemIndex> Comparer { get; set; }

        private BoundingFrustum _cameraFrustum;

        public StandardPipelineStage(RenderContext rc, string name, Framebuffer framebuffer = null)
        {
            RenderContext = rc;
            Name = name;
            OverrideFramebuffer = framebuffer;
        }

        public void ExecuteStage(VisibiltyManager visibilityManager, Vector3 viewPosition)
        {
            if (OverrideFramebuffer == null)
            {
                RenderContext.SetDefaultFramebuffer();
            }
            else
            {
                RenderContext.SetFramebuffer(OverrideFramebuffer);
            }
            RenderContext.SetViewport(0, 0, RenderContext.CurrentFramebuffer.Width, RenderContext.CurrentFramebuffer.Height);
            _renderQueue.Clear();
            visibilityManager.CollectVisibleObjects(_renderQueue, Name, ref _cameraFrustum, viewPosition);
            if (Comparer != null)
            {
                _renderQueue.Sort(Comparer);
            }
            else
            {
                _renderQueue.Sort();
            }

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
