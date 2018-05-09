using System;
using System.Diagnostics;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to bind a particular set of <see cref="BindableResource"/> objects to a <see cref="CommandList"/>.
    /// See <see cref="ResourceSetDescription"/>.
    /// </summary>
    public abstract class ResourceSet : DeviceResource, IDisposable
    {
        internal ResourceSet(ref ResourceSetDescription description)
        {
#if VALIDATE_USAGE
            Layout = description.Layout;

            ResourceKind[] kinds = description.Layout.ResourceKinds;
            BindableResource[] resources = description.BoundResources;

            if (kinds.Length != resources.Length)
            {
                throw new VeldridException(
                    $"The number of resources specified ({resources.Length}) must be equal to the number of resources in the {nameof(ResourceLayout)} ({kinds.Length}).");
            }

            for (uint i = 0; i < kinds.Length; i++)
            {
                ValidateResourceKind(kinds[i], resources[i], i);
            }
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

#if VALIDATE_USAGE
        internal ResourceLayout Layout { get; }

        private void ValidateResourceKind(ResourceKind kind, BindableResource resource, uint slot)
        {
            switch (kind)
            {
                case ResourceKind.UniformBuffer:
                    {
                        if (!(resource is DeviceBuffer b && (b.Usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer))
                        {
                            throw new VeldridException(
                                $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the {nameof(ResourceLayout)}. It must be a {nameof(DeviceBuffer)} with {nameof(BufferUsage)}.{nameof(BufferUsage.UniformBuffer)}.");
                        }
                        break;
                    }
                case ResourceKind.StructuredBufferReadOnly:
                    {
                        if (!(resource is DeviceBuffer b
                            && (b.Usage & (BufferUsage.StructuredBufferReadOnly | BufferUsage.StructuredBufferReadWrite)) != 0))
                        {
                            throw new VeldridException(
                                $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the {nameof(ResourceLayout)}. It must be a {nameof(DeviceBuffer)} with {nameof(BufferUsage)}.{nameof(BufferUsage.StructuredBufferReadOnly)}.");
                        }
                        break;
                    }
                case ResourceKind.StructuredBufferReadWrite:
                    {
                        if (!(resource is DeviceBuffer b && (b.Usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite))
                        {
                            throw new VeldridException(
                                $"Resource in slot {slot} does not match {nameof(ResourceKind)} specified in the {nameof(ResourceLayout)}. It must be a {nameof(DeviceBuffer)} with {nameof(BufferUsage)}.{nameof(BufferUsage.StructuredBufferReadWrite)}.");
                        }
                        break;
                    }
                case ResourceKind.TextureReadOnly:
                    {
                        if (!(resource is TextureView tv && (tv.Target.Usage & TextureUsage.Sampled) == TextureUsage.Sampled))
                        {
                            throw new VeldridException(
                                $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the {nameof(ResourceLayout)}. It must be a {nameof(TextureView)} whose target has {nameof(TextureUsage)}.{nameof(TextureUsage.Sampled)}.");
                        }
                        break;
                    }
                case ResourceKind.TextureReadWrite:
                    {
                        if (!(resource is TextureView tv && (tv.Target.Usage & TextureUsage.Storage) == TextureUsage.Storage))
                        {
                            throw new VeldridException(
                                $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the {nameof(ResourceLayout)}. It must be a {nameof(TextureView)} whose target has {nameof(TextureUsage)}.{nameof(TextureUsage.Storage)}.");
                        }
                        break;
                    }
                case ResourceKind.Sampler:
                    {
                        if (!(resource is Sampler s))
                        {
                            throw new VeldridException(
                                $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the {nameof(ResourceLayout)}. It must be a {nameof(Sampler)}.");
                        }
                        break;
                    }
                default:
                    Debug.Fail($"Unexpected ResourceKind: {kind}.");
                    break;
            }
        }
#endif
    }
}
