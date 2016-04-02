using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class ShadowMapPreview : RenderItem
    {
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Material _material;

        public Vector2 ScreenPosition { get; set; }
        public Vector2 Scale { get; set; }

        public ShadowMapPreview(RenderContext rc)
        {
            ChangeRenderContext(rc);
        }

        public void ChangeRenderContext(RenderContext context)
        {
            throw new NotImplementedException();
        }

        public RenderOrderKey GetRenderOrderKey()
        {
            return new RenderOrderKey();
        }

        public IEnumerable<string> GetStagesParticipated()
        {
            yield return "Overlay";
        }

        public void Render(RenderContext context, string pipelineStage)
        {
            throw new NotImplementedException();
        }
    }
}
