using System;
using System.Buffers;
using System.Globalization;
using UnityEngine.Assertions;

namespace uOSC
{
    public struct OCSValue: IDisposable, IEquatable<OCSValue>
    {
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Create(32768, 1024);
        public static OCSValue Invalid = new(ValueType.Invalid);
        public enum ValueType
        {
            Invalid = 0,
            Int,
            Float,
            String,
            Bool,
            Blob,
        }

        public ValueType Type { get; private set; }
        private long _longValue;
        private double _doubleValue;
        private string _stringValue;
        private byte[] _buffer;
        private int _length;
        private bool _ownBuffer;

        public bool IsValid => Type != ValueType.Invalid;

        public int IntValue
        {
            get
            {
                Assert.AreEqual(Type, ValueType.Int);
                return (int)_longValue;
            }
        }
        
        public bool BoolValue
        {
            get
            {
                Assert.AreEqual(Type, ValueType.Bool);
                return _longValue != 0;
            }
        }

        public float FloatValue
        {
            get
            {
                Assert.AreEqual(Type, ValueType.Float);
                return (float)_doubleValue;
            }
        }
        
        public string StringValue
        {
            get
            {
                Assert.AreEqual(Type, ValueType.String);
                return _stringValue;
            }
        }

        public ReadOnlySpan<byte> AsSpan
        {
            get
            {
                Assert.IsTrue(Type is ValueType.Blob);
                return _buffer.AsSpan(0, _length);
            }
        }

        private OCSValue(ValueType type)
        {
            Type = type;
            _longValue = 0;
            _doubleValue = 0;
            _stringValue = null;
            _buffer = null;
            _length = 0;
            _ownBuffer = false; 
        }
        
        public OCSValue(int value) : this(ValueType.Int)
        {
            _longValue = value;
        }
        
        public OCSValue(bool value) : this(ValueType.Bool)
        {
            _longValue = value ? 1 : 0;
        }
        
        public OCSValue(float value) : this(ValueType.Float)
        {
            _doubleValue = value;
        }
        
        public OCSValue(string value) : this(ValueType.String)
        {
            _stringValue = value;
        }
        
        public OCSValue(byte[] value, bool copy = true) : this(ValueType.Blob)
        {
            if (copy)
            {
                _buffer = Pool.Rent(value.Length);
                Array.Copy(value, _buffer, value.Length);
                _length = value.Length;
                _ownBuffer = true; 
            }
            else
            {
                _buffer = value;
                _length = value.Length;
                _ownBuffer = false; 
            }
        }
        
        public OCSValue(ReadOnlySpan<byte> value) : this(ValueType.Blob)
        {
            _buffer = Pool.Rent(value.Length);
            value.CopyTo(_buffer);
            _length = value.Length;
            _ownBuffer = true; 
        }
        
        public static implicit operator OCSValue(int v) => new(v); 
        public static implicit operator OCSValue(bool v) => new(v); 
        public static implicit operator OCSValue(float v) => new(v); 
        public static implicit operator OCSValue(string v) => new(v); 
        public static implicit operator OCSValue(byte[] v) => new(v);
        public static implicit operator OCSValue(ReadOnlySpan<byte> v) => new(v);
        public static implicit operator int(OCSValue v) => v.IntValue;
        public static explicit operator bool(OCSValue v) => v.BoolValue;
        public static implicit operator float(OCSValue v) => v.FloatValue;

        public void Dispose()
        {
            if (_ownBuffer && _buffer != null)
            {
                Pool.Return(_buffer, true);
                _buffer = null;
                _length = 0;
                _ownBuffer = false;
                Type = ValueType.Invalid;
            }
        }

        public bool Equals(OCSValue other)
        {
            return _longValue == other._longValue && _doubleValue.Equals(other._doubleValue) && _stringValue == other._stringValue && Equals(_buffer, other._buffer) && _length == other._length && _ownBuffer == other._ownBuffer && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is OCSValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_longValue, _doubleValue, _stringValue, _buffer, _length, _ownBuffer, (int)Type);
        }

        public static bool operator ==(OCSValue left, OCSValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OCSValue left, OCSValue right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return Type switch
            {
                ValueType.Int => _longValue.ToString(),
                ValueType.Float => _doubleValue.ToString(CultureInfo.InvariantCulture),
                ValueType.String => _stringValue,
                ValueType.Bool => _longValue.ToString(),
                ValueType.Blob => AsSpan.ToString(),
                ValueType.Invalid => "!INVALID",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}