using System;
using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Graphics;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DebugPhysicsDrawEngine : IQueryingEntitiesEngine, IScheduledGraphicsEngine
    {
        public DebugPhysicsDrawEngine(EngineScheduler engineScheduler, IGraphics graphics)
        {
            _engineScheduler = engineScheduler;
            _graphics        = graphics;
        }

        public void Draw(FixedPoint normalisedDelta)
        {
            foreach (var ((transforms, count), _) in entitiesDB.QueryEntities<TransformEntityComponent>(
                GameGroups.RigidBodies.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var transformEntityComponent = ref transforms[i];

                    var (drawX, drawY) = transformEntityComponent.Interpolate(normalisedDelta);

                    _graphics.DrawPlus(Colour.Aqua, (int) Math.Round(drawX), (int) Math.Round(drawY), 3);
                }

            foreach (var ((transforms, colliders, count), _) in entitiesDB
               .QueryEntities<TransformEntityComponent, BoxColliderEntityComponent>(
                    GameGroups.WithBoxCollider.Groups))
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

            foreach (var ((transforms, colliders, count), _) in entitiesDB
               .QueryEntities<TransformEntityComponent, CircleColliderEntityComponent>(
                    GameGroups.WithCircleCollider.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var transformEntityComponent      = ref transforms[i];
                    ref var circleColliderEntityComponent = ref colliders[i];

                    var point = transformEntityComponent.Interpolate(normalisedDelta);

                    var x = FixedPoint.ConvertToInteger(
                        MathFixedPoint.Round(point.X + circleColliderEntityComponent.Center.X));
                    var y = FixedPoint.ConvertToInteger(
                        MathFixedPoint.Round(point.Y + circleColliderEntityComponent.Center.Y));
                    var radius = FixedPoint.ConvertToInteger(circleColliderEntityComponent.Radius);

                    _graphics.DrawCircle(Colour.PaleVioletRed, x, y, radius);
                }
        }

        public void Ready() { _engineScheduler.RegisterScheduledGraphicsEngine(this); }

        readonly EngineScheduler _engineScheduler;
        readonly IGraphics       _graphics;
        public   EntitiesDB      entitiesDB { get; set; }

        public string Name => nameof(DebugPhysicsDrawEngine);
    }
}