using System;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGL.GLX
{
    internal static unsafe class GLXNative
    {
        internal const string GlxLibName = "libGL";
        internal const string X11LibName = "libX11";

        internal const int GLX_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
        internal const int GLX_CONTEXT_MINOR_VERSION_ARB = 0x2092;
        
        internal const int GLX_CONTEXT_PROFILE_MASK_ARB = 0x9126;
        internal const int GLX_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
        internal const int GLX_CONTEXT_ES_PROFILE_BIT_EXT = 0x00000004;

        [DllImport(GlxLibName)]
        internal static extern XVisualInfo* glXGetVisualFromFBConfig(IntPtr dpy, IntPtr config);

        [DllImport(GlxLibName)]
        internal static extern IntPtr glXChooseFBConfig(IntPtr dpy, int screen, int* attribList, int* nitems);

        [DllImport(GlxLibName)]
        internal static extern int glXGetFBConfigAttrib(IntPtr dpy, IntPtr config, int attribute, int* value);

        [DllImport(GlxLibName)]
        internal static extern IntPtr glXCreateContextAttribsARB(
            IntPtr dpy,
            IntPtr config,
            IntPtr share_context,
            int direct,
            int* attrib_list);

        [DllImport(GlxLibName)]
        internal static extern int glXMakeCurrent(IntPtr dpy, IntPtr drawable, IntPtr ctx);

        [DllImport(GlxLibName)]
        internal static extern IntPtr glXGetProcAddress(string name);

        [DllImport(GlxLibName)]
        internal static extern void glXSwapBuffers(IntPtr dpy, IntPtr drawable);

        [DllImport(GlxLibName)]
        internal static extern IntPtr glXGetCurrentContext();

        [DllImport(GlxLibName)]
        internal static extern void glXDestroyContext(IntPtr dpy, IntPtr ctx);

        [DllImport(GlxLibName)]
        internal static extern IntPtr glXCreateContext(IntPtr dpy, XVisualInfo* vis, IntPtr shareList, int direct);

        [DllImport(GlxLibName)]
        internal static extern IntPtr glXGetFBConfigFromVisualSGIX(IntPtr dpy, XVisualInfo* vis);

        [DllImport(X11LibName)]
        internal static extern int XFree(void* data);

        [DllImport(X11LibName)]
        internal static extern int XDefaultScreen(IntPtr display);

        [DllImport(X11LibName)]
        internal static extern IntPtr XDefaultScreenOfDisplay(IntPtr display);

        [DllImport(X11LibName)]
        internal static extern XVisualInfo* XGetVisualInfo(
            IntPtr display,
            long vinfo_mask,
            XVisualInfo* vinfo_template,
            int* nitems_return);

        [DllImport(X11LibName)]
        internal static extern int XGetWindowAttributes(IntPtr display, IntPtr w, XWindowAttributes* window_attributes_return);

        internal struct XVisualInfo
        {
            public IntPtr visual;
            public ulong visualid;
            public int screen;
            public int depth;
            public int c_class;
            public ulong red_mask;
            public ulong green_mask;
            public ulong blue_mask;
            public int colormap_size;
            public int bits_per_rgb;
        }

        internal struct XWindowAttributes
        {
            public int x, y;
            /* location of window */

            public int width, height;
            /* width and height of window */

            public int border_width;
            /* border width of window */

            public int depth;
            /* depth of window */

            public IntPtr visual;
            /* the associated visual structure */

            public IntPtr root;
            /* root of screen containing window */

            public int @class;
            /* InputOutput, InputOnly*/

            public int bit_gravity;
            /* one of the bit gravity values */

            public int win_gravity;
            /* one of the window gravity values */

            public int backing_store;
            /* NotUseful, WhenMapped, Always */

            public ulong backing_planes;/* planes to be preserved if possible */

            public ulong backing_pixel;/* value to be used when restoring planes */

            public int save_under;
            /* boolean, should bits under be saved? */

            public ulong colormap;
            /* color map to be associated with window */

            public int map_installed;
            /* boolean, is color map currently installed*/

            public int map_state;
            /* IsUnmapped, IsUnviewable, IsViewable */

            public long all_event_masks;
            /* set of events all people have interest in*/

            public long your_event_mask;
            /* my event mask */

            public long do_not_propagate_mask;/* set of events that should not propagate */

            public int override_redirect;
            /* boolean value for override-redirect */

            public IntPtr screen;
            /* back pointer to correct screen */
        }
    }
}
