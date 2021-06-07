using System.Runtime.CompilerServices;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEGID(EntityReference entityReference, out EGID egid)
        {
            return _enginesRoot.TryGetEGID(entityReference, out egid);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGID GetEGID(EntityReference entityReference)
        {
            return _enginesRoot.GetEGID(entityReference);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(EGID egid)
        {
            return _enginesRoot.GetEntityReference(egid);
        }
    }
}