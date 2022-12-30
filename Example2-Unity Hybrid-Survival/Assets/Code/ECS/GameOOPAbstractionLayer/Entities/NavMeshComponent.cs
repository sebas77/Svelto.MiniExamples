using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public struct NavMeshComponent: IEntityComponent
    {
        public Vector3 navMeshDestination;
        public bool setCapsuleAsTrigger;
        public bool navMeshEnabled;
    }
}