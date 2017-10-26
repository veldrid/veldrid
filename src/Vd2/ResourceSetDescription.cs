using System;

namespace Vd2
{
    public struct ResourceSetDescription : IEquatable<ResourceSetDescription>
    {
        public ResourceLayout Layout;
        public BindableResource[] BoundResources;

        public ResourceSetDescription(ResourceLayout layout, params BindableResource[] boundResources)
        {
            Layout = layout;
            BoundResources = boundResources;
        }

        public bool Equals(ResourceSetDescription other)
        {
            return Layout.Equals(other.Layout) && Util.ArrayEquals(BoundResources, other.BoundResources);
        }
    }
}