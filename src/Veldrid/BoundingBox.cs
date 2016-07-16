using System;
using System.Numerics;
using Veldrid.Graphics;

namespace Veldrid
{

    /// <summary>
    /// An axis-aligned bounding-box (AABB).
    /// </summary>
    public struct BoundingBox: IEquatable<BoundingBox>
    {
        public Vector3 Min;
        public Vector3 Max;

        public BoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public ContainmentType Contains(ref BoundingBox other)
        {
            if (Max.X < other.Min.X || Min.X > other.Max.X
                || Max.Y < other.Min.Y || Min.Y > other.Max.Y
                || Max.Z < other.Min.Z || Min.Z > other.Min.Z)
            {
                return ContainmentType.Disjoint;
            }
            else if (Min.X <= other.Min.X && Max.X >= other.Max.X
                && Min.Y <= other.Min.Y && Max.Y >= other.Max.Y
                && Min.Z <= other.Min.Z && Max.Z >= other.Max.Z)
            {
                return ContainmentType.Contains;
            }
            else
            {
                return ContainmentType.Intersects;
            }
        }

        public Vector3 GetCenter()
        {
            return (Max + Min) / 2f;
        }

        public Vector3 GetDimensions()
        {
            return Max - Min;
        }

        public static BoundingBox CreateFromVertices(VertexPositionNormalTexture[] vertices, Quaternion rotation, Vector3 offset, Vector3 scale)
        {
            Vector3 min = Vector3.Transform(vertices[0].Position, rotation);
            Vector3 max = Vector3.Transform(vertices[0].Position, rotation);

            for (int i = 1; i < vertices.Length; i++)
            {
                Vector3 pos = Vector3.Transform(vertices[i].Position, rotation);

                if (min.X > pos.X) min.X = pos.X;
                if (max.X < pos.X) max.X = pos.X;

                if (min.Y > pos.Y) min.Y = pos.Y;
                if (max.Y < pos.Y) max.Y = pos.Y;

                if (min.Z > pos.Z) min.Z = pos.Z;
                if (max.Z < pos.Z) max.Z = pos.Z;
            }

            return new BoundingBox((min * scale) + offset, (max * scale) + offset);
        }

        public static bool operator ==(BoundingBox first, BoundingBox second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(BoundingBox first, BoundingBox second)
        {
            return !first.Equals(second);
        }

        public bool Equals(BoundingBox other)
        {
            return Min == other.Min && Max == other.Max;
        }

        public override string ToString()
        {
            return string.Format("Min:{0}, Max:{1}", Min, Max);
        }
    }
}
