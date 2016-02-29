namespace Veldrid.Graphics
{
    public class MaterialTextureInputs
    {
        public MaterialTextureInputElement[] Elements { get; }

        public MaterialTextureInputs(MaterialTextureInputElement[] elements)
        {
            Elements = elements;
        }
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
