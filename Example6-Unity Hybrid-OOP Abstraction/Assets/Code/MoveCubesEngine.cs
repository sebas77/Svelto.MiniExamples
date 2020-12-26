using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction
{
    class MoveCubesEngine : ITickingEngine, IQueryingEntitiesEngine
    {
        public void Step()
        {
            var (buffer, count) = entitiesDB.QueryEntities<TransformViewComponent>(ExampleGroups.CubeGroup);
            
            for (int i = 0; i < count; i++)
            {
                buffer[i].transform.position = Oscillation(buffer[i].transform.position, i);
            }
        }

        Vector3 Oscillation(Vector3 transformPosition, int i)
        {
            float transformPositionY = Mathf.Cos(Time.fixedTime * 3 / Mathf.PI);
            
            transformPosition.y = transformPositionY + i * 1.5f;    
            
            return transformPosition;
        }

        public string     name       => nameof(MoveCubesEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
    }
}