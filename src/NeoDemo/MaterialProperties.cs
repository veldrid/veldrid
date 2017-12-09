using System;
using System.Numerics;

namespace Veldrid.NeoDemo
{
    public struct MaterialProperties
    {
        public Vector3 SpecularIntensity;
        public float SpecularPower;
        public float Reflectivity;
#pragma warning disable 0169
        private Vector3 _padding0;
#pragma warning restore 0169
    }
}
