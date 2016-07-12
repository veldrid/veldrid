using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Veldrid
{
    public static class BoundingFrustumTests
    {
        [Theory]
        [MemberData(nameof(FrustumContainsSphereData))]
        public static void FrustumContainsSphere(BoundingFrustum frustum, BoundingSphere sphere, ContainmentType expected)
        {
            Assert.Equal(expected, frustum.Contains(sphere));
        }

        private static IEnumerable<object[]> FrustumContainsSphereData()
        {
            Matrix4x4 centeredMat =
                Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY)
                * Matrix4x4.CreatePerspectiveFieldOfView(1.0f, 1.0f, 2f, 10f);
            BoundingFrustum bf = new BoundingFrustum(centeredMat);

            Vector3 zero = Vector3.Zero;
            Vector3 forward = -Vector3.UnitZ;
            Vector3 unitY = Vector3.UnitY;

            // Front and back face edges, straight on.
            yield return new object[] { bf, new BoundingSphere(Vector3.Zero, 1.9999999f), ContainmentType.Disjoint };
            yield return new object[] { bf, new BoundingSphere(Vector3.Zero, 2.0f), ContainmentType.Intersects };
            yield return new object[] { bf, new BoundingSphere(new Vector3(0, 0, -12f), 1.9999999f), ContainmentType.Disjoint };
            yield return new object[] { bf, new BoundingSphere(new Vector3(0, 0, -12f), 2.0f), ContainmentType.Intersects };

            // Inner
            yield return new object[] { bf, new BoundingSphere(new Vector3(0, 0, -0.09260497969f), 0.999999f), ContainmentType.Disjoint };
            yield return new object[] { bf, new BoundingSphere(new Vector3(0, 0, -1.09260497969f), 1f), ContainmentType.Intersects };
            yield return new object[] { bf, new BoundingSphere(new Vector3(0, 0, -11), 2.1f), ContainmentType.Intersects };
            yield return new object[] { bf, new BoundingSphere(new Vector3(0, 0, -5), 1f), ContainmentType.Contains };
            yield return new object[] { bf, new BoundingSphere(new Vector3(2, 2, -5), 0.3f), ContainmentType.Contains };

            // Far plane corners / faces
            yield return new object[] { bf, new BoundingSphere(new Vector3(-6.46302489844f, 0, -10f), 0.87f), ContainmentType.Disjoint };
            yield return new object[] { bf, new BoundingSphere(new Vector3(-6.46302489844f, 6, -10f), 0.87f), ContainmentType.Disjoint };
            yield return new object[] { bf, new BoundingSphere(new Vector3(-6.46302489844f, 0, -10f), 1.1f), ContainmentType.Intersects };
            yield return new object[] { bf, new BoundingSphere(new Vector3(-6.46302489844f, -6, -10f), 1.1f), ContainmentType.Intersects };

            yield return new object[] { bf, new BoundingSphere(new Vector3(6.46302489844f, 0, -10f), 0.87f), ContainmentType.Disjoint };
            yield return new object[] { bf, new BoundingSphere(new Vector3(6.46302489844f, -6, -10f), 0.87f), ContainmentType.Disjoint };
            yield return new object[] { bf, new BoundingSphere(new Vector3(6.46302489844f, 0, -10f), 1.1f), ContainmentType.Intersects };
            yield return new object[] { bf, new BoundingSphere(new Vector3(6.46302489844f, 6, -10f), 1.1f), ContainmentType.Intersects };
        }
    }
}
