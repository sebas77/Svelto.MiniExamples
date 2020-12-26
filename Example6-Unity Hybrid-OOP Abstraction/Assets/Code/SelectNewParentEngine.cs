using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction
{
    class SelectNewParentEngine : ITickingEngine, IQueryingEntitiesEngine
    {
        public void Step()
        {
            if (Input.anyKeyDown)
            {
                var (bufferSphere, _) = entitiesDB.QueryEntities<TransformViewComponent>(ExampleGroups.SphereGroup);
                var (bufferCube, count) = entitiesDB.QueryEntities<TransformViewComponent>(ExampleGroups.CubeGroup);

                if (_index == count)
                    _index = 0;
                
                bufferSphere[0].transform.parent =
                    new ValueReference<TransformImplementor>(bufferCube[_index].transform);
                
                ++_index;
            }
        }

        public string     name       => nameof(SelectNewParentEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
        
        int _index;
    }
}