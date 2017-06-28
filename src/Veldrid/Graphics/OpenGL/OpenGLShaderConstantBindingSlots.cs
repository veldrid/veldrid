using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLShaderConstantBindingSlots : ShaderConstantBindingSlots
    {
        private readonly UniformBinding[] _bindings;

        public ShaderConstantDescription[] Constants { get; }

        public OpenGLShaderConstantBindingSlots(
            RenderContext rc,
            ShaderSet shaderSet,
            ShaderConstantDescription[] constants)
        {
            Constants = constants;
            var programID = ((OpenGLShaderSet)shaderSet).ProgramID;
            int bindingIndex = 0;
            int constantsCount = constants.Length;
            _bindings = new UniformBinding[constantsCount];
            for (int i = 0; i < constantsCount; i++)
            {
                ShaderConstantDescription description = Constants[i];

                int blockIndex = GL.GetUniformBlockIndex(programID, description.Name);
                if (blockIndex != -1)
                {
                    ValidateBlockSize(programID, blockIndex, description.DataSizeInBytes, description.Name);
                    _bindings[i] = new UniformBlockBinding(
                        programID,
                        blockIndex,
                        bindingIndex,
                        description.DataSizeInBytes);
                    bindingIndex += 1;
                }
                else
                {
                    int uniformLocation = GL.GetUniformLocation(programID, description.Name);
                    if (uniformLocation == -1)
                    {
                        throw new InvalidOperationException($"No uniform or uniform block with name {description.Name} was found.");
                    }

                    _bindings[i] = new UniformLocationBinding(
                        programID,
                        uniformLocation);
                }
            }
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

        public abstract class UniformBinding
        {
            public int ProgramID { get; }

            public UniformBinding(int programID)
            {
                ProgramID = programID;
            }

            public abstract void Bind(OpenGLConstantBuffer cb);
        }

        [DebuggerDisplay("Prog:{ProgramID} BlockInd:{BlockIndex} BindingInd:{BindingIndex}")]
        private class UniformBlockBinding : UniformBinding
        {
            private readonly int _dataSizeInBytes;

            public int BlockIndex { get; }
            public int BindingIndex { get; }

            public UniformBlockBinding(
                int programID,
                int blockIndex,
                int bindingIndex,
                int dataSizeInBytes)
                : base(programID)
            {
                _dataSizeInBytes = dataSizeInBytes;
                BlockIndex = blockIndex;
                BindingIndex = bindingIndex;
            }

            public override void Bind(OpenGLConstantBuffer cb)
            {
                cb.BindToBlock(ProgramID, BlockIndex, _dataSizeInBytes, BindingIndex);
            }
        }

        private class UniformLocationBinding : UniformBinding
        {
            public int UniformLocation { get; }

            public UniformLocationBinding(
                int programID,
                int uniformLocation) : base(programID)
            {
                UniformLocation = uniformLocation;
            }


            public override void Bind(OpenGLConstantBuffer cb)
            {
                throw new NotImplementedException();
            }
        }
    }
}