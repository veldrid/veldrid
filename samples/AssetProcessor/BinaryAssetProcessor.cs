using System.IO;
using System.Threading.Tasks;

namespace AssetProcessor
{
    public abstract class BinaryAssetProcessor
    {
        public abstract Task Process(Stream stream, string extension);
    }

    public abstract class BinaryAssetProcessor<T> : BinaryAssetProcessor
    {
        public override Task Process(Stream stream, string extension) => ProcessT(stream, extension);

        public abstract Task<T> ProcessT(Stream stream, string extension);
    }
}
