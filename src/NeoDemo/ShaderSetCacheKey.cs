using System;

namespace Veldrid.NeoDemo
{
    public struct ShaderSetCacheKey : IEquatable<ShaderSetCacheKey>
    {
        public string Name { get; }
        public SpecializationConstant[] Specializations { get; }

        public ShaderSetCacheKey(string name, SpecializationConstant[] specializations) : this()
        {
            Name = name;
            Specializations = specializations;
        }

        public bool Equals(ShaderSetCacheKey other)
        {
            return Name.Equals(other.Name) && ArraysEqual(Specializations, other.Specializations);
        }

        public override int GetHashCode()
        {
            int hash = Name.GetHashCode();
            foreach (var specConst in Specializations)
            {
                hash ^= specConst.GetHashCode();
            }
            return hash;
        }

        private bool ArraysEqual<T>(T[] a, T[] b) where T : IEquatable<T>
        {
            if (a.Length != b.Length) { return false; }

            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].Equals(b[i])) { return false; }
            }

            return true;
        }
    }
}
