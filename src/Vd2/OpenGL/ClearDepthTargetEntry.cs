namespace Vd2.OpenGL
{
    internal class ClearDepthTargetEntry : OpenGLCommandEntry
    {
        public readonly float Depth;

        public ClearDepthTargetEntry(float depth)
        {
            Depth = depth;
        }
    }
}