﻿using System;
using System.Collections.Generic;
using Veldrid.ImageSharp;

namespace Veldrid.NeoDemo
{
    // Non-thread-safe cache for resources.
    internal static class StaticResourceCache
    {
        private static readonly Dictionary<PipelineDescription, Pipeline> s_pipelines
            = new Dictionary<PipelineDescription, Pipeline>();

        private static readonly Dictionary<ResourceLayoutDescription, ResourceLayout> s_layouts
            = new Dictionary<ResourceLayoutDescription, ResourceLayout>();

        private static readonly Dictionary<(string, ShaderStages), Shader> s_shaders
            = new Dictionary<(string, ShaderStages), Shader>();

        private static readonly Dictionary<ImageSharpTexture, Texture> s_textures
            = new Dictionary<ImageSharpTexture, Texture>();

        private static readonly Dictionary<Texture, TextureView> s_textureViews = new Dictionary<Texture, TextureView>();

        private static readonly Dictionary<ResourceSetDescription, ResourceSet> s_resourceSets
            = new Dictionary<ResourceSetDescription, ResourceSet>();

        private static Texture _pinkTex;

        public static readonly ResourceLayoutDescription ProjViewLayoutDescription = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex));

        public static Pipeline GetPipeline(ResourceFactory factory, ref PipelineDescription desc)
        {
            if (!s_pipelines.TryGetValue(desc, out Pipeline p))
            {
                p = factory.CreatePipeline(ref desc);
                s_pipelines.Add(desc, p);
            }

            return p;
        }

        public static ResourceLayout GetResourceLayout(ResourceFactory factory, ResourceLayoutDescription desc)
        {
            if (!s_layouts.TryGetValue(desc, out ResourceLayout p))
            {
                p = factory.CreateResourceLayout(ref desc);
                s_layouts.Add(desc, p);
            }

            return p;
        }

        public static Shader GetShader(ResourceFactory factory, string name, ShaderStages stage)
        {
            if (!s_shaders.TryGetValue((name, stage), out Shader shader))
            {
                shader = ShaderHelper.LoadShader(factory, name, stage);
                s_shaders.Add((name, stage), shader);
            }

            return shader;
        }

        public static void DestroyAllDeviceObjects()
        {
            foreach (KeyValuePair<PipelineDescription, Pipeline> kvp in s_pipelines)
            {
                kvp.Value.Dispose();
            }
            s_pipelines.Clear();

            foreach (KeyValuePair<ResourceLayoutDescription, ResourceLayout> kvp in s_layouts)
            {
                kvp.Value.Dispose();
            }
            s_layouts.Clear();

            foreach (KeyValuePair<(string, ShaderStages), Shader> kvp in s_shaders)
            {
                kvp.Value.Dispose();
            }
            s_shaders.Clear();

            foreach (KeyValuePair<ImageSharpTexture, Texture> kvp in s_textures)
            {
                kvp.Value.Dispose();
            }
            s_textures.Clear();

            foreach (KeyValuePair<Texture, TextureView> kvp in s_textureViews)
            {
                kvp.Value.Dispose();
            }
            s_textureViews.Clear();

            _pinkTex.Dispose();
            _pinkTex = null;

            foreach (KeyValuePair<ResourceSetDescription, ResourceSet> kvp in s_resourceSets)
            {
                kvp.Value.Dispose();
            }
            s_resourceSets.Clear();
        }

        internal static Texture GetTexture2D(ResourceFactory factory, ImageSharpTexture textureData, CommandList cl)
        {
            if (!s_textures.TryGetValue(textureData, out Texture tex))
            {
                tex = textureData.CreateDeviceTexture(factory, cl);
                s_textures.Add(textureData, tex);
            }

            return tex;
        }

        internal static TextureView GetTextureView(ResourceFactory factory, Texture texture)
        {
            if (!s_textureViews.TryGetValue(texture, out TextureView view))
            {
                view = factory.CreateTextureView(texture);
                s_textureViews.Add(texture, view);
            }

            return view;
        }

        internal static unsafe Texture GetPinkTexture(ResourceFactory factory, CommandList cl)
        {
            if (_pinkTex == null)
            {
                RgbaByte pink = RgbaByte.Pink;
                _pinkTex = factory.CreateTexture(new TextureDescription(1, 1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
                cl.UpdateTexture(_pinkTex, (IntPtr)(&pink), 4, 0, 0, 0, 1, 1, 1, 0, 0);
            }

            return _pinkTex;
        }

        internal static ResourceSet GetResourceSet(ResourceFactory factory, ResourceSetDescription description)
        {
            if (!s_resourceSets.TryGetValue(description, out ResourceSet ret))
            {
                ret = factory.CreateResourceSet(ref description);
                s_resourceSets.Add(description, ret);
            }

            return ret;
        }
    }
}