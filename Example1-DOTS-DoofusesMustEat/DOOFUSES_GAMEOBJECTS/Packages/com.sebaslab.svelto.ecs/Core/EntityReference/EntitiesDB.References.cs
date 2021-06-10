using System.Runtime.CompilerServices;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEGID(EntityReference entityReference, out EGID egid)
        {
            return _entityLocator.TryGetEGID(entityReference, out egid);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGID GetEGID(EntityReference entityReference)
        {
            return _entityLocator.GetEGID(entityReference);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnginesRoot.LocatorMap GetEntityLocator()
        {
            return _entityLocator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(EGID egid)
        {
            return _entityLocator.GetEntityReference(egid);
        }
    }
}