using System;

namespace Veldrid
{
    public struct ShaderDescription : IEquatable<ShaderDescription>
    {
        public ShaderStages Stage;
        public byte[] ShaderBytes;

        public ShaderDescription(ShaderStages stage, byte[] shaderBytes)
        {
            Stage = stage;
            ShaderBytes = shaderBytes;
        }

        public bool Equals(ShaderDescription other)
        {
            return Stage == other.Stage && ShaderBytes == other.ShaderBytes;
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(Stage.GetHashCode(), ShaderBytes.GetHashCode());
        }
    }
}