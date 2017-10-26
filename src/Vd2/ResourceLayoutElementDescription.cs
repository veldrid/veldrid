using System;

namespace Vd2
{
    public struct ResourceLayoutElementDescription : IEquatable<ResourceLayoutElementDescription>
    {
        public string Name;
        public ResourceKind Kind;
        public ShaderStages Stages;

        public ResourceLayoutElementDescription(string name, ResourceKind kind, ShaderStages stages)
        {
            Name = name;
            Kind = kind;
            Stages = stages;
        }

        public bool Equals(ResourceLayoutElementDescription other)
        {
            return Name.Equals(other.Name) && Kind == other.Kind && Stages == other.Stages;
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(Name.GetHashCode(), Kind.GetHashCode(), Stages.GetHashCode());
        }
    }
}
