using System.Runtime.InteropServices;

namespace Vd2.NeoDemo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DepthCascadeLimits
    {
        public float NearLimit;
        public float MidLimit;
        public float FarLimit;
        private float _padding;
    }
}
