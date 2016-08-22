using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Veldrid.Graphics
{
    public class EmbeddedResourceShaderLoader : ShaderLoader
    {
        private readonly Assembly _assembly;
        private readonly Dictionary<string, string> _shaderToManifestNames = new Dictionary<string, string>();

        public EmbeddedResourceShaderLoader(Assembly assembly)
        {
            _assembly = assembly;
            foreach (var name in assembly.GetManifestResourceNames())
            {
                _shaderToManifestNames.Add(GetFinalPortion(name), name);
            }
        }

        public bool TryOpenShader(string name, string extension, out Stream dataStream)
        {
            string namePlusExtension = name + "." + extension;
            string manifestName;
            if (_shaderToManifestNames.TryGetValue(namePlusExtension, out manifestName))
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
