using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to bind a particular set of <see cref="BindableResource"/> objects to a <see cref="CommandList"/>.
    /// See <see cref="ResourceSetDescription"/>.
    /// </summary>
    public abstract class ResourceSet : DeviceResource, IDisposable
    {
        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
