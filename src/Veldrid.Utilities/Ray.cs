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

        public bool Intersects(BoundingBox box)
        {
            return Intersects(ref box);
        }

        public bool Intersects(ref BoundingBox box)
        {
            // http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection

            float tmin = (box.Min.X - Origin.X) / Direction.X;
            float tmax = (box.Max.X - Origin.X) / Direction.X;

            if (tmin > tmax)
            {
                Swap(ref tmin, ref tmax);
            }

            float tymin = (box.Min.Y - Origin.Y) / Direction.Y;
            float tymax = (box.Max.Y - Origin.Y) / Direction.Y;

            if (tymin > tymax)
            {
                Swap(ref tymin, ref tymax);
            }

            if ((tmin > tymax) || (tymin > tmax))
            {
                return false;
            }

            if (tymin > tmin)
            {
                tmin = tymin;
            }

            if (tymax < tmax)
            {
                tmax = tymax;
            }

            float tzmin = (box.Min.Z - Origin.Z) / Direction.Z;
            float tzmax = (box.Max.Z - Origin.Z) / Direction.Z;

            if (tzmin > tzmax)
            {
                Swap(ref tzmin, ref tzmax);
            }

            if ((tmin > tzmax) || (tzmin > tmax))
            {
                return false;
            }

            if (tzmin > tmin)
            {
                tmin = tzmin;
            }

            if (tzmax < tmax)
            {
                tmax = tzmax;
            }

            return true;
        }

        void Swap(ref float a, ref float b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        public static Ray Transform(Ray ray, Matrix4x4 mat)
        {
            return new Ray(Vector3.Transform(ray.Origin, mat), Vector3.Normalize(Vector3.TransformNormal(ray.Direction, mat)));
        }

        // Ray-Triangle Intersection, using the Möller–Trumbore intersection algorithm.
        // https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm

        public bool Intersects(ref Vector3 V1, ref Vector3 V2, ref Vector3 V3, out float distance)
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
            { //ray intersection
                distance = t;
                return true;
            }

            // No hit, no win
            distance = 0f;
            return false;
        }
    }
}