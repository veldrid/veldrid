using System;

namespace Vd2
{
    public struct TextureViewDescription : IEquatable<TextureViewDescription>
    {
        public Texture Target;

        public TextureViewDescription(Texture target)
        {
            Target = target;
        }

        public bool Equals(TextureViewDescription other)
        {
            return Target.Equals(other.Target);
        }

        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }
    }
}