using System.IO;

namespace Veldrid.Graphics
{
    public interface ShaderLoader
    {
        bool TryOpenShader(string name, string extension, out Stream dataStream);
    }
}
