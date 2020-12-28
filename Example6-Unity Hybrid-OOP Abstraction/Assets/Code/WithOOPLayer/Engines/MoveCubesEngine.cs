using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    class MoveCubesEngine : ITickingEngine, IQueryingEntitiesEngine
    {
        public void Step()
        {
            foreach (var ((buffer, count), _) in entitiesDB.QueryEntities<TransformComponent>(ExampleGroups.CubePrimitive.Groups))
                for (int i = 0; i < count; i++)
                {
                    Oscillation(ref buffer[i].position, i);
                }
        }

        void Oscillation(ref Vector3 transformPosition, int i)
        {
            float transformPositionY = Mathf.Cos(Time.fixedTime * 3 / Mathf.PI);
            
            transformPosition.y = transformPositionY + i * 1.5f;
        }

        public string     name       => nameof(MoveCubesEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
    }
}