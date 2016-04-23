using System;

namespace Veldrid.Graphics
{
    public class MaterialTextureInputs
    {
        public MaterialTextureInputElement[] Elements { get; }

        public MaterialTextureInputs(params MaterialTextureInputElement[] elements)
        {
            Elements = elements;
        }

        public static MaterialTextureInputs Empty { get; private set; }
            = new MaterialTextureInputs(Array.Empty<MaterialTextureInputElement>());
    }

    public abstract class MaterialTextureInputElement
    {
        public string Name { get; set; }

        public abstract DeviceTexture GetDeviceTexture(RenderContext rc);

        protected MaterialTextureInputElement(string name)
        {
            Name = name;
        }
    }

    public class TextureDataInputElement : MaterialTextureInputElement
    {
        public TextureData TextureData { get; }

        public TextureDataInputElement(string name, TextureData textureData)
            : base(name)
        {
            TextureData = textureData;
        }

        public override DeviceTexture GetDeviceTexture(RenderContext rc)
        {
            return TextureData.CreateDeviceTexture(rc.ResourceFactory);
        }
    }

    public class ContextTextureInputElement : MaterialTextureInputElement
    {
        public string ContextKey { get; set; }

        public ContextTextureInputElement() : this("<NONAME>") { }
        public ContextTextureInputElement(string name) : this(name, name) { }
        public ContextTextureInputElement(string name, string contextKey) : base(name)
        {
            ContextKey = contextKey;
        }

        public override DeviceTexture GetDeviceTexture(RenderContext rc)
        {
            return rc.GetTextureContextBinding(ContextKey).Value;
        }
    }
}
