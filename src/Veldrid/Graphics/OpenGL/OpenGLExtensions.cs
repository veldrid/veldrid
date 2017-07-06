using System.Collections.Generic;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLExtensions
    {
        private readonly HashSet<string> _extensions;

        public OpenGLExtensions(HashSet<string> extensions)
        {
            _extensions = extensions;

            ARB_DirectStateAccess = IsExtensionSupported("GL_ARB_direct_state_access");
            ARB_MultiBind = IsExtensionSupported("GL_ARB_multi_bind");
        }

        public readonly bool ARB_DirectStateAccess;
        public readonly bool ARB_MultiBind;

        /// <summary>
        /// Returns a value indicating whether the given extension is supported.
        /// </summary>
        /// <param name="extension">The name of the extensions. For example, "</param>
        /// <returns></returns>
        public bool IsExtensionSupported(string extension)
        {
            return _extensions.Contains(extension);
        }
    }
}
