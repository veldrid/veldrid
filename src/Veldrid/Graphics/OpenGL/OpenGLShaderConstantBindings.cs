using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLShaderConstantBindings : ShaderConstantBindings
    {
        private readonly GlobalBindingPair[] _globalUniformBindings;
        private readonly UniformBinding[] _perObjectBindings;

        public OpenGLShaderConstantBindings(
            RenderContext rc,
            ShaderSet shaderSet, 
            MaterialInputs<MaterialGlobalInputElement> globalInputs, 
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs)
        {
            var programID = ((OpenGLShaderSet)shaderSet).ProgramID;
            int globalInputsCount = globalInputs.Elements.Length;
            int bindingIndex = 0;
            _globalUniformBindings = new GlobalBindingPair[globalInputsCount];
            for (int i = 0; i < globalInputsCount; i++)
            {
                var element = globalInputs.Elements[i];

                ConstantBufferDataProvider dataProvider = element.UseGlobalNamedBuffer
                    ? rc.GetNamedGlobalBufferProviderPair(element.GlobalProviderName).DataProvider
                    : element.DataProvider;
                int blockIndex = GL.GetUniformBlockIndex(programID, element.Name);
                if (blockIndex != -1)
                {
                    ValidateBlockSize(programID, blockIndex, dataProvider.DataSizeInBytes);
                    _globalUniformBindings[i] = new GlobalBindingPair(
                        new UniformBlockBinding(
                            programID,
                            blockIndex,
                            bindingIndex,
                            new OpenGLConstantBuffer(dataProvider),
                            dataProvider.DataSizeInBytes),
                        dataProvider);
                    bindingIndex += 1;
                }
                else
                {
                    int uniformLocation = GL.GetUniformLocation(programID, element.Name);
                    if (uniformLocation == -1)
                    {
                        throw new InvalidOperationException($"No uniform or uniform block with name {element.Name} was found.");
                    }

                    _globalUniformBindings[i] = new GlobalBindingPair(
                        new UniformLocationBinding(
                            programID,
                            uniformLocation),
                        dataProvider);
                }
            }

            int perObjectInputsCount = perObjectInputs.Elements.Length;
            _perObjectBindings = new UniformBinding[perObjectInputsCount];
            for (int i = 0; i < perObjectInputsCount; i++)
            {
                var element = perObjectInputs.Elements[i];

                int blockIndex = GL.GetUniformBlockIndex(programID, element.Name);
                if (blockIndex != -1)
                {
                    _perObjectBindings[i] = new UniformBlockBinding(
                        programID,
                        blockIndex,
                        bindingIndex,
                        new OpenGLConstantBuffer(),
                        element.BufferSizeInBytes);
                    bindingIndex += 1;
                }
                else
                {
                    int uniformLocation = GL.GetUniformLocation(programID, element.Name);
                    if (uniformLocation == -1)
                    {
                        throw new InvalidOperationException($"No uniform or uniform block with name {element.Name} was found.");
                    }

                    _perObjectBindings[i] = new UniformLocationBinding(
                        programID,
                        uniformLocation);
                }
            }
        }

        public void Apply()
        {
            foreach (var globalBinding in _globalUniformBindings)
            {
                globalBinding.Bind();
            }
            foreach (var perObjectBinding in _perObjectBindings)
            {
                perObjectBinding.Bind();
            }
        }

        public void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider)
        {
            _perObjectBindings[0].SetData(dataProvider);
        }

        public void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders)
        {
            for (int i = 0; i < dataProviders.Length; i++)
            {
                _perObjectBindings[i].SetData(dataProviders[i]);
            }
        }

        public void Dispose()
        {
            foreach (var globalBinding in _globalUniformBindings)
            {
                globalBinding.Binding.Dispose();
            }

            foreach (var perObjectbinding in _perObjectBindings)
            {
                perObjectbinding.Dispose();
            }
        }

        [Conditional("DEBUG")]
        private void ValidateBlockSize(int programID, int blockIndex, int providerSize)
        {
            int blockSize;
            GL.GetActiveUniformBlock(programID, blockIndex, ActiveUniformBlockParameter.UniformBlockDataSize, out blockSize);

            if (blockSize != providerSize)
            {
                throw new InvalidOperationException(
                    $"Declared shader uniform block size does not match provider's data size. The provider has size {providerSize}, but the buffer has size {blockSize}.");
            }
        }

        private abstract class UniformBinding : IDisposable
        {
            public int ProgramID { get; }

            public UniformBinding(int programID)
            {
                ProgramID = programID;
            }

            public abstract void Bind();
            public abstract void SetData(ConstantBufferDataProvider dataProvider);
            public abstract void Dispose();
        }

        [DebuggerDisplay("Prog:{ProgramID} BlockInd:{BlockIndex} BindingInd:{BindingIndex}")]
        private class UniformBlockBinding : UniformBinding
        {
            private readonly int _dataSizeInBytes;

            public int BlockIndex { get; }
            public int BindingIndex { get; }
            public OpenGLConstantBuffer ConstantBuffer { get; }

            public UniformBlockBinding(
                int programID,
                int blockIndex,
                int bindingIndex,
                OpenGLConstantBuffer constantBuffer,
                int dataSizeInBytes)
                : base(programID)
            {
                _dataSizeInBytes = dataSizeInBytes;
                BlockIndex = blockIndex;
                BindingIndex = bindingIndex;
                ConstantBuffer = constantBuffer;
            }

            public override void Bind()
            {
                ConstantBuffer.BindToBlock(ProgramID, BlockIndex, _dataSizeInBytes, BindingIndex);
            }

            public override void SetData(ConstantBufferDataProvider dataProvider)
            {
                dataProvider.SetData(ConstantBuffer);
            }

            public override void Dispose()
            {
                ConstantBuffer.Dispose();
            }
        }

        private class UniformLocationBinding : UniformBinding
        {
            public OpenGLUniformStorageAdapter StorageAdapter { get; }

            public UniformLocationBinding(
                int programID,
                int uniformLocation) : base(programID)
            {
                StorageAdapter = new OpenGLUniformStorageAdapter(ProgramID, uniformLocation);
            }

            public override void Bind()
            {
            }

            public override void SetData(ConstantBufferDataProvider dataProvider)
            {
                dataProvider.SetData(StorageAdapter);
            }

            public override void Dispose()
            {
            }
        }

        private struct GlobalBindingPair
        {
            public UniformBinding Binding { get; }
            public ConstantBufferDataProvider DataProvider { get; }

            public GlobalBindingPair(UniformBinding binding, ConstantBufferDataProvider dataProvider)
            {
                Binding = binding;
                DataProvider = dataProvider;
            }

            public void Bind()
            {
                Binding.Bind();
                Binding.SetData(DataProvider);
            }
        }
    }
}