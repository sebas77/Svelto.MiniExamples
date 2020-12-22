using System;
using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.Descriptors;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using SveltoDeterministic2DPhysicsDemo;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Builders
{
    /// <summary>
    /// Factory to build Rigidbody Entities
    /// </summary>
    public class RigidBodyWithColliderBuilder
    {
        public void Build(IEntityFactory entityFactory)
        {
            EntityInitializer initializer;
            switch (_colliderType)
            {
                case ColliderType.Box:
                    if (_isKinematic)
                        initializer = entityFactory.BuildEntity<RigidBodyWithBoxColliderDescriptor>(
                            EgidFactory.GetNextId(), GameGroups.KinematicRigidBodyWithBoxColliders.BuildGroup);
                    else
                        initializer = entityFactory.BuildEntity<RigidBodyWithBoxColliderDescriptor>(
                            EgidFactory.GetNextId(), GameGroups.DynamicRigidBodyWithBoxColliders.BuildGroup);
                    break;
                case ColliderType.Circle:
                    if (_isKinematic)
                        initializer = entityFactory.BuildEntity<RigidBodyWithCircleColliderDescriptor>(
                            EgidFactory.GetNextId(), GameGroups.KinematicRigidBodyWithCircleColliders.BuildGroup);
                    else
                        initializer = entityFactory.BuildEntity<RigidBodyWithCircleColliderDescriptor>(
                            EgidFactory.GetNextId(), GameGroups.DynamicRigidBodyWithCircleColliders.BuildGroup);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown {_colliderType}");
            }

            initializer.Init(new TransformEntityComponent(_position, _position));
            initializer.Init(new RigidbodyEntityComponent(_speed, _direction, FixedPointVector2.Zero, FixedPoint.Zero, _restitution, _mass, _isKinematic));

            switch (_colliderType)
            {
                case ColliderType.Box:
                    initializer.Init(new BoxColliderEntityComponent(_boxColliderSize, _boxColliderCentre));
                    break;

                case ColliderType.Circle:
                    initializer.Init(new CircleColliderEntityComponent(_circleColliderRadius, _circleColliderCentre));
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unknown {_colliderType}");
            }
        }

        public static RigidBodyWithColliderBuilder Create() { return new RigidBodyWithColliderBuilder(); }

        public RigidBodyWithColliderBuilder SetBoxCollider(FixedPointVector2 size, FixedPointVector2? centre = null)
        {
            _boxColliderSize   = size;
            _boxColliderCentre = centre ?? FixedPointVector2.Zero;
            _colliderType      = ColliderType.Box;

            return this;
        }

        public RigidBodyWithColliderBuilder SetCircleCollider(FixedPoint radius, FixedPointVector2? centre = null)
        {
            _circleColliderRadius = radius;
            _circleColliderCentre = centre ?? FixedPointVector2.Zero;
            _colliderType         = ColliderType.Circle;

            return this;
        }

        public RigidBodyWithColliderBuilder SetDirection(FixedPointVector2 value)
        {
            _direction = value;
            return this;
        }

        public RigidBodyWithColliderBuilder SetMass(FixedPoint value)
        {
            _mass = value;
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

        public RigidBodyWithColliderBuilder SetRestitution(FixedPoint value)
        {
            _restitution = value;
            return this;
        }

        public RigidBodyWithColliderBuilder SetSpeed(FixedPoint value)
        {
            _speed = value;
            return this;
        }

        FixedPointVector2 _boxColliderCentre    = FixedPointVector2.Zero;
        FixedPointVector2 _boxColliderSize      = FixedPointVector2.Zero;
        FixedPointVector2 _circleColliderCentre = FixedPointVector2.Zero;
        FixedPoint        _circleColliderRadius = FixedPoint.Zero;
        ColliderType      _colliderType         = ColliderType.Box;
        FixedPointVector2 _direction            = FixedPointVector2.Zero;
        FixedPoint        _mass                 = FixedPoint.One;
        FixedPointVector2 _position             = FixedPointVector2.Zero;
        FixedPoint        _restitution          = FixedPoint.One;
        FixedPoint        _speed                = FixedPoint.Zero;

        bool _isKinematic;
    }

    public enum ColliderType
    {
        Box
      , Circle
    }
}