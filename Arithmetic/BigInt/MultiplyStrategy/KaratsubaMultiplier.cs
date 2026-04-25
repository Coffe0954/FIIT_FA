using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        ReadOnlySpan<uint> digitsA = a.GetDigits();
        ReadOnlySpan<uint> digitsB = b.GetDigits();

        if (digitsA.Length < 32 || digitsB.Length < 32)
        {
            return new SimpleMultiplier().Multiply(a, b);
        }

        int n = Math.Max(digitsA.Length, digitsB.Length);
        int m = n / 2;

        BetterBigInteger high1 = new BetterBigInteger(digitsA.Length > m ? digitsA.Slice(m).ToArray() : [], false);
        BetterBigInteger low1 = new BetterBigInteger(digitsA.Length > m ? digitsA.Slice(0, m).ToArray() : digitsA.ToArray(), false);
        BetterBigInteger high2 = new BetterBigInteger(digitsB.Length > m ? digitsB.Slice(m).ToArray() : [], false);
        BetterBigInteger low2 = new BetterBigInteger(digitsB.Length > m ? digitsB.Slice(0, m).ToArray() : digitsB.ToArray(), false);

        BetterBigInteger z0 = Multiply(low1, low2);
        BetterBigInteger z1 = Multiply(low1 + high1, low2 + high2);
        BetterBigInteger z2 = Multiply(high1, high2);

        BetterBigInteger result = (z2 << (2 * m * 32)) + ((z1 - z2 - z0) << (m * 32)) + z0;
        result._signBit = a.IsNegative != b.IsNegative ? 1 : 0;
        return result;
    }
}