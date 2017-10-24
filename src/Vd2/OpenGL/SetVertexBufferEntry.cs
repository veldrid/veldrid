namespace Vd2.OpenGL
{
    internal class SetVertexBufferEntry : OpenGLCommandEntry
    {
        public readonly uint Index;
        public readonly VertexBuffer VertexBuffer;

        public SetVertexBufferEntry(uint index, VertexBuffer vb)
        {
            Index = index;
            VertexBuffer = vb;
        }
    }
}