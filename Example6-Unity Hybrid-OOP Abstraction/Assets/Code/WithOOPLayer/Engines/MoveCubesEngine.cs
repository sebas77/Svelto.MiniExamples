using Svelto.ECS.Example.OOPAbstraction.OOPLayer;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.WithOOPLayer
{
    class MoveCubesEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public void Ready() { }

        public void Step()
        {
            foreach (var ((buffer, count), _) in entitiesDB.QueryEntities<TransformComponent>(
                ExampleGroups.CubePrimitive.Groups))
                for (var i = 0; i < count; i++)
                    Oscillation(ref buffer[i].position, i);
        }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(MoveCubesEngine);

        void Oscillation(ref Vector3 transformPosition, int i)
        {
            var transformPositionY = Mathf.Cos(Time.fixedTime * 3 / Mathf.PI);

            transformPosition.y = transformPositionY + i * 1.5f;
        }
    }
}