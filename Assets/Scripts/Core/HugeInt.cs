using System;
using System.Numerics;
using UnityEngine;

[Serializable]
public struct HugeInt : IComparable<HugeInt>, IEquatable<HugeInt>, ISerializationCallbackReceiver
{
    [SerializeField] private string _serialized; // for Unity serialization
    private BigInteger _value;

    // Constructors
    public HugeInt(BigInteger v)
    {
        _value = v;

        if (v > int.MaxValue)
        {
            _serialized = "Beeg";
        }
        else
        {
            _serialized = v.ToString();
        }
    }
    public HugeInt(long v)
    {
        _value = new BigInteger(v);
        
        if (v > int.MaxValue)
        {
            _serialized = "Beeg";
        }
        else
        {
            _serialized = v.ToString();
        }
    }

    // Implicit conversions
    public static implicit operator HugeInt(int v) => new HugeInt(v);
    public static implicit operator HugeInt(long v) => new HugeInt(v);
    public static implicit operator HugeInt(BigInteger v) => new HugeInt(v);

    public static explicit operator BigInteger(HugeInt h) => h._value;
    public static explicit operator long(HugeInt h) => (long)h._value;

    // Operators
    public static HugeInt operator +(HugeInt a, HugeInt b) => new HugeInt(a._value + b._value);
    public static HugeInt operator -(HugeInt a, HugeInt b) => new HugeInt(a._value - b._value);
    public static HugeInt operator *(HugeInt a, HugeInt b) => new HugeInt(a._value * b._value);
    public static HugeInt operator /(HugeInt a, HugeInt b) => new HugeInt(a._value / b._value);
    public static HugeInt operator %(HugeInt a, HugeInt b) => new HugeInt(a._value % b._value);

    public static bool operator >(HugeInt a, HugeInt b) => a._value > b._value;
    public static bool operator <(HugeInt a, HugeInt b) => a._value < b._value;
    public static bool operator >=(HugeInt a, HugeInt b) => a._value >= b._value;
    public static bool operator <=(HugeInt a, HugeInt b) => a._value <= b._value;
    public static bool operator ==(HugeInt a, HugeInt b) => a._value == b._value;
    public static bool operator !=(HugeInt a, HugeInt b) => a._value != b._value;

    // Interfaces
    public int CompareTo(HugeInt other) => _value.CompareTo(other._value);
    public bool Equals(HugeInt other) => _value.Equals(other._value);
    public override bool Equals(object obj) => obj is HugeInt h && Equals(h);
    public override int GetHashCode() => _value.GetHashCode();

    // Unity serialization hooks
    public void OnBeforeSerialize() => _serialized = _value.ToString();
    public void OnAfterDeserialize() => _value = string.IsNullOrEmpty(_serialized) ? BigInteger.Zero : BigInteger.Parse(_serialized);

    // Formatting helpers
    public override string ToString() => _value.ToString();
    public string ToScientificString(int sigDigits = 3)
    {
        if (_value.IsZero) return "0";
        var s = _value.ToString();
        int exp = s.Length - 1;
        string mantissa = s[0] + (sigDigits > 1 && s.Length > 1 ? "." + s.Substring(1, Math.Min(sigDigits - 1, s.Length - 1)) : "");
        return $"{mantissa}e{exp}";
    }

    public static HugeInt Pow(HugeInt @base, int exponent)
    {
        if (exponent < 0)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Negative exponents not supported for integers.");
        if (exponent == 0) return One;  // define static HugeInt One = new(BigInteger.One);

        BigInteger result = BigInteger.One;
        BigInteger b = @base._value;
        int e = exponent;

        // exponentiation by squaring
        while (e > 0)
        {
            if ((e & 1) == 1) result *= b;
            b *= b;
            e >>= 1;
        }

        return new HugeInt(result);
    }
    public static HugeInt Pow(HugeInt @base, HugeInt exponent)
    {
        // Negative exponents make no sense for integers
        if (exponent._value.Sign < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent must be ≥ 0.");
        }

        // Fast paths
        if (exponent._value.IsZero) 
        {
            return One; 
        }

        if (@base._value.IsZero)
        {
            return Zero;
        }

        if (@base._value.IsOne)
        {
            return One;
        }

        BigInteger result = BigInteger.One;
        BigInteger b = @base._value;
        BigInteger e = exponent._value;

        // exponentiation by squaring
        while (e > BigInteger.Zero)
        {
            if (!e.IsEven)
                result *= b;
            b *= b;
            e >>= 1;
        }

        return new HugeInt(result);
    }

    public static HugeInt Pow10(int exponent)
    {
        if (exponent < 0)
            throw new ArgumentOutOfRangeException(nameof(exponent));
        return new HugeInt(BigInteger.Pow(10, exponent));
    }

    /// <summary>Natural log (ln) as double.</summary>
    public static double Log(HugeInt v)
    {
        if (v._value.Sign <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(v), "Log undefined for <= 0.");
        }

        return BigInteger.Log(v._value); // ln
    }

    /// <summary>Log base 10.</summary>
    public static double Log10(HugeInt v)
    {
        if (v._value.Sign <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(v));
        }

        return BigInteger.Log10(v._value);
    }

    /// <summary>Log base 'newBase'.</summary>
    public static double Log(HugeInt v, double newBase)
    {
        if (newBase <= 0 || Math.Abs(newBase - 1.0) < double.Epsilon)
        {
            throw new ArgumentOutOfRangeException(nameof(newBase));
        }

        return Log(v) / Math.Log(newBase);
    }

    /// <summary>Floor( a * d ). Throws on NaN/Infinity.</summary>
    public static HugeInt MulFloor(HugeInt a, double d)
        => MulRoundDown(a, d);

    /// <summary>Floor( a * f ) for float.</summary>
    public static HugeInt MulFloor(HugeInt a, float f)
        => MulRoundDown(a, (double)f);

    public static HugeInt operator *(HugeInt a, double d) => MulRoundDown(a, d);
    public static HugeInt operator *(double d, HugeInt a) => MulRoundDown(a, d);
    public static HugeInt operator *(HugeInt a, float f) => MulRoundDown(a, (double)f);
    public static HugeInt operator *(float f, HugeInt a) => MulRoundDown(a, (double)f);

    public static HugeInt MulRound(HugeInt a, double d, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        if (double.IsNaN(d) || double.IsInfinity(d))
            throw new ArgumentOutOfRangeException(nameof(d), "NaN/Infinity not supported for HugeInt multiplication.");

        if (d == 0.0) return Zero;
        if (a._value.IsZero) return Zero;
        if (d == 1.0) return a;
        if (d == -1.0) return new HugeInt(BigInteger.Negate(a._value));

        // Decompose double: d = sign * mantissa * 2^exp  (mantissa is 53-bit integer)
        int sign;
        ulong mantissa;
        int exp;
        DecomposeDouble(d, out sign, out mantissa, out exp);

        // numerator = a * sign * mantissa
        BigInteger numerator = a._value * sign * (BigInteger)mantissa;

        if (exp >= 0)
        {
            // simple left shift if exponent positive
            return new HugeInt(numerator << exp);
        }
        else
        {
            // need to divide by 2^{-exp} with rounding
            int shift = -exp;

            BigInteger quotient = numerator >> shift;
            BigInteger remainder = numerator - (quotient << shift);

            // Half of the divisor (= 2^(shift-1))
            BigInteger half = BigInteger.One << (shift - 1);

            int cmp = remainder.CompareTo(half); // <0, =0, >0
            bool roundUp = false;

            switch (mode)
            {
                case MidpointRounding.AwayFromZero:
                    if (cmp > 0) roundUp = true;
                    if (cmp == 0) roundUp = true;
                    break;

                case MidpointRounding.ToEven:
                    if (cmp > 0) roundUp = true;
                    else if (cmp == 0 && !quotient.IsEven) roundUp = true;
                    break;

                default:
                    // Treat unknown as AwayFromZero
                    if (cmp >= 0) roundUp = true;
                    break;
            }

            if (roundUp)
                quotient += numerator.Sign > 0 ? BigInteger.One : BigInteger.Negate(BigInteger.One);

            return new HugeInt(quotient);
        }
    }
    private static HugeInt MulRoundDown(HugeInt a, double d)
    {
        if (double.IsNaN(d) || double.IsInfinity(d))
            throw new ArgumentOutOfRangeException(nameof(d), "NaN/Infinity not supported.");

        if (a._value.IsZero || d == 0.0) return Zero;

        // Decompose |d| into mantissa (53-bit int) and 2^exp2
        int sign;
        ulong mantissa;
        int exp2;
        DecomposeDouble(d, out sign, out mantissa, out exp2);

        // Multiply integer parts
        BigInteger result = a._value * (BigInteger)sign * (BigInteger)mantissa;

        if (exp2 >= 0)
        {
            // Shift left = multiply by 2^exp2
            result <<= exp2;
            return new HugeInt(result);
        }
        else
        {
            // Divide by 2^-exp2 with FLOOR
            int shift = -exp2;
            BigInteger divisor = BigInteger.One << shift;

            BigInteger quotient = BigInteger.DivRem(result, divisor, out BigInteger remainder);

            if (remainder != 0 && result.Sign < 0)
            {
                // BigInteger.DivRem truncates toward zero for negatives,
                // but we need floor => step one more negative.
                quotient -= BigInteger.One;
            }

            return new HugeInt(quotient);
        }
    }
    public static HugeInt DivRound(HugeInt a, double d,
        MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        if (d == 0)
        {
            throw new DivideByZeroException();
        }

        return MulRound(a, 1.0 / d, mode);
    }

    /// <summary>
    /// Breaks a double into sign, 53-bit mantissa, and base-2 exponent.
    /// Result: value = sign * mantissa * 2^exp, where mantissa has the hidden 1 included.
    /// </summary>
    private static void DecomposeDouble(double value, out int sign, out ulong mantissa, out int exponent)
    {
        long bits = BitConverter.DoubleToInt64Bits(value);

        sign = (bits < 0) ? -1 : 1;
        int rawExp = (int)((bits >> 52) & 0x7FFL);
        ulong rawMantissa = (ulong)bits & 0xFFFFFFFFFFFFFL;

        if (rawExp == 0)
        {
            // subnormal number
            if (rawMantissa == 0)
            {
                mantissa = 0;
                exponent = 0;
                return;
            }
            // For subnormals, exponent is 1-1023, but leading bit is NOT implicit 1
            exponent = -1022 - 52; // will normalize to 53-bit integer
            mantissa = rawMantissa;
            // normalize: shift left until top bit set
            while ((mantissa & (1UL << 52)) == 0)
            {
                mantissa <<= 1;
                exponent--;
            }
        }
        else
        {
            // normal number: implicit leading 1
            mantissa = rawMantissa | (1UL << 52);
            exponent = rawExp - 1023 - 52;
        }
    }

    private static readonly BigInteger TEN_P9 =
        new BigInteger(1_000_000_000);

    // convenience constants
    public static readonly HugeInt Zero = new HugeInt(BigInteger.Zero);
    public static readonly HugeInt One = new HugeInt(BigInteger.One);
}
public static class HugeIntMath
{
    public static HugeInt MulDivFloor(HugeInt value, int num, int den)
    {
        // value * num / den   (automatic floor from integer division)
        return (value * (HugeInt)num) / den;
    }

    public static HugeInt PowInt(HugeInt v, int exp)   // v^exp, exp >= 0
    {
        if (exp == 0) return HugeInt.One;
        HugeInt result = HugeInt.One;
        HugeInt baseV = v;
        int e = exp;
        while (e > 0)
        {
            if ((e & 1) == 1) result *= baseV;
            baseV *= baseV;
            e >>= 1;
        }
        return result;
    }

    public static HugeInt PowInt(int v, int exp) => PowInt((HugeInt)v, exp);
}