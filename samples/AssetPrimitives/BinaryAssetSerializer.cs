using System.IO;

namespace AssetPrimitives
{
    public abstract class BinaryAssetSerializer
    {
        public abstract object Read(BinaryReader reader);
        public abstract void Write(BinaryWriter writer, object value);
    }

    public abstract class BinaryAssetSerializer<T> : BinaryAssetSerializer
    {
        public override void Write(BinaryWriter writer, object value) => WriteT(writer, (T)value);
        public override object Read(BinaryReader reader) => ReadT(reader);

        public abstract T ReadT(BinaryReader reader);
        public abstract void WriteT(BinaryWriter writer, T value);
    }
}
