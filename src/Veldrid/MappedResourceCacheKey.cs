using System;

namespace Veldrid
{
    internal readonly struct MappedResourceCacheKey : IEquatable<MappedResourceCacheKey>
    {
        public readonly MappableResource Resource;
        public readonly uint Subresource;

        public MappedResourceCacheKey(MappableResource resource, uint subresource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Subresource = subresource;
        }

        public bool Equals(MappedResourceCacheKey other)
        {
            return Resource.Equals(other.Resource)
                && Subresource.Equals(other.Subresource);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(Resource.GetHashCode(), Subresource.GetHashCode());
        }
    }
}
