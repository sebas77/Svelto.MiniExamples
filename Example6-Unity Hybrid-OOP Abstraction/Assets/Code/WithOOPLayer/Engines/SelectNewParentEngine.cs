using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    class SelectNewParentEngine : ITickingEngine, IQueryingEntitiesEngine
    {
        public void Step()
        {
            if (Input.anyKeyDown)
            {
                var (bufferSphere, _)   = entitiesDB.QueryEntities<OOPIndexComponent>(ExampleGroups.SpherePrimitive.Groups[0]);
                var (bufferCube, count) = entitiesDB.QueryEntities<OOPIndexComponent>(ExampleGroups.CubePrimitive.Groups[0]);

                if (_index == count)
                    _index = 0;
                
                bufferSphere[0].parent = bufferCube[_index].index;
                
                ++_index;
            }
        }

        public string     name       => nameof(SelectNewParentEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
        
        int _index;
    }
}