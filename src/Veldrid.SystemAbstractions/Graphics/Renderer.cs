using System.Collections.Generic;
using System.Numerics;
using Veldrid.Graphics.Pipeline;

namespace Veldrid.Graphics
{
    public class Renderer
    {
        private RenderContext _rc;
        private PipelineStage[] _stages;

        public RenderContext RenderContext
        {
            get { return _rc; }
        }

        public void SetRenderContext(RenderContext value)
        {
            _rc = value;
            foreach (var stage in _stages)
            {
                stage.ChangeRenderContext(_rc);
            }
        }

        public IReadOnlyList<PipelineStage> Stages => _stages;

        public Renderer(RenderContext rc, PipelineStage[] stages)
        {
            _stages = stages;
            SetRenderContext(rc);
        }

        public void RenderFrame(VisibiltyManager visibilityManager, Vector3 viewPosition)
        {
            _rc.SetDefaultFramebuffer();
            _rc.ClearBuffer();

            foreach (PipelineStage stage in _stages)
            {
                if (stage.Enabled)
                {
                     stage.ExecuteStage(visibilityManager, viewPosition);
                }
            }

            _rc.SwapBuffers();
        }
    }
}
