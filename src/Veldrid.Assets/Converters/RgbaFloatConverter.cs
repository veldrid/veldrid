using System;
using Newtonsoft.Json;
using Veldrid.Graphics;
using Newtonsoft.Json.Linq;

namespace Veldrid.Assets.Converters
{
    public class RgbaFloatConverter : JsonConverter
    {
        private readonly Type _type = typeof(RgbaFloat);

        public override bool CanConvert(Type objectType)
        {
            return objectType == _type;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);
            return new RgbaFloat(jToken["R"].Value<float>(), jToken["G"].Value<float>(), jToken["B"].Value<float>(), jToken["A"].Value<float>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);
            t.WriteTo(writer);
        }
    }
}
