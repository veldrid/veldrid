using System;
using System.Numerics;

namespace Veldrid.Utilities
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Vector3 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }

        public bool Intersects(BoundingBox box, out float distance)
        {
            Vector3 dirFactor = new Vector3(1f) / Direction;
            Vector3 max = (box.Max - Origin) * dirFactor;
            Vector3 min = (box.Min - Origin) * dirFactor;
            Vector3 tminv = Vector3.Min(min, max);
            Vector3 tmaxv = Vector3.Max(min, max);

            float tmax = MathF.Min(MathF.Min(tmaxv.X, tmaxv.Y), tmaxv.Z);
            distance = tmax;

            // ray is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0)
            {
                return false;
            }

            float tmin = MathF.Max(MathF.Max(tminv.X, tminv.Y), tminv.Z);

            // ray doesn't intersect AABB
            if (tmin > tmax)
            {
                return false;
            }

            distance = tmin;
            return true;
        }

        public bool Intersects(BoundingBox box)
        {
            return Intersects(box, out _);
        }

        public static Ray Transform(Ray ray, Matrix4x4 mat)
        {
            return new Ray(
                Vector3.Transform(ray.Origin, mat),
                Vector3.Normalize(Vector3.TransformNormal(ray.Direction, mat)));
        }

        /// <summary>
        /// Ray-Triangle Intersection, using the Möller–Trumbore intersection algorithm.
        /// </summary>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
        /// </remarks>
        public bool Intersects(Vector3 V1, Vector3 V2, Vector3 V3, out float distance)
        {
            const float EPSILON = 1E-6f;

            Vector3 e1, e2;  //Edge1, Edge2
            Vector3 P, Q, T;
            float det, inv_det, u, v;
            float t;

            //Find vectors for two edges sharing V1
            e1 = V2 - V1;
            e2 = V3 - V1;
            //Begin calculating determinant - also used to calculate u parameter
            P = Vector3.Cross(Direction, e2);
            //if determinant is near zero, ray lies in plane of triangle or ray is parallel to plane of triangle
            det = Vector3.Dot(e1, P);
            //NOT CULLIN
            if (det > -EPSILON && det < EPSILON)
            {
                distance = 0f;
                return false;
            }

            inv_det = 1.0f / det;

            //calculate distance from V1 to ray origin
            T = Origin - V1;

            //Calculate u parameter and test bound
            u = Vector3.Dot(T, P) * inv_det;
            //The intersection lies outside of the triangle
            if (u < 0.0f || u > 1.0f)
            {
                distance = 0f;
                return false;
            }

            //Prepare to test v parameter
            Q = Vector3.Cross(T, e1);

            //Calculate V parameter and test bound
            v = Vector3.Dot(Direction, Q) * inv_det;
            //The intersection lies outside of the triangle
            if (v < 0.0f || u + v > 1.0f)
            {
                distance = 0f;
                return false;
            }

            t = Vector3.Dot(e2, Q) * inv_det;

            if (t > EPSILON)
            {
                //ray intersection
                distance = t;
                return true;
            }

            // No hit, no win
            distance = 0f;
            return false;
        }
    }
}
