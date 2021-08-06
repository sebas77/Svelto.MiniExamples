using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Pickups
{
    public struct AmmoPickupEntityViewComponent : IEntityViewComponent
    {
        public ITransformComponent    transformComponent;
        public ISpawnedComponent      spawnedComponent;
        public IAmmoTriggerComponent  targetTriggerComponent;

        public EGID ID { get; set; }
    }
}