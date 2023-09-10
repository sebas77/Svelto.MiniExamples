using System;
using System.Runtime.InteropServices;
using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI
{
    /// <summary>
    /// Contains a struct value of variable type, with type embedded.
    /// This constrains which types we actually support in this feature.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct StructValue : IEquatable<StructValue>
    {
        public override bool Equals(object obj)
        {
            return obj is StructValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)type;
                hashCode = (hashCode * 397) ^ boolValue.GetHashCode();
                hashCode = (hashCode * 397) ^ floatValue.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)uintValue;
                hashCode = (hashCode * 397) ^ textValue.GetHashCode();
                return hashCode;
            }
        }

        [FieldOffset(0)] readonly ValueType type;
        [FieldOffset(4)] readonly bool boolValue;
        [FieldOffset(4)] readonly float floatValue;
        [FieldOffset(4)] readonly uint uintValue;
        [FieldOffset(4)] readonly EcsResource textValue;
        [FieldOffset(4)] readonly EntityReference entityValue;

        public ValueType Type => type;

        public StructValue(EcsResource s) : this()
        {
            textValue = s;
            type = ValueType.String;
        }

        public StructValue(float f) : this()
        {
            floatValue = f;
            type = ValueType.Float;
        }

        public StructValue(uint i) : this()
        {
            uintValue = i;
            type = ValueType.Uint;
        }

        public StructValue(bool b) : this()
        {
            boolValue = b;
            type = ValueType.Bool;
        }

        public StructValue(byte c) : this()
        {
            uintValue = c;
            type = ValueType.Color;
        }

        public StructValue(EntityReference e) : this()
        {
            entityValue = e;
            type = ValueType.Entity;
        }

        public bool AsBool()
        {
            switch (type)
            {
                case ValueType.Bool:
                    return boolValue;
                case ValueType.Float:
                    return floatValue != 0f;
                case ValueType.Uint:
                    return uintValue != 0;
                default:
                    return false;
            }
        }

        public float AsFloat()
        {
            switch (type)
            {
                case ValueType.Bool:
                    return boolValue ? 1f : 0f;
                case ValueType.Float:
                    return floatValue;
                case ValueType.Uint:
                    return uintValue;
                case ValueType.Color:
                    return uintValue;
                default:
                    return 0f;
            }
        }

        public EcsResource AsString()
        {
            switch (type)
            {
                case ValueType.String:
                    return textValue;
                default:
                    return default;
            }
        }

        public EntityReference AsEntity()
        {
            switch (Type)
            {
                case ValueType.Entity:
                    return entityValue;
                default:
                    return default;
            }
        }

        public uint AsUint()
        {
            switch (type)
            {
                case ValueType.Bool:
                    return boolValue ? 1u : 0u;
                case ValueType.Float:
                    return (uint)floatValue;
                case ValueType.Uint:
                    return uintValue;
                case ValueType.Color:
                    return uintValue;
                default:
                    return 0;
            }
        }

        public int AsInt()
        {
            switch (type)
            {
                case ValueType.Bool:
                    return boolValue ? 1 : 0;
                case ValueType.Float:
                    return (int)floatValue;
                case ValueType.Uint:
                    return (int)uintValue;
                case ValueType.Color:
                    return (int)uintValue;
                default:
                    return 0;
            }
        }


        public byte AsByte()
        {
            return (byte)AsUint();
        }

        public bool Equals(StructValue other)
        {
            if (type != other.type)
            {
                return false;
            }
            switch (type)
            {
                case ValueType.String:
                    return textValue == other.textValue;

                default:
                    return uintValue == other.uintValue;
            }
        }

        public static bool operator ==(StructValue a, StructValue b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(StructValue a, StructValue b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return $"<{type}: {(type == ValueType.String ? textValue.ToString() : AsFloat().ToString())}>";
        }
    }

    public enum ValueType : byte
    {
        Bool,
        Uint,
        Float,
        String,
        Color,
        Entity
    }
}