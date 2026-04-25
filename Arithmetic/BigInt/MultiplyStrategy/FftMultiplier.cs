using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class FftMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        ReadOnlySpan<uint> digitsA = a.GetDigits();
        ReadOnlySpan<uint> digitsB = b.GetDigits();
        if (digitsA.Length == 0 || digitsB.Length == 0) return new BetterBigInteger([], false);

        int n = 1;
        while (n < digitsA.Length + digitsB.Length) n <<= 1;

        // FFT with uint digits directly is prone to precision issues for large numbers.
        // For 32-bit digits, we need to split them into 16-bit parts.
        int n2 = 1;
        while (n2 < (digitsA.Length + digitsB.Length) * 2) n2 <<= 1;

        System.Numerics.Complex[] fa = new System.Numerics.Complex[n2];
        System.Numerics.Complex[] fb = new System.Numerics.Complex[n2];

        for (int i = 0; i < digitsA.Length; i++)
        {
            fa[2 * i] = new System.Numerics.Complex(digitsA[i] & 0xFFFF, 0);
            fa[2 * i + 1] = new System.Numerics.Complex(digitsA[i] >> 16, 0);
        }
        for (int i = 0; i < digitsB.Length; i++)
        {
            fb[2 * i] = new System.Numerics.Complex(digitsB[i] & 0xFFFF, 0);
            fb[2 * i + 1] = new System.Numerics.Complex(digitsB[i] >> 16, 0);
        }
        n = n2;

        FFT(fa, false);
        FFT(fb, false);
        for (int i = 0; i < n; i++) fa[i] *= fb[i];
        FFT(fa, true);

        uint[] result = new uint[digitsA.Length + digitsB.Length + 2];
        ulong carry = 0;
        for (int i = 0; i < n; i++)
        {
            ulong val = (ulong)Math.Round(fa[i].Real) + carry;
            uint part = (uint)(val & 0xFFFF);
            carry = val >> 16;

            int wordIndex = i / 2;
            if (i % 2 == 0)
            {
                if (wordIndex < result.Length) result[wordIndex] |= part;
            }
            else
            {
                if (wordIndex < result.Length) result[wordIndex] |= part << 16;
            }
        }
        int lastWord = n / 2;
        while (carry > 0 && lastWord < result.Length)
        {
            ulong val = (ulong)result[lastWord] + carry;
            result[lastWord] = (uint)val;
            carry = val >> 32;
            lastWord++;
        }

        return new BetterBigInteger(result, a.IsNegative != b.IsNegative);
    }

    private void FFT(System.Numerics.Complex[] a, bool invert)
    {
        int n = a.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (a[i], a[j]) = (a[j], a[i]);
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = 2 * Math.PI / len * (invert ? -1 : 1);
            System.Numerics.Complex wlen = new System.Numerics.Complex(Math.Cos(ang), Math.Sin(ang));
            for (int i = 0; i < n; i += len)
            {
                System.Numerics.Complex w = new System.Numerics.Complex(1, 0);
                for (int j = 0; j < len / 2; j++)
                {
                    System.Numerics.Complex u = a[i + j], v = a[i + j + len / 2] * w;
                    a[i + j] = u + v;
                    a[i + j + len / 2] = u - v;
                    w *= wlen;
                }
            }
        }

        if (invert)
        {
            for (int i = 0; i < n; i++) a[i] /= n;
        }
    }
}