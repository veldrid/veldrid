using System;
using System.IO;
using System.Reflection;
using Veldrid.SPIRV;

namespace Veldrid.Tests
{
    internal static class TestShaders
    {
        private static byte[] ReadResourceBytes(string resourceName)
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Veldrid.Tests.Shaders.{resourceName}"))
            {
                stream.Seek(0, SeekOrigin.End);
                var len = (int)stream.Position;
                stream.Seek(0, SeekOrigin.Begin);

                using(var reader = new BinaryReader(stream))
                {
                    return reader.ReadBytes(len);
                }
            }
        }

        private static string GetResourceName(string setName, ShaderStages stage) => $"{setName}.{stage.ToString().ToLowerInvariant().Substring(0, 4)}";

        public static Shader[] LoadVertexFragment(ResourceFactory factory, string setName)
        {
            return factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, ReadResourceBytes(GetResourceName(setName, ShaderStages.Vertex)), "main"),
                new ShaderDescription(ShaderStages.Fragment, ReadResourceBytes(GetResourceName(setName, ShaderStages.Fragment)), "main"),
                new CrossCompileOptions(false, false, new SpecializationConstant[]
                {
                    new SpecializationConstant(100, false)
                }));
        }

        public static Shader LoadCompute(ResourceFactory factory, string setName)
        {
            return factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Compute, ReadResourceBytes(GetResourceName(setName, ShaderStages.Compute)), "main"),
                new CrossCompileOptions(false, false, new SpecializationConstant[]
                {
                    new SpecializationConstant(100, false)
                }));
        }
    }
}
