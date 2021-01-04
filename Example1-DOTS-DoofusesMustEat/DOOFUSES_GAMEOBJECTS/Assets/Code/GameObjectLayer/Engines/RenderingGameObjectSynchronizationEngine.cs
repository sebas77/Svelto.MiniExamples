using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Svelto.ECS.MiniExamples.GameObjectsLayer
{
    [Sequenced(nameof(GameObjectLayerEngineNames.RenderingGameObjectSynchronizationEngine))]
    class RenderingGameObjectSynchronizationEngine : IQueryingEntitiesEngine, IJobifiedEngine
    {
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }

        public RenderingGameObjectSynchronizationEngine(GameObjectManager goManager) { this._goManager = goManager; }

        public JobHandle Execute(JobHandle inputDeps)
        {
            JobHandle combineDependencies = inputDeps;

            //Completely abstract engine. There is no assumption of in which groups the component can be.
            //I am planning to add the concept of disabled groups in future as the state enabled/disable is very
            //abstract and it makes sense to have the concept at framework level
            foreach (var ((positions, count), group) in entitiesDB.QueryEntities<PositionEntityComponent>())
            {
                Check.Require(_goManager.Transforms((int) (uint) group).length == count
                            , $"component array length doesn't match. Expected {count} - found {_goManager.Transforms((int) (uint) group).length} - group {group.ToName()}");
                combineDependencies = JobHandle.CombineDependencies(inputDeps, new ParallelLabelTransformJob()
                {
                    _position = positions
                }.Schedule(_goManager.Transforms((int) (uint) group), inputDeps), combineDependencies);
            }

            return combineDependencies;
        }

        public string name => nameof(RenderingGameObjectSynchronizationEngine);

        readonly GameObjectManager _goManager;

        struct ParallelLabelTransformJob : IJobParallelForTransform
        {
            public NB<PositionEntityComponent> _position;

            public void Execute(int index, TransformAccess transform)
            {
                transform.position = _position[index].position;
            }
        }
    }
}