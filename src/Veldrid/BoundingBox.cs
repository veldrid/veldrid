using System;
using System.Collections.Generic;
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

        public static BoundingBox CreateFromVertices(IList<VertexPositionNormalTexture> vertices)
        {
            return CreateFromVertices(vertices, Quaternion.Identity, Vector3.Zero, Vector3.One);
        }

        public static BoundingBox CreateFromVertices(IList<VertexPositionNormalTexture> vertices, Quaternion rotation, Vector3 offset, Vector3 scale)
        {
            Vector3 min = Vector3.Transform(vertices[0].Position, rotation);
            Vector3 max = Vector3.Transform(vertices[0].Position, rotation);

            for (int i = 1; i < vertices.Count; i++)
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

        public static unsafe BoundingBox Transform(BoundingBox box, Matrix4x4 mat)
        {
            AlignedBoxCorners corners = box.GetCorners();
            Vector3* cornersPtr = (Vector3*)&corners;

            Vector3 min = Vector3.Transform(cornersPtr[0], mat);
            Vector3 max = Vector3.Transform(cornersPtr[0], mat);

            for (int i = 1; i < 8; i++)
            {
                min = Vector3.Min(min, Vector3.Transform(cornersPtr[i], mat));
                max = Vector3.Max(max, Vector3.Transform(cornersPtr[i], mat));
            }

            return new BoundingBox(min, max);
        }

        public static unsafe BoundingBox CreateFromVertices(Vector3* vertices, int numVertices, Quaternion rotation, Vector3 offset, Vector3 scale)
        {
            Vector3 min = Vector3.Transform(vertices[0], rotation);
            Vector3 max = Vector3.Transform(vertices[0], rotation);

            for (int i = 1; i < numVertices; i++)
            {
                Vector3 pos = Vector3.Transform(vertices[i], rotation);

                if (min.X > pos.X) min.X = pos.X;
                if (max.X < pos.X) max.X = pos.X;

                if (min.Y > pos.Y) min.Y = pos.Y;
                if (max.Y < pos.Y) max.Y = pos.Y;

                if (min.Z > pos.Z) min.Z = pos.Z;
                if (max.Z < pos.Z) max.Z = pos.Z;
            }

            return new BoundingBox((min * scale) + offset, (max * scale) + offset);
        }

        public static unsafe BoundingBox CreateFromVertices(Vector3[] vertices)
        {
            return CreateFromVertices(vertices, Quaternion.Identity, Vector3.Zero, Vector3.One);
        }

        public static unsafe BoundingBox CreateFromVertices(Vector3[] vertices, Quaternion rotation, Vector3 offset, Vector3 scale)
        {
            Vector3 min = Vector3.Transform(vertices[0], rotation);
            Vector3 max = Vector3.Transform(vertices[0], rotation);

            for (int i = 1; i < vertices.Length; i++)
            {
                Vector3 pos = Vector3.Transform(vertices[i], rotation);

                if (min.X > pos.X) min.X = pos.X;
                if (max.X < pos.X) max.X = pos.X;

                if (min.Y > pos.Y) min.Y = pos.Y;
                if (max.Y < pos.Y) max.Y = pos.Y;

                if (min.Z > pos.Z) min.Z = pos.Z;
                if (max.Z < pos.Z) max.Z = pos.Z;
            }

            return new BoundingBox((min * scale) + offset, (max * scale) + offset);
        }

        public static BoundingBox Combine(BoundingBox box1, BoundingBox box2)
        {
            return new BoundingBox(
                Vector3.Min(box1.Min, box2.Min),
                Vector3.Max(box1.Max, box2.Max));
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

        public override bool Equals(object obj)
        {
            return obj is BoundingBox && ((BoundingBox)obj).Equals(this);
        }

        public override int GetHashCode()
        {
            int h1 = Min.GetHashCode();
            int h2 = Max.GetHashCode();
            uint shift5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)shift5 + h1) ^ h2;
        }

        public AlignedBoxCorners GetCorners()
        {
            AlignedBoxCorners corners;
            corners.NearBottomLeft = new Vector3(Min.X, Min.Y, Max.Z);
            corners.NearBottomRight = new Vector3(Max.X, Min.Y, Max.Z);
            corners.NearTopLeft = new Vector3(Min.X, Max.Y, Max.Z);
            corners.NearTopRight = new Vector3(Max.X, Max.Y, Max.Z);

            corners.FarBottomLeft = new Vector3(Min.X, Min.Y, Min.Z);
            corners.FarBottomRight = new Vector3(Max.X, Min.Y, Min.Z);
            corners.FarTopLeft = new Vector3(Min.X, Max.Y, Min.Z);
            corners.FarTopRight = new Vector3(Max.X, Max.Y, Min.Z);

            return corners;
        }

        public bool ContainsNaN()
        {
            return float.IsNaN(Min.X) || float.IsNaN(Min.Y) || float.IsNaN(Min.Z)
                || float.IsNaN(Max.X) || float.IsNaN(Max.Y) || float.IsNaN(Max.Z);
        }

    }

    public struct AlignedBoxCorners
    {
        public Vector3 NearTopLeft;
        public Vector3 NearTopRight;
        public Vector3 NearBottomLeft;
        public Vector3 NearBottomRight;
        public Vector3 FarTopLeft;
        public Vector3 FarTopRight;
        public Vector3 FarBottomLeft;
        public Vector3 FarBottomRight;
    }
}
