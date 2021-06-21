namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerAnimationEngine: IQueryingEntitiesEngine, IStepEngine, IReactOnAddAndRemove<PlayerEntityViewComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        { }

        public void Step()
        {
            //Remembering the syntax to query groups may be hard at first. You get used of it over the time, but 
            //Rider and VS can help you with this work. Follow this trick:
            //first use a random variable name in the foreach like asd
            // foreach (var asd in entitiesDB.QueryEntities<PlayerInputDataComponent, PlayerEntityViewComponent>(Player.Groups))
            //then go over asd and ask Rider or VS to deconstruct the variable,  now it should look like
            //foreach (var ((buffer1, buffer2, count), exclusiveGroupStruct) in entitiesDB.QueryEntities<PlayerInputDataComponent, PlayerEntityViewComponent>(Player.Groups))
            foreach (var ((playersInput, playersView, count), _) in entitiesDB
               .QueryEntities<PlayerInputDataComponent, PlayerEntityViewComponent>(Player.Groups))
            {
                for (var i = 0; i < count; i++)
                {
                    var input = playersInput[i].input;

                    // Create a boolean that is true if either of the input axes is non-zero.
                    var walking = input.x != 0f || input.z != 0f;

                    // Tell the animator whether or not the player is walking.
                    playersView[i].animationComponent.animationState = new AnimationState("IsWalking", walking);
                }
            }
        }
        
        public void Add(ref PlayerEntityViewComponent entityComponent, EGID egid) {  }

        public void Remove(ref PlayerEntityViewComponent playerEntityView, EGID egid)
        {
            //Disable the RB to avoid further gameobject collisions while the player entity doesn't exist
            playerEntityView.rigidBodyComponent.isKinematic = true;
            playerEntityView.animationComponent.playAnimation        = "Die";
        }

        public string name => nameof(PlayerAnimationEngine);
    }
}