namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetPipelineEntry : OpenGLCommandEntry
    {
        public Pipeline Pipeline;

        public SetPipelineEntry(Pipeline pipeline)
        {
            Pipeline = pipeline;
        }

        public SetPipelineEntry() { }

        public SetPipelineEntry Init(Pipeline pipeline)
        {
            Pipeline = pipeline;
            return this;
        }

        public override void ClearReferences()
        {
            Pipeline = null;
        }
    }
}