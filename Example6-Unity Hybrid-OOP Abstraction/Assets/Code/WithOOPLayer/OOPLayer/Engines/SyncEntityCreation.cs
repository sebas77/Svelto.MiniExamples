namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    /// <summary>
    /// Using this engine allows to completely abstract away the oopManager from specialised layers.
    /// Once an Entity with ObjectIndexComponent is created, regardless the descriptor used, an Object
    /// is registered and index registered. Destruction of entities is not contemplated in this demo, but it would
    /// be simple to add.
    /// </summary>
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