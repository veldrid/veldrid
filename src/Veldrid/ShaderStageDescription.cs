using System;

namespace Veldrid
{
    public struct ShaderStageDescription : IEquatable<ShaderStageDescription>
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

        public bool Equals(ShaderStageDescription other)
        {
            return Stage == other.Stage && Shader.Equals(other.Shader) && EntryPoint.Equals(other.EntryPoint);
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(Stage.GetHashCode(), Shader.GetHashCode(), EntryPoint.GetHashCode());
        }
    }
}
