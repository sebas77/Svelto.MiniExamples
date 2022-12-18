using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.AI;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyMovementImplementor
        : MonoBehaviour, IImplementor, IEnemyMovementComponent, ILayerComponent
    {
        public bool       navMeshEnabled      { set => _nav.enabled = value; }
        public Vector3    navMeshDestination  { set => _nav.destination           = value; }
        public bool       setCapsuleAsTrigger { set => _capsuleCollider.isTrigger = value; }
        public int        layer               { set => gameObject.layer = value; }

        void Awake()
        {
            _nav             = GetComponent<NavMeshAgent>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
        }

        CapsuleCollider _capsuleCollider; // Reference to the capsule collider.
        NavMeshAgent    _nav;             // Reference to the nav mesh agent.
    }
}