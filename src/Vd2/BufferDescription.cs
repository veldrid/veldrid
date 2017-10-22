namespace Vd2
{
    public struct BufferDescription
    {
        public ulong SizeInBytes;
        public bool Dynamic;

        public BufferDescription(ulong sizeInBytes)
        {
            SizeInBytes = sizeInBytes;
            Dynamic = false;
        }

        public BufferDescription(ulong sizeInBytes, bool dynamic)
        {
            SizeInBytes = sizeInBytes;
            Dynamic = dynamic;
        }
    }
}
