using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Veldrid
{
    public class OctreeTests
    {
        [Fact]
        public static void Basic()
        {
            BoundingBox octreeBounds = new BoundingBox(Vector3.Zero, new Vector3(100, 100, 100));
            OctreeNode<string> octree = Octree.CreateNewTree<string>(ref octreeBounds, 3);

            Vector3[] mins = new Vector3[]
            {
                new Vector3(2, 2, 2),
                new Vector3(5, 6, 7),
                new Vector3(65, 65, 65),
                new Vector3(90, 90, 90)
            };

            Vector3[] extents = new Vector3[]
            {
                new Vector3(1, 1, 1),
                new Vector3(5, 5, 5),
                new Vector3(4, 1, 1),
                new Vector3(3, 2, 1)
            };

            int counter = 0;
            foreach (var min in mins)
            {
                foreach (var extent in extents)
                {
                    BoundingBox itemBounds = new BoundingBox(min, min + extent);
                    octree.AddItem(ref itemBounds, counter.ToString());
                    counter++;
                }
            }

            BoundingFrustum frustum = new BoundingFrustum(Matrix4x4.CreatePerspectiveFieldOfView(1.0f, 1.0f, 2f, 10f));
            List<string> results = new List<string>();
            octree.GetContainedObjects(ref frustum, results);
        }
    }
}
