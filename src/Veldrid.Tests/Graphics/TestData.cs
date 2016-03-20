using System;
using System.Collections.Generic;
using System.Linq;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Platform;

namespace Veldrid.Graphics
{
    public static class TestData
    {
        public static IEnumerable<RenderContext> RenderContexts()
        {
            return new RenderContext[]
            {
                new D3DRenderContext(new TestWindow()),
                new OpenGLRenderContext(new TestWindow())
            };
        }

        internal static IEnumerable<object> DataValueArrays()
        {
            int[] starts = { 0, 1, 10 };
            int[] lengths = { 1, 10, 10000 };
            foreach (int start in starts)
            {
                foreach (int length in lengths)
                {
                    yield return Enumerable.Range(start, start + length).ToArray();
                    yield return Enumerable.Range(start, start + length).Select(i => (uint)i).ToArray();
                    yield return Enumerable.Range(start, start + length).Select(i => (float)i).ToArray();
                    yield return Enumerable.Range(start, start + length).Select(i => (ushort)i).ToArray();
                }
            }
        }

        internal static IEnumerable<int> IntRange(int first, int count, int interval)
        {
            int ret = first;
            yield return ret;
            for (int i = 0; i < count; i++)
            {
                ret += interval;
                yield return ret;
            }
        }

        public static object[] Array(params object[] items)
        {
            return items;
        }
    }
}
