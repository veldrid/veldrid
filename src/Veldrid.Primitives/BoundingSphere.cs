using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Graphics;

namespace Veldrid
{
    public struct BoundingSphere
    {
        public Vector3 Center;
        public float Radius;

        public BoundingSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public override string ToString()
        {
            return string.Format("Center:{0}, Radius:{1}", Center, Radius);
        }

        public bool Contains(Vector3 point)
        {
            return (Center - point).LengthSquared() <= Radius * Radius;
        }

        public static BoundingSphere CreateFromPoints(IList<Vector3> points)
        {
            Vector3 center = Vector3.Zero;
            foreach (Vector3 pt in points)
            {
                center += pt;
            }

            center /= points.Count;

            float maxDistanceSquared = 0f;
            foreach (Vector3 pt in points)
            {
                float distSq = Vector3.DistanceSquared(center, pt);
                if (distSq > maxDistanceSquared)
                {
                    maxDistanceSquared = distSq;
                }
            }

            return new BoundingSphere(center, (float)Math.Sqrt(maxDistanceSquared));
        }

        public static BoundingSphere CreateFromPoints(IList<VertexPositionNormalTexture> points)
        {
            Vector3 center = Vector3.Zero;
            foreach (VertexPositionNormalTexture pt in points)
            {
                center += pt.Position;
            }

            center /= points.Count;

            float maxDistanceSquared = 0f;
            foreach (VertexPositionNormalTexture pt in points)
            {
                float distSq = Vector3.DistanceSquared(center, pt.Position);
                if (distSq > maxDistanceSquared)
                {
                    maxDistanceSquared = distSq;
                }
            }

            return new BoundingSphere(center, (float)Math.Sqrt(maxDistanceSquared));
        }
    }
}
