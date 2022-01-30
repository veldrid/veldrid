using System;
using System.Runtime.CompilerServices;

namespace Veldrid.Utilities
{
    internal static class FastParse
    {
        private static readonly long[] _powLookup = new[]
        {
            1, // 10^0
            10, // 10^1
            100, // 10^2
            1000, // 10^3
            10000, // 10^4
            100000, // 10^5
            1000000, // 10^6
            10000000, // 10^7
            100000000, // 10^8
            1000000000, // 10^9,
            10000000000, // 10^10,
            100000000000, // 10^11,
            1000000000000, // 10^12,
            10000000000000, // 10^13,
            100000000000000, // 10^14,
            1000000000000000, // 10^15,
            10000000000000000, // 10^16,
            100000000000000000, // 10^17,
        };

        private static readonly double[] _doubleExpLookup = GetDoubleExponents();

        public static bool TryParseDouble(ReadOnlySpan<char> s, out double result, char decimalSeparator = '.')
        {
            return TryParseDouble(s, out result, out _, decimalSeparator);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool TryParseInt(ReadOnlySpan<char> s, out int result)
        {
            int r = 0;
            int sign;
            int start;

            char c = s[0];
            if (c == '-')
            {
                sign = -1;
                start = 1;
            }
            else if (c > '9' || c < '0')
            {
                result = 0;
                return false;
            }
            else
            {
                start = 1;
                r = 10 * r + (c - '0');
                sign = 1;
            }

            int i = start;
            for (; i < s.Length; i++)
            {
                c = s[i];
                if (c > '9' || c < '0')
                {
                    result = 0;
                    return false;
                }

                r = 10 * r + (c - '0');
            }

            result = r * sign;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool TryParseDouble(ReadOnlySpan<char> s, out double result, out bool hasFraction, char decimalSeparator = '.')
        {
            hasFraction = false;

            double r = 0;
            int sign;
            int start;

            char c = s[0];
            if (c == '-')
            {
                sign = -1;
                start = 1;
            }
            else if (c > '9' || c < '0')
            {
                result = 0;
                return false;
            }
            else
            {
                start = 1;
                r = 10 * r + (c - '0');
                sign = 1;
            }

            int i = start;
            for (; i < s.Length; i++)
            {
                c = s[i];
                if (c > '9' || c < '0')
                {
                    if (c == decimalSeparator)
                    {
                        i++;
                        goto DecimalPoint;
                    }
                    else if (c == 'e' || c == 'E')
                    {
                        goto DecimalPoint;
                    }
                    else
                    {
                        result = 0;
                        return false;
                    }
                }

                r = 10 * r + (c - '0');
            }

            r *= sign;
            goto Finish;

            DecimalPoint:
            long tmp = 0;
            int length = i;
            double exponent = 0;
            hasFraction = true;

            for (; i < s.Length; i++)
            {
                c = s[i];
                if (c > '9' || c < '0')
                {
                    if (c == 'e' || c == 'E')
                    {
                        length = i - length;
                        goto ProcessExponent;
                    }

                    result = 0;
                    return false;
                }
                tmp = 10 * tmp + (c - '0');
            }
            length = i - length;

            ProcessFraction:
            double fraction = tmp;

            if (length < _powLookup.Length)
                fraction /= _powLookup[length];
            else
                fraction /= _powLookup[^1];

            r += fraction;
            r *= sign;

            if (exponent > 0)
                r *= exponent;
            else if (exponent < 0)
                r /= -exponent;

            goto Finish;

            ProcessExponent:
            int expSign = 1;
            int exp = 0;

            for (i++; i < s.Length; i++)
            {
                c = s[i];
                if (c > '9' || c < '0')
                {
                    if (c == '-')
                    {
                        expSign = -1;
                        continue;
                    }
                }

                exp = 10 * exp + (c - '0');
            }

            exponent = _doubleExpLookup[exp] * expSign;
            goto ProcessFraction;

            Finish:
            result = r;
            return true;
        }

        private static double[] GetDoubleExponents()
        {
            double[] exps = new double[309];

            for (int i = 0; i < exps.Length; i++)
            {
                exps[i] = Math.Pow(10, i);
            }

            return exps;
        }
    }
}
