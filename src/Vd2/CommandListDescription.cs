using System;

namespace Vd2
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