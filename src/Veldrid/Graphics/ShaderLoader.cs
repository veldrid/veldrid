using System.IO;

namespace Veldrid.Graphics
{
    public interface ShaderLoader
    {
        bool TryOpenShader(string name, GraphicsBackend backend, out Stream dataStream);
    }
}
