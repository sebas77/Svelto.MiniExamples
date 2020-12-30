namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    class SyncTransformEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public SyncTransformEngine(OOPManager oopManager) { _oopManager = oopManager; }
        public void Ready() { }

        public void Step()
        {
            foreach (var ((transforms, indices, count), _) in entitiesDB
               .QueryEntities<TransformComponent, ObjectIndexComponent>(LayerGroups.Primitive.Groups))
                for (var i = 0; i < count; i++)
                    _oopManager.SetPosition(indices[i].index, transforms[i].position);
        }

        public   EntitiesDB entitiesDB { get; set; }
        public   string     name => nameof(SyncTransformEngine);
        
        readonly OOPManager _oopManager;
    }
}