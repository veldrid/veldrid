using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Veldrid.Tests
{
    public class FormatSizeHelpersTests : IDisposable
    {
        private TraceListener[] _traceListeners;

        public FormatSizeHelpersTests()
        {
            // temporarily disables debug trace listeners to prevent Debug.Assert
            // from causing test failures in cases where we're explicitly trying
            // to test invalid inputs
            _traceListeners = new TraceListener[Trace.Listeners.Count];
            Trace.Listeners.CopyTo(_traceListeners, 0);
            Trace.Listeners.Clear();
        }

        public void Dispose()
        {
            Trace.Listeners.AddRange(_traceListeners);
        }

        [Fact]
        public void GetSizeInBytes_DefinedForAllVertexElementFormats()
        {
            foreach (VertexElementFormat format in Enum.GetValues<VertexElementFormat>())
            {
                Assert.True(0 < FormatSizeHelpers.GetSizeInBytes(format));
            }
        }

        private static HashSet<PixelFormat> CompressedPixelFormats = new() {
            PixelFormat.BC1_Rgba_UNorm,
            PixelFormat.BC1_Rgba_UNorm_SRgb,
            PixelFormat.BC1_Rgb_UNorm,
            PixelFormat.BC1_Rgb_UNorm_SRgb,

            PixelFormat.BC2_UNorm,
            PixelFormat.BC2_UNorm_SRgb,

            PixelFormat.BC3_UNorm,
            PixelFormat.BC3_UNorm_SRgb,

            PixelFormat.BC4_SNorm,
            PixelFormat.BC4_UNorm,

            PixelFormat.BC5_SNorm,
            PixelFormat.BC5_UNorm,

            PixelFormat.BC7_UNorm,
            PixelFormat.BC7_UNorm_SRgb,

            PixelFormat.ETC2_R8_G8_B8_A1_UNorm,
            PixelFormat.ETC2_R8_G8_B8_A8_UNorm,
            PixelFormat.ETC2_R8_G8_B8_UNorm,
        };

        private static IEnumerable<PixelFormat> UncompressedPixelFormats
            = Enum.GetValues<PixelFormat>()
            .Where(format => !CompressedPixelFormats.Contains(format));

        public static IEnumerable<object[]> CompressedPixelFormatMemberData => CompressedPixelFormats.Select(format => new object[] { format });
        public static IEnumerable<object[]> UncompressedPixelFormatMemberData => UncompressedPixelFormats.Select(format => new object[] { format });

        [Theory]
        [MemberData(nameof(UncompressedPixelFormatMemberData))]
        public void GetSizeInBytes_DefinedForAllNonCompressedPixelFormats(PixelFormat format)
        {
            Assert.True(0 < FormatSizeHelpers.GetSizeInBytes(format));
        }

        [Theory]
        [MemberData(nameof(CompressedPixelFormatMemberData))]
        public void GetSizeInBytes_ThrowsForAllCompressedPixelFormats(PixelFormat format)
        {
            Assert.ThrowsAny<VeldridException>(() => FormatSizeHelpers.GetSizeInBytes(format));
        }
    }
}
