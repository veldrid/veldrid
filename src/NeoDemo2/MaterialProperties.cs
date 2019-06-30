using System;
using System.Numerics;

namespace Veldrid.NeoDemo
{
    public struct MaterialProperties
    {
        public Vector3 SpecularIntensity;
        public float SpecularPower;
#pragma warning disable 0169
        private Vector3 _padding0;
#pragma warning restore 0169
        public float Reflectivity;
    }
}
