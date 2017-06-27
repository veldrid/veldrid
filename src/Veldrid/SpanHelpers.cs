using System;

namespace Veldrid
{
    internal static class SpanHelpers
    {
        public static string GetString(ReadOnlySpan<char> span)
        {
            return new string(span.ToArray());
        }
    }
}
