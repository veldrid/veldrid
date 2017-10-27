using System;

namespace Veldrid
{
    public struct CommandListDescription : IEquatable<CommandListDescription>
    {
        public bool Equals(CommandListDescription other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}