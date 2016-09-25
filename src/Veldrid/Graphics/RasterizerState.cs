using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device object controlling behavior of the rasterizer.
    /// </summary>
    public interface RasterizerState : IDisposable
    {
        /// <summary>
        /// Constrols how primitive faces are culled.
        /// </summary>
        FaceCullingMode CullMode { get; }

        /// <summary>
        /// Controls how triangles are filled by the rasterizer.
        /// </summary>
        TriangleFillMode FillMode { get; }
        
        /// <summary>
        /// Controls whether or not the rasterizer clips fragments by depth.
        /// </summary>
        bool IsDepthClipEnabled { get; }

        /// <summary>
        /// Controls whether or not the rasterizer clips fragments with the scissor test.
        /// See <see cref="RenderContext.SetScissorRectangle(int, int, int, int)"/> also.
        /// </summary>
        bool IsScissorTestEnabled { get; }
    }
}
