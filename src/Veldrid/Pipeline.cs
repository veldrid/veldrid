using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource encapsulating all state in a graphics pipeline. Used in 
    /// <see cref="CommandList.SetPipeline(Pipeline)"/> to prepare a <see cref="CommandList"/> for draw commands.
    /// See <see cref="GraphicsPipelineDescription"/>.
    /// </summary>
    public abstract class Pipeline : DeviceResource, IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance represents a compute Pipeline.
        /// If false, this instance is a graphics pipeline.
        /// </summary>
        public abstract bool IsComputePipeline { get; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
