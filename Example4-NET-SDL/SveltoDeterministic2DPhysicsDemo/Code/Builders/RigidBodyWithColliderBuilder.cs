using System;
using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.Descriptors;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Builders
{
    /// <summary>
    /// Factory to build Rigidbody Entities. This is not a pattern, you can use or not use factories and design
    /// factories like you wish
    /// </summary>
    public struct RigidBodyWithColliderBuilder
    {
        public void Build(IEntityFactory entityFactory)
        {
            EntityInitializer initializer;
            switch (_colliderType)
            {
                case ColliderType.Box:
                    if (_isKinematic)
                        //Svelto code to create entities. Initializer is used to initialise components
                        initializer = entityFactory.BuildEntity<RigidBodyWithBoxColliderDescriptor>(
                            EgidFactory.GetNextId(), GameGroups.KinematicRigidBodyWithBoxColliders.BuildGroup);
                    else
                        initializer = entityFactory.BuildEntity<RigidBodyWithBoxColliderDescriptor>(
                            EgidFactory.GetNextId(), GameGroups.DynamicRigidBodyWithBoxColliders.BuildGroup);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown {_colliderType}");
            }

            //initialise components before they are submitted
            initializer.Init(new TransformEntityComponent(_position, _position));
            initializer.Init(new RigidbodyEntityComponent(_speed, _direction, FixedPointVector2.Zero, _restitution));

            switch (_colliderType)
            {
                case ColliderType.Box:
                    initializer.Init(new BoxColliderEntityComponent(_boxColliderSize, _boxColliderCentre));
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unknown {_colliderType}");
            }
        }

        public static RigidBodyWithColliderBuilder Create()
        {
            return new RigidBodyWithColliderBuilder()
            {
                    _boxColliderCentre = FixedPointVector2.Zero,
                    _boxColliderSize = FixedPointVector2.Zero,
                    _colliderType = ColliderType.Box,
                    _direction = FixedPointVector2.Zero,
                    _mass = FixedPoint.One,
                    _position = FixedPointVector2.Zero,
                    _restitution = FixedPoint.One,
                    _speed = FixedPoint.Zero
            };
        }

        public RigidBodyWithColliderBuilder SetBoxCollider(FixedPointVector2 size, FixedPointVector2? centre = null)
        {
            _boxColliderSize = size;
            _boxColliderCentre = centre ?? FixedPointVector2.Zero;
            _colliderType = ColliderType.Box;

            return this;
        }

        public RigidBodyWithColliderBuilder SetDirection(FixedPointVector2 value)
        {
            _direction = value;
            return this;
        }

        public RigidBodyWithColliderBuilder SetPosition(FixedPointVector2 value)
        {
            _position = value;
            return this;
        }

        public RigidBodyWithColliderBuilder SetIsKinematic(bool isKinematic)
        {
            _isKinematic = isKinematic;
            return this;
        }

        public RigidBodyWithColliderBuilder SetSpeed(FixedPoint value)
        {
            _speed = value;
            return this;
        }

        FixedPointVector2 _boxColliderCentre;
        FixedPointVector2 _boxColliderSize;
        ColliderType _colliderType;
        FixedPointVector2 _direction;
        FixedPoint _mass;
        FixedPointVector2 _position;
        FixedPoint _restitution;
        FixedPoint _speed;

        bool _isKinematic;
    }

    public enum ColliderType
    {
        Box
    }
}