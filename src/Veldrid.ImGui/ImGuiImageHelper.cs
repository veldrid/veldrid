using System.Collections.Generic;
using Veldrid;
using System;

namespace Veldrid
{
    /// <summary>
    /// Helper class for registering textures to be drawn with ImGui.Image() or ImageButton().
    /// </summary>
    public static class ImGuiImageHelper
    {
        private static readonly Dictionary<Texture, TextureViewInfo> s_viewsByTexture = new Dictionary<Texture, TextureViewInfo>();
        private static Dictionary<IntPtr, TextureViewInfo> s_viewsById = new Dictionary<IntPtr, TextureViewInfo>();
        private static int s_lastAssigned = 100;

        private struct TextureViewInfo
        {
            public readonly IntPtr ImGuiBinding;
            public readonly TextureView View;

            public TextureViewInfo(IntPtr imGuiBinding, TextureView textureView)
            {
                ImGuiBinding = imGuiBinding;
                View = textureView;
            }
        }

        /// <summary>
        /// Gets or creates a handle for a texture to be drawn with ImGui.
        /// Pass the returned handle to Image() or ImageButton().
        /// </summary>
        public static IntPtr GetOrCreateImGuiBinding(GraphicsDevice gd, Texture texture)
        {
            TextureView texView = gd.ResourceFactory.CreateTextureView(texture);
            return GetOrCreateImGuiBinding(gd, texView);
        }

        /// <summary>
        /// Gets or creates a handle for a texture to be drawn with ImGui.
        /// Pass the returned handle to Image() or ImageButton().
        /// </summary>
        public static IntPtr GetOrCreateImGuiBinding(GraphicsDevice gd, TextureView view)
        {
            if (!s_viewsByTexture.TryGetValue(view.Target, out TextureViewInfo tvi))
            {
                IntPtr imGuiBinding = new IntPtr(++s_lastAssigned);
                tvi = new TextureViewInfo(imGuiBinding, view);

                s_viewsByTexture.Add(view.Target, tvi);
                s_viewsById.Add(imGuiBinding, tvi);
            }

            return tvi.ImGuiBinding;
        }

        /// <summary>
        /// Retrieves the shader texture binding for the given helper handle.
        /// </summary>
        public static TextureView GetShaderTextureBinding(IntPtr imGuiBinding)
        {
            if (!s_viewsById.TryGetValue(imGuiBinding, out TextureViewInfo tvi))
            {
                throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding.ToString());
            }

            return tvi.View;
        }

        /// <summary>
        /// Clears out and disposes all of the existing handles in the cache.
        /// This disposes all of the ShaderTextureBinding objects that are stored.
        /// </summary>
        public static void InvalidateCache()
        {
            foreach (KeyValuePair<Texture, TextureViewInfo> kvp in s_viewsByTexture)
            {
                kvp.Value.View.Dispose();
            }
        }
    }
}
