using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Veldrid.Graphics
{
    public class VertexPositionColorTests
    {
        [Fact]
        public static unsafe void SizeOf()
        {
            Assert.Equal(28, sizeof(VertexPositionColor));
            Assert.Equal(28, Marshal.SizeOf<VertexPositionColor>());
        }

        [Fact]
        public static unsafe void Offsets()
        {
            Assert.Equal(new IntPtr(12), Marshal.OffsetOf<VertexPositionColor>(nameof(VertexPositionColor.Color)));
        }
    }
}
