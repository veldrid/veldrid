using System;
using System.Collections.Generic;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public static class SharedDataProviders
    {
        private static readonly Dictionary<string, ConstantBufferDataProvider> s_providers
            = new Dictionary<string, ConstantBufferDataProvider>();

        private static Dictionary<string, (int Size, ConstantBuffer Buffer)> s_buffers
            = new Dictionary<string, (int, ConstantBuffer)>();

        private static RenderContext _rc;

        public static void RegisterGlobalDataProvider(string name, ConstantBufferDataProvider provider)
        {
            if (_rc == null)
            {
                throw new InvalidOperationException("No RenderContext set on SharedDataProviders.");
            }

            s_providers.Add(name, provider);
            int dataSize = provider.DataSizeInBytes;
            s_buffers.Add(name, (dataSize, _rc.ResourceFactory.CreateConstantBuffer(dataSize)));
        }

        public static ConstantBufferDataProvider GetProvider(string name)
        {
            if (!s_providers.TryGetValue(name, out ConstantBufferDataProvider provider))
            {
                throw new InvalidOperationException("No registered provider with the name " + name);
            }

            return provider;
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
            return s_buffers[name].Buffer;
        }

        public static ConstantBuffer ProjectionMatrixBuffer => GetBuffer("ProjectionMatrix");
        public static ConstantBuffer ViewMatrixBuffer => GetBuffer("ViewMatrix");
        public static ConstantBuffer LightBuffer => GetBuffer("LightInfo");

        public static void ChangeRenderContext(RenderContext rc)
        {
            _rc = rc;
            var newDictionary = new Dictionary<string, (int, ConstantBuffer)>(s_buffers.Count);
            foreach (KeyValuePair<string, (int Size, ConstantBuffer Buffer)> kvp in s_buffers)
            {
                kvp.Value.Buffer.Dispose();
                newDictionary.Add(kvp.Key, (kvp.Value.Size, rc.ResourceFactory.CreateConstantBuffer(kvp.Value.Size)));
            }

            s_buffers = newDictionary;
        }
    }
}
