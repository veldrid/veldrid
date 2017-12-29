using System.Collections.Generic;

namespace Veldrid.OpenGL
{
    internal class OpenGLExtensions
    {
        private readonly HashSet<string> _extensions;

        public OpenGLExtensions(HashSet<string> extensions)
        {
            _extensions = extensions;
            ARB_TextureStorage = IsExtensionSupported("GL_ARB_texture_storage"); // OpenGL 4.2 / 4.3 (multisampled)
            ARB_DirectStateAccess = IsExtensionSupported("GL_ARB_direct_state_access");
            ARB_MultiBind = IsExtensionSupported("GL_ARB_multi_bind");
            ARB_TextureView = IsExtensionSupported("GL_ARB_texture_view"); // OpenGL 4.3
            ARB_CopyImage = IsExtensionSupported("GL_ARB_copy_image");
            ARB_DebugOutput = IsExtensionSupported("GL_ARB_debug_output");
            KHR_Debug = IsExtensionSupported("GL_KHR_debug");
            ARB_ComputeShader = IsExtensionSupported("GL_ARB_compute_shader");
        }

        public readonly bool ARB_DirectStateAccess;
        public readonly bool ARB_MultiBind;
        public readonly bool ARB_TextureStorage;
        public readonly bool ARB_TextureView;
        public readonly bool ARB_CopyImage;
        public readonly bool ARB_DebugOutput;
        public readonly bool KHR_Debug;
        public readonly bool ARB_ComputeShader;

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
