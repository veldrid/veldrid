using NativeLibraryLoader;
using System;

namespace Veldrid.MetalBindings
{
    internal class CalliTargetAttribute : Attribute { }

    // The rewriter should do all of this.
    internal static class CalliRewriteHelper
    {
        public static NativeLibrary LoadLibrary(string name)
        {
            return new NativeLibrary(name);
        }

        public static IntPtr LoadFunction(NativeLibrary lib, string function) => lib.LoadFunction(function);
    }
}
