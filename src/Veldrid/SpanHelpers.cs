using System;

namespace Veldrid
{
    internal static class SpanHelpers
    {
        public static string GetString(ReadOnlySpan<char> libName)
        {
            return new string(libName.ToArray());
        }
    }
}
