using System;

namespace Veldrid.Assets
{
    public struct AssetID : IEquatable<AssetID>
    {
        private string _value;
        public string Value { get { return _value; } set { _value = value; } }

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        public AssetID(string value)
        {
            _value = value;
        }

        public bool Equals(AssetID other)
        {
            return other._value.Equals(_value);
        }

        public override bool Equals(object obj)
        {
            return obj is AssetID && ((AssetID)(obj)).Equals(this);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return $"ID:{_value}";
        }

        public static implicit operator AssetID(string s) => new AssetID(s);
        public static implicit operator string(AssetID id) => id._value;
    }
}