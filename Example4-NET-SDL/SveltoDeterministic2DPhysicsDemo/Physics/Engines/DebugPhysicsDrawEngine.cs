using System;
using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Graphics;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DebugPhysicsDrawEngine : IQueryingEntitiesEngine, IScheduledGraphicsEngine
    {
        public DebugPhysicsDrawEngine(IGraphics graphics)
        {
            _graphics        = graphics;
        }

        public void Draw(in FixedPoint normalisedDelta)
        {
            foreach (var ((transforms, colliders, count), _) in entitiesDB
                .QueryEntities<TransformEntityComponent, BoxColliderEntityComponent>(
                    GameGroups.DynamicRigidBodyWithBoxColliders.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var transformEntityComponent   = ref transforms[i];
                    ref var boxColliderEntityComponent = ref colliders[i];

                    var point = transformEntityComponent.Interpolate(normalisedDelta);
                    var aabb  = boxColliderEntityComponent.ToAABB(point);

                    var (minX, minY) = aabb.Min;
                    var (maxX, maxY) = aabb.Max;

                    _graphics.DrawBox(Colour.GreenYellow, (int) Math.Round(minX), (int) Math.Round(minY)
                        , (int) Math.Round(maxX), (int) Math.Round(maxY));
                }

            foreach (var ((transforms, colliders, count), _) in entitiesDB
                .QueryEntities<TransformEntityComponent, BoxColliderEntityComponent>(
                    GameGroups.KinematicRigidBodyWithBoxColliders.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var transformEntityComponent   = ref transforms[i];
                    ref var boxColliderEntityComponent = ref colliders[i];

                    var point = transformEntityComponent.Interpolate(normalisedDelta);
                    var aabb  = boxColliderEntityComponent.ToAABB(point);

                    var (minX, minY) = aabb.Min;
                    var (maxX, maxY) = aabb.Max;

                    _graphics.DrawBox(Colour.PaleVioletRed, (int) Math.Round(minX), (int) Math.Round(minY)
                        , (int) Math.Round(maxX), (int) Math.Round(maxY));
                }
        }

        public void Ready() {}
        public   EntitiesDB entitiesDB { get; set; }
        public   string    Name => nameof(DebugPhysicsDrawEngine);
        
        readonly IGraphics _graphics;
    }
}