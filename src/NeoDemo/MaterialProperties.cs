using System.Numerics;

namespace Veldrid.NeoDemo
{
    public struct MaterialProperties
    {
        public Vector3 SpecularIntensity;
        public float SpecularPower;
#pragma warning disable IDE0051 // Remove unused private members
        private Vector3 _padding0;
#pragma warning restore IDE0051 // Remove unused private members
        public float Reflectivity;
    }
}
