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
        public string Name { get; }

        public abstract DeviceTexture GetDeviceTexture(RenderContext rc);

        public MaterialTextureInputElement(string name)
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
        public ContextTextureInputElement(string name) : base(name)
        {
        }

        public override DeviceTexture GetDeviceTexture(RenderContext rc)
        {
            return rc.GetTextureContextBinding(Name).Value;
        }
    }
}
