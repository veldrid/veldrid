using System;
using System.Numerics;

namespace Veldrid
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
    }
}
