namespace Vd2
{
    public struct IndexBufferDescription
    {
        public ulong SizeInBytes;
        public IndexFormat Format;

        public IndexBufferDescription(ulong sizeInBytes, IndexFormat format)
        {
            SizeInBytes = sizeInBytes;
            Format = format;
        }
    }
}
