using System;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.AI;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyMovementImplementor
        : MonoBehaviour, IImplementor, IEnemyMovementComponent, ITransformComponent, ILayerComponent
    {
        CapsuleCollider _capsuleCollider; // Reference to the capsule collider.
        NavMeshAgent    _nav;             // Reference to the nav mesh agent.
        Transform       _transform;

        public bool navMeshEnabled { set { _nav.enabled = value; } get { return _nav.enabled; } }

        public Vector3 navMeshDestination  { set { _nav.destination           = value; } }
        public bool    setCapsuleAsTrigger { set { _capsuleCollider.isTrigger = value; } }

        public int layer { set { gameObject.layer = value; } }

        public Vector3 position { get { return _transform.position; } set { _transform.position = value; } }

        public Quaternion rotation { set { _transform.rotation = value; } }

        void Awake()
        {
            _nav             = GetComponent<NavMeshAgent>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _transform       = transform;
        }
    }
}