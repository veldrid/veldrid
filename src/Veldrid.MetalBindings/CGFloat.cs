using System;
using System.Runtime.CompilerServices;

namespace Veldrid.MetalBindings
{
    public unsafe struct CGFloat
    {
        private readonly IntPtr _value;

        public CGFloat(double value)
        {
            IntPtr ptrValue;

            if (IntPtr.Size == 4)
            {
                Unsafe.Write(&ptrValue, (float)value);
            }
            else
            {
                Unsafe.Write(&ptrValue, value);
            }

            _value = ptrValue;
        }

        public double Value
        {
            get
            {
                IntPtr value = _value;
                if (IntPtr.Size == 4)
                {
                    return Unsafe.Read<float>(&value);
                }
                else
                {
                    return Unsafe.Read<double>(&value);
                }
            }
        }

        public static implicit operator CGFloat(double value) => new CGFloat(value);
        public static implicit operator double(CGFloat cgf) => cgf.Value;
    }
}