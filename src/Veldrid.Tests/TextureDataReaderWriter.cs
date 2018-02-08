using System;

namespace Veldrid.Tests
{
    internal unsafe class TextureDataReaderWriter
    {
        public int RedBits { get; }
        public int GreenBits { get; }
        public int BlueBits { get; }
        public int AlphaBits { get; }
        public int PixelBytes { get; }

        public ulong RMaxValue => (ulong)Math.Pow(2, RedBits) - 1;
        public ulong GMaxValue => (ulong)Math.Pow(2, GreenBits) - 1;
        public ulong BMaxValue => (ulong)Math.Pow(2, BlueBits) - 1;
        public ulong AMaxValue => (ulong)Math.Pow(2, AlphaBits) - 1;

        public TextureDataReaderWriter(int redBits, int greenBits, int blueBits, int alphaBits)
        {
            RedBits = redBits;
            GreenBits = greenBits;
            BlueBits = blueBits;
            AlphaBits = alphaBits;
            PixelBytes = (redBits + blueBits + greenBits + alphaBits) / 8;
        }

        public WidePixel ReadPixel(byte* pixelPtr)
        {
            ulong? r = ReadBits(pixelPtr, 0, RedBits);
            ulong? g = ReadBits(pixelPtr, RedBits, GreenBits);
            ulong? b = ReadBits(pixelPtr, RedBits + GreenBits, BlueBits);
            ulong? a = ReadBits(pixelPtr, RedBits + GreenBits + BlueBits, AlphaBits);

            return new WidePixel(r, g, b, a);
        }

        private ulong? ReadBits(byte* pixelPtr, int bitOffset, int numBits)
        {
            if (numBits == 0)
            {
                return null;
            }

            ulong ret = 0;

            for (int i = 0; i < numBits; i++)
            {
                if (IsBitSet(pixelPtr, bitOffset + i))
                {
                    SetBit((byte*)&ret, i);
                }
            }

            return ret;
        }

        public void WritePixel(byte* pixelPtr, WidePixel pixel)
        {
            WriteBits(pixel.R, pixelPtr, 0, RedBits);
            WriteBits(pixel.G, pixelPtr, RedBits, GreenBits);
            WriteBits(pixel.B, pixelPtr, RedBits + GreenBits, BlueBits);
            WriteBits(pixel.A, pixelPtr, RedBits + GreenBits + BlueBits, AlphaBits);
        }

        internal void WriteBits(ulong? value, byte* basePtr, int bitOffset, int numBits)
        {
            if (value == null)
            {
                return;
            }

            ulong val = value.Value;

            for (int i = 0; i < numBits; i++)
            {
                if (IsBitSet((byte*)&val, i))
                {
                    SetBit(basePtr, bitOffset + i);
                }
            }
        }

        internal byte[] GetDataArray(uint srcWidth, uint srcHeight, uint srcDepth)
        {
            return new byte[PixelBytes * srcWidth * srcHeight * srcDepth];
        }

        internal WidePixel GetTestPixel(uint x, uint y, uint z)
        {
            ulong? r = x % RMaxValue;
            ulong? g = GreenBits != 0 ? (y % GMaxValue) : (ulong?)null;
            ulong? b = BlueBits != 0 ? (z % BMaxValue) : (ulong?)null;
            ulong? a = AlphaBits != 0 ? 1 : (ulong?)null;
            return new WidePixel(r, g, b, a);
        }

        private bool IsBitSet(byte* basePtr, int bit)
        {
            int index = Math.DivRem(bit, 8, out int remainder);
            byte val = basePtr[index];
            ulong mask = 1ul << remainder;
            return (val & mask) != 0;
        }

        private void SetBit(byte* basePtr, int bit)
        {
            int index = Math.DivRem(bit, 8, out int remainder);
            byte val = basePtr[index];
            byte mask = (byte)(1 << remainder);
            byte newVal = (byte)(val | mask);
            basePtr[index] = newVal;
        }
    }

    internal struct WidePixel : IEquatable<WidePixel>
    {
        public readonly ulong? R;
        public readonly ulong? G;
        public readonly ulong? B;
        public readonly ulong? A;

        public WidePixel(ulong? r, ulong? g, ulong? b, ulong? a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public bool Equals(WidePixel other)
        {
            return R.HasValue == other.R.HasValue && R.GetValueOrDefault().Equals(other.R.GetValueOrDefault())
                && G.HasValue == other.G.HasValue && G.GetValueOrDefault().Equals(other.G.GetValueOrDefault())
                && B.HasValue == other.B.HasValue && B.GetValueOrDefault().Equals(other.B.GetValueOrDefault())
                && A.HasValue == other.A.HasValue && A.GetValueOrDefault().Equals(other.A.GetValueOrDefault());
        }

        public override string ToString()
        {
            return $"{R}, {G}, {B}, {A}";
        }
    }
}
