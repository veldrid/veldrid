using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class ResourceFactoryEx
    {
        public static Material CreateMaterial(
            this ResourceFactory factory,
            RenderContext rc,
            string vertexShaderName,
            string fragmentShaderName,
            VertexInputDescription vertexInputs,
            ShaderConstantDescription[] constants,
            ShaderTextureInput[] textureInputs)
        {
            return CreateMaterial(factory, rc, vertexShaderName, fragmentShaderName, new[] { vertexInputs }, constants, textureInputs);
        }

        public static Material CreateMaterial(
            this ResourceFactory factory,
            RenderContext rc,
            string vertexShaderName,
            string fragmentShaderName,
            VertexInputDescription vertexInputs0,
            VertexInputDescription vertexInputs1,
            ShaderConstantDescription[] constants,
            ShaderTextureInput[] textureInputs)
        {
            return CreateMaterial(factory, rc, vertexShaderName, fragmentShaderName, new[] { vertexInputs0, vertexInputs1 }, constants, textureInputs);
        }

        public static Material CreateMaterial(
            this ResourceFactory factory,
            RenderContext rc,
            string vertexShaderName,
            string fragmentShaderName,
            VertexInputDescription[] vertexInputs,
            ShaderConstantDescription[] constants,
            ShaderTextureInput[] textureInputs)
        {
            Shader vs = factory.CreateShader(ShaderType.Vertex, vertexShaderName);
            Shader fs = factory.CreateShader(ShaderType.Fragment, fragmentShaderName);
            VertexInputLayout inputLayout = factory.CreateInputLayout(vertexInputs);
            ShaderSet shaderSet = factory.CreateShaderSet(inputLayout, vs, fs);
            ShaderConstantBindingSlots constantBindings = factory.CreateShaderConstantBindingSlots(shaderSet, constants);
            ShaderTextureBindingSlots textureSlots = factory.CreateShaderTextureBindingSlots(shaderSet, textureInputs);

            return new Material(shaderSet, constantBindings, textureSlots);
        }
    }
}
