using System;

namespace Veldrid
{
    public struct VertexElementDescription : IEquatable<VertexElementDescription>
    {
        public string Name;
        public VertexElementSemantic Semantic;
        public VertexElementFormat Format;
        public uint InstanceStepRate; // 0 == per-vertex.

        public VertexElementDescription(string name, VertexElementSemantic semantic, VertexElementFormat format)
            : this(name, format, semantic, 0)
        {
        }

        public VertexElementDescription(
            string name,
            VertexElementFormat format,
            VertexElementSemantic semantic,
            uint instanceStepRate)
        {
            Name = name;
            Format = format;
            Semantic = semantic;
            InstanceStepRate = instanceStepRate;
        }

        public bool Equals(VertexElementDescription other)
        {
            return Name.Equals(other.Name)
                && Format == other.Format
                && Semantic == other.Semantic
                && InstanceStepRate == other.InstanceStepRate;
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(
                Name.GetHashCode(),
                Format.GetHashCode(),
                Semantic.GetHashCode(),
                InstanceStepRate.GetHashCode());
        }
    }
}