using Svelto.DataStructures;
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
                var groupsEnumerable = entitiesDB
                                      .QueryEntities<ObjectIndexComponent>(ExampleGroups.CubePrimitive.Groups)
                                      .GetEnumerator();

                uint                     sphereIndex = 0;
                NB<ObjectIndexComponent> bufferCube  = default;
                int                      cubeIndex   = 0, cubeCount = 0;

                //as long as there are valid groups SpherePrimitive 
                foreach (var ((bufferSphere, egids, sphereCount), _) in entitiesDB
                   .QueryEntities<ObjectParentComponent, EGIDComponent>(ExampleGroups.SpherePrimitive.Groups))
                {
                    //as long as in the current group there are still spheres to iterate
                    while (sphereIndex < sphereCount)
                    {
                        //if there aren't more cubes to iterate
                        if (cubeIndex == cubeCount)
                        {
                            //move to the next cube group, if any, otherwise return
                            if (groupsEnumerable.MoveNext() == false)
                                goto end;

                            ((bufferCube, cubeCount), _) = groupsEnumerable.Current;
                            cubeIndex                    = 0;
                        }

                        //the current sphere is parented with the current cube
                        bufferSphere[sphereIndex].parentIndex = bufferCube[(uint) ((_index + cubeIndex) % cubeCount)].index;

                        //publish the change
                        entitiesDB.PublishEntityChange<ObjectParentComponent>(egids[sphereIndex].ID);

                        //move to the next sphere and next cube
                        sphereIndex++;
                        cubeIndex++;
                    }
                }
end:
                //move cursor so that different parents are chosen every time
                _index++;
            }
        }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(SelectNewParentEngine);
        
        int           _index;
    }
}