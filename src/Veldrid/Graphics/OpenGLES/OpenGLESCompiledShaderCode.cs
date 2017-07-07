namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESCompiledShaderCode : CompiledShaderCode
    {
        public string ShaderCode { get; }

        public OpenGLESCompiledShaderCode(string shaderCode)
        {
            ShaderCode = shaderCode;
        }
    }
}
