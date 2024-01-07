namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    /// <summary>
    /// This engine has the responsibility to synchronise the entity transforms with the object transform
    /// note that the engine is abstract even if it's using group compound as it is using a generic tag
    /// that specialised entities must adopt to be fetched by this engine.
    /// </summary>
    class SyncTransformEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public SyncTransformEngine(OOPManager oopManager) { _oopManager = oopManager; }
        public void Ready() { }

        public void Step()
        {
            foreach (var ((transforms, indices, count), _) in entitiesDB
                            .QueryEntities<TransformComponent, ObjectIndexComponent>(LayerGroups.Primitive.Groups))
            {
                for (var i = 0; i < count; i++)
                    _oopManager.SetPosition(indices[i].index, transforms[i].position);
            }
            //this is the same as the above, but it's using a different syntax. Useful when you need to query many components
            
//            foreach (var ((transforms, count), group) in entitiesDB
//                            .QueryEntities<TransformComponent>(LayerGroups.Primitive.Groups))
//            {
//                var (indices, countIndices) = entitiesDB.QueryEntities<ObjectIndexComponent>(group);
//                
//                for (var i = 0; i < count; i++)
//                    _oopManager.SetPosition(indices[i].index, transforms[i].position);
//            }
        }

        public   EntitiesDB entitiesDB { get; set; }
        public   string     name => nameof(SyncTransformEngine);
        
        readonly OOPManager _oopManager;
    }
}