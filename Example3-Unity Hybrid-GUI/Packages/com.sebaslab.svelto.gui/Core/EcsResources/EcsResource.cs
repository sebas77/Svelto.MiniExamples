using System;
using System.Runtime.InteropServices;

namespace Svelto.ECS.GUI.Resources
{
    [StructLayout(LayoutKind.Explicit)]
    public struct EcsResource : IEquatable<EcsResource>
    {
        [FieldOffset(0)] internal readonly uint  _type;
        [FieldOffset(4)] internal readonly uint  _id;
        [FieldOffset(8)] internal readonly uint  _versioning;
        [FieldOffset(4)] internal readonly ulong _realID;

        public EcsResource(uint type, uint id, uint versioning) : this()
        {
            _type       = type;
            _id         = id;
            _versioning = versioning;
        }

        public bool Equals(EcsResource other) { return _type == other._type && _realID == other._realID; }

        public static bool operator ==(EcsResource options1, EcsResource options2)
        {
            return options1._type == options2._type && options1._realID == options2._realID;
        }

        public static bool operator !=(EcsResource options1, EcsResource options2)
        {
            return options1._type != options2._type && options1._realID != options2._realID;
        }

        public override bool Equals(object obj)
        {
            throw new NotSupportedException(); //this is on purpose
        }

        public override int GetHashCode() { return _type.GetHashCode() * 12582917 * _realID.GetHashCode(); }

        public override string ToString()
        {
            return $"<EcsResource type:{_type} id:{_id} version:{_versioning}";
        }

        public static EcsResource Empty => default;
    }
}