namespace Veldrid.MetalBindings
{
    public struct Bool8
    {
        public readonly byte Value;

        public Bool8(byte value)
        {
            Value = value;
        }

        public Bool8(bool value)
        {
            Value = value ? (byte)1 : (byte)0;
        }

        public static implicit operator bool(Bool8 b) => b.Value != 0;
        public static implicit operator byte(Bool8 b) => b.Value;
        public static implicit operator Bool8(bool b) => new Bool8(b);
    }
}