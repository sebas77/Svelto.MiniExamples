namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    class SyncHierarchyEngine : ITickingEngine, IQueryingEntitiesEngine
    {
        public SyncHierarchyEngine(OOPManager oopManager) { _oopLayer = oopManager; }

        public void Step()
        {
            foreach (var ((indices, count), _) in entitiesDB.QueryEntities<OOPIndexComponent>(ExampleGroups.SpherePrimitive.Groups))
                for (int i = 0; i < count; i++)
                {
                    _oopLayer.SetParent(indices[i].index, indices[i].parent);
                }
        }

        public string     name       => nameof(SyncTransformEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }

        readonly OOPManager _oopLayer;
    }
}