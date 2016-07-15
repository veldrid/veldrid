using System.Numerics;
using System.Runtime.CompilerServices;

namespace Veldrid
{
    public unsafe struct BoundingFrustum
    {
        private readonly Matrix4x4 _projectionMatrix;

        private SixPlane _planes;

        private struct SixPlane
        {
            public Plane Left;
            public Plane Right;
            public Plane Bottom;
            public Plane Top;
            public Plane Near;
            public Plane Far;
        }

        public BoundingFrustum(Matrix4x4 m)
        {
            _projectionMatrix = m;

            // Plane computations: http://gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf
            _planes.Left = Plane.Normalize(
                new Plane(
                    m.M14 + m.M11,
                    m.M24 + m.M21,
                    m.M34 + m.M31,
                    m.M44 + m.M41));

            _planes.Right = Plane.Normalize(
                new Plane(
                    m.M14 - m.M11,
                    m.M24 - m.M21,
                    m.M34 - m.M31,
                    m.M44 - m.M41));

            _planes.Bottom = Plane.Normalize(
                new Plane(
                    m.M14 + m.M12,
                    m.M24 + m.M22,
                    m.M34 + m.M32,
                    m.M44 + m.M42));

            _planes.Top = Plane.Normalize(
                new Plane(
                    m.M14 - m.M12,
                    m.M24 - m.M22,
                    m.M34 - m.M32,
                    m.M44 - m.M42));

            _planes.Near = Plane.Normalize(
                new Plane(
                    m.M13,
                    m.M23,
                    m.M33,
                    m.M43));

            _planes.Far = Plane.Normalize(
                new Plane(
                    m.M14 - m.M13,
                    m.M24 - m.M23,
                    m.M34 - m.M33,
                    m.M44 - m.M43));
        }

        public ContainmentType Contains(Vector3 point)
        {
            Plane* planes = (Plane*)Unsafe.AsPointer(ref _planes); // Is this safe?

            for (int i = 0; i < 6; i++)
            {
                if (Plane.DotCoordinate(planes[i], point) < 0)
                {
                    return ContainmentType.Disjoint;
                }
            }

            return ContainmentType.Contains;
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            Plane* planes = (Plane*)Unsafe.AsPointer(ref _planes);

            ContainmentType result = ContainmentType.Contains;
            for (int i = 0; i < 6; i++)
            {
                float distance = Plane.DotCoordinate(planes[i], sphere.Center);
                if (distance < -sphere.Radius)
                {
                    return ContainmentType.Disjoint;
                }
                else if (distance < sphere.Radius)
                {
                    result = ContainmentType.Intersects;
                }
            }

            return result;
        }

        public ContainmentType Contains(BoundingBox box)
        {
            Plane* planes = (Plane*)Unsafe.AsPointer(ref _planes);

            ContainmentType result = ContainmentType.Contains;
            for (int i = 0; i < 6; i++)
            {
                Plane plane = planes[i];

                // Approach: http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

                Vector3 positive = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
                Vector3 negative = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

                if (plane.Normal.X >= 0)
                {
                    positive.X = box.Max.X;
                    negative.X = box.Min.X;
                }
                if (plane.Normal.Y >= 0)
                {
                    positive.Y = box.Max.Y;
                    negative.Y = box.Min.Y;
                }
                if (plane.Normal.Z >= 0)
                {
                    positive.Z = box.Max.Z;
                    negative.Z = box.Min.Z;
                }

                // If the positive vertex is outside (behind plane), the box is disjoint.
                float positiveDistance = Plane.DotCoordinate(plane, positive);
                if (positiveDistance < 0)
                {
                    return ContainmentType.Disjoint;
                }

                // If the negative vertex is outside (behind plane), the box is intersecting.
                // Because the above check failed, the positive vertex is in front of the plane,
                // and the negative vertex is behind. Thus, the box is intersecting this plane.
                float negativeDistance = Plane.DotCoordinate(plane, negative);
                if (negativeDistance < 0)
                {
                    result = ContainmentType.Intersects;
                }
            }

            return result;
        }
    }
}
