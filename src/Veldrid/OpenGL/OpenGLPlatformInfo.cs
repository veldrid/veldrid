using System;

namespace Veldrid.OpenGL
{
    /// <summary>
    /// Encapsulates various pieces of OpenGL context, necessary for creating a <see cref="GraphicsDevice"/> using the OpenGL
    /// API.
    /// </summary>
    public class OpenGLPlatformInfo
    {
        /// <summary>
        /// The OpenGL context handle.
        /// </summary>
        public IntPtr OpenGLContextHandle { get; }
        /// <summary>
        /// A delegate which can be used to retrieve OpenGL function pointers by name.
        /// </summary>
        public Func<string, IntPtr> GetProcAddress { get; }
        /// <summary>
        /// A delegate which can be used to make the given OpenGL context current on the calling thread.
        /// </summary>
        public Action<IntPtr> MakeCurrent { get; }
        /// <summary>
        /// A delegate which can be used to retrieve the calling thread's active OpenGL context.
        /// </summary>
        public Func<IntPtr> GetCurrentContext { get; }
        /// <summary>
        /// A delegate which can be used to clear the calling thread's GL context.
        /// </summary>
        public Action ClearCurrentContext { get; }
        /// <summary>
        /// A delegate which can be used to delete the given context.
        /// </summary>
        public Action<IntPtr> DeleteContext { get; }
        /// <summary>
        /// A delegate which can be used to swap the main back buffer associated with the OpenGL context.
        /// </summary>
        public Action SwapBuffers { get; }
        /// <summary>
        /// A delegate which can be used to set the synchronization behavior of the OpenGL context.
        /// </summary>
        public Action<bool> SetSyncToVerticalBlank { get; }

        /// <summary>
        /// Constructs a new OpenGLPlatformInfo.
        /// </summary>
        /// <param name="openGLContextHandle">The OpenGL context handle.</param>
        /// <param name="getProcAddress">A delegate which can be used to retrieve OpenGL function pointers by name.</param>
        /// <param name="makeCurrent">A delegate which can be used to make the given OpenGL context current on the calling
        /// thread.</param>
        /// <param name="getCurrentContext">A delegate which can be used to retrieve the calling thread's active OpenGL context.</param>
        /// <param name="clearCurrentContext">A delegate which can be used to clear the calling thread's GL context.</param>
        /// <param name="deleteContext">A delegate which can be used to delete the given context.</param>
        /// <param name="swapBuffers">A delegate which can be used to swap the main back buffer associated with the OpenGL
        /// context.</param>
        /// <param name="setSyncToVerticalBlank">A delegate which can be used to set the synchronization behavior of the OpenGL
        /// context.</param>
        public OpenGLPlatformInfo(
            IntPtr openGLContextHandle,
            Func<string, IntPtr> getProcAddress,
            Action<IntPtr> makeCurrent,
            Func<IntPtr> getCurrentContext,
            Action clearCurrentContext,
            Action<IntPtr> deleteContext,
            Action swapBuffers,
            Action<bool> setSyncToVerticalBlank)
        {
            OpenGLContextHandle = openGLContextHandle;
            GetProcAddress = getProcAddress;
            MakeCurrent = makeCurrent;
            GetCurrentContext = getCurrentContext;
            ClearCurrentContext = clearCurrentContext;
            DeleteContext = deleteContext;
            SwapBuffers = swapBuffers;
            SetSyncToVerticalBlank = setSyncToVerticalBlank;
        }
    }
}