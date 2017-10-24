namespace Vd2.OpenGL
{
    internal class SetIndexBufferEntry : OpenGLCommandEntry
    {
        public readonly IndexBuffer IndexBuffer;

        public SetIndexBufferEntry(IndexBuffer ib)
        {
            IndexBuffer = ib;
        }
    }
}