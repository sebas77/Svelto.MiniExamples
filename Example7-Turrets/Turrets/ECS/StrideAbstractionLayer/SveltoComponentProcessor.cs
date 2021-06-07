using Stride.Engine;

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

            botInitializer.Init(new ChildComponent(turretInitializer.reference));
            botInitializer.Init(new TRSComponent(botStrideEntityTransform.Position / turretStrideEntityTransform.Scale
                                               , botStrideEntityTransform.Rotation
                                               , botStrideEntityTransform.Scale / turretStrideEntityTransform.Scale));
            turretInitializer.Init(new StartPositionsComponent(turretStrideEntityTransform.Position));
            turretInitializer.Init(new TRSComponent(turretStrideEntityTransform.Position
                                                  , turretStrideEntityTransform.Rotation
                                                  , turretStrideEntityTransform.Scale));

            turretStrideEntityTransform.UseTRS = false;
            botStrideEntityTransform.UseTRS    = false;
        }

        IEntityFactory         _entityFactory;
        ECSStrideEntityManager _ecsManager;
    }
}