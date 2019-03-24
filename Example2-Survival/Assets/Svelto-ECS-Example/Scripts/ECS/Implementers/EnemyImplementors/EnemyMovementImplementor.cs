using System;
using UnityEngine;
using UnityEngine.AI;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyMovementImplementor : MonoBehaviour, IImplementor, IEnemyMovementComponent, ITransformComponent, ILayerComponent
    {
        NavMeshAgent    _nav;                       // Reference to the nav mesh agent.
        CapsuleCollider _capsuleCollider;           // Reference to the capsule collider.
        Transform       _transform;
        Action          _removeAction;

        public bool navMeshEnabled { set { _nav.enabled = value; }
            get { return _nav.enabled; }
        }
        public Vector3 navMeshDestination { set { _nav.destination = value;} }
        public bool setCapsuleAsTrigger { set {_capsuleCollider.isTrigger = value; } }

        void Awake ()
        {
            _nav = GetComponent <NavMeshAgent>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _transform = transform;
        }

        public Vector3 position
        {
            get { return _transform.position; }
            set { _transform.position = value; }
        }

        public Quaternion rotation
        {
            set { _transform.rotation = value; }
        }

        public int layer
        {
            set { gameObject.layer = value; }
        }
    }
}
