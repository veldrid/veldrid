using System.Linq;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public class MaterialAsset
    {
        public string Name { get; private set; } = "NONAME";
        public string VertexShader { get; private set; } = "NOVERTEXSHADER";
        public string FragmentShader { get; private set; } = "NOFRAGMENTSHADER";
        public MaterialVertexInput VertexInputs { get; private set; }
        public MaterialGlobalInputDescription[] GlobalInputs { get; private set; }
        public MaterialPerObjectInputElement[] PerObjectInputs { get; private set; }
        public ContextTextureInputElement TextureInputs { get; private set; }

        public int[] TestArrayOfInts { get; private set; }

        public Material Create(RenderContext rc)
        {
            var materialTextureInputs = new MaterialTextureInputs(TextureInputs);
            MaterialInputs<MaterialGlobalInputElement> globalInputs = new MaterialInputs<MaterialGlobalInputElement>(GlobalInputs.Select(mgid => mgid.Create(rc)).ToArray());
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(PerObjectInputs);
            return rc.ResourceFactory.CreateMaterial(rc, VertexShader, FragmentShader, VertexInputs, globalInputs, perObjectInputs, materialTextureInputs);
        }
    }

    public class MaterialGlobalInputDescription
    {
        public string Name { get; private set; }
        public MaterialInputType Type { get; private set; }
        public string ProviderName { get; private set; }

        public MaterialGlobalInputElement Create(RenderContext rc)
        {
            return new MaterialGlobalInputElement(Name, Type, rc.DataProviders[ProviderName]);
        }
    }
}
