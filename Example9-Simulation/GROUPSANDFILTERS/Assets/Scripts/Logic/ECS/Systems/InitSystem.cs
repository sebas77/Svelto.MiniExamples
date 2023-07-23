// Copyright (c) Sean Nowotny

using Logic.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace Logic.ECS.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class InitSystem : SystemBase
    {
        protected override void OnCreate()
        {
            var randomEntity = EntityManager.CreateEntity();
            var random = new Random(42);
            random.InitState(42);
            EntityManager.AddComponentData(randomEntity, new SingletonRandom {Random = random});
        }

        protected override void OnUpdate()
        {
        }
    }
}