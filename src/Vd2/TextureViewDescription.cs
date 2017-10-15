namespace Vd2
{
    public struct TextureViewDescription
    {
        public Texture Target;

        public TextureViewDescription(Texture target)
        {
            Target = target;
        }
    }
}