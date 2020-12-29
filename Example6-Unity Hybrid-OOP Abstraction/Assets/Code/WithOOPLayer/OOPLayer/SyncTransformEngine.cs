namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    class SyncTransformEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public SyncTransformEngine(OOPManager oopManager) { _oopManager = oopManager; }
        public void Ready() { }

        public void Step()
        {
            foreach (var ((transforms, indices, count), _) in entitiesDB
               .QueryEntities<TransformComponent, ObjectIndexComponent>(ExampleGroups.Primitive.Groups))
                for (var i = 0; i < count; i++)
                    _oopManager.SetPotion(indices[i].index, transforms[i].position);
        }

        readonly OOPManager _oopManager;
        public   EntitiesDB entitiesDB { get; set; }

        public string name => nameof(SyncTransformEngine);
    }
}