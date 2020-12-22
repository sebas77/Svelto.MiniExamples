using Svelto.ECS.Components;
using Unity.Entities;

namespace Svelto.ECS.MiniExamples.Example1
{
    public struct UnityECSEntityStruct : IEntityStruct
    {
        public Entity        uecsEntity;
        public ECSVector3    spawnPosition;
        public ComponentType unityComponent;

        public EGID ID { get; set; }
    }
}