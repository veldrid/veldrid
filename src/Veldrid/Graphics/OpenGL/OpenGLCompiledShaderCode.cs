namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLCompiledShaderCode : CompiledShaderCode
    {
        public string ShaderCode { get; }

        public OpenGLCompiledShaderCode(string shaderCode)
        {
            ShaderCode = shaderCode;
        }
    }
}
