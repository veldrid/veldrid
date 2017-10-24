namespace Vd2.OpenGL
{
    internal class SetPipelineEntry : OpenGLCommandEntry
    {
        private Pipeline pipeline;

        public SetPipelineEntry(Pipeline pipeline)
        {
            this.pipeline = pipeline;
        }
    }
}