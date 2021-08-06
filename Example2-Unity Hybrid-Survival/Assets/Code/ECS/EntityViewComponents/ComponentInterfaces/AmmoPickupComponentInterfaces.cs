using System;

namespace Svelto.ECS.Example.Survive.Pickups
{
    public interface ISpawnedComponent
    {
        bool spawned {get; set;}
    }


    public interface IAmmoTriggerComponent
    {
        DispatchOnChange<EntityReference> hitChange { get; set; }
    }
}