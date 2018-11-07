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
            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vsBytes, "main", debug),
                new ShaderDescription(ShaderStages.Fragment, fsBytes, "main", debug),
                GetOptions(gd));

            Shader vs = shaders[0];
            Shader fs = shaders[1];

            vs.Name = setName + "-Vertex";
            fs.Name = setName + "-Fragment";

            return (vs, fs);
        }

        private static CrossCompileOptions GetOptions(GraphicsDevice gd)
        {
            SpecializationConstant[] specializations = GetSpecializations(gd);

            bool fixClipZ = (gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
                && !gd.IsDepthRangeZeroToOne;
            bool invertY = false;

            return new CrossCompileOptions(fixClipZ, invertY, specializations);
        }

        public static SpecializationConstant[] GetSpecializations(GraphicsDevice gd)
        {
            bool glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;

            List<SpecializationConstant> specializations = new List<SpecializationConstant>();
            specializations.Add(new SpecializationConstant(100, gd.IsClipSpaceYInverted));
            specializations.Add(new SpecializationConstant(101, glOrGles)); // TextureCoordinatesInvertedY
            specializations.Add(new SpecializationConstant(102, gd.IsDepthRangeZeroToOne));

            PixelFormat swapchainFormat = gd.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
            bool swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb
                || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
            specializations.Add(new SpecializationConstant(103, swapchainIsSrgb));

            return specializations.ToArray();
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
