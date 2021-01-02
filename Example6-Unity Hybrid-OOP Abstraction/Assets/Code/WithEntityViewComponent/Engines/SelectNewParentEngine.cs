using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.EntityViewComponents
{
    class SelectNewParentEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public void Ready() { }

        public void Step()
        {
            if (Input.anyKeyDown)
            {
                var (bufferSphere, _)   = entitiesDB.QueryEntities<TransformViewComponent>(ExampleGroups.SphereGroup);
                var (bufferCube, count) = entitiesDB.QueryEntities<TransformViewComponent>(ExampleGroups.CubeGroup);

                if (_index == count)
                    _index = 0;

                bufferSphere[0].transform.parent = new ValueReference<ITransformImplementor>(bufferCube[_index].transform);

                ++_index;
            }
        }

        int               _index;
        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(SelectNewParentEngine);
    }
}