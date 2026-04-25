using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        ReadOnlySpan<uint> digitsA = a.GetDigits();
        ReadOnlySpan<uint> digitsB = b.GetDigits();
        if (digitsA.Length == 0 || digitsB.Length == 0) return new BetterBigInteger([], false);

        uint[] result = new uint[digitsA.Length + digitsB.Length];
        for (int i = 0; i < digitsA.Length; i++)
        {
            ulong carry = 0;
            for (int j = 0; j < digitsB.Length || carry > 0; j++)
            {
                ulong current = (ulong)result[i + j] +
                                (ulong)digitsA[i] * (j < digitsB.Length ? digitsB[j] : 0) + carry;
                result[i + j] = (uint)current;
                carry = current >> 32;
            }
        }

        return new BetterBigInteger(result, a.IsNegative != b.IsNegative);
    }
}