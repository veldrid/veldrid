using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Text;
using System;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLShader : Shader, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly ShaderType _shaderType;
        private readonly StagingBlock _stagingBlock;

        private bool _disposed;
        private string _name;
        private bool _nameChanged;
        public string Name { get => _name; set { _name = value; _nameChanged = true; } }

        private uint _shader;

        public uint Shader => _shader;

        public OpenGLShader(OpenGLGraphicsDevice gd, ShaderStages stage, StagingBlock stagingBlock)
            : base(stage)
        {
            _gd = gd;
            _shaderType = OpenGLFormats.VdToGLShaderType(stage);
            _stagingBlock = stagingBlock;
        }

        public bool Created { get; private set; }

        public void EnsureResourcesCreated()
        {
            if (!Created)
            {
                CreateGLResources();
            }
            if (_nameChanged)
            {
                _nameChanged = false;
                SetObjectLabel(ObjectLabelIdentifier.Shader, _shader, _name);
            }
        }

        private void CreateGLResources()
        {
            _shader = glCreateShader(_shaderType);
            CheckLastError();

            fixed (byte* arrayPtr = &_stagingBlock.Array[0])
            {
                byte* textPtr = arrayPtr;
                int length = (int)_stagingBlock.SizeInBytes;
                byte** textsPtr = &textPtr;

                glShaderSource(_shader, 1, textsPtr, &length);
            }
            CheckLastError();

            glCompileShader(_shader);
            CheckLastError();

            int compileStatus;
            glGetShaderiv(_shader, ShaderParameter.CompileStatus, &compileStatus);
            CheckLastError();

            if (compileStatus != 1)
            {
                int infoLogLength;
                glGetShaderiv(_shader, ShaderParameter.InfoLogLength, &infoLogLength);
                CheckLastError();

                byte* infoLog = stackalloc byte[infoLogLength];
                uint returnedInfoLength;
                glGetShaderInfoLog(_shader, (uint)infoLogLength, &returnedInfoLength, infoLog);
                CheckLastError();

                string message = Encoding.UTF8.GetString(infoLog, (int)returnedInfoLength);

                throw new VeldridException($"Unable to compile shader code for shader [{_name}] of type {_shaderType}: {message}");
            }

            _stagingBlock.Free();
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public void DestroyGLResources()
        {
            if (!_disposed)
            {
                _disposed = true;
                glDeleteShader(_shader);
                CheckLastError();
            }
        }
    }
}