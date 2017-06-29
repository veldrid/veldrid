using System;
using System.Collections.Generic;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class SharedDataProviders
    {
        private static readonly Dictionary<string, Entry> s_entries
            = new Dictionary<string, Entry>();

        private static RenderContext _rc;

        public static void RegisterGlobalDataProvider(string name, ConstantBufferDataProvider provider)
        {
            if (_rc == null)
            {
                throw new InvalidOperationException("No RenderContext set on SharedDataProviders.");
            }

            Entry entry = new Entry();
            entry.Provider = provider;
            entry.Buffer = _rc.ResourceFactory.CreateConstantBuffer(provider.DataSizeInBytes);

            s_entries.Add(name, entry);
        }

        public static ConstantBufferDataProvider GetProvider(string name)
        {
            if (!s_entries.TryGetValue(name, out Entry entry))
            {
                throw new InvalidOperationException("No registered provider with the name " + name);
            }

            return entry.Provider;
        }

        public static ConstantBufferDataProvider<T> GetProvider<T>(string name)
        {
            ConstantBufferDataProvider provider = GetProvider(name);
            if (!(provider is ConstantBufferDataProvider<T> typedProvider))
            {
                throw new InvalidOperationException(
                    $"The provider registered to name {name} is not a provider of type {typeof(T).Name}.");
            }

            return typedProvider;
        }

        public static ConstantBuffer GetBuffer(string name)
        {
            return s_entries[name].Buffer;
        }

        public static void UpdateBuffers()
        {
            foreach (var kvp in s_entries)
            {
                kvp.Value.Provider.SetData(kvp.Value.Buffer);
            }
        }

        public static ConstantBuffer ProjectionMatrixBuffer => GetBuffer("ProjectionMatrix");
        public static ConstantBuffer ViewMatrixBuffer => GetBuffer("ViewMatrix");
        public static ConstantBuffer DirectionalLightBuffer => GetBuffer("LightBuffer");
        public static ConstantBuffer LightInfoBuffer => GetBuffer("LightInfo");
        public static ConstantBuffer CameraInfoBuffer => GetBuffer("CameraInfo");
        public static ConstantBuffer LightViewMatrixBuffer => GetBuffer("LightViewMatrix");
        public static ConstantBuffer LightProjMatrixBuffer => GetBuffer("LightProjMatrix");
        public static ConstantBuffer PointLightsBuffer => GetBuffer("PointLights");

        public static void ChangeRenderContext(RenderContext rc)
        {
            _rc = rc;
            foreach (var kvp in s_entries)
            {
                kvp.Value.Buffer.Dispose();
                kvp.Value.Buffer = rc.ResourceFactory.CreateConstantBuffer(kvp.Value.Provider.DataSizeInBytes);
            }
        }

        private class Entry
        {
            public ConstantBufferDataProvider Provider { get; set; }
            public ConstantBuffer Buffer { get; set; }
        }
    }
}
