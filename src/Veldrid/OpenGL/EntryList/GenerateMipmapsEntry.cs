namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocGenerateMipmapsEntry
    {
        public readonly Tracked<Texture> Texture;

        public NoAllocGenerateMipmapsEntry(Tracked<Texture> texture)
        {
            Texture = texture;
        }
    }
}