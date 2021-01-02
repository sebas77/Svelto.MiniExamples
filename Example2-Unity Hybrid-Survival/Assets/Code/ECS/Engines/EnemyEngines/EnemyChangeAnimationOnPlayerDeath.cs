namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyChangeAnimationOnPlayerDeath : IReactOnAddAndRemove<EnemyEntityViewComponent>
    {
        public   void        Add(ref EnemyEntityViewComponent entityComponent, EGID egid)    { }

        public void Remove(ref EnemyEntityViewComponent entityComponent, EGID egid)
        {
            entityComponent.animationComponent.playAnimation = "PlayerDead";
        }
    }
}