using System.Text;

namespace Veldrid.MetalBindings
{
    public static class MTLUtil
    {
        public static Encoding UTF8 { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public static unsafe string GetUtf8String(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return UTF8.GetString(stringStart, characters);
        }

        public static T AllocInit<T>(string typeName) where T : struct
        {
            ObjCClass cls = new(typeName);
            return cls.AllocInit<T>();
        }
    }
}
