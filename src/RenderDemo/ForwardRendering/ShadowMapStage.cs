using System;
using System.Numerics;
using Veldrid.Graphics;
using Veldrid.Graphics.Pipeline;

namespace Veldrid.RenderDemo.ForwardRendering
{
    public class ShadowMapStage : PipelineStage
    {
        private readonly RenderQueue _queue = new RenderQueue();

        private Framebuffer _shadowMapFramebuffer;

        public bool Enabled { get; set; }

        public string Name => "ShadowMap";

        public RenderContext RenderContext { get; private set; }

        public ShadowMapStage(RenderContext rc)
        {
            InitializeContextObjects(rc);
        }

        public void ChangeRenderContext(RenderContext rc)
        {
            RenderContext = rc;
            Dispose();
            InitializeContextObjects(rc);
        }

        private void InitializeContextObjects(RenderContext rc)
        {
            _shadowMapFramebuffer = rc.ResourceFactory.CreateFramebuffer(rc.Window.Width, rc.Window.Height);
        }

        public void ExecuteStage(VisibiltyManager visibilityManager)
        {
            RenderContext.SetFramebuffer(_shadowMapFramebuffer);
            _queue.Clear();
            visibilityManager.CollectVisibleObjects(_queue, "ShadowMap", Vector3.Zero, Vector3.Zero);
            _queue.Sort();
            foreach (RenderItem item in _queue)
            {
                item.Render(RenderContext, "ShadowMap");
            }
        }

        private void Dispose()
        {
            _shadowMapFramebuffer.Dispose();
        }
    }
}
