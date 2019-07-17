using System.Text;

namespace Veldrid.MetalBindings
{
    public static class MTLUtil
    {
        public static unsafe string GetUtf8String(byte* stringStart)
        {
            if (stringStart == null)
            {
                return null;
            }
            
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(stringStart, characters);
        }

        public static T AllocInit<T>(string typeName) where T : struct
        {
            var cls = new ObjCClass(typeName);
            return cls.AllocInit<T>();
        }
    }
}