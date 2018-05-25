using System;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGL.GLX
{
    internal static unsafe class GLXNative
    {
        internal const string GlxLibName = "libGL";
        internal const string X11LibName = "libX11";

        /*
        * Tokens for glXChooseVisual and glXGetConfig:
        */
        internal const int GLX_USE_GL = 1;
        internal const int GLX_BUFFER_SIZE = 2;
        internal const int GLX_LEVEL = 3;
        internal const int GLX_RGBA = 4;
        internal const int GLX_DOUBLEBUFFER = 5;
        internal const int GLX_STEREO = 6;
        internal const int GLX_AUX_BUFFERS = 7;
        internal const int GLX_RED_SIZE = 8;
        internal const int GLX_GREEN_SIZE = 9;
        internal const int GLX_BLUE_SIZE = 10;
        internal const int GLX_ALPHA_SIZE = 11;
        internal const int GLX_DEPTH_SIZE = 12;
        internal const int GLX_STENCIL_SIZE = 13;
        internal const int GLX_ACCUM_RED_SIZE = 14;
        internal const int GLX_ACCUM_GREEN_SIZE = 15;
        internal const int GLX_ACCUM_BLUE_SIZE = 16;
        internal const int GLX_ACCUM_ALPHA_SIZE = 17;

        /*
         * Error codes returned by glXGetConfig:
         */
        internal const int GLX_BAD_SCREEN = 1;
        internal const int GLX_BAD_ATTRIBUTE = 2;
        internal const int GLX_NO_EXTENSION = 3;
        internal const int GLX_BAD_VISUAL = 4;
        internal const int GLX_BAD_CONTEXT = 5;
        internal const int GLX_BAD_VALUE = 6;
        internal const int GLX_BAD_ENUM = 7;

        /*
         * GLX 1.1 and later:
         */
        internal const int GLX_VENDOR = 1;
        internal const int GLX_VERSION = 2;
        internal const int GLX_EXTENSIONS = 3;

        /*
         * GLX 1.3 and later:
         */
        internal const int GLX_CONFIG_CAVEAT = 0x20;
        internal const int GLX_DONT_CARE = unchecked((int)0xFFFFFFFF);
        internal const int GLX_X_VISUAL_TYPE = 0x22;
        internal const int GLX_TRANSPARENT_TYPE = 0x23;
        internal const int GLX_TRANSPARENT_INDEX_VALUE = 0x24;
        internal const int GLX_TRANSPARENT_RED_VALUE = 0x25;
        internal const int GLX_TRANSPARENT_GREEN_VALUE = 0x26;
        internal const int GLX_TRANSPARENT_BLUE_VALUE = 0x27;
        internal const int GLX_TRANSPARENT_ALPHA_VALUE = 0x28;
        internal const int GLX_WINDOW_BIT = 0x00000001;
        internal const int GLX_PIXMAP_BIT = 0x00000002;
        internal const int GLX_PBUFFER_BIT = 0x00000004;
        internal const int GLX_AUX_BUFFERS_BIT = 0x00000010;
        internal const int GLX_FRONT_LEFT_BUFFER_BIT = 0x00000001;
        internal const int GLX_FRONT_RIGHT_BUFFER_BIT = 0x00000002;
        internal const int GLX_BACK_LEFT_BUFFER_BIT = 0x00000004;
        internal const int GLX_BACK_RIGHT_BUFFER_BIT = 0x00000008;
        internal const int GLX_DEPTH_BUFFER_BIT = 0x00000020;
        internal const int GLX_STENCIL_BUFFER_BIT = 0x00000040;
        internal const int GLX_ACCUM_BUFFER_BIT = 0x00000080;
        internal const int GLX_NONE = 0x8000;
        internal const int GLX_SLOW_CONFIG = 0x8001;
        internal const int GLX_TRUE_COLOR = 0x8002;
        internal const int GLX_DIRECT_COLOR = 0x8003;
        internal const int GLX_PSEUDO_COLOR = 0x8004;
        internal const int GLX_STATIC_COLOR = 0x8005;
        internal const int GLX_GRAY_SCALE = 0x8006;
        internal const int GLX_STATIC_GRAY = 0x8007;
        internal const int GLX_TRANSPARENT_RGB = 0x8008;
        internal const int GLX_TRANSPARENT_INDEX = 0x8009;
        internal const int GLX_VISUAL_ID = 0x800B;
        internal const int GLX_SCREEN = 0x800C;
        internal const int GLX_NON_CONFORMANT_CONFIG = 0x800D;
        internal const int GLX_DRAWABLE_TYPE = 0x8010;
        internal const int GLX_RENDER_TYPE = 0x8011;
        internal const int GLX_X_RENDERABLE = 0x8012;
        internal const int GLX_FBCONFIG_ID = 0x8013;
        internal const int GLX_RGBA_TYPE = 0x8014;
        internal const int GLX_COLOR_INDEX_TYPE = 0x8015;
        internal const int GLX_MAX_PBUFFER_WIDTH = 0x8016;
        internal const int GLX_MAX_PBUFFER_HEIGHT = 0x8017;
        internal const int GLX_MAX_PBUFFER_PIXELS = 0x8018;
        internal const int GLX_PRESERVED_CONTENTS = 0x801B;
        internal const int GLX_LARGEST_PBUFFER = 0x801C;
        internal const int GLX_WIDTH = 0x801D;
        internal const int GLX_HEIGHT = 0x801E;
        internal const int GLX_EVENT_MASK = 0x801F;
        internal const int GLX_DAMAGED = 0x8020;
        internal const int GLX_SAVED = 0x8021;
        internal const int GLX_WINDOW = 0x8022;
        internal const int GLX_PBUFFER = 0x8023;
        internal const int GLX_PBUFFER_HEIGHT = 0x8040;
        internal const int GLX_PBUFFER_WIDTH = 0x8041;
        internal const int GLX_RGBA_BIT = 0x00000001;
        internal const int GLX_COLOR_INDEX_BIT = 0x00000002;
        internal const int GLX_PBUFFER_CLOBBER_MASK = 0x08000000;

        /*
         * GLX 1.4 and later:
         */
        internal const int GLX_SAMPLE_BUFFERS = 0x186a0; /*100000*/
        internal const int GLX_SAMPLES = 0x186a1; /*100001*/

        internal const int GLX_CONTEXT_DEBUG_BIT_ARB = 0x00000001;
        internal const int GLX_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB = 0x00000002;
        internal const int GLX_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
        internal const int GLX_CONTEXT_MINOR_VERSION_ARB = 0x2092;
        internal const int GLX_CONTEXT_FLAGS_ARB = 0x2094;
        internal const int GLX_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
        internal const int GLX_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB = 0x00000002;
        internal const int GLX_CONTEXT_PROFILE_MASK_ARB = 0x9126;

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
