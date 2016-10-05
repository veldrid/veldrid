namespace Veldrid.Graphics
{
    internal static class FormatHelpers
    {
        public static int GetIndexFormatElementByteSize(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt32:
                    return 4;
                case IndexFormat.UInt16:
                    return 2;
                case IndexFormat.UInt8:
                    return 1;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }
    }
}
