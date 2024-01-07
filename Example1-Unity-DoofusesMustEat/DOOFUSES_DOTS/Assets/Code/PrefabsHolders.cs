using Unity.Entities;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    public class PrefabsHolder: MonoBehaviour
    {
        public GameObject BlueDoofus;
        public GameObject RedDoofus;
        public GameObject SpecialDoofus;
        public GameObject RedFood;
        public GameObject BlueFood;

        public class MyBaker: Baker<PrefabsHolder>
        {
            public override void Bake(PrefabsHolder authoring)
            {
                AddComponent(
                    new PrefabsComponents
                    {
                        BlueDoofus = GetEntity(authoring.BlueDoofus, TransformUsageFlags.Dynamic),
                        RedDoofus = GetEntity(authoring.RedDoofus, TransformUsageFlags.Dynamic),
                        SpecialDoofus = GetEntity(authoring.SpecialDoofus, TransformUsageFlags.Dynamic),
                        RedFood = GetEntity(authoring.RedFood, TransformUsageFlags.Renderable),
                        BlueFood = GetEntity(authoring.BlueFood, TransformUsageFlags.Renderable),
                    });
            }
        }
    }
}