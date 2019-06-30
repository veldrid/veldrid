using System.Numerics;
using Veldrid;

namespace Veldrid.NeoDemo.Objects
{
    public static class CommonMaterials
    {
        public static MaterialPropsAndBuffer Brick { get; }
        public static MaterialPropsAndBuffer Vase { get; }
        public static MaterialPropsAndBuffer Reflective { get; }

        static CommonMaterials()
        {
            Brick = new MaterialPropsAndBuffer(new MaterialProperties { SpecularIntensity = new Vector3(0.2f), SpecularPower = 10f }) { Name = "Brick" };
            Vase = new MaterialPropsAndBuffer(new MaterialProperties { SpecularIntensity = new Vector3(1.0f), SpecularPower = 10f }) { Name = "Vase" };
            Reflective = new MaterialPropsAndBuffer(new MaterialProperties { SpecularIntensity = new Vector3(0.2f), SpecularPower = 10f, Reflectivity = 0.3f }) { Name = "Reflective" };
        }

        public static void CreateAllDeviceObjects(GraphicsDevice gd)
        {
            Brick.CreateDeviceObjects(gd);
            Vase.CreateDeviceObjects(gd);
            Reflective.CreateDeviceObjects(gd);
        }

        public static void FlushAll(CommandBuffer cb)
        {
            Brick.FlushChanges(cb);
            Vase.FlushChanges(cb);
            Reflective.FlushChanges(cb);
        }

        public static void DestroyAllDeviceObjects()
        {
            Brick.DestroyDeviceObjects();
            Vase.DestroyDeviceObjects();
            Reflective.DestroyDeviceObjects();
        }
    }
}
