using static Vd2.OpenGLBinding.OpenGLNative;
using static Vd2.OpenGL.OpenGLUtil;
using Vd2.OpenGLBinding;
using System.Text;
using System;

namespace Vd2.OpenGL
{
    internal unsafe class OpenGLShader : Shader, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly ShaderType _shaderType;
        private readonly StagingBlock _stagingBlock;

        private uint _shader;

        public uint Shader => _shader;

        public OpenGLShader(OpenGLGraphicsDevice gd, ShaderStages stage, StagingBlock stagingBlock)
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
        }

        private void CreateGLResources()
        {
            _shader = glCreateShader(_shaderType);
            CheckLastError();

            byte* textPtr = (byte*)_stagingBlock.Data.ToPointer();
            int length = (int)_stagingBlock.SizeInBytes;
            byte** textsPtr = &textPtr;

            glShaderSource(_shader, 1, textsPtr, &length);
            CheckLastError();

            int compileStatus;
            glGetShaderiv(_shader, ShaderParameter.CompileStatus, &compileStatus);
            CheckLastError();

            if (compileStatus != 1)
            {
                byte* infoLog = stackalloc byte[4096];
                uint returnedInfoLength;
                glGetShaderInfoLog(_shader, 4096, &returnedInfoLength, infoLog);
                CheckLastError();

                string message = Encoding.UTF8.GetString(infoLog, (int)returnedInfoLength);

                throw new VdException("Unabled to compile shader code: " + message);
            }

            _stagingBlock.Free();
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public void DestroyGLResources()
        {
            glDeleteShader(_shader);
            CheckLastError();
        }
    }
}