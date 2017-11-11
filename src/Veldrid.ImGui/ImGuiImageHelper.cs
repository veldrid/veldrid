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
        private static readonly Dictionary<ResourceSet, ResourceSetInfo> s_viewsByTexture = new Dictionary<ResourceSet, ResourceSetInfo>();
        private static Dictionary<IntPtr, ResourceSetInfo> s_viewsById = new Dictionary<IntPtr, ResourceSetInfo>();
        private static int s_lastAssigned = 100;

        private struct ResourceSetInfo
        {
            public readonly IntPtr ImGuiBinding;
            public readonly ResourceSet ResourceSet;

            public ResourceSetInfo(IntPtr imGuiBinding, ResourceSet resourceSet)
            {
                ImGuiBinding = imGuiBinding;
                ResourceSet = resourceSet;
            }
        }

        /// <summary>
        /// Gets or creates a handle for a texture to be drawn with ImGui.
        /// Pass the returned handle to Image() or ImageButton().
        /// </summary>
        public static IntPtr GetOrCreateImGuiBinding(ResourceSet resourceSet)
        {
            if (!s_viewsByTexture.TryGetValue(resourceSet, out ResourceSetInfo tvi))
            {
                IntPtr imGuiBinding = new IntPtr(++s_lastAssigned);
                tvi = new ResourceSetInfo(imGuiBinding, resourceSet);

                s_viewsByTexture.Add(resourceSet, tvi);
                s_viewsById.Add(imGuiBinding, tvi);
            }

            return tvi.ImGuiBinding;
        }

        /// <summary>
        /// Retrieves the shader texture binding for the given helper handle.
        /// </summary>
        public static ResourceSet GetResourceSet(IntPtr imGuiBinding)
        {
            if (!s_viewsById.TryGetValue(imGuiBinding, out ResourceSetInfo tvi))
            {
                throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding.ToString());
            }

            return tvi.ResourceSet;
        }

        /// <summary>
        /// Clears out and disposes all of the existing handles in the cache.
        /// This disposes all of the ShaderTextureBinding objects that are stored.
        /// </summary>
        public static void InvalidateCache()
        {
            foreach (KeyValuePair<ResourceSet, ResourceSetInfo> kvp in s_viewsByTexture)
            {
                kvp.Value.ResourceSet.Dispose();
            }
        }
    }
}
