using System;

namespace Veldrid.Graphics
{
    public struct RenderOrderKey : IComparable<RenderOrderKey>, IComparable
    {
        private readonly ulong _rawKey;

        public RenderOrderKey(ulong rawKey)
        {
            _rawKey = rawKey;
        }

        public static RenderOrderKey Create(uint materialID) => Create(float.MaxValue, materialID);
        public static RenderOrderKey Create(int materialID) => Create(float.MaxValue, (uint)materialID);
        public static RenderOrderKey Create(float cameraDistance, int materialID) => Create(cameraDistance, (uint)materialID);
        public static RenderOrderKey Create(float cameraDistance, uint materialID)
        {
            if (cameraDistance < 0)
            {
                throw new ArgumentException("Camera distance must not be negative.");
            }

            uint cameraDistanceInt = (uint)Math.Min(uint.MaxValue, (cameraDistance * 1000f));

            return new RenderOrderKey(
                ((ulong)materialID << 32) +
                cameraDistanceInt);
        }

        public float GetDistance()
        {
            return (_rawKey & 0x00000000FFFFFFFF) / 1000f;
        }

        public int CompareTo(object obj)
        {
            return ((IComparable)_rawKey).CompareTo(obj);
        }

        public int CompareTo(RenderOrderKey other)
        {
            return _rawKey.CompareTo(other._rawKey);
        }

        public override string ToString() => _rawKey.ToString();
    }
}
