using Svelto.ECS.Example.Survive.Camera;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerMovementImplementor
        : MonoBehaviour, IImplementor, IRigidBodyComponent, ICameraTargetComponent, ISpeedComponent, ITransformComponent
    {
        Rigidbody    _playerRigidbody; // Reference to the player's rigidbody.
        Transform    _playerTransform;
        public float speed = 6f; // The speed that the player will move at.

        public Vector3 position
        {
            get { return _playerTransform.position; }
            set { _playerRigidbody.MovePosition(value); }
        }

        public bool       isKinematic   { set { _playerRigidbody.isKinematic = value; } }
        public float      movementSpeed { get { return speed; } }
        public Quaternion rotation      { set { _playerRigidbody.MoveRotation(value); } }

        void Awake()
        {
            _playerRigidbody = GetComponent<Rigidbody>();
            _playerTransform = transform;
        }
    }
}