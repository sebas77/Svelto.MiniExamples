using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyChangeAnimationOnTargetDeathEngine : IReactOnSwapEx<HealthComponent>, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
        
        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<HealthComponent> entities, ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup)
        {
            if (EnemyTarget.Includes(fromGroup) && Dead.Includes(toGroup)) //were the entities swapped EntityTargets and just died?
            {
                foreach (var ((navMesh, anim, count), _) in entitiesDB.QueryEntities<NavMeshComponent, AnimationComponent>(EnemyAliveGroup.Groups))
                {
                    for (int i = 0; i < count; i++)
                    {
                        anim[i].animationState = new AnimationState((int)EnemyAnimations.TargetDead);
                        navMesh[i].navMeshEnabled = false;
                    }
                }
            }
        }
    }
}

