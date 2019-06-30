using System;

namespace Veldrid
{
    public struct CommandBufferDescription : IEquatable<CommandBufferDescription>
    {
        public CommandBufferFlags Flags;

        public CommandBufferDescription(CommandBufferFlags flags)
        {
            Flags = flags;
        }

        public bool Equals(CommandBufferDescription other)
        {
            return Flags == other.Flags;
        }
    }

    [Flags]
    public enum CommandBufferFlags
    {
        None,
        Reusable = 1,
    }
}
