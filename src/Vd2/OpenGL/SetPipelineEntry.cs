namespace Vd2.OpenGL
{
    internal class SetPipelineEntry : OpenGLCommandEntry
    {
        public readonly Pipeline Pipeline;

        public SetPipelineEntry(Pipeline pipeline)
        {
            Pipeline = pipeline;
        }
    }
}