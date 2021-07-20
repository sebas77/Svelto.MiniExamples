using Stride.Core.Mathematics;
using Stride.Engine;
using Matrix = BulletSharp.Math.Matrix;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// This is a naive and super focused example on how to possibly create factories in Stride that automatically
    /// convert Stride Entities into Svelto Entities. I didn't have the time and it wasn't in the scope to make
    /// this more generic, but it's surely possible to do so. Current version is very dedicated to the demo purposes.
    /// </summary>
    public class SveltoComponentProcessor : EntityProcessor<TurretEntityHolder>
    {
        protected override void OnSystemAdd()
        {
            _entityFactory = Services.GetService<IEntityFactory>();
            _ecsManager    = Services.GetService<ECSStrideEntityManager>();
        }

        protected override void OnEntityComponentAdding
            (Entity turretStrideEntity, TurretEntityHolder turretStrideEntityComponent, TurretEntityHolder data)
        {
            var sveltoEntityID      = _ecsManager.RegisterStrideEntity(turretStrideEntity);
            var sveltoEntityIDChild = _ecsManager.RegisterStrideEntity(turretStrideEntityComponent.child);

            var botInitializer = _entityFactory.BuildEntity(new EGID(sveltoEntityIDChild, BotTag.BuildGroup)
                                                          , turretStrideEntityComponent.GetChildDescriptor());
            var turretInitializer = _entityFactory.BuildEntity(new EGID(sveltoEntityID, TurretTag.BuildGroup)
                                                             , turretStrideEntityComponent.GetDescriptor());
            var botStrideEntityTransform    = turretStrideEntityComponent.child.Transform;
            var turretStrideEntityTransform = turretStrideEntity.Transform;

            turretStrideEntityTransform.UpdateWorldMatrix();
            botStrideEntityTransform.UpdateWorldMatrix();

            var turretWorldMatrix    = turretStrideEntityTransform.WorldMatrix;
            var turretBotWorldMatrix = botStrideEntityTransform.WorldMatrix;

            turretWorldMatrix.Decompose(out Vector3 scaleA, out Quaternion rotationA, out Vector3 translationA);
            turretWorldMatrix.Invert();
            turretBotWorldMatrix *= turretWorldMatrix;
            turretBotWorldMatrix.Decompose(out Vector3 scaleB, out Quaternion rotationB, out Vector3 translationB);
            

            botInitializer.Init(new ChildComponent(turretInitializer.reference));
            botInitializer.Init(new PositionComponent(translationB));
            botInitializer.Init(new RotationComponent(rotationB));
            botInitializer.Init(new ScalingComponent(scaleB));
            botInitializer.Init(new DirectionComponent()
            {
                vector = Vector3.UnitX
            });

            turretInitializer.Init(new StartPositionsComponent(translationA));
            turretInitializer.Init(new PositionComponent(translationA));
            turretInitializer.Init(new RotationComponent(rotationA));
            turretInitializer.Init(new ScalingComponent(scaleA));

            turretStrideEntityTransform.UseTRS = false;
            botStrideEntityTransform.UseTRS    = false;
            //
            // turretStrideEntityTransform.WorldMatrix = Stride.Core.Mathematics.Matrix.Identity;
            // botStrideEntityTransform.WorldMatrix    = Stride.Core.Mathematics.Matrix.Identity;
            //
            // botStrideEntityTransform.UpdateLocalFromWorld();
            // turretStrideEntityTransform.UpdateLocalFromWorld();
        }

        IEntityFactory         _entityFactory;
        ECSStrideEntityManager _ecsManager;
    }
}