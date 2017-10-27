namespace Veldrid.OpenGL
{
    internal class OpenGLResourceSet : ResourceSet
    {
        public ResourceLayout Layout { get; }
        public BindableResource[] Resources { get; }

        public OpenGLResourceSet(ref ResourceSetDescription description)
        {
            Layout = description.Layout;
            Resources = description.BoundResources;
        }

        public override void Dispose()
        {
        }
    }
}