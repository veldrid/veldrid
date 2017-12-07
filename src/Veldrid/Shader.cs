using System;

namespace Veldrid
{
    /// <summary>
    /// A device resource encapsulating a single shader module.
    /// See <see cref="ShaderDescription"/>.
    /// </summary>
    public abstract class Shader : DeviceResource, IDisposable
    {
        internal Shader(ShaderStages stage)
        {
            Stage = stage;
        }

        /// <summary>
        /// The shader stage this instance can be used in.
        /// </summary>
        public ShaderStages Stage { get; }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public abstract void Dispose();
    }
}
