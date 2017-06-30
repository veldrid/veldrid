using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    // TODO: There is a major issue with OpenGL constant bindings.
    // If the value is stored in a uniform location, rather than a uniform block,
    // then data needs to be loaded into the buffer at the time it is bound 
    // (RenderContext.SetConstantBuffer), rather than the time it is used.
    public class OpenGLShaderConstantBindingSlots : ShaderConstantBindingSlots
    {
        private readonly UniformBinding[] _bindings;

        public ShaderConstantDescription[] Constants { get; }

        public OpenGLShaderConstantBindingSlots(
            ShaderSet shaderSet,
            ShaderConstantDescription[] constants)
        {
            Constants = constants;
            var programID = ((OpenGLShaderSet)shaderSet).ProgramID;
            int constantsCount = constants.Length;
            _bindings = new UniformBinding[constantsCount];
            for (int i = 0; i < constantsCount; i++)
            {
                ShaderConstantDescription description = Constants[i];

                int blockIndex = GL.GetUniformBlockIndex(programID, description.Name);
                if (blockIndex != -1)
                {
                    ValidateBlockSize(programID, blockIndex, description.DataSizeInBytes, description.Name);
                    _bindings[i] = new UniformBinding(programID, blockIndex, description.DataSizeInBytes);
                }
                else
                {
                    int uniformLocation = GL.GetUniformLocation(programID, description.Name);
                    if (uniformLocation == -1)
                    {
                        throw new InvalidOperationException($"No uniform or uniform block with name {description.Name} was found.");
                    }

                    OpenGLUniformStorageAdapter storageAdapter = new OpenGLUniformStorageAdapter(programID, uniformLocation);
                    _bindings[i] = new UniformBinding(programID, storageAdapter);
                }
            }
        }

        public UniformBinding GetUniformBindingForSlot(int slot)
        {
            return _bindings[slot];
        }

        [Conditional("DEBUG")]
        private void ValidateBlockSize(int programID, int blockIndex, int providerSize, string elementName)
        {
            int blockSize;
            GL.GetActiveUniformBlock(programID, blockIndex, ActiveUniformBlockParameter.UniformBlockDataSize, out blockSize);

            bool sizeMismatched = (blockSize != providerSize);

            if (sizeMismatched)
            {
                string nameInProgram = GL.GetActiveUniformName(programID, blockIndex);
                bool nameMismatched = nameInProgram != elementName;
                string errorMessage = $"Uniform block validation failed for Program {programID}.";
                if (nameMismatched)
                {
                    errorMessage += Environment.NewLine + $"Expected name: {elementName}, Actual name: {nameInProgram}.";
                }
                if (sizeMismatched)
                {
                    errorMessage += Environment.NewLine + $"Provider size in bytes: {providerSize}, Actual buffer size in bytes: {blockSize}.";
                }

                throw new InvalidOperationException(errorMessage);
            }
        }

        public class UniformBinding
        {
            public int ProgramID { get; }
            public int BlockLocation { get; } = -1;
            public int DataSizeInBytes { get; } = -1;
            public OpenGLUniformStorageAdapter StorageAdapter { get; }

            public UniformBinding(int programID, int blockLocation, int dataSizeInBytes)
            {
                ProgramID = programID;
                BlockLocation = blockLocation;
                DataSizeInBytes = dataSizeInBytes;
            }

            public UniformBinding(int programID, OpenGLUniformStorageAdapter storageAdapter)
            {
                ProgramID = programID;
                StorageAdapter = storageAdapter;
            }
        }
    }
}
