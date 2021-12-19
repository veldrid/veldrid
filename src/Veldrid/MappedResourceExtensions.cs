using System.Collections.Generic;

namespace Veldrid
{
    public static class MappedResourceViewExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this MappedResourceView<T> view) where T : struct
        {
            for (int i = 0; i < view.Count; i++)
                yield return view[i];
        }
    }
}
