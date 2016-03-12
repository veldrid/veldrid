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
        public TextureData TextureData { get; }

        public MaterialTextureInputElement(string name, TextureData textureData)
        {
            Name = name;
            TextureData = textureData;
        }
    }
}
