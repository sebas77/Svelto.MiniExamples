using Svelto.Common;
using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Graphics;
using SveltoDeterministic2DPhysicsDemo.Maths;
using SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents;
using SveltoDeterministic2DPhysicsDemo.Physics.Loggers;

namespace SveltoDeterministic2DPhysicsDemo.Physics.Engines
{
    [Sequenced(nameof(PhysicsEngineNames.ResolvePenetrationEngine))]
    public class ResolvePenetrationEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public ResolvePenetrationEngine(IEngineScheduler engineScheduler) { _engineScheduler = engineScheduler; }

        public void Execute(ulong tick)
        {
            foreach (var ((transforms, rigidbodies, manifolds, count), _) in entitiesDB
               .QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, CollisionManifoldEntityComponent>(
                    GameGroups.RigidBodies.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var transform = ref transforms[i];
                    ref var manifold  = ref manifolds[i];
                    ref var rigidbody = ref rigidbodies[i];

                    if (rigidbody.IsKinematic)
                        continue;

                    if (!manifold.CollisionManifold.HasValue)
                        continue;

                    var collisionManifold = manifold.CollisionManifold.Value;

                    FixedPointVector2Logger.Instance.DrawCross(
                        transform.Position - collisionManifold.Normal * collisionManifold.Penetration, tick
                      , Colour.Orange
                      , FixedPoint.ConvertToInteger(MathFixedPoint.Round(collisionManifold.Penetration)));

                    transform = TransformEntityComponent.From(transform.Position - collisionManifold.Normal
                                                            , transform.PositionLastPhysicsTick
                                                            , transform.Position - collisionManifold.Normal
                                                            / FixedPoint.Two);
                }
        }

        public void Ready() { _engineScheduler.RegisterScheduledPhysicsEngine(this); }

        readonly IEngineScheduler _engineScheduler;
        public   EntitiesDB       entitiesDB { get; set; }

        public string Name => nameof(ResolvePenetrationEngine);
    }
}