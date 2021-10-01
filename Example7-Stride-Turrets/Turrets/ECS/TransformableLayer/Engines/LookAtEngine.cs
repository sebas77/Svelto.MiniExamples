using Stride.Core.Mathematics;
using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// Look for all the entities that have a look At direction and compute the rotation out of it. This must be
    /// executed before ComputeTransformEngine
    /// </summary>
    class LookAtEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            var groups = entitiesDB.FindGroups<LookAtComponent, RotationComponent>();

            foreach (var ((directions, rotation, count), _) in entitiesDB
               .QueryEntities<LookAtComponent, RotationComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    rotation[i].rotation.LookAt(directions[i].vector, Vector3.Zero);
                }
            }
        }
    }
}