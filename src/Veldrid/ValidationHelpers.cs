using System.Diagnostics;

namespace Veldrid
{
    internal static class ValidationHelpers
    {
        [Conditional("VALIDATE_USAGE")]
        internal static void ValidateResourceSet(GraphicsDevice gd, ref ResourceSetDescription description)
        {
#if VALIDATE_USAGE
            ResourceLayoutElementDescription[] elements = description.Layout.Description.Elements;
            BindableResource[] resources = description.BoundResources;

            if (elements.Length != resources.Length)
            {
                throw new VeldridException(
                    $"The number of resources specified ({resources.Length}) must be equal to the number of resources in the {nameof(ResourceLayout)} ({elements.Length}).");
            }

            for (uint i = 0; i < elements.Length; i++)
            {
                ValidateResourceKind(elements[i].Kind, resources[i], i);
            }

            for (int i = 0; i < description.Layout.Description.Elements.Length; i++)
            {
                ResourceLayoutElementDescription element = description.Layout.Description.Elements[i];
                if (element.Kind == ResourceKind.UniformBuffer
                    || element.Kind == ResourceKind.StructuredBufferReadOnly
                    || element.Kind == ResourceKind.StructuredBufferReadWrite)
                {
                    DeviceBufferRange range = Util.GetBufferRange(description.BoundResources[i], 0);

                    if (!gd.Features.BufferRangeBinding && (range.Offset != 0 || range.SizeInBytes != range.Buffer.SizeInBytes))
                    {
                        throw new VeldridException($"The {nameof(DeviceBufferRange)} in slot {i} uses a non-zero offset or less-than-full size, " +
                            $"which requires {nameof(GraphicsDeviceFeatures)}.{nameof(GraphicsDeviceFeatures.BufferRangeBinding)}.");
                    }

                    uint alignment = element.Kind == ResourceKind.UniformBuffer
                       ? gd.UniformBufferMinOffsetAlignment
                       : gd.StructuredBufferMinOffsetAlignment;

                    if ((range.Offset % alignment) != 0)
                    {
                       throw new VeldridException($"The {nameof(DeviceBufferRange)} in slot {i} has an invalid offset: {range.Offset}. " +
                           $"The offset for this buffer must be a multiple of {alignment}.");
                    }
                }
            }
#endif
        }

        [Conditional("VALIDATE_USAGE")]
        private static void ValidateResourceKind(ResourceKind kind, BindableResource resource, uint slot)
        {
            switch (kind)
            {
                case ResourceKind.UniformBuffer:
                {
                    if (!Util.GetDeviceBuffer(resource, out DeviceBuffer b)
                        || (b.Usage & BufferUsage.UniformBuffer) == 0)
                    {
                        throw new VeldridException(
                            $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the {nameof(ResourceLayout)}. " +
                            $"It must be a {nameof(DeviceBuffer)} or {nameof(DeviceBufferRange)} with {nameof(BufferUsage)}.{nameof(BufferUsage.UniformBuffer)}.");
                    }
                    break;
                }
                case ResourceKind.StructuredBufferReadOnly:
                {
                    if (!Util.GetDeviceBuffer(resource, out DeviceBuffer b)
                        || (b.Usage & (BufferUsage.StructuredBufferReadOnly | BufferUsage.StructuredBufferReadWrite)) == 0)
                    {
                        throw new VeldridException(
                            $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the {nameof(ResourceLayout)}. It must be a {nameof(DeviceBuffer)} with {nameof(BufferUsage)}.{nameof(BufferUsage.StructuredBufferReadOnly)}.");
                    }
                    break;
                }
                case ResourceKind.StructuredBufferReadWrite:
                {
                    if (!Util.GetDeviceBuffer(resource, out DeviceBuffer b)
                        || (b.Usage & BufferUsage.StructuredBufferReadWrite) == 0)
                    {
                        throw new VeldridException(
                            $"Resource in slot {slot} does not match {nameof(ResourceKind)} specified in the {nameof(ResourceLayout)}. It must be a {nameof(DeviceBuffer)} with {nameof(BufferUsage)}.{nameof(BufferUsage.StructuredBufferReadWrite)}.");
                    }
                    break;
                }
                case ResourceKind.TextureReadOnly:
                {
                    if (!(resource is TextureView tv && (tv.Target.Usage & TextureUsage.Sampled) != 0)
                        && !(resource is Texture t && (t.Usage & TextureUsage.Sampled) != 0))
                    {
                        throw new VeldridException(
                            $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the " +
                            $"{nameof(ResourceLayout)}. It must be a {nameof(Texture)} or {nameof(TextureView)} whose target " +
                            $"has {nameof(TextureUsage)}.{nameof(TextureUsage.Sampled)}.");
                    }
                    break;
                }
                case ResourceKind.TextureReadWrite:
                {
                    if (!(resource is TextureView tv && (tv.Target.Usage & TextureUsage.Storage) != 0)
                        && !(resource is Texture t && (t.Usage & TextureUsage.Storage) != 0))
                    {
                        throw new VeldridException(
                            $"Resource in slot {slot} does not match {nameof(ResourceKind)}.{kind} specified in the " +
                            $"{nameof(ResourceLayout)}. It must be a {nameof(Texture)} or {nameof(TextureView)} whose target " +
                            $"has {nameof(TextureUsage)}.{nameof(TextureUsage.Storage)}.");
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
                    throw Illegal.Value<ResourceKind>();
            }
        }
    }
}
