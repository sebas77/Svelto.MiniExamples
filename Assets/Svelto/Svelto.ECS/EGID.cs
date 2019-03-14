using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    public struct EGID:IEquatable<EGID>,IEqualityComparer<EGID>,IComparable<EGID>
    {
        readonly ulong _GID;
        
        public EGID(int entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) : this()
        {
            DBC.ECS.Check.Require(entityID < bit22, "the entityID value is outside the range");
            DBC.ECS.Check.Require(groupID < bit20, "the groupID value is outside the range");
            
            _GID = MAKE_GLOBAL_ID(entityID, groupID, 0);
        }

        const uint bit22 = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0011_1111_1111_1111_1111_1111;
        const uint bit20 = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111_1111;
        const long bit42 = 0b0000_0000_0000_0000_0000_0011_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;

        public int entityID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (int) (_GID & bit22); }
        }

        public ExclusiveGroup.ExclusiveGroupStruct groupID =>
            new ExclusiveGroup.ExclusiveGroupStruct((int) ((_GID >> 22) & bit20));

        ulong maskedGID => _GID & bit42;

        public static bool operator ==(EGID obj1, EGID obj2)
        {
            return obj1.maskedGID == obj2.maskedGID;
        }    
        
        public static bool operator !=(EGID obj1, EGID obj2)
        {
            return obj1.maskedGID != obj2.maskedGID;
        }
        
        internal static ulong MAKE_GLOBAL_ID(int entityId, int groupId, uint realId)
        {
            return ((uint)groupId & bit20) << 22 | ((uint)entityId & bit22) | (realId & bit22) << (22+20);
        }

        public static explicit operator int(EGID id)
        {
            return id.entityID;
        }
        
        public static explicit operator ulong(EGID id)
        {
            return id._GID;
        }

        public bool Equals(EGID other)
        {
            return maskedGID == other.maskedGID;
        }

        public bool Equals(EGID x, EGID y)
        {
            return x.maskedGID == y.maskedGID;
        }

        public int GetHashCode(EGID egid)
        {
            var hash = 11400714819323198485u * egid.maskedGID;
            hash >>= 32;
                    
            return (int) ((hash * uint.MaxValue) >> 32);
        }

        public int CompareTo(EGID other)
        {
            return maskedGID.CompareTo(other.maskedGID);
        }
        
        internal EGID(int entityID, int groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID, 0);
        }
        
        internal EGID(ulong egid) : this()
        {
            _GID = egid;
        }
    }
}