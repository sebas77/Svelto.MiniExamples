using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.EntityViewComponents
{
    class MoveSpheresEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public void Ready() { }

        public void Step()
        {
            var (buffer, count) = entitiesDB.QueryEntities<TransformViewComponent>(ExampleGroups.SphereGroup);

            for (var i = 0; i < count; i++)
                buffer[i].transform.position = Oscillation(buffer[i].transform.position, i);
        }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(MoveSpheresEngine);

        Vector3 Oscillation(Vector3 transformPosition, int i)
        {
            var transformPositionX = Mathf.Cos(Time.fixedTime * 3 / Mathf.PI);

            transformPosition.x = transformPositionX + i * 1.5f;

            return transformPosition;
        }
    }
}