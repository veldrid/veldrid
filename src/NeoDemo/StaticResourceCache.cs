using System;
using System.Collections.Generic;
using Vd2.ImageSharp;

namespace Vd2.NeoDemo
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

        private static readonly Dictionary<ImageSharpTexture, Texture2D> s_textures
            = new Dictionary<ImageSharpTexture, Texture2D>();

        private static readonly Dictionary<Texture, TextureView> s_textureViews = new Dictionary<Texture, TextureView>();

        private static Texture2D _pinkTex;

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

            foreach (KeyValuePair<ImageSharpTexture, Texture2D> kvp in s_textures)
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
        }

        internal static Texture2D GetTexture2D(ResourceFactory factory, ImageSharpTexture textureData, CommandList cl)
        {
            if (!s_textures.TryGetValue(textureData, out Texture2D tex))
            {
                tex = textureData.CreateDeviceTexture(factory, cl);
                s_textures.Add(textureData, tex);
            }

            return tex;
        }

        internal static TextureView GetTextureView(ResourceFactory factory, Texture2D texture)
        {
            if (!s_textureViews.TryGetValue(texture, out TextureView view))
            {
                view = factory.CreateTextureView(texture);
                s_textureViews.Add(texture, view);
            }

            return view;
        }

        internal static unsafe Texture2D GetPinkTexture(ResourceFactory factory, CommandList cl)
        {
            if (_pinkTex == null)
            {
                RgbaByte pink = RgbaByte.Pink;
                _pinkTex = factory.CreateTexture2D(new TextureDescription(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
                cl.UpdateTexture2D(_pinkTex, (IntPtr)(&pink), 4, 0, 0, 1, 1, 0, 0);
            }

            return _pinkTex;
        }
    }
}
