namespace Veldrid.OpenGL
{
    internal class OpenGLResourceSet : ResourceSet
    {
        public new OpenGLResourceLayout Layout { get; }
        public BindableResource[] Resources { get; }
        public override string Name { get; set; }

        public OpenGLResourceSet(ref ResourceSetDescription description)
            : base(ref description)
        {
            Layout = Util.AssertSubtype<ResourceLayout, OpenGLResourceLayout>(description.Layout);
            Resources = Util.ShallowClone(description.BoundResources);
        }

        public override void Dispose()
        {
        }
    }
}