namespace Veldrid.WebGL
{
    internal class WebGLResourceSet : ResourceSet
    {
        public new WebGLResourceLayout Layout { get; }
        public new BindableResource[] Resources { get; }
        public override string Name { get; set; }

        public WebGLResourceSet(WebGLGraphicsDevice gd, ref ResourceSetDescription description)
            : base(ref description)
        {
            Layout = Util.AssertSubtype<ResourceLayout, WebGLResourceLayout>(description.Layout);
            Resources = Util.ShallowClone(description.BoundResources);
        }

        public override void Dispose()
        {
        }
    }
}
