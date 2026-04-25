using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic.BigInt;

public sealed class BetterBigInteger : IBigInteger
{
    internal int _signBit;
    
    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;
    
    public bool IsNegative => _signBit == 1;
    
    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        int len = digits.Length;
        while (len > 0 && digits[len - 1] == 0) len--;

        if (len == 0)
        {
            _signBit = 0;
            _smallValue = 0;
            _data = null;
        }
        else if (len == 1)
        {
            _smallValue = digits[0];
            _signBit = (isNegative && _smallValue != 0) ? 1 : 0;
            _data = null;
        }
        else
        {
            _signBit = isNegative ? 1 : 0;
            _data = new uint[len];
            Array.Copy(digits, _data, len);
        }
    }

    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false)
        : this(digits.ToArray(), isNegative)
    {
    }

    public BetterBigInteger(string value, int radix)
    {
        // Simple but inefficient parsing, will be improved if needed
        BetterBigInteger result = Parse(value, radix);
        _signBit = result._signBit;
        _smallValue = result._smallValue;
        _data = result._data;
    }

    private static BetterBigInteger Parse(string value, int radix)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Empty string");
        bool negative = false;
        int start = 0;
        if (value[0] == '-')
        {
            negative = true;
            start = 1;
        }

        BetterBigInteger result = new BetterBigInteger([], false);
        BetterBigInteger bRadix = new BetterBigInteger([(uint)radix], false);

        for (int i = start; i < value.Length; i++)
        {
            int digit = ParseChar(value[i]);
            if (digit >= radix) throw new ArgumentException("Invalid digit for radix");
            result = (result * bRadix) + new BetterBigInteger([(uint)digit], false);
        }

        if (negative && result != new BetterBigInteger([], false))
        {
            result._signBit = 1;
        }
        return result;
    }

    private static int ParseChar(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'a' && c <= 'z') return c - 'a' + 10;
        if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
        throw new ArgumentException("Invalid character");
    }

    public ReadOnlySpan<uint> GetDigits()
    {
        if (_data != null) return _data;
        if (_smallValue == 0) return ReadOnlySpan<uint>.Empty;
        return new ReadOnlySpan<uint>(ref _smallValue);
    }
    
    public int CompareTo(IBigInteger? other)
    {
        if (other == null) return 1;
        if (IsNegative && !other.IsNegative) return -1;
        if (!IsNegative && other.IsNegative) return 1;

        int cmp = CompareMagnitude(GetDigits(), other.GetDigits());
        return IsNegative ? -cmp : cmp;
    }

    private static int CompareMagnitude(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        if (a.Length != b.Length) return a.Length.CompareTo(b.Length);
        for (int i = a.Length - 1; i >= 0; i--)
        {
            if (a[i] != b[i]) return a[i].CompareTo(b[i]);
        }
        return 0;
    }

    public bool Equals(IBigInteger? other)
    {
        if (other == null) return false;
        if (IsNegative != other.IsNegative) return false;
        return GetDigits().SequenceEqual(other.GetDigits());
    }

    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(IsNegative);
        foreach (uint digit in GetDigits())
        {
            hash.Add(digit);
        }
        return hash.ToHashCode();
    }
    
    
    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b)
    {
        if (a.IsNegative == b.IsNegative)
        {
            return new BetterBigInteger(AddMagnitudes(a.GetDigits(), b.GetDigits()), a.IsNegative);
        }

        int cmp = CompareMagnitude(a.GetDigits(), b.GetDigits());
        if (cmp == 0) return new BetterBigInteger([], false);

        if (cmp > 0)
        {
            return new BetterBigInteger(SubtractMagnitudes(a.GetDigits(), b.GetDigits()), a.IsNegative);
        }
        return new BetterBigInteger(SubtractMagnitudes(b.GetDigits(), a.GetDigits()), b.IsNegative);
    }

    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b)
    {
        return a + (-b);
    }

    public static BetterBigInteger operator -(BetterBigInteger a)
    {
        return new BetterBigInteger(a.GetDigits().ToArray(), !a.IsNegative);
    }

    private static uint[] AddMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        int len = Math.Max(a.Length, b.Length);
        uint[] result = new uint[len + 1];
        ulong carry = 0;
        for (int i = 0; i < len || carry > 0; i++)
        {
            ulong sum = carry + (i < a.Length ? a[i] : 0) + (i < b.Length ? b[i] : 0);
            if (i >= result.Length)
            {
                Array.Resize(ref result, i + 1);
            }
            result[i] = (uint)sum;
            carry = sum >> 32;
        }
        return result;
    }

    private static uint[] SubtractMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        uint[] result = new uint[a.Length];
        long borrow = 0;
        for (int i = 0; i < a.Length; i++)
        {
            long diff = (long)a[i] - (i < b.Length ? b[i] : 0) - borrow;
            if (diff < 0)
            {
                diff += 1L << 32;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }
            result[i] = (uint)diff;
        }
        return result;
    }
    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b)
    {
        return Divide(a, b).Quotient;
    }

    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b)
    {
        return Divide(a, b).Remainder;
    }

    private static (BetterBigInteger Quotient, BetterBigInteger Remainder) Divide(BetterBigInteger a, BetterBigInteger b)
    {
        if (b == new BetterBigInteger([], false)) throw new DivideByZeroException();

        int cmp = CompareMagnitude(a.GetDigits(), b.GetDigits());
        if (cmp < 0) return (new BetterBigInteger([], false), a);
        if (cmp == 0) return (new BetterBigInteger([1], a.IsNegative != b.IsNegative), new BetterBigInteger([], false));

        // Simplified schoolbook division
        BetterBigInteger quotient = new BetterBigInteger([], false);
        BetterBigInteger remainder = new BetterBigInteger([], false);
        BetterBigInteger bAbs = new BetterBigInteger(b.GetDigits().ToArray(), false);

        ReadOnlySpan<uint> aDigits = a.GetDigits();
        for (int i = aDigits.Length - 1; i >= 0; i--)
        {
            remainder = (remainder << 32) + new BetterBigInteger([aDigits[i]], false);
            uint q = 0;
            uint low = 0, high = uint.MaxValue;
            while (low <= high)
            {
                uint mid = low + (high - low) / 2;
                if (bAbs * new BetterBigInteger([mid], false) <= remainder)
                {
                    q = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            quotient = (quotient << 32) + new BetterBigInteger([q], false);
            remainder -= bAbs * new BetterBigInteger([q], false);
        }

        quotient._signBit = a.IsNegative != b.IsNegative ? 1 : 0;
        remainder._signBit = a.IsNegative ? 1 : 0;
        return (quotient, remainder);
    }
    
    
    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
    {
        int len = Math.Max(a.GetDigits().Length, b.GetDigits().Length);
        IMultiplier strategy = len switch
        {
            < 32 => new SimpleMultiplier(),
            < 128 => new KaratsubaMultiplier(),
            _ => new FftMultiplier()
        };
        return strategy.Multiply(a, b);
    }
    
    public static BetterBigInteger operator ~(BetterBigInteger a)
    {
        return -(a + new BetterBigInteger([1], false));
    }

    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b)
    {
        return BitwiseOp(a, b, (x, y) => x & y);
    }

    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b)
    {
        return BitwiseOp(a, b, (x, y) => x | y);
    }

    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b)
    {
        return BitwiseOp(a, b, (x, y) => x ^ y);
    }

    private static BetterBigInteger BitwiseOp(BetterBigInteger a, BetterBigInteger b, Func<uint, uint, uint> op)
    {
        int maxLen = Math.Max(a.GetDigits().Length, b.GetDigits().Length);
        uint[] a2 = a.Get2sComplement(maxLen);
        uint[] b2 = b.Get2sComplement(maxLen);

        int len = Math.Max(a2.Length, b2.Length);
        uint[] result = new uint[len];

        for (int i = 0; i < len; i++)
        {
            result[i] = op(a2[i], b2[i]);
        }

        return From2sComplement(result);
    }

    private static BetterBigInteger From2sComplement(uint[] digits)
    {
        bool negative = (digits.Length > 0 && (digits[^1] & 0x80000000) != 0);
        if (!negative) return new BetterBigInteger(digits, false);

        uint[] mag = new uint[digits.Length];
        ulong carry = 1;
        for (int i = 0; i < digits.Length; i++)
        {
            ulong val = (ulong)(~digits[i]) + carry;
            mag[i] = (uint)val;
            carry = val >> 32;
        }
        BetterBigInteger result = new BetterBigInteger(mag, true);

        // If the result is zero, it must be positive
        bool isZero = true;
        foreach(var d in result.GetDigits()) if (d != 0) { isZero = false; break; }
        if (isZero) result._signBit = 0;

        return result;
    }

    private uint[] Get2sComplement(int minLen)
    {
        ReadOnlySpan<uint> digits = GetDigits();
        int len = Math.Max(digits.Length, minLen) + 1; // +1 for sign extension
        uint[] result = new uint[len];
        uint extension = IsNegative ? 0xFFFFFFFF : 0;

        if (!IsNegative)
        {
            for (int i = 0; i < len; i++) result[i] = i < digits.Length ? digits[i] : extension;
            return result;
        }

        ulong carry = 1;
        for (int i = 0; i < len; i++)
        {
            uint d = i < digits.Length ? digits[i] : 0;
            ulong val = (ulong)(~d) + carry;
            result[i] = (uint)val;
            carry = val >> 32;
        }
        return result;
    }
    public static BetterBigInteger operator <<(BetterBigInteger a, int shift)
    {
        if (shift < 0) return a >> -shift;
        if (shift == 0) return a;

        int fullWords = shift / 32;
        int bits = shift % 32;

        ReadOnlySpan<uint> aDigits = a.GetDigits();
        uint[] result = new uint[aDigits.Length + fullWords + 1];
        ulong carry = 0;

        for (int i = 0; i < aDigits.Length; i++)
        {
            ulong val = ((ulong)aDigits[i] << bits) | carry;
            result[i + fullWords] = (uint)val;
            carry = val >> 32;
        }
        result[aDigits.Length + fullWords] = (uint)carry;

        return new BetterBigInteger(result, a.IsNegative);
    }

    public static BetterBigInteger operator >>(BetterBigInteger a, int shift)
    {
        if (shift < 0) return a << -shift;
        if (shift == 0) return a;
        return RightShift(a, shift);
    }

    private static BetterBigInteger RightShift(BetterBigInteger a, int shift)
    {
        if (!a.IsNegative)
        {
            int fullWords = shift / 32;
            int bits = shift % 32;
            ReadOnlySpan<uint> aDigits = a.GetDigits();
            if (fullWords >= aDigits.Length) return new BetterBigInteger([], false);

            uint[] result = new uint[aDigits.Length - fullWords];
            for (int i = 0; i < result.Length; i++)
            {
                ulong low = aDigits[i + fullWords];
                ulong high = (i + fullWords + 1 < aDigits.Length) ? aDigits[i + fullWords + 1] : 0;
                result[i] = (uint)((low >> bits) | (high << (32 - bits)));
            }
            return new BetterBigInteger(result, false);
        }
        else
        {
            // For negative numbers, right shift is arithmetic shift in 2's complement
            uint[] a2 = a.Get2sComplement(a.GetDigits().Length);
            int fullWords = shift / 32;
            int bits = shift % 32;
            if (fullWords >= a2.Length) return new BetterBigInteger([1], true); // -1

            uint[] res2 = new uint[a2.Length - fullWords];
            for (int i = 0; i < res2.Length; i++)
            {
                ulong low = a2[i + fullWords];
                ulong high = (i + fullWords + 1 < a2.Length) ? a2[i + fullWords + 1] : 0xFFFFFFFF;
                res2[i] = (uint)((low >> bits) | (high << (32 - bits)));
            }
            return From2sComplement(res2);
        }
    }
    
    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;
    
    public override string ToString() => ToString(10);

    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException(nameof(radix));
        if (_smallValue == 0 && _data == null) return "0";

        BetterBigInteger current = new BetterBigInteger(GetDigits().ToArray(), false);
        BetterBigInteger bRadix = new BetterBigInteger([(uint)radix], false);
        BetterBigInteger zero = new BetterBigInteger([], false);

        List<char> chars = new();
        while (current != zero)
        {
            BetterBigInteger rem = current % bRadix;
            uint digitValue = rem._smallValue;
            chars.Add(GetDigitChar((int)digitValue));
            current /= bRadix;
        }

        if (IsNegative) chars.Add('-');
        chars.Reverse();
        return new string(chars.ToArray());
    }

    private static char GetDigitChar(int value)
    {
        if (value < 10) return (char)('0' + value);
        return (char)('a' + (value - 10));
    }
    
}