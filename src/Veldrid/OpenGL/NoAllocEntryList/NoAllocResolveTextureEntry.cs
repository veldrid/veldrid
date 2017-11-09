namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocResolveTextureEntry
    {
        public readonly HandleTracked<Texture> Source;
        public readonly HandleTracked<Texture> Destination;

        public NoAllocResolveTextureEntry(Texture source, Texture destination)
        {
            Source = new HandleTracked<Texture>(source);
            Destination = new HandleTracked<Texture>(destination);
        }
    }
}