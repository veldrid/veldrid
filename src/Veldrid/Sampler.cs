using System;

namespace Veldrid
{
    /// <summary>
    /// A bindable device resource which controls how texture values are sampled within a shader.
    /// See <see cref="SamplerDescription"/>.
    /// </summary>
    public abstract class Sampler : DeviceResource, BindableResource, IDisposable
    {
        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
