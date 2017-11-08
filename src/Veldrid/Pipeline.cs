using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource encapsulating all state in a graphics pipeline. Used in 
    /// <see cref="CommandList.SetPipeline(Pipeline)"/> to prepare a <see cref="CommandList"/> for draw commands.
    /// See <see cref="PipelineDescription"/>.
    /// </summary>
    public abstract class Pipeline : DeviceResource, IDisposable
    {
        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
