namespace Vd2.OpenGL
{
    internal class OpenGLPipeline : Pipeline
    {
        private PipelineDescription description;

        public OpenGLPipeline(ref PipelineDescription description)
        {
            this.description = description;
        }

        public override void Dispose()
        {
        }
    }
}