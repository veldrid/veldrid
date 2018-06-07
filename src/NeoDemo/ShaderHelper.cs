using System;
using System.Collections.Generic;
using System.IO;
using Veldrid.SPIRV;

namespace Veldrid.NeoDemo
{
    public static class ShaderHelper
    {
        public static (Shader vs, Shader fs) LoadSPIRV(
            GraphicsDevice gd,
            ResourceFactory factory,
            string setName)
        {
            byte[] vsBytes = LoadBytecode(GraphicsBackend.Vulkan, setName, ShaderStages.Vertex);
            byte[] fsBytes = LoadBytecode(GraphicsBackend.Vulkan, setName, ShaderStages.Fragment);
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            (Shader vs, Shader fs) = factory.CreateFromSPIRV(
                new ShaderDescription(ShaderStages.Vertex, vsBytes, "main", debug),
                new ShaderDescription(ShaderStages.Fragment, fsBytes, "main", debug),
                GetOptions(gd));

            vs.Name = setName + "-Vertex";
            fs.Name = setName + "-Fragment";

            return (vs, fs);
        }

        private static CompilationOptions GetOptions(
            GraphicsDevice gd)
        {
            bool fixClip = false;
            bool invertY = false;
            List<SpecializationConstant> specializations = new List<SpecializationConstant>();
            specializations.Add(SpecializationConstant.Create(102, gd.IsDepthRangeZeroToOne));
            switch (gd.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                case GraphicsBackend.Metal:
                    specializations.Add(SpecializationConstant.Create(100, false));
                    break;
                case GraphicsBackend.Vulkan:
                    specializations.Add(SpecializationConstant.Create(100, true));
                    break;
                case GraphicsBackend.OpenGL:
                case GraphicsBackend.OpenGLES:
                    specializations.Add(SpecializationConstant.Create(100, false));
                    specializations.Add(SpecializationConstant.Create(101, true));
                    fixClip = !gd.IsDepthRangeZeroToOne;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new CompilationOptions(fixClip, invertY, specializations.ToArray());
        }

        public static Shader LoadShader(
            GraphicsDevice gd,
            ResourceFactory factory,
            string setName,
            ShaderStages stage,
            string entryPoint)
        {
            if (factory.BackendType != GraphicsBackend.Vulkan) { throw new NotImplementedException(); }

            Shader shader = factory.CreateShader(new ShaderDescription(stage, LoadBytecode(factory.BackendType, setName, stage), entryPoint));
            shader.Name = $"{setName}-{stage.ToString()}";
            return shader;
        }

        public static byte[] LoadBytecode(GraphicsBackend backend, string setName, ShaderStages stage)
        {
            string stageExt = stage == ShaderStages.Vertex ? "vert" : "frag";
            string name = setName + "." + stageExt;

            if (backend == GraphicsBackend.Vulkan || backend == GraphicsBackend.Direct3D11)
            {
                string bytecodeExtension = GetBytecodeExtension(backend);
                string bytecodePath = AssetHelper.GetPath(Path.Combine("Shaders", name + bytecodeExtension));
                if (File.Exists(bytecodePath))
                {
                    return File.ReadAllBytes(bytecodePath);
                }
            }

            string extension = GetSourceExtension(backend);
            string path = AssetHelper.GetPath(Path.Combine("Shaders.Generated", name + extension));
            return File.ReadAllBytes(path);
        }

        private static string GetBytecodeExtension(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11: return ".hlsl.bytes";
                case GraphicsBackend.Vulkan: return ".spv";
                case GraphicsBackend.OpenGL:
                    throw new InvalidOperationException("OpenGL and OpenGLES do not support shader bytecode.");
                default: throw new InvalidOperationException("Invalid Graphics backend: " + backend);
            }
        }

        private static string GetSourceExtension(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11: return ".hlsl";
                case GraphicsBackend.Vulkan: return ".450.glsl";
                case GraphicsBackend.OpenGL:
                    return ".330.glsl";
                case GraphicsBackend.OpenGLES:
                    return ".300.glsles";
                case GraphicsBackend.Metal:
                    return ".metallib";
                default: throw new InvalidOperationException("Invalid Graphics backend: " + backend);
            }
        }
    }
}
