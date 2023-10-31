using System;
using Veldrid.SPIRV;
using Veldrid.Tests.Utilities;

namespace Veldrid.Tests
{
    internal static class TestShaders
    {
        public static IShaderProvider ShaderProvider { get; set; } = new FileShaderProvider(AppContext.BaseDirectory);

        public static ShaderDescription GetShaderDescription(ShaderStages stage, string name, string entryPoint = "main")
        {
            string path = ShaderProvider.GetPath($"{name}.{stage.ToString().ToLowerInvariant()[..4]}");
            return new ShaderDescription(stage, ShaderProvider.ReadAllBytes(path), "main");
        }

        public static Shader[] LoadVertexFragment(ResourceFactory factory, string setName)
        {
            return factory.CreateFromSpirv(
                GetShaderDescription(ShaderStages.Vertex, setName),
                GetShaderDescription(ShaderStages.Fragment, setName),
                new CrossCompileOptions(false, false, new SpecializationConstant[]
                {
                    new SpecializationConstant(100, false)
                }));
        }

        public static Shader LoadCompute(ResourceFactory factory, string setName)
        {
            return factory.CreateFromSpirv(
                GetShaderDescription(ShaderStages.Compute, setName),
                new CrossCompileOptions(false, false, new SpecializationConstant[]
                {
                    new SpecializationConstant(100, false)
                }));
        }
    }
}
