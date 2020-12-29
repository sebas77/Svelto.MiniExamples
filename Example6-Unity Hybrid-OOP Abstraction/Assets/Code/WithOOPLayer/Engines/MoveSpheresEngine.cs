using Svelto.ECS.Example.OOPAbstraction.OOPLayer;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.WithOOPLayer
{
    class MoveSpheresEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public void Ready() { }

        public void Step()
        {
            foreach (var ((buffer, count), _) in entitiesDB.QueryEntities<TransformComponent>(
                ExampleGroups.SpherePrimitive.Groups))
                for (var i = 0; i < count; i++)
                    Oscillation(ref buffer[i].position, i);
        }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(MoveSpheresEngine);

        void Oscillation(ref Vector3 transformPosition, int i)
        {
            var transformPositionX = Mathf.Cos(Time.fixedTime * 3 / Mathf.PI);

            transformPosition.x = transformPositionX + i * 1.5f;
        }
    }
}