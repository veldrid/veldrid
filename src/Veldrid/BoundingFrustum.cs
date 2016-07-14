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
    }
}
