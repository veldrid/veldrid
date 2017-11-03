namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class ClearDepthTargetEntry : OpenGLCommandEntry
    {
        public float Depth;

        public ClearDepthTargetEntry(float depth)
        {
            Depth = depth;
        }

        public ClearDepthTargetEntry() { }

        public ClearDepthTargetEntry Init(float depth)
        {
            Depth = depth;
            return this;
        }

        public override void ClearReferences()
        {
        }
    }
}