namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerAnimationEngine: IQueryingEntitiesEngine, IStepEngine, IReactOnAddAndRemove<PlayerEntityViewComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        { }

        public void Step()
        {
            var groups =
                entitiesDB.QueryEntities<PlayerInputDataComponent, PlayerEntityViewComponent>(ECSGroups.PlayersGroup);

            var (playersInput, playersView, count) = groups;

            for (var i = 0; i < count; i++)
            {
                var input = playersInput[i].input;

                // Create a boolean that is true if either of the input axes is non-zero.
                var walking = input.x != 0f || input.z != 0f;

                // Tell the animator whether or not the player is walking.
                playersView[i].animationComponent.animationState = new AnimationState("IsWalking", walking);
            }
        }
        
        public void Add(ref PlayerEntityViewComponent entityComponent, EGID egid) {  }

        //this assumes that removing an entity means its death. If necessary I could check the group it comes from
        //to have a sort of transition callback on state change
        public void Remove(ref PlayerEntityViewComponent playerEntityView, EGID egid)
        {
            playerEntityView.rigidBodyComponent.isKinematic = true;
            playerEntityView.animationComponent.playAnimation        = "Die";
        }

        public string name => nameof(PlayerAnimationEngine);
    }
}