using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.EntityViewComponents
{
    class MoveCubesEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public void Ready() { }

        public void Step()
        {
            var (buffer, count) = entitiesDB.QueryEntities<TransformViewComponent>(ExampleGroups.CubeGroup);

            for (var i = 0; i < count; i++)
                buffer[i].transform.position = Oscillation(buffer[i].transform.position, i);
        }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(MoveCubesEngine);

        Vector3 Oscillation(Vector3 transformPosition, int i)
        {
            var transformPositionY = Mathf.Cos(Time.fixedTime * 3 / Mathf.PI);

            transformPosition.y = transformPositionY + i * 1.5f;

            return transformPosition;
        }
    }
}