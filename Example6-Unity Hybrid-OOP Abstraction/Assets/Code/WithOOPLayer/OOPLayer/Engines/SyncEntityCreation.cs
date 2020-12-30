using System;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    class SyncEntityCreation : IReactOnAddAndRemove<ObjectIndexComponent>
    {
        public SyncEntityCreation(OOPManager oopManager)
        {
            _oopManager = oopManager;
        }

        public void Add(ref ObjectIndexComponent entityComponent, EGID egid)
        {
            entityComponent.index = _oopManager.RegisterEntity(entityComponent.type);
        }

        public void Remove(ref ObjectIndexComponent entityComponent, EGID egid)
        {
        }

        readonly OOPManager _oopManager;
    }
}