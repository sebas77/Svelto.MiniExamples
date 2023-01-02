using Unity.Entities;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class PrefabsHolders: MonoBehaviour
    {
        public GameObject[] prefabs;
        
        public class MyBaker: Baker<PrefabsHolders>
        {
            public override void Bake(PrefabsHolders authoring)
            {
                Ready = true;
            }
        }
        
        public static bool Ready;
    }
}