using System;

namespace Veldrid
{
    public struct ResourceLayoutDescription : IEquatable<ResourceLayoutDescription>
    {
        public ResourceLayoutElementDescription[] Elements;

        public ResourceLayoutDescription(params ResourceLayoutElementDescription[] elements)
        {
            Elements = elements;
        }

        public bool Equals(ResourceLayoutDescription other)
        {
            return Util.ArrayEqualsEquatable(Elements, other.Elements);
        }

        public override int GetHashCode()
        {
            return HashHelper.Array(Elements);
        }
    }
}
