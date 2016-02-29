using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLShader : IDisposable
    {
        private int _shaderID;

        public OpenGLShader(string source, ShaderType type)
        {
            _shaderID = GL.CreateShader(type);
            GL.ShaderSource(_shaderID, source);
            GL.CompileShader(_shaderID);
            int compileStatus;
            GL.GetShader(_shaderID, ShaderParameter.CompileStatus, out compileStatus);
            if (compileStatus != 1)
            {
                string shaderLog = GL.GetShaderInfoLog(_shaderID);
                throw new InvalidOperationException($"Error compiling {type} shader. {shaderLog}");
            }
        }

        public int ID => _shaderID;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
