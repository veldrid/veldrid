namespace Vd2
{
    public struct ShaderStageDescription
    {
        public ShaderStages Stage;
        public Shader Shader;
        public string EntryPoint; // Vulkan requires entry point name when a Pipeline is created with bytecode.

        public ShaderStageDescription(ShaderStages stage, Shader shader, string entryPoint)
        {
            Stage = stage;
            Shader = shader;
            EntryPoint = entryPoint;
        }
    }
}
