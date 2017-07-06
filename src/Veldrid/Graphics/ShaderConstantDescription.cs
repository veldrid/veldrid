using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A description of an individual constant value defined in a shader.
    /// </summary>
    public class ShaderConstantDescription
    {
        /// <summary>
        /// The name of the constant in the shader.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The type of shader constant.
        /// </summary>
        public ShaderConstantType Type { get; }
        /// <summary>
        /// The size of the shader constant, in bytes.
        /// </summary>
        public int DataSizeInBytes { get; }

        /// <summary>
        /// Constructs a new <see cref="ShaderConstantDescription"/> with the given name and type.
        /// </summary>
        /// <param name="name">The name of the constant.</param>
        /// <param name="type">THe type of the constant.</param>
        public ShaderConstantDescription(string name, ShaderConstantType type)
        {
            if (!FormatHelpers.GetShaderConstantTypeByteSize(type, out int dataSizeInBytes))
            {
                throw new ArgumentException($"When using ShaderConstantType.{type}, the data size must be given explicitly.");
            }

            Name = name;
            Type = type;
            DataSizeInBytes = dataSizeInBytes;
        }

        /// <summary>
        /// Constructs a new <see cref="ShaderConstantDescription"/> with the given name and size.
        /// This constructor implies that the type is <see cref="ShaderConstantType.Custom"/>.
        /// </summary>
        /// <param name="name">The name of the constant.</param>
        /// <param name="dataSizeInBytes">The size of the constant data, in bytes.</param>
        public ShaderConstantDescription(string name, int dataSizeInBytes)
        {
            Name = name;
            Type =  ShaderConstantType.Custom;
            DataSizeInBytes = dataSizeInBytes;
        }
    }
}
