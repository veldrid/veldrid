using System.Numerics;
using Vd2;

namespace Vd2.NeoDemo.Objects
{
    public static class CommonMaterials
    {
        public static MaterialPropsAndBuffer Brick { get; }

        static CommonMaterials()
        {
            Brick = new MaterialPropsAndBuffer(new MaterialProperties { SpecularIntensity = new Vector3(0.2f), SpecularPower = 42f }) { Name = "Brick" };
        }

        public static void CreateAllDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            Brick.CreateDeviceObjects(gd, cl, sc);
        }

        public static void FlushAll(CommandList cl)
        {
            Brick.FlushChanges(cl);
        }

        public static void DestroyAllDeviceObjects()
        {
            Brick.DestroyDeviceObjects();
        }
    }
}
