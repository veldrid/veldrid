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

        private bool _disposeRequested;
        private bool _disposed;
        private string _name;
        private bool _nameChanged;
        public override string Name { get => _name; set { _name = value; _nameChanged = true; } }
        public override bool IsDisposed => _disposeRequested;

        private uint _shader;

        public uint Shader => _shader;

        public OpenGLShader(OpenGLGraphicsDevice gd, ShaderStages stage, StagingBlock stagingBlock, string entryPoint)
            : base(stage, entryPoint)
        {
#if VALIDATE_USAGE
            if (stage == ShaderStages.Compute && !gd.Extensions.ComputeShaders)
            {
                if (_gd.BackendType == GraphicsBackend.OpenGLES)
                {
                    throw new VeldridException("Compute shaders require OpenGL ES 3.1.");
                }
                else
                {
                    throw new VeldridException($"Compute shaders require OpenGL 4.3 or ARB_compute_shader.");
                }
            }
#endif
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
                if (_gd.Extensions.KHR_Debug)
                {
                    SetObjectLabel(ObjectLabelIdentifier.Shader, _shader, _name);
                }
            }
        }

        private void CreateGLResources()
        {
            _shader = glCreateShader(_shaderType);
            CheckLastError();

            byte* textPtr = (byte*)_stagingBlock.Data;
            int length = (int)_stagingBlock.SizeInBytes;
            byte** textsPtr = &textPtr;

            glShaderSource(_shader, 1, textsPtr, &length);
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

                string message = infoLog != null
                    ? Encoding.UTF8.GetString(infoLog, (int)returnedInfoLength)
                    : "<null>";

                throw new VeldridException($"Unable to compile shader code for shader [{_name}] of type {_shaderType}: {message}");
            }

            _gd.StagingMemoryPool.Free(_stagingBlock);
            Created = true;
        }

        public override void Dispose()
        {
            if (!_disposeRequested)
            {
                _disposeRequested = true;
                _gd.EnqueueDisposal(this);
            }
        }

        public void DestroyGLResources()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (Created)
                {
                    glDeleteShader(_shader);
                    CheckLastError();
                }
                else
                {
                    _gd.StagingMemoryPool.Free(_stagingBlock);
                }
            }
        }
    }
}
