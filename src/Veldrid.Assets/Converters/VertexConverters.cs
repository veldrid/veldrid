using System;
using Newtonsoft.Json;
using Veldrid.Graphics;
using System.Numerics;

namespace Veldrid.Assets.Converters
{
    public class VertexPositionNormalTextureConverter : JsonConverter
    {
        private readonly Type _type = typeof(VertexPositionNormalTexture);

        public override bool CanConvert(Type objectType)
        {
            return objectType == _type;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = Newtonsoft.Json.Linq.JArray.Load(reader);

            Vector3 position = new Vector3((float)array[0], (float)array[1], (float)array[2]);
            Vector3 normal = new Vector3((float)array[3], (float)array[4], (float)array[5]);
            Vector2 texCoords = new Vector2((float)array[6], (float)array[7]);

            return new VertexPositionNormalTexture(position, normal, texCoords);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            VertexPositionNormalTexture v = (VertexPositionNormalTexture)value;
            writer.WriteStartArray();
            writer.WriteValue(v.Position.X);
            writer.WriteValue(v.Position.Y);
            writer.WriteValue(v.Position.Z);
            writer.WriteValue(v.Normal.X);
            writer.WriteValue(v.Normal.Y);
            writer.WriteValue(v.Normal.Z);
            writer.WriteValue(v.TextureCoordinates.X);
            writer.WriteValue(v.TextureCoordinates.Y);
            writer.WriteEndArray();
        }
    }
}
