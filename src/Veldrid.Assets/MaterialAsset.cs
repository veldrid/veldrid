using System.Linq;
using Veldrid.Graphics;

namespace Veldrid.Assets
{
    public class MaterialAsset
    {
        public string Name { get; set; } = "NONAME";
        public string VertexShader { get; set; } = "NOVERTEXSHADER";
        public string FragmentShader { get; set; } = "NOFRAGMENTSHADER";
        public MaterialVertexInput VertexInputs { get; set; } = new MaterialVertexInput();
        public MaterialGlobalInputDescription[] GlobalInputs { get; set; } = new MaterialGlobalInputDescription[0];
        public MaterialPerObjectInputElement[] PerObjectInputs { get; set; } = new MaterialPerObjectInputElement[0];
        public ContextTextureInputElement[] ContextTextures { get; set; } = new ContextTextureInputElement[0];

        public Material Create(RenderContext rc, params MaterialTextureInputElement[] additionalTextureInputs)
        {
            var allTextures = additionalTextureInputs.Concat(ContextTextures).ToArray();
            var materialTextureInputs = new MaterialTextureInputs(allTextures);
            MaterialInputs<MaterialGlobalInputElement> globalInputs = new MaterialInputs<MaterialGlobalInputElement>(GlobalInputs.Select(mgid => mgid.Create(rc)).ToArray());
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs = new MaterialInputs<MaterialPerObjectInputElement>(PerObjectInputs);
            return rc.ResourceFactory.CreateMaterial(rc, VertexShader, FragmentShader, VertexInputs, globalInputs, perObjectInputs, materialTextureInputs);
        }
    }

    public class MaterialGlobalInputDescription
    {
        public string Name { get; set; }
        public MaterialInputType Type { get; set; }
        public string ProviderName { get; set; }

        public MaterialGlobalInputElement Create(RenderContext rc)
        {
            return new MaterialGlobalInputElement(Name, Type, rc.DataProviders[ProviderName]);
        }
    }
}
