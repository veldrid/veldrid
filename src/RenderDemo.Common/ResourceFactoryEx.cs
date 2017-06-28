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
            MaterialVertexInput vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            ShaderTextureInput[] textureInputs)
        {
            return CreateMaterial(factory, rc, vertexShaderName, fragmentShaderName, new[] { vertexInputs }, globalInputs, perObjectInputs, textureInputs);
        }

        public static Material CreateMaterial(
            this ResourceFactory factory,
            RenderContext rc,
            string vertexShaderName,
            string fragmentShaderName,
            MaterialVertexInput vertexInputs0,
            MaterialVertexInput vertexInputs1,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            ShaderTextureInput[] textureInputs)
        {
            return CreateMaterial(factory, rc, vertexShaderName, fragmentShaderName, new[] { vertexInputs0, vertexInputs1 }, globalInputs, perObjectInputs, textureInputs);
        }

        public static Material CreateMaterial(
            this ResourceFactory factory,
            RenderContext rc,
            string vertexShaderName,
            string fragmentShaderName,
            MaterialVertexInput[] vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            ShaderTextureInput[] textureInputs)
        {
            Shader vs = factory.CreateShader(ShaderType.Vertex, vertexShaderName);
            Shader fs = factory.CreateShader(ShaderType.Fragment, fragmentShaderName);
            VertexInputLayout inputLayout = factory.CreateInputLayout(vertexInputs);
            ShaderSet shaderSet = factory.CreateShaderSet(inputLayout, vs, fs);
            ShaderConstantBindings constantBindings = factory.CreateShaderConstantBindings(rc, shaderSet, globalInputs, perObjectInputs);
            ShaderTextureBindingSlots textureSlots = factory.CreateShaderTextureBindingSlots(shaderSet, textureInputs);

            return new Material(shaderSet, constantBindings, textureSlots);
        }
    }
}
