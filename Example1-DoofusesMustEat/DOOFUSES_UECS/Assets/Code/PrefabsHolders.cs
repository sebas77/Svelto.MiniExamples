using Unity.Entities;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class PrefabsHolders: MonoBehaviour
    {
        public GameObject prefab1;
        public GameObject prefab2;
        public GameObject prefab3;
        public GameObject prefab4;
        public GameObject prefab5;
        
        public class MyBaker: Baker<PrefabsHolders>
        {
            public override void Bake(PrefabsHolders authoring)
            {
                AddComponent(new MyComponent
                {
                        BlueDoofus = GetEntity(authoring.prefab1),
                        RedDoofus = GetEntity(authoring.prefab2),
                        SpecialDoofus = GetEntity(authoring.prefab3),
                        RedFood = GetEntity(authoring.prefab4),
                        BlueFood = GetEntity(authoring.prefab5),
                } );
            }
        }
        
        public struct MyComponent : IComponentData
        {
            public Entity BlueDoofus;
            public Entity RedDoofus;
            public Entity SpecialDoofus;
            public Entity RedFood;
            public Entity BlueFood;
        }
    }
}