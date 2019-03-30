using System.IO;

namespace AssetPrimitives
{
    public class ByteArraySerializer : BinaryAssetSerializer<byte[]>
    {
        public override byte[] ReadT(BinaryReader reader)
        {
            return reader.ReadByteArray();
        }

        public override void WriteT(BinaryWriter writer, byte[] value)
        {
            writer.WriteByteArray(value);
        }
    }
}
