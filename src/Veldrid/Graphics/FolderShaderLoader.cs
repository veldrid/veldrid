using System.IO;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A simple <see cref="ShaderLoader"/> which loads from a single folder.
    /// </summary>
    public class FolderShaderLoader : ShaderLoader
    {
        private readonly string _basePath;

        /// <summary>
        /// Constructs a new <see cref="FolderShaderLoader"/> which loads from the given path.
        /// </summary>
        /// <param name="basePath"></param>
        public FolderShaderLoader(string basePath)
        {
            _basePath = basePath;
        }

        public bool TryOpenShader(string name, string extension, out Stream dataStream)
        {
            string path = GetFullPath(name, extension);
            if (File.Exists(path))
            {
                dataStream = File.OpenRead(path);
                return true;
            }
            else
            {
                dataStream = null;
                return false;
            }
        }

        private string GetFullPath(string name, string extension)
        {
            return Path.Combine(_basePath, name + "." + extension);
        }
    }
}
