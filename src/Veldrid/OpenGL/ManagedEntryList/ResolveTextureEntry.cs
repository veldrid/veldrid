namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class ResolveTextureEntry : OpenGLCommandEntry
    {
        public Texture Source;
        public Texture Destination;

        public ResolveTextureEntry(Texture source, Texture destination)
        {
            Source = source;
            Destination = destination;
        }

        public ResolveTextureEntry() { }

        public ResolveTextureEntry Init(Texture source, Texture destination)
        {
            Source = source;
            Destination = destination;
            return this;
        }

        public override void ClearReferences()
        {
            Source = null;
            Destination = null;
        }
    }
}