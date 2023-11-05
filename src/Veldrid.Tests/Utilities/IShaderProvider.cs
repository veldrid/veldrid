using System.IO;

namespace Veldrid.Tests.Utilities
{
    internal interface IShaderProvider
    {
        Stream OpenRead(string path);

        string GetPath(string name);

        byte[] ReadAllBytes(string path)
        {
            using Stream stream = OpenRead(path);
            using MemoryStream memoryStream = new(stream.CanSeek ? (int)stream.Length : 0);
            stream.CopyTo(memoryStream);
            if (memoryStream.Length == memoryStream.Capacity)
            {
                return memoryStream.GetBuffer();
            }
            return memoryStream.ToArray();
        }
    }
}
