using Svelto.ECS.Example.OOPAbstraction.OOPLayer;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.WithOOPLayer
{
    class SelectNewParentEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public void Ready() { }

        public void Step()
        {
            if (Input.anyKeyDown)
            {
                uint sphereIndex = 0;
                foreach (var ((bufferSphere, egids, sphereCount), _) in entitiesDB.QueryEntities<ObjectParentComponent, EGIDComponent>(ExampleGroups.SpherePrimitive.Groups))
                foreach (var ((bufferCube, cubeCount), _) in entitiesDB.QueryEntities<ObjectIndexComponent>(ExampleGroups.CubePrimitive.Groups))
                {
                    while (sphereIndex < sphereCount && sphereIndex < cubeCount)
                    {
                        bufferSphere[sphereIndex].parent = bufferCube[(uint) ((_index + sphereIndex) % cubeCount)].index;
                        entitiesDB.PublishEntityChange<ObjectParentComponent>(egids[sphereIndex].ID);
                        sphereIndex++;
                    }
                }

                _index++;
            }
        }

        int               _index;
        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(SelectNewParentEngine);
    }
}