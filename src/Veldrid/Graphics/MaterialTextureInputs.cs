using System;

namespace Veldrid.Graphics
{
    public class MaterialTextureInputs
    {
        public MaterialTextureInputElement[] Elements { get; }

        public MaterialTextureInputs(MaterialTextureInputElement[] elements)
        {
            Elements = elements;
        }

        public static MaterialTextureInputs Empty { get; private set; }
            = new MaterialTextureInputs(Array.Empty<MaterialTextureInputElement>());
    }

    public class MaterialTextureInputElement
    {
        public string Name { get; }
        public Texture Texture { get; }

        public MaterialTextureInputElement(string name, Texture texture)
        {
            Name = name;
            Texture = texture;
        }
    }
}
