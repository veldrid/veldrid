using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource which describes the layout and kind of <see cref="BindableResource"/> objects available
    /// to a shader set.
    /// See <see cref="ResourceLayoutDescription"/>.
    /// </summary>
    public abstract class ResourceLayout : DeviceResource, IDisposable
    {
        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
