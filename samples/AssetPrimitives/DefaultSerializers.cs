using System;
using System.Collections.Generic;

namespace AssetPrimitives
{
    public static class DefaultSerializers
    {
        public static Dictionary<Type, BinaryAssetSerializer> Get()
        {
            return new Dictionary<Type, BinaryAssetSerializer>()
            {
                { typeof(ProcessedTexture), new ProcessedTextureDataSerializer() },
                { typeof(ProcessedModel), new ProcessedModelSerializer() },
                { typeof(byte[]), new ByteArraySerializer() }
            };
        }
    }
}
