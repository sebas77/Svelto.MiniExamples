using System;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public struct EnemySinkStruct : IEntityStruct
    {
        public float    sinkAnimSpeed;
        public DateTime animationTime;
        public EGID     ID { get; set; }
    }
}