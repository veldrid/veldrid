namespace Veldrid.OpenGL
{
    internal class OpenGLResourceLayout : ResourceLayout
    {
        public ResourceLayoutElementDescription[] Elements { get; }

        public override string Name { get; set; }

        public OpenGLResourceLayout(ref ResourceLayoutDescription description)
            : base(ref description)
        {
            Elements = Util.ShallowClone(description.Elements);
        }

        public bool IsDynamicBuffer(uint slot)
        {
            return (Elements[slot].Options & ResourceLayoutElementOptions.DynamicBinding) != 0;
        }

        public override void Dispose()
        {
        }
    }
}
