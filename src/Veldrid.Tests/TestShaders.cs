using System;
using System.IO;
using Veldrid.SPIRV;

namespace Veldrid.Tests
{
    internal static class TestShaders
    {
        public static Shader[] LoadVertexFragment(ResourceFactory factory, string setName, bool loadSpirv = false)
        {
            return factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes(GetPath(setName, ShaderStages.Vertex, loadSpirv)), "main"),
                new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes(GetPath(setName, ShaderStages.Fragment, loadSpirv)), "main"),
                new CrossCompileOptions(false, false, new SpecializationConstant[]
                {
                    new SpecializationConstant(100, false)
                }));
        }

        public static Shader LoadCompute(ResourceFactory factory, string setName)
        {
            return factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Compute, File.ReadAllBytes(GetPath(setName, ShaderStages.Compute, false)), "main"),
                new CrossCompileOptions(false, false, new SpecializationConstant[]
                {
                    new SpecializationConstant(100, false)
                }));
        }

        public static string GetPath(string setName, ShaderStages stage, bool loadSpirv)
        {
            string extension = loadSpirv ? ".spv" : string.Empty;
            return Path.Combine(
                AppContext.BaseDirectory,
                "Shaders",
                $"{setName}.{stage.ToString().ToLowerInvariant().Substring(0, 4)}{extension}");
        }
    }
}
