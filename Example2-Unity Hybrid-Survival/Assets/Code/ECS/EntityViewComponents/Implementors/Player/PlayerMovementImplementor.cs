using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerMovementImplementor : MonoBehaviour, IImplementor, IRigidBodyComponent, ITransformComponent
    {
        public Vector3 position
        {
            get => _playerTransform.position;
            set => _playerRigidbody.MovePosition(value);
        }

        public bool isKinematic
        {
            set
            {
                _playerRigidbody.isKinematic = value;
                _playerCollider.enabled      = !value;
            }
        }

        public Vector3 velocity { set => _playerRigidbody.velocity = value; }

        public Quaternion rotation      { set => _playerRigidbody.MoveRotation(value); }

        void Awake()
        {
            _playerRigidbody = GetComponent<Rigidbody>();
            _playerCollider  = GetComponent<Collider>();
            _playerTransform = transform;
        }
        
        Rigidbody _playerRigidbody; // Reference to the player's rigidbody.
        Transform _playerTransform;
        Collider  _playerCollider;
    }
}