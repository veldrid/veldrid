using System.IO;
using System.Threading.Tasks;

namespace AssetProcessor
{
    public class KtxFileProcessor : BinaryAssetProcessor<byte[]>
    {
        public override async Task<byte[]> ProcessT(Stream stream, string extension)
        {
            MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
