namespace Vd2
{
    public struct IndexBufferDescription
    {
        public ulong SizeInBytes;
        public IndexFormat Format;
        public bool Dynamic;

        public IndexBufferDescription(ulong sizeInBytes, IndexFormat format)
        {
            SizeInBytes = sizeInBytes;
            Format = format;
            Dynamic = false;
        }

        public IndexBufferDescription(ulong sizeInBytes, IndexFormat format, bool dynamic)
        {
            SizeInBytes = sizeInBytes;
            Format = format;
            Dynamic = dynamic;
        }
    }
}
