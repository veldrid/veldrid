using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A <see cref="ShaderLoader"/> which can load <see cref="Shader"/> objects from files embedded in <see cref="Assembly"/> resources.
    /// </summary>
    public class EmbeddedResourceShaderLoader : ShaderLoader
    {
        private readonly Assembly _assembly;
        private readonly Dictionary<string, string> _shaderToManifestNames = new Dictionary<string, string>();

        /// <summary>
        /// Constructs a new <see cref="EmbeddedResourceShaderLoader"/> which loads <see cref="Shader"/> objects from the given <see cref="Assembly"/>
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to load resources from.</param>
        public EmbeddedResourceShaderLoader(Assembly assembly)
        {
            _assembly = assembly;
            foreach (string name in assembly.GetManifestResourceNames())
            {
                _shaderToManifestNames.Add(GetFinalPortion(name), name);
            }
        }

        public bool TryOpenShader(string name, GraphicsBackend backend, out Stream dataStream)
        {
            string extension = backend == GraphicsBackend.Direct3D11 ? "hlsl" : "glsl";
            string namePlusExtension = name + "." + extension;
            if (_shaderToManifestNames.TryGetValue(namePlusExtension, out string manifestName))
            {
                dataStream = _assembly.GetManifestResourceStream(manifestName);
                return true;
            }

            dataStream = null;
            return false;
        }

        private string GetFinalPortion(string shaderName)
        {
            var sections = shaderName.Split('.');
            return sections[sections.Length - 2] + "." + sections[sections.Length - 1];
        }
    }
}
