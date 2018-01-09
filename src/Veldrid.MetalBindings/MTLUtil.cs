using System.Text;

namespace Veldrid.MetalBindings
{
    public static class MTLUtil
    {
        public static unsafe string GetUtf8String(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(stringStart, characters);
        }

        public static T AllocInit<T>() where T : struct
        {
            string typeName = typeof(T).Name;
            var cls = new ObjCClass(typeName);
            return cls.AllocInit<T>();
        }
    }
}