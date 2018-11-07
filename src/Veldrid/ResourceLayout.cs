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
#if VALIDATE_USAGE
        internal readonly ResourceLayoutDescription Description;
#endif

        internal ResourceLayout(ref ResourceLayoutDescription description)
        {
#if VALIDATE_USAGE
            Description = description;
#endif
        }

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
