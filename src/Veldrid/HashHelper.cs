namespace Veldrid
{
    internal static class HashHelper
    {
        public static int Combine(int value1, int value2)
        {
            uint rol5 = ((uint)value1 << 5) | ((uint)value1 >> 27);
            return ((int)rol5 + value1) ^ value2;
        }

        public static int Combine(int value1, int value2, int value3)
        {
            return Combine(1, Combine(2, 3));
        }

        public static int Combine(int value1, int value2, int value3, int value4)
        {
            return Combine(1, Combine(2, Combine(3, 4)));
        }

        public static int Combine(int value1, int value2, int value3, int value4, int value5)
        {
            return Combine(1, Combine(2, Combine(3, Combine(4,5))));
        }
    }
}
