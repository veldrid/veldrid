using System;
using System.Collections.Generic;

namespace Veldrid.OpenGL
{
    internal class OpenGLExtensions
    {
        private readonly HashSet<string> _extensions;
        private readonly int _major;
        private readonly int _minor;

        public OpenGLExtensions(HashSet<string> extensions, GraphicsBackend backend, int major, int minor)
        {
            _extensions = extensions;
            _major = major;
            _minor = minor;

            TextureStorage = IsExtensionSupported("GL_ARB_texture_storage") // OpenGL 4.2 / 4.3 (multisampled)
                || GLESVersion(backend, 3, 0);
            ARB_DirectStateAccess = IsExtensionSupported("GL_ARB_direct_state_access");
            ARB_MultiBind = IsExtensionSupported("GL_ARB_multi_bind");
            ARB_TextureView = IsExtensionSupported("GL_ARB_texture_view"); // OpenGL 4.3
            CopyImage = IsExtensionSupported("GL_ARB_copy_image")
                || GLESVersion(backend, 3, 2)
                || IsExtensionSupported("GL_OES_copy_image")
                || IsExtensionSupported("GL_EXT_copy_image");
            ARB_DebugOutput = IsExtensionSupported("GL_ARB_debug_output");
            KHR_Debug = IsExtensionSupported("GL_KHR_debug");

            ComputeShaders = IsExtensionSupported("GL_ARB_compute_shader") || GLESVersion(backend, 3, 1);
        }

        public readonly bool ARB_DirectStateAccess;
        public readonly bool ARB_MultiBind;
        public readonly bool ARB_TextureView;
        public readonly bool ARB_DebugOutput;
        public readonly bool KHR_Debug;

        // Differs between GL / GLES
        public readonly bool TextureStorage;
        public readonly bool CopyImage;
        public readonly bool ComputeShaders;

        /// <summary>
        /// Returns a value indicating whether the given extension is supported.
        /// </summary>
        /// <param name="extension">The name of the extensions. For example, "</param>
        /// <returns></returns>
        public bool IsExtensionSupported(string extension)
        {
            return _extensions.Contains(extension);
        }


        private bool GLESVersion(GraphicsBackend backend, int major, int minor)
        {
            if (backend == GraphicsBackend.OpenGLES)
            {
                if (_major > major)
                {
                    return true;
                }
                else
                {
                    return _major == major && _minor >= minor;
                }
            }

            return false;
        }
    }
}
