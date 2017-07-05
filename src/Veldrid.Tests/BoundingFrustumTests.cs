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

        [Theory]
        [InlineData(0f, 0f, 0f, 0f, 0f, -1f)]
        [InlineData(0f, 0f, 10f, 0f, 0f, -1f)]
        public static void GetCorners(float eyeX, float eyeY, float eyeZ, float viewX, float viewY, float viewZ)
        {
            Vector3 viewOrigin = new Vector3(eyeX, eyeY, eyeZ);
            Vector3 viewDir = Vector3.Normalize(new Vector3(viewX, viewY, viewZ));
            Matrix4x4 view = Matrix4x4.CreateLookAt(viewOrigin, viewDir, Vector3.UnitY);
            const float nearDist = 2f;
            const float farDist = 10f;
            const float fov = 1.0f;
            const float ratio = 1.0f;
            Matrix4x4 perspectiveProj = Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, nearDist, farDist);
            BoundingFrustum frustum = new BoundingFrustum(view * perspectiveProj);

            FrustumCorners corners = frustum.GetCorners();
            Vector3 nearCenter = viewOrigin + viewDir * nearDist;
            float nearHalfHeight = (float)Math.Tan(fov / 2f) * nearDist;
            float nearHalfWidth = nearHalfHeight * ratio;
            Quaternion forwardRotation = Utilities.FromToRotation(-Vector3.UnitZ, viewDir);
            Vector3 up = Vector3.Transform(Vector3.UnitY, forwardRotation);
            Vector3 right = -Vector3.Cross(up, viewDir);

            FuzzyComparer fuzzyComparer = new FuzzyComparer();
            AssertEqual(nearCenter - nearHalfWidth * right + nearHalfHeight * up, corners.NearTopLeft, fuzzyComparer);
            AssertEqual(nearCenter + nearHalfWidth * right + nearHalfHeight * up, corners.NearTopRight, fuzzyComparer);
            AssertEqual(nearCenter - nearHalfWidth * right - nearHalfHeight * up, corners.NearBottomLeft, fuzzyComparer);
            AssertEqual(nearCenter + nearHalfWidth * right - nearHalfHeight * up, corners.NearBottomRight, fuzzyComparer);

            Vector3 farCenter = viewOrigin + viewDir * farDist;
            float farHalfHeight = (float)Math.Tan(fov / 2f) * farDist;
            float farHalfWidth = farHalfHeight * ratio;
            AssertEqual(farCenter - farHalfWidth * right + farHalfHeight * up, corners.FarTopLeft, fuzzyComparer);
            AssertEqual(farCenter + farHalfWidth * right + farHalfHeight * up, corners.FarTopRight, fuzzyComparer);
            AssertEqual(farCenter - farHalfWidth * right - farHalfHeight * up, corners.FarBottomLeft, fuzzyComparer);
            AssertEqual(farCenter + farHalfWidth * right - farHalfHeight * up, corners.FarBottomRight, fuzzyComparer);
        }

        private static void AssertEqual(Vector3 expected, Vector3 actual, IEqualityComparer<float> comparer)
        {
            Assert.Equal(expected.X, actual.X, comparer);
            Assert.Equal(expected.Y, actual.Y, comparer);
            Assert.Equal(expected.Z, actual.Z, comparer);
        }
    }

    public class FuzzyComparer : IComparer<float>, IEqualityComparer<float>
    {
        public float AllowedDiff { get; } = .00001f;
        public int Compare(float x, float y)
        {
            float diff = x - y;
            if ((float)Math.Abs(diff) < AllowedDiff)
            {
                return 0;
            }
            else
            {
                return diff > 0 ? 1 : -1;
            }
        }

        public bool Equals(float x, float y)
        {
            float diff = x - y;
            return ((float)Math.Abs(diff) < AllowedDiff);
            
        }

        public int GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }
}
