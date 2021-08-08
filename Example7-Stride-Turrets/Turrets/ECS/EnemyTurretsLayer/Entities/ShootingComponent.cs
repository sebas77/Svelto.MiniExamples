using System;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public struct ShootingComponent : IEntityComponent
    {
        public float  time;
        public int randomTime;
    }
}