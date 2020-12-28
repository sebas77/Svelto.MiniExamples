namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    class SyncTransformEngine : ITickingEngine, IQueryingEntitiesEngine
    {
        public SyncTransformEngine(OOPManager oopManager) { _oopLayer = oopManager; }

        public void Step()
        {
            foreach (var ((transforms, indices, count), _) in entitiesDB.QueryEntities<TransformComponent, OOPIndexComponent>(ExampleGroups.Primitive.Groups))
                for (int i = 0; i < count; i++)
                {
                    _oopLayer.SetPotion(indices[i].index, transforms[i].position);
                }
        }

        public string     name       => nameof(SyncTransformEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }

        readonly OOPManager _oopLayer;
    }
}