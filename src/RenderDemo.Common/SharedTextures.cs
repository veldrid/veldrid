using System.Collections.Generic;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class SharedTextures
    {
        private static Dictionary<string, ShaderTextureBinding> s_sharedTextures = new Dictionary<string, ShaderTextureBinding>();

        public static void SetTextureBinding(string name, ShaderTextureBinding binding)
        {
            s_sharedTextures[name] = binding;
        }

        public static ShaderTextureBinding GetTextureBinding(string name)
        {
            return s_sharedTextures[name];
        }
    }
}