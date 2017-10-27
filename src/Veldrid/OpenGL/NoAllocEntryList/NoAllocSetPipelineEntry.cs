namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetPipelineEntry
    {
        public readonly HandleTracked<Pipeline> Pipeline;

        public NoAllocSetPipelineEntry(Pipeline pipeline)
        {
            Pipeline = new HandleTracked<Pipeline>(pipeline);
        }
    }
}