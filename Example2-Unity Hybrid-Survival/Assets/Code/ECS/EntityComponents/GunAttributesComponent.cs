using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public struct GunAttributesComponent : IEntityComponent, INeedEGID
    {
        //being lazy here, it should be read from json file
        const int   DamagePerShot      = 20;    // The damage inflicted by each bullet.
        const float Range              = 100f;  // The distance the gun can fire.
        const float TimeBetweenBullets = 0.15f; // The time between each shot.

        public float   timeBetweenBullets => TimeBetweenBullets;
        public float   range              => Range;
        public int     damagePerShot      => DamagePerShot;
        public float   timer;
        public Vector3 lastTargetPosition;

        public EGID ID { get; set; }
    }
}