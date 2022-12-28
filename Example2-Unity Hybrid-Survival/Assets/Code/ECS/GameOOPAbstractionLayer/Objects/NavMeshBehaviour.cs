using UnityEngine;
using UnityEngine.AI;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class NavMeshBehaviour: MonoBehaviour
    {
        public bool navMeshEnabled { set => _nav.enabled = value; }
        public Vector3 navMeshDestination { set => _nav.destination = value; }
        public bool setCapsuleAsTrigger { set => _capsuleCollider.isTrigger = value; }

        void Awake()
        {
            _nav = GetComponent<NavMeshAgent>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
        }

        CapsuleCollider _capsuleCollider; // Reference to the capsule collider.
        NavMeshAgent _nav;                // Reference to the nav mesh agent.
    }
}