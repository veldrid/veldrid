using System;

namespace Veldrid
{
    /// <summary>
    /// Describes an individual resource element in a <see cref="ResourceLayout"/>.
    /// </summary>
    public struct ResourceLayoutElementDescription : IEquatable<ResourceLayoutElementDescription>
    {
        /// <summary>
        /// The name of the element.
        /// </summary>
        public string Name;
        /// <summary>
        /// The kind of resource.
        /// </summary>
        public ResourceKind Kind;
        /// <summary>
        /// The <see cref="ShaderStages"/> in which this element is used.
        /// </summary>
        public ShaderStages Stages;

        /// <summary>
        /// Constructs a new ResourceLayoutElementDescription.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="kind">The kind of resource.</param>
        /// <param name="stages">The <see cref="ShaderStages"/> in which this element is used.</param>
        public ResourceLayoutElementDescription(string name, ResourceKind kind, ShaderStages stages)
        {
            Name = name;
            Kind = kind;
            Stages = stages;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(ResourceLayoutElementDescription other)
        {
            return Name.Equals(other.Name) && Kind == other.Kind && Stages == other.Stages;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(Name.GetHashCode(), (int)Kind, (int)Stages);
        }
    }
}
