using System;

namespace Veldrid.Graphics
{
    public class ShaderConstantDescription
    {
        public string Name { get; }
        public ShaderConstantType Type { get; }
        public int DataSizeInBytes { get; }

        public ShaderConstantDescription(string name, ShaderConstantType type)
        {
            if (FormatHelpers.GetShaderConstantTypeByteSize(type, out int dataSizeInBytes))
            {
                throw new ArgumentException($"When using ShaderConstantType.{type}, the data size must be given explicitly.");
            }

            Name = name;
            Type = type;
            DataSizeInBytes = dataSizeInBytes;
        }

        public ShaderConstantDescription(string name, ShaderConstantType type, int dataSizeInBytes)
        {
            Name = name;
            Type = type;
            DataSizeInBytes = dataSizeInBytes;
        }
    }
}
